using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proto.Persistence.MySql;
using Xunit;

namespace Proto.Persistence.PgSql.Tests
{
	
	public class DatabaseIntegrationTests
	{
		private static string ConnectionString =
			"host=x;Database=x;Username=x;Password=x";

		private readonly PgSqlProvider _sut;

		public DatabaseIntegrationTests()
		{
			_sut = new PgSqlProvider(
				ConnectionString,
				"test17", "test1",
				o => (string) o, s => s);
		}

		[Fact(Skip = "Needs database")]
		public async Task InsertSnapshots()
		{
			await _sut.DeleteSnapshotsAsync("a1", 1337);
			await _sut.PersistSnapshotAsync("a1", 1337, "object1");
			var (snapshot, index) = await _sut.GetSnapshotAsync("a1");
			Assert.Equal("object1", snapshot);
			Assert.Equal(1337, index);
		}

		[Fact(Skip = "Needs database")]
		public async Task InsertEvents()
		{
			await _sut.DeleteEventsAsync("a2", 1337);
			await _sut.PersistEventAsync("a2", 1337, "object2");
			await _sut.PersistEventAsync("a2", 1338, "object3");
			var list = new List<object>();
			await _sut.GetEventsAsync("a2", 0, long.MaxValue, list.Add);

			Assert.Equal(2, list.Count);
			Assert.Equal(new[] { "object2", "object3" }, list);
		}
	}
}
