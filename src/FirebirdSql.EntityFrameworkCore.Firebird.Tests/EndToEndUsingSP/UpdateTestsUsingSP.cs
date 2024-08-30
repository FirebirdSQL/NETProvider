/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests.EndToEnd;

public class UpdateTestsUsingSP : EntityFrameworkCoreTestsBase
{
	class UpdateContext : FbTestDbContext
	{
		public UpdateContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnTestModelCreating(ModelBuilder modelBuilder)
		{
			base.OnTestModelCreating(modelBuilder);

			var insertEntityConf = modelBuilder.Entity<UpdateEntity>();
			insertEntityConf.Property(x => x.Id).HasColumnName("ID");
			insertEntityConf.Property(x => x.Foo).HasColumnName("FOO");
			insertEntityConf.Property(x => x.Bar).HasColumnName("BAR");
			insertEntityConf.ToTable("TEST_UPDATE_USP");
			modelBuilder.Entity<UpdateEntity>().UpdateUsingStoredProcedure("SP_TEST_UPDATE",
				storedProcedureBuilder =>
				{
					storedProcedureBuilder.HasOriginalValueParameter(x => x.Id);
					storedProcedureBuilder.HasParameter(x => x.Foo);
					storedProcedureBuilder.HasParameter(x => x.Bar);
					storedProcedureBuilder.HasRowsAffectedResultColumn();
				});
		}
	}
	class UpdateEntity
	{
		public int Id { get; set; }
		public string Foo { get; set; }
		public string Bar { get; set; }
	}
	[Test]
	public async Task Update()
	{
		await using (var db = await GetDbContext<UpdateContext>())
		{
			await db.Database.ExecuteSqlRawAsync("create table test_update_usp (id int primary key, foo varchar(20), bar varchar(20))");
			await db.Database.ExecuteSqlRawAsync("update or insert into test_update_usp values (66, 'foo', 'bar')");
			var sp = """
					 create procedure sp_test_update (
					     pid integer,
					     pfoo varchar(20),
					     pbar varchar(20))
					 returns (rowcount integer)
					 as
					 begin
					     update test_update_usp set foo = :pfoo, bar = :pbar
					     where id = :pid;
					     rowcount = row_count;
					     suspend;
					 end
					 """;
			await db.Database.ExecuteSqlRawAsync(sp);
			var entity = new UpdateEntity() { Id = 66, Foo = "test", Bar = "test" };
			var entry = db.Attach(entity);
			entry.Property(x => x.Foo).IsModified = true;
			await db.SaveChangesAsync();
			var value = await db.Set<UpdateEntity>()
				.FromSqlRaw("select * from test_update_usp where id = 66")
				.AsNoTracking()
				.FirstAsync();
			Assert.AreEqual("test", value.Foo);
			Assert.AreEqual("test", value.Bar);
		}
	}

	class ComputedUpdateContext : FbTestDbContext
	{
		public ComputedUpdateContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnTestModelCreating(ModelBuilder modelBuilder)
		{
			base.OnTestModelCreating(modelBuilder);

			var insertEntityConf = modelBuilder.Entity<ComputedUpdateEntity>();
			insertEntityConf.Property(x => x.Id).HasColumnName("ID");
			insertEntityConf.Property(x => x.Foo).HasColumnName("FOO");
			insertEntityConf.Property(x => x.Bar).HasColumnName("BAR");
			insertEntityConf.Property(x => x.Computed).HasColumnName("COMPUTED")
				.ValueGeneratedOnAddOrUpdate();
			insertEntityConf.ToTable("TEST_UPDATE_COMPUTED_USP");
			modelBuilder.Entity<ComputedUpdateEntity>().UpdateUsingStoredProcedure("SP_TEST_UPDATE_COMPUTED",
				storedProcedureBuilder =>
				{
					storedProcedureBuilder.HasOriginalValueParameter(x => x.Id);
					storedProcedureBuilder.HasParameter(x => x.Foo);
					storedProcedureBuilder.HasParameter(x => x.Bar);
					storedProcedureBuilder.HasResultColumn(x => x.Computed);
				});
		}
	}
	class ComputedUpdateEntity
	{
		public int Id { get; set; }
		public string Foo { get; set; }
		public string Bar { get; set; }
		public string Computed { get; set; }
	}
	[Test]
	public async Task ComputedUpdate()
	{
		await using (var db = await GetDbContext<ComputedUpdateContext>())
		{
			await db.Database.ExecuteSqlRawAsync("create table test_update_computed_usp (id int primary key, foo varchar(20), bar varchar(20), computed generated always as (foo || bar))");
			await db.Database.ExecuteSqlRawAsync("update or insert into test_update_computed_usp values (66, 'foo', 'bar')");
			var sp = """
					 create procedure sp_test_update_computed (
					     pid integer,
					     pfoo varchar(20),
					     pbar varchar(20))
					 returns (rcomputed varchar(50))
					 as
					 begin
					     update test_update_computed_usp set foo = :pfoo, bar = :pbar
					     where id = :pid
					     returning computed into :rcomputed;
					     if (row_count > 0) then
					        suspend;
					 end
					 """;
			await db.Database.ExecuteSqlRawAsync(sp);
			var entity = new ComputedUpdateEntity() { Id = 66, Foo = "test", Bar = "bar" };
			var entry = db.Attach(entity);
			entry.Property(x => x.Foo).IsModified = true;
			await db.SaveChangesAsync();
			Assert.AreEqual("testbar", entity.Computed);
		}
	}

	class ConcurrencyUpdateContext : FbTestDbContext
	{
		public ConcurrencyUpdateContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnTestModelCreating(ModelBuilder modelBuilder)
		{
			base.OnTestModelCreating(modelBuilder);

			var insertEntityConf = modelBuilder.Entity<ConcurrencyUpdateEntity>();
			insertEntityConf.Property(x => x.Id).HasColumnName("ID");
			insertEntityConf.Property(x => x.Foo).HasColumnName("FOO");
			insertEntityConf.Property(x => x.Stamp).HasColumnName("STAMP")
				.IsRowVersion();
			insertEntityConf.ToTable("TEST_UPDATE_CONCURRENCY_USP");
			modelBuilder.Entity<ConcurrencyUpdateEntity>().UpdateUsingStoredProcedure("SP_TEST_UPDATE_CONCURRENCY",
				storedProcedureBuilder =>
				{
					storedProcedureBuilder.HasOriginalValueParameter(x => x.Id);
					storedProcedureBuilder.HasParameter(x => x.Foo);
					storedProcedureBuilder.HasOriginalValueParameter(x => x.Stamp);
					storedProcedureBuilder.HasResultColumn(x => x.Stamp);
				});
		}
	}
	class ConcurrencyUpdateEntity
	{
		public int Id { get; set; }
		public string Foo { get; set; }
		public DateTime Stamp { get; set; }
	}
	[Test]
	public async Task ConcurrencyUpdate()
	{
		await using (var db = await GetDbContext<ConcurrencyUpdateContext>())
		{
			await db.Database.ExecuteSqlRawAsync("create table test_update_concurrency_usp (id int primary key, foo varchar(20), stamp timestamp)");
			await db.Database.ExecuteSqlRawAsync("update or insert into test_update_concurrency_usp values (66, 'foo', current_timestamp)");
			var sp = """
					 create procedure sp_test_update_concurrency (
					     pid integer,
					     pfoo varchar(20),
					     pstamp timestamp)
					 returns (rstamp timestamp)
					 as
					 begin
					     update test_update_concurrency_usp set foo = :pfoo, stamp = current_timestamp
					     where id = :pid and stamp = :pstamp
					     returning stamp into :rstamp;
					     if (row_count > 0) then
					         suspend;
					 end
					 """;
			await db.Database.ExecuteSqlRawAsync(sp);
			var entity = new ConcurrencyUpdateEntity() { Id = 66, Foo = "test", Stamp = new DateTime(1970, 1, 1) };
			var entry = db.Attach(entity);
			entry.Property(x => x.Foo).IsModified = true;
			Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => db.SaveChangesAsync());
		}
	}

	class ConcurrencyUpdateNoGeneratedContext : FbTestDbContext
	{
		public ConcurrencyUpdateNoGeneratedContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnTestModelCreating(ModelBuilder modelBuilder)
		{
			base.OnTestModelCreating(modelBuilder);

			var insertEntityConf = modelBuilder.Entity<ConcurrencyUpdateNoGeneratedEntity>();
			insertEntityConf.Property(x => x.Id).HasColumnName("ID");
			insertEntityConf.Property(x => x.Foo).HasColumnName("FOO");
			insertEntityConf.Property(x => x.Stamp).HasColumnName("STAMP")
				.IsConcurrencyToken();
			insertEntityConf.ToTable("TEST_UPDATE_CONCURRENCY_NG_USP");
			modelBuilder.Entity<ConcurrencyUpdateNoGeneratedEntity>().UpdateUsingStoredProcedure("SP_TEST_UPDATE_CONCURRENCY_NG",
				storedProcedureBuilder =>
				{
					storedProcedureBuilder.HasOriginalValueParameter(x => x.Id);
					storedProcedureBuilder.HasParameter(x => x.Foo);
					storedProcedureBuilder.HasOriginalValueParameter(x => x.Stamp);
					storedProcedureBuilder.HasRowsAffectedResultColumn();
				});
		}
	}
	class ConcurrencyUpdateNoGeneratedEntity
	{
		public int Id { get; set; }
		public string Foo { get; set; }
		public DateTime Stamp { get; set; }
	}
	[Test]
	public async Task ConcurrencyUpdateNoGenerated()
	{
		await using (var db = await GetDbContext<ConcurrencyUpdateNoGeneratedContext>())
		{
			await db.Database.ExecuteSqlRawAsync("create table test_update_concurrency_ng_usp (id int primary key, foo varchar(20), stamp timestamp)");
			await db.Database.ExecuteSqlRawAsync("update or insert into test_update_concurrency_ng_usp values (66, 'foo', current_timestamp)");
			var sp = """
					 create procedure sp_test_update_concurrency_ng (
					     pid integer,
					     pfoo varchar(20),
					     pstamp timestamp)
					 returns (rowcount integer)
					 as
					 begin
					     update test_update_concurrency_ng_usp set foo = :pfoo, stamp = current_timestamp
					     where id = :pid and stamp = :pstamp;
					     rowcount = row_count;
					     if (rowcount > 0) then
					         suspend;
					 end
					 """;
			await db.Database.ExecuteSqlRawAsync(sp);
			var entity = new ConcurrencyUpdateNoGeneratedEntity() { Id = 66, Foo = "test", Stamp = new DateTime(1970, 1, 1) };
			var entry = db.Attach(entity);
			entry.Property(x => x.Foo).IsModified = true;
			entry.Property(x => x.Stamp).IsModified = true;
			Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => db.SaveChangesAsync());
		}
	}

	class TwoComputedUpdateContext : FbTestDbContext
	{
		public TwoComputedUpdateContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnTestModelCreating(ModelBuilder modelBuilder)
		{
			base.OnTestModelCreating(modelBuilder);

			var insertEntityConf = modelBuilder.Entity<TwoComputedUpdateEntity>();
			insertEntityConf.Property(x => x.Id).HasColumnName("ID");
			insertEntityConf.Property(x => x.Foo).HasColumnName("FOO");
			insertEntityConf.Property(x => x.Bar).HasColumnName("BAR");
			insertEntityConf.Property(x => x.Computed1).HasColumnName("COMPUTED1")
				.ValueGeneratedOnAddOrUpdate();
			insertEntityConf.Property(x => x.Computed2).HasColumnName("COMPUTED2")
				.ValueGeneratedOnAddOrUpdate();
			insertEntityConf.ToTable("TEST_UPDATE_2COMPUTED_USP");
			modelBuilder.Entity<TwoComputedUpdateEntity>().UpdateUsingStoredProcedure("SP_TEST_UPDATE_2COMPUTED",
				storedProcedureBuilder =>
				{
					storedProcedureBuilder.HasOriginalValueParameter(x => x.Id);
					storedProcedureBuilder.HasParameter(x => x.Foo);
					storedProcedureBuilder.HasParameter(x => x.Bar);
					storedProcedureBuilder.HasResultColumn(x => x.Computed1);
					storedProcedureBuilder.HasResultColumn(x => x.Computed2);
				});
		}
	}
	class TwoComputedUpdateEntity
	{
		public int Id { get; set; }
		public string Foo { get; set; }
		public string Bar { get; set; }
		public string Computed1 { get; set; }
		public string Computed2 { get; set; }
	}
	[Test]
	public async Task TwoComputedUpdate()
	{
		await using (var db = await GetDbContext<TwoComputedUpdateContext>())
		{
			await db.Database.ExecuteSqlRawAsync("create table test_update_2computed_usp (id int primary key, foo varchar(20), bar varchar(20), computed1 generated always as (foo || bar), computed2 generated always as (bar || bar))");
			await db.Database.ExecuteSqlRawAsync("update or insert into test_update_2computed_usp values (66, 'foo', 'bar')");
			var sp = """
					 create procedure sp_test_update_2computed (
					     pid integer,
					     pfoo varchar(20),
					     pbar varchar(20))
					 returns (
					     rcomputed1 varchar(50),
					     rcomputed2 varchar(50))
					 as
					 begin
					     update test_update_2computed_usp set foo = :pfoo, bar = :pbar
					     where id = :pid
					     returning computed1, computed2 into :rcomputed1, :rcomputed2;
					     if (row_count > 0) then
					         suspend;
					 end
					 """;
			await db.Database.ExecuteSqlRawAsync(sp);
			var entity = new TwoComputedUpdateEntity() { Id = 66, Foo = "test", Bar = "bar" };
			var entry = db.Attach(entity);
			entry.Property(x => x.Foo).IsModified = true;
			await db.SaveChangesAsync();
			Assert.AreEqual("testbar", entity.Computed1);
			Assert.AreEqual("barbar", entity.Computed2);
		}
	}
}
