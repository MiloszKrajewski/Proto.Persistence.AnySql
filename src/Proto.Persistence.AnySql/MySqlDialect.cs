namespace Proto.Persistence.AnySql
{
	/// <summary>
	/// Reference implementation of <see cref="IAnySqlDialect"/> for MySql.
	/// </summary>
	/// <seealso cref="IAnySqlDialect" />
	public class MySqlDialect: IAnySqlDialect
	{
		private static string Combine(string schemaName, string tableName) =>
			string.IsNullOrWhiteSpace(schemaName)
				? $"`{tableName}`"
				: $"`{schemaName}`.`{tableName}`";

		public string EventsTable(string schemaName, string tableName) =>
			Combine(schemaName, tableName + "_events");

		public string SnapshotsTable(string schemaName, string tableName) =>
			Combine(schemaName, tableName + "_snapshots");

		private static string CreateEither(string objectName)
		{
			return $@"
				create table if not exists {objectName} (
					`actor` varchar(255) not null,
					`index` bigint not null,
					`data` longtext,
					primary key (`actor`, `index`)
				)
			";
		}

		public string InsertEither(string objectName) =>
			$@"insert into {objectName} (`actor`, `index`, `data`) values (@actor, @index, @data)";

		public string DeleteEither(string objectName) =>
			$@"delete from {objectName} where `actor` = @actor and `index` <= @index_end";

		public string SelectEvents(string objectName)
		{
			return $@"
				select `index`, `data` 
				from {objectName}
				where `actor` = @actor and `index` between @index_start and @index_end
				order by `index` asc
			";
		}

		public string SelectSnapshot(string objectName)
		{
			return $@"
				select `index`, `data` 
				from {objectName}
				where `actor` = @actor
				order by `index` desc
				limit 1
			";
		}

		public string CreateEventsTable(string objectName) => CreateEither(objectName);

		public string CreateSnapshotsTable(string objectName) => CreateEither(objectName);

		public string InsertEvent(string objectName) => InsertEither(objectName);

		public string DeleteEvents(string objectName) => DeleteEither(objectName);

		public string InsertSnapshot(string objectName) => InsertEither(objectName);

		public string DeleteSnapshots(string objectName) => DeleteEither(objectName);
	}
}
