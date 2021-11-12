# ADO.NET

### Steps

* Install `FirebirdSql.Data.FirebirdClient` from NuGet.
* Add `using FirebirdSql.Data.FirebirdClient;`.
* Basic classes are `FbConnection`, `FbTransaction`, `FbCommand` and `FbDataReader`.
* Connection string can be built using `FbConnectionStringBuilder`.

### Code

```csharp
using (var connection = new FbConnection("database=localhost:demo.fdb;user=sysdba;password=masterkey"))
{
	connection.Open();
	using (var transaction = connection.BeginTransaction())
	{
		using (var command = new FbCommand("select * from demo", connection, transaction))
		{
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var values = new object[reader.FieldCount];
					reader.GetValues(values);
					Console.WriteLine(string.Join("|", values));
				}
			}
		}
	}
}
```

### Scripts

```sql
create table demo (id int primary key, foobar varchar(20) character set utf8);
```

```sql
insert into demo values (6, 'FooBar');
```
