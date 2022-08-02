# Entity Framework 6

### Steps

* Install `EntityFramework.Firebird` from NuGet.
* Create `DbProviderFactories` record (see below).
* Create configuration (see below).
* Create your `DbContext`.
* Firebird 2.5 and up is supported.

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

### DbProviderFactories

.NET Framework:
```xml
<system.data>
	<DbProviderFactories>
		<remove invariant="FirebirdSql.Data.FirebirdClient" />
		<add name="FirebirdClient" description="FirebirdClient" invariant="FirebirdSql.Data.FirebirdClient" type="FirebirdSql.Data.FirebirdClient.FirebirdClientFactory, FirebirdSql.Data.FirebirdClient" />
	</DbProviderFactories>
</system.data>
```

.NET Core/.NET 5+:
```csharp
System.Data.Common.DbProviderFactories.RegisterFactory(FbProviderServices.ProviderInvariantName, FirebirdClientFactory.Instance);
```

### Configuration

.NET Framework:
```xml
<configSections>
	<section name="entityFramework" type="System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false" />
</configSections>
<entityFramework>
	<defaultConnectionFactory type="EntityFramework.Firebird.FbConnectionFactory, EntityFramework.Firebird" />
	<providers>
		<provider invariantName="FirebirdSql.Data.FirebirdClient" type="EntityFramework.Firebird.FbProviderServices, EntityFramework.Firebird" />
	</providers>
</entityFramework>
```

.NET Core/.NET 5+:
```csharp
public class Conf : DbConfiguration
{
	public Conf()
	{
		SetProviderServices(FbProviderServices.ProviderInvariantName, FbProviderServices.Instance);
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
