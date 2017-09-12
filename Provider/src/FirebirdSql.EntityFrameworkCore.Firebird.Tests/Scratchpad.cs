using System;
using System.Linq;
using System.Reflection;
using FirebirdSql.EntityFrameworkCore.Firebird.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnitLite;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests
{
	static class Program
	{
		static int Main()
		{
			return new AutoRun(Assembly.GetExecutingAssembly()).Execute(new[] { "--noresult" });
		}
	}

	[TestFixture]
	public class Scratchpad
	{
		[Test]
		public void Test()
		{
			using (var db = new TestDbContext())
			{
				var loggerFactory = db.GetService<ILoggerFactory>();
				loggerFactory.AddConsole();

				db.Set<MonAttachment>()
					.Select(x => new
					{
						Name = x.AttachmentName.Trim(),
						Test = x.Timestamp.Second,
					})
					.ToList();

				db.Database.ExecuteSqlCommand("recreate table test (id int primary key, name varchar(20))");
				db.Set<TestEntity>().Add(new TestEntity() { Id = 6, Name = "foobar" });
				db.SaveChanges();
			}
		}
	}

	class TestDbContext : DbContext
	{
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseFirebird(@"database=localhost:test.fdb;user=sysdba;password=masterkey");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			var monAttachmentConf = modelBuilder.Entity<MonAttachment>();
			monAttachmentConf.HasKey(x => x.AttachmentId);
			monAttachmentConf.Property(x => x.AttachmentId).HasColumnName("MON$ATTACHMENT_ID");
			monAttachmentConf.Property(x => x.AttachmentName).HasColumnName("MON$ATTACHMENT_NAME");
			monAttachmentConf.Property(x => x.Timestamp).HasColumnName("MON$TIMESTAMP");
			monAttachmentConf.ToTable("MON$ATTACHMENTS");

			var testEntityConf = modelBuilder.Entity<TestEntity>();
			testEntityConf.Property(x => x.Id).HasColumnName("ID");
			testEntityConf.Property(x => x.Name).HasColumnName("NAME");
			testEntityConf.ToTable("TEST");
		}
	}

	class MonAttachment
	{
		public int AttachmentId { get; set; }
		public string AttachmentName { get; set; }
		public DateTime Timestamp { get; set; }
	}

	class TestEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
