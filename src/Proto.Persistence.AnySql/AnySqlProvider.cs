using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Proto.Persistence.AnySql
{
	/// <summary>
	/// Base implementation of persistence provider for any SQL database.
	/// </summary>
	/// <seealso cref="IProvider" />
	public class AnySqlProvider: IProvider
	{
		private readonly Func<DbConnection> _connect;
		private readonly Func<object, string> _serialize;
		private readonly Func<string, object> _deserialize;
		private readonly IAnySqlDialect _dialect;

		private readonly string _eventsTable;
		private readonly string _snapshotsTable;
		private readonly Task _ready;

		/// <summary>Initializes a new instance of the <see cref="AnySqlProvider"/> class.</summary>
		/// <param name="connect">The <see cref="DbConnection"/> factory function.</param>
		/// <param name="schema">The schema name. Can be <c>null</c>.</param>
		/// <param name="table">The table name prefix.</param>
		/// <param name="serialize">The serialization function.</param>
		/// <param name="deserialize">The deserialization function.</param>
		/// <param name="dialect">The SQL dialect.</param>
		public AnySqlProvider(
			Func<DbConnection> connect,
			string schema, string table,
			Func<object, string> serialize,
			Func<string, object> deserialize,
			IAnySqlDialect dialect)
		{
			_connect = connect;
			_serialize = serialize;
			_deserialize = deserialize;
			_dialect = dialect;

			_eventsTable = Dialect.EventsTable(schema, table);
			_snapshotsTable = Dialect.SnapshotsTable(schema, table);
			_ready = CreateTables();
		}

		protected async Task<DbConnection> Connect(Task ready)
		{
			await (ready ?? Task.CompletedTask);

			DbConnection connection = null;
			try
			{
				connection = _connect();
				await connection.OpenAsync();
				return connection;
			}
			catch
			{
				connection?.Dispose();
				throw;
			}
		}

		private async Task<DbConnection> Connect() => await Connect(_ready);

		private string Serialize(object obj) => _serialize(obj);
		private object Deserialize(string str) => _deserialize(str);
		private IAnySqlDialect Dialect => _dialect;

		private async Task CreateTables()
		{
			using (var connection = await Connect(Task.CompletedTask))
			{
				await connection
					.CreateCommand(Dialect.CreateEventsTable(_eventsTable))
					.ExecuteNonQueryAsync();
				await connection
					.CreateCommand(Dialect.CreateSnapshotsTable(_snapshotsTable))
					.ExecuteNonQueryAsync();
			}
		}

		/// <summary>Reapplies events.</summary>
		/// <param name="actorName">Name of the actor.</param>
		/// <param name="indexStart">The start index.</param>
		/// <param name="indexEnd">The last index.</param>
		/// <param name="callback">The callback.</param>
		/// <returns>last applied index.</returns>
		public async Task<long> GetEventsAsync(
			string actorName, long indexStart, long indexEnd, Action<object> callback)
		{
			using (var connection = await Connect())
			{
				var lastIndex = -1L;

				var command = connection
					.CreateCommand(Dialect.SelectEvents(_eventsTable))
					.AddParameter("actor", DbType.String, actorName)
					.AddParameter("index_start", DbType.Int64, indexStart)
					.AddParameter("index_end", DbType.Int64, indexEnd);

				using (var reader = await command.ExecuteReaderAsync())
				{
					var indexField = reader.IndexOf("index");
					var blobField = reader.IndexOf("data");

					while (await reader.ReadAsync())
					{
						var index = reader.GetInt64(indexField);
						var blob = reader.GetString(blobField);
						callback(Deserialize(blob));
						lastIndex = index;
					}
				}

				return lastIndex;
			}
		}

		/// <summary>Persists event.</summary>
		/// <param name="actorName">Name of the actor.</param>
		/// <param name="index">The event index.</param>
		/// <param name="event">The event data.</param>
		/// <returns>New event index.</returns>
		public async Task<long> PersistEventAsync(string actorName, long index, object @event)
		{
			using (var connection = await Connect())
			{
				var command = connection
					.CreateCommand(Dialect.InsertEvent(_eventsTable))
					.AddParameter("actor", DbType.String, actorName)
					.AddParameter("index", DbType.Int64, index)
					.AddParameter("data", DbType.String, Serialize(@event));
				await command.ExecuteNonQueryAsync();
				return index + 1;
			}
		}

		/// <summary>Deletes events.</summary>
		/// <param name="actorName">Name of the actor.</param>
		/// <param name="inclusiveToIndex">Index of last event to delete.</param>
		/// <returns>Completed task when finished.</returns>
		public async Task DeleteEventsAsync(string actorName, long inclusiveToIndex)
		{
			using (var connection = await Connect())
			{
				var command = connection
					.CreateCommand(Dialect.DeleteEvents(_eventsTable))
					.AddParameter("actor", DbType.String, actorName)
					.AddParameter("index_end", DbType.Int64, inclusiveToIndex);
				await command.ExecuteNonQueryAsync();
			}
		}

		/// <summary>Gets the snapshot.</summary>
		/// <param name="actorName">Name of the actor.</param>
		/// <returns>Snapshot and its index.</returns>
		/// <exception cref="InvalidOperationException">More than one snapshot returned</exception>
		public async Task<(object Snapshot, long Index)> GetSnapshotAsync(string actorName)
		{
			using (var connection = await Connect())
			{
				var command = connection
					.CreateCommand(Dialect.SelectSnapshot(_snapshotsTable))
					.AddParameter("actor", DbType.String, actorName);

				using (var reader = await command.ExecuteReaderAsync())
				{
					var indexField = reader.IndexOf("index");
					var blobField = reader.IndexOf("data");

					if (!await reader.ReadAsync())
						return (null, -1L);

					var index = reader.GetInt64(indexField);
					var blob = reader.GetString(blobField);
					if (await reader.ReadAsync())
						throw new InvalidOperationException("More than one snapshot returned");

					return (Deserialize(blob), index);
				}
			}
		}

		/// <summary>Persists snapshot.</summary>
		/// <param name="actorName">Name of the actor.</param>
		/// <param name="index">The index.</param>
		/// <param name="snapshot">The snapshot.</param>
		/// <returns>Completed task when finished.</returns>
		public async Task PersistSnapshotAsync(string actorName, long index, object snapshot)
		{
			using (var connection = await Connect())
			{
				var command = connection
					.CreateCommand(Dialect.InsertSnapshot(_snapshotsTable))
					.AddParameter("actor", DbType.String, actorName)
					.AddParameter("index", DbType.Int64, index)
					.AddParameter("data", DbType.String, Serialize(snapshot));
				await command.ExecuteNonQueryAsync();
			}
		}

		/// <summary>Deletes the snapshots.</summary>
		/// <param name="actorName">Name of the actor.</param>
		/// <param name="inclusiveToIndex">Index of last deleted snapshot.</param>
		/// <returns>Completed task when finished.</returns>
		public async Task DeleteSnapshotsAsync(string actorName, long inclusiveToIndex)
		{
			using (var connection = await Connect())
			{
				var command = connection
					.CreateCommand(Dialect.DeleteSnapshots(_snapshotsTable))
					.AddParameter("actor", DbType.String, actorName)
					.AddParameter("index_end", DbType.Int64, inclusiveToIndex);
				await command.ExecuteNonQueryAsync();
			}
		}
	}
}
