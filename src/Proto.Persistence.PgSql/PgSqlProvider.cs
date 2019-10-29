using System;
using System.Data.Common;
using Npgsql;
using Proto.Persistence.AnySql;

namespace Proto.Persistence.MySql
{
	/// <summary>
	/// Proto.Actor persistence provider for MySql.
	/// </summary>
	/// <seealso cref="AnySqlProvider" />
	public class PgSqlProvider: AnySqlProvider
	{
		public PgSqlProvider(
			string connectionString,
			string schema, string table,
			Func<object, string> serialize,
			Func<string, object> deserialize)
			: base(
				() => Connect(connectionString),
				schema,
				table,
				serialize,
				deserialize,
				new PgSqlDialect()) { }

		private static DbConnection Connect(string connectionString) =>
			new NpgsqlConnection(connectionString);
	}
}
