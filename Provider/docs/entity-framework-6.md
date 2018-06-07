# Entity Framework 6

### Steps

* Install `EntityFramework.Firebird` from NuGet.
* Add `DbProviderFactories` record.
```xml
	<system.data>
		<DbProviderFactories>
			<remove invariant="FirebirdSql.Data.FirebirdClient" />
			<add name="FirebirdClient" description="FirebirdClient" invariant="FirebirdSql.Data.FirebirdClient" type="FirebirdSql.Data.FirebirdClient.FirebirdClientFactory, FirebirdSql.Data.FirebirdClient" />
		</DbProviderFactories>
	</system.data>
```
* Add/modify `entityFramework` configuration section.
```xml
	<configSections>
		<section name="entityFramework" type="System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false" />
	</configSections>
	<entityFramework>
		<defaultConnectionFactory type="EntityFramework.Firebird.FbConnectionFactory, EntityFramework.Firebird" />
		<providers>
			<provider invariantName="FirebirdSql.Data.FirebirdClient" type="EntityFramework.Firebird.FbProviderServices, EntityFramework.Firebird" />
			<provider invariantName="System.Data.SqlClient" type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer" />
		</providers>
	</entityFramework>
```
* Create your `DbContext`.

### Code

```csharp
class Program
{
	static void Main(string[] args)
	{
		using (var db = new MyContext("database=localhost:demo.fdb;user=sysdba;password=masterkey"))
		{
			db.Database.Log = Console.WriteLine;

			db.Demos.ToList();
		}
	}
}

class MyContext : DbContext
{
	public MyContext(string connectionString)
		: base(new FbConnection(connectionString), true)
	{ }

	public DbSet<Demo> Demos { get; set; }

	protected override void OnModelCreating(DbModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Properties()
			.Configure(x => x.HasColumnName(x.ClrPropertyInfo.Name.ToUpper()));

		var demoConf = modelBuilder.Entity<Demo>();
		demoConf.ToTable("DEMO");
	}
}

class Demo
{
	public int Id { get; set; }
	public string FooBar { get; set; }
}
``` 

### Scripts

```sql
create table demo (id int primary key, foobar varchar(20) character set utf8);
```

```sql
insert into demo values (6, 'FooBar');
```
