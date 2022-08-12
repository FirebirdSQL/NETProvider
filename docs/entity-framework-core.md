# Entity Framework Core 6.x

* Install `FirebirdSql.EntityFrameworkCore.Firebird` from NuGet.
* Create your `DbContext`.
* Call `UseFirebird` in `OnConfiguring`.
* Firebird 3 and up is supported.

### Code

```csharp
class Program
{
	static void Main(string[] args)
	{
		using (var db = new MyContext("database=localhost:demo.fdb;user=sysdba;password=masterkey"))
		{
			db.Demos.ToList();
		}
	}
}

class MyContext : DbContext
{
	static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

	readonly string _connectionString;

	public MyContext(string connectionString)
	{
		_connectionString = connectionString;
	}

	public DbSet<Demo> Demos { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);

		optionsBuilder
			.UseLoggerFactory(MyLoggerFactory)
			.UseFirebird(_connectionString);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		var demoConf = modelBuilder.Entity<Demo>();
		demoConf.Property(x => x.Id).HasColumnName("ID");
		demoConf.Property(x => x.FooBar).HasColumnName("FOOBAR");
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
