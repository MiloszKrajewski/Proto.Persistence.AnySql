namespace Proto.Persistence.AnySql
{
	/// <summary>
	/// SQL dialect. Requires specific implementation per database vendor.
	/// Unfortunately, SQL has lot of flavours so every database is slightly different.
	/// Please note, that SQL statements provided by this dialect object are 
	/// tightly coupled with AnySqlProvider, and require field and parameter names to be
	/// very specific. Please take a look at the reference implementation in
	/// <see cref="MySqlDialect"/>. It's almost impossible to get it right without it.
	/// </summary>
	public interface IAnySqlDialect
	{
		/// <summary>Name of events table.</summary>
		/// <param name="schemaName">Schema (may be <c>null</c>).</param>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>Name of events table.</returns>
		string EventsTable(string schemaName, string tableName);

		/// <summary>Name of snapshots table.</summary>
		/// <param name="schemaName">Schema (may be <c>null</c>).</param>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>Name of snapshots table.</returns>
		string SnapshotsTable(string schemaName, string tableName);

		/// <summary>SQL to create the events table.</summary>
		/// <param name="objectName">Name of the object.</param>
		/// <returns>SQL which will create events table.</returns>
		string CreateEventsTable(string objectName);

		/// <summary>SQL to create the snapshots table.</summary>
		/// <param name="objectName">Name of the object.</param>
		/// <returns>SQL which will create snapshots table.</returns>
		string CreateSnapshotsTable(string objectName);

		/// <summary>SQL to select events.</summary>
		/// <param name="objectName">Name of the object.</param>
		/// <returns>SQL to select events between <c>index_start</c> and <c>index_end</c>.</returns>
		string SelectEvents(string objectName);

		/// <summary>SQL to insert single event.</summary>
		/// <param name="objectName">Name of the object.</param>
		/// <returns>SQL to insert single event.</returns>
		string InsertEvent(string objectName);

		/// <summary>SQL to delete events.</summary>
		/// <param name="objectName">Name of the object.</param>
		/// <returns>SQL to select events earlier than <c>index_end</c>.</returns>
		string DeleteEvents(string objectName);

		/// <summary>SQL to select snapshot.</summary>
		/// <param name="objectName">Name of the object.</param>
		/// <returns>SQL to select last snapshot.</returns>
		string SelectSnapshot(string objectName);

		/// <summary>SQL to insert snapshot.</summary>
		/// <param name="objectName">Name of the object.</param>
		/// <returns>SQL to insert snapshot.</returns>
		string InsertSnapshot(string objectName);

		/// <summary>SQL to delete snapshot.</summary>
		/// <param name="objectName">Name of the object.</param>
		/// <returns>SQL to delete snapshot.</returns>
		string DeleteSnapshots(string objectName);
	}
}
