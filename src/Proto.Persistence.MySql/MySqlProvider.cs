using System;
using System.Data.Common;
using MySql.Data.MySqlClient;
using Proto.Persistence.AnySql;

namespace Proto.Persistence.MySql
{
	/// <summary>
	/// Proto.Actor persistence provider for MySql.
	/// </summary>
	/// <seealso cref="AnySqlProvider" />
	public class MySqlProvider: AnySqlProvider
	{
		public MySqlProvider(
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
				new MySqlDialect()) { }

		private static DbConnection Connect(string connectionString) =>
			new MySqlConnection(connectionString);
	}
}
