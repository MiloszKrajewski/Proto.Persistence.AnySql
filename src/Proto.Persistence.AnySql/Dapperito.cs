using System;
using System.Data;
using System.Data.Common;

namespace Proto.Persistence.AnySql
{
	/// <summary>
	/// Helper class replacing Dapper.
	/// </summary>
	internal static class Dapperito
	{
		public static DbCommand CreateCommand(this DbConnection connection, string sql)
		{
			var command = connection.CreateCommand();
			command.CommandText = sql;
			return command;
		}

		public static DbCommand CreateCommand(this DbTransaction transaction, string sql)
		{
			var connection = transaction.Connection;
			var command = connection.CreateCommand();
			command.CommandText = sql;
			command.Transaction = transaction;
			return command;
		}

		public static DbCommand AddParameter(
			this DbCommand command, string name, DbType type, object value)
		{
			var parameter = command.CreateParameter();
			parameter.ParameterName = name;
			parameter.DbType = type;
			parameter.Value = value;
			command.Parameters.Add(parameter);
			return command;
		}

		public static int IndexOf(this DbDataReader reader, string fieldName)
		{
			const StringComparison options = StringComparison.OrdinalIgnoreCase;

			for (var i = 0; i < reader.FieldCount; i++)
			{
				if (string.Compare(reader.GetName(i), fieldName, options) == 0)
					return i;
			}

			throw new ArgumentException($"Field '{fieldName}' could not be found");
		}
	}
}
