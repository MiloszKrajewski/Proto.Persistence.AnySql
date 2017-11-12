# Proto.Persistence.AnySql

![NuGet Stats](https://img.shields.io/nuget/v/Proto.Persistence.AnySql.svg)

Persistence provider for [Proto.Actor](http://proto.actor/) (potentially) handling any SQL database.

AnySqlProvider has two holes to fill in: **connection factory** and **sql dialect**:

```csharp
IProvider provider = new AnySqlProvider(
    () => new SomeDbConnecion(...),  // <-- here
    "schema", "table",
    obj => Serialize(obj),
    str => Deserialize(str),
    new SomeSqlDialect()); // <-- here
```

Both of them need to implemented for specific database.

# Proto.Persistence.MySql

![NuGet Stats](https://img.shields.io/nuget/v/Proto.Persistence.MySql.svg)

Currently only MySQL dialect is included and, to be honest, it might be the only one, as all others are already implemented (PostgreSQL, Microsoft SQL Server, SQLite). The only one missing is Oracle but I wouldn't be even able to test it as I don't have any Oracle databases around me.

# Usage

```csharp
IProvider provider = new MySqlProvider(
    "Server=...;Database=...;Uid=...;Pwd=...;Allow User Variables=true",
    "schema", "table", obj => Serialize(obj), str => Deserialize(str));
```

It is important to have `Allow User Variables=true` is connection string.

It is also worth noting, that provider itself does not tie you up to any specific serializer as long as it serializes to string.
If your serialized serializes to `byte[]`, well, `Convert.ToBase64String(...)` is your friend.

Easiest to set up is `Netwtonsoft.Json`:

```csharp
private static readonly JsonSerializerSettings JsonSettings = 
    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

public static string Serialize(object o) =>
    JsonConvert.SerializeObject(o, typeof(object), JsonSettings);

public static object Deserialize(string s) =>
    JsonConvert.DeserializeObject(s, JsonSettings);
```

Please note `{ TypeNameHandling = TypeNameHandling.All }`. This is important to allow polymorphic serialization. If you know what you are doing it is possible to configure it to use `TypeNameHandling.Auto`, but it requires a little bit of knowledge how `Newtonsoft.Json` works.

Why `string` not `byte[]`?
I guess this project is still in *debug* mode and JSON will me most frequently used serialization mechanism, so storing it as `string` will make it human readable and easier to handle with third-party tools. 

# Implementing your own dialect (let's say Oracle)

To implement a dialect you need to provide those 5 SQL queries translated to your flavour of SQL:

* Creating tables

```sql
create table if not exists <events_or_snapshots> (
    `actor` varchar(255) not null,
    `index` bigint not null,
    `data` longtext,
    primary key (`actor`, `index`)
)
```

* Inserting events or snapshots

```sql
insert into <events_or_snapshots> (`actor`, `index`, `data`) values (@actor, @index, @data)
```

* Deleting events or snapshots

```sql
delete from <events_or_snapshots> where `actor` = @actor and `index` <= @index_end
```

* Retrieving events

```sql
select `index`, `data`
from <events>
where `actor` = @actor and `index` between @index_start and @index_end
order by `index` asc
```

* Retrieving (most recent) snapshot

```sql
select `index`, `data`
from <snapshots>
where `actor` = @actor
order by `index` desc
limit 1
```

Please note `... order by index desc limit 1` in last query. This is possibly the only catch here.

See [`MySqlDialect`](src/Proto.Persistence.AnySql/MySqlDialect.cs) for reference.

# Build

```shell
fake build
```
