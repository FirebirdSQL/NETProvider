﻿/*
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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests.EndToEnd;

public class InsertTestsUsingSP : EntityFrameworkCoreTestsBase
{
	class InsertContext : FbTestDbContext
	{
		public InsertContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnTestModelCreating(ModelBuilder modelBuilder)
		{
			base.OnTestModelCreating(modelBuilder);

			var insertEntityConf = modelBuilder.Entity<InsertEntity>();
			insertEntityConf.Property(x => x.Id).HasColumnName("ID");
			insertEntityConf.Property(x => x.Name).HasColumnName("NAME");
			insertEntityConf.ToTable("TEST_INSERT_USP");
			modelBuilder.Entity<InsertEntity>().InsertUsingStoredProcedure("SP_TEST_INSERT",
				storedProcedureBuilder =>
				{
					storedProcedureBuilder.HasParameter(x => x.Id);
					storedProcedureBuilder.HasParameter(x => x.Name);
					storedProcedureBuilder.HasResultColumn(x => x.Id);
				});
		}
	}
	class InsertEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
	[Test]
	public async Task Insert()
	{
		await using (var db = await GetDbContext<InsertContext>())
		{
			await db.Database.ExecuteSqlRawAsync("create table test_insert_usp (id int primary key, name varchar(20))");
			var sp = """
					 create procedure sp_test_insert (
					     pid integer,
					     pname varchar(20))
					 returns (id integer)
					 as
					 begin
					     insert into test_insert_usp (id, name)
					     values (:pid, :pname)
					     returning id into :id;
					     suspend;
					 end
					 """;
			await db.Database.ExecuteSqlRawAsync(sp);
			var entity = new InsertEntity() { Id = -6, Name = "foobar" };
			await db.AddAsync(entity);
			await db.SaveChangesAsync();
			Assert.AreEqual(-6, entity.Id);
		}
	}

	class InsertContextWithoutReturns : FbTestDbContext
	{
		public InsertContextWithoutReturns(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnTestModelCreating(ModelBuilder modelBuilder)
		{
			base.OnTestModelCreating(modelBuilder);

			var insertEntityConf = modelBuilder.Entity<InsertEntity>();
			insertEntityConf.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
			insertEntityConf.Property(x => x.Name).HasColumnName("NAME");
			insertEntityConf.ToTable("TEST_INSERT_NORETURNS_USP");
			modelBuilder.Entity<InsertEntity>().InsertUsingStoredProcedure("SP_TEST_INSERT_NORETURNS",
				storedProcedureBuilder =>
				{
					storedProcedureBuilder.HasParameter(x => x.Id);
					storedProcedureBuilder.HasParameter(x => x.Name);
				});
		}
	}

	[Test]
	public async Task InsertWithoutReturns()
	{
		await using (var db = await GetDbContext<InsertContextWithoutReturns>())
		{
			await db.Database.ExecuteSqlRawAsync("create table test_insert_noreturns_usp (id int primary key, name varchar(20))");
			var sp = """
					 create procedure sp_test_insert_noreturns (
					     pid integer,
					     pname varchar(20))
					 as
					 begin
					     insert into test_insert_noreturns_usp (id, name)
					     values (:pid, :pname);
					 end
					 """;
			await db.Database.ExecuteSqlRawAsync(sp);
			var entity = new InsertEntity() { Id = -6, Name = "foobar" };
			await db.AddAsync(entity);
			await db.SaveChangesAsync();
			Assert.AreEqual(-6, entity.Id);
		}
	}

	class IdentityInsertContext : FbTestDbContext
	{
		public IdentityInsertContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnTestModelCreating(ModelBuilder modelBuilder)
		{
			base.OnTestModelCreating(modelBuilder);

			var insertEntityConf = modelBuilder.Entity<IdentityInsertEntity>();
			insertEntityConf.Property(x => x.Id).HasColumnName("ID")
				.UseIdentityColumn();
			insertEntityConf.Property(x => x.Name).HasColumnName("NAME");
			insertEntityConf.ToTable("TEST_INSERT_IDENTITY_USP");
			modelBuilder.Entity<IdentityInsertEntity>().InsertUsingStoredProcedure("SP_TEST_INSERT_IDENTITY",
				storedProcedureBuilder =>
				{
					storedProcedureBuilder.HasParameter(x => x.Name);
					storedProcedureBuilder.HasResultColumn(x => x.Id);
				});
		}
	}
	class IdentityInsertEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
	[Test]
	public async Task IdentityInsert()
	{
		if (!EnsureServerVersionAtLeast(new Version(3, 0, 0, 0)))
			return;

		var id = ServerVersion >= new Version(4, 0, 0, 0) ? 26 : 27;

		await using (var db = await GetDbContext<IdentityInsertContext>())
		{
			await db.Database.ExecuteSqlRawAsync("create table test_insert_identity_usp (id int generated by default as identity (start with 26) primary key, name varchar(20))");
			var sp = """
					 create procedure sp_test_insert_identity (
					     pname varchar(20))
					 returns (id integer)
					 as
					 begin
					     insert into test_insert_identity_usp (name)
					     values (:pname)
					     returning id into :id;
					     suspend;
					 end
					 """;
			await db.Database.ExecuteSqlRawAsync(sp);
			var entity = new IdentityInsertEntity() { Name = "foobar" };
			await db.AddAsync(entity);
			await db.SaveChangesAsync();
			Assert.AreEqual(id, entity.Id);
		}
	}

	class SequenceInsertContext : FbTestDbContext
	{
		public SequenceInsertContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnTestModelCreating(ModelBuilder modelBuilder)
		{
			base.OnTestModelCreating(modelBuilder);

			var insertEntityConf = modelBuilder.Entity<SequenceInsertEntity>();
			insertEntityConf.Property(x => x.Id).HasColumnName("ID")
				.UseSequenceTrigger();
			insertEntityConf.Property(x => x.Name).HasColumnName("NAME");
			insertEntityConf.ToTable("TEST_INSERT_SEQUENCE_USP");
			modelBuilder.Entity<SequenceInsertEntity>().InsertUsingStoredProcedure("SP_TEST_INSERT_SEQUENCE",
				storedProcedureBuilder =>
				{
					storedProcedureBuilder.HasParameter(x => x.Name);
					storedProcedureBuilder.HasResultColumn(x => x.Id);
				});
		}
	}
	class SequenceInsertEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
	[Test]
	public async Task SequenceInsert()
	{
		var id = ServerVersion >= new Version(4, 0, 0, 0) ? 30 : 31;

		await using (var db = await GetDbContext<SequenceInsertContext>())
		{
			await db.Database.ExecuteSqlRawAsync("create table test_insert_sequence_usp (id int primary key, name varchar(20))");
			await db.Database.ExecuteSqlRawAsync("create sequence seq_test_insert_sequence_usp");
			await db.Database.ExecuteSqlRawAsync("alter sequence seq_test_insert_sequence_usp restart with 30");
			await db.Database.ExecuteSqlRawAsync("create trigger test_insert_sequence_id_usp before insert on test_insert_sequence_usp as begin if (new.id is null) then begin new.id = next value for seq_test_insert_sequence_usp; end end");
			var sp = """
					 create procedure sp_test_insert_sequence (
					     pname varchar(20))
					 returns (id integer)
					 as
					 begin
					     insert into test_insert_sequence_usp (name)
					     values (:pname)
					     returning id into :id;
					     suspend;
					 end
					 """;
			await db.Database.ExecuteSqlRawAsync(sp);
			var entity = new SequenceInsertEntity() { Name = "foobar" };
			await db.AddAsync(entity);
			await db.SaveChangesAsync();
			Assert.AreEqual(id, entity.Id);
		}
	}

	class DefaultValuesInsertContext : FbTestDbContext
	{
		public DefaultValuesInsertContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnTestModelCreating(ModelBuilder modelBuilder)
		{
			base.OnTestModelCreating(modelBuilder);

			var insertEntityConf = modelBuilder.Entity<DefaultValuesInsertEntity>();
			insertEntityConf.Property(x => x.Id).HasColumnName("ID")
				.ValueGeneratedOnAdd();
			insertEntityConf.Property(x => x.Name).HasColumnName("NAME")
				.ValueGeneratedOnAdd();
			insertEntityConf.ToTable("TEST_INSERT_DEVAULTVALUES_USP");
			modelBuilder.Entity<DefaultValuesInsertEntity>().InsertUsingStoredProcedure("SP_TEST_INSERT_DEFAULTVALUES",
				storedProcedureBuilder =>
				{
					storedProcedureBuilder.HasResultColumn(x => x.Id);
					storedProcedureBuilder.HasResultColumn(x => x.Name);
				});
		}
	}
	class DefaultValuesInsertEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
	[Test]
	public async Task DefaultValuesInsert()
	{
		if (!EnsureServerVersionAtLeast(new Version(3, 0, 0, 0)))
			return;

		var id = ServerVersion >= new Version(4, 0, 0, 0) ? 26 : 27;

		await using (var db = await GetDbContext<DefaultValuesInsertContext>())
		{
			await db.Database.ExecuteSqlRawAsync("create table test_insert_devaultvalues_usp (id int generated by default as identity (start with 26) primary key, name generated always as (id || 'foobar'))");
			var sp = """
					 create procedure sp_test_insert_defaultvalues
					 returns (id integer, name varchar(20))
					 as
					 begin
					     insert into test_insert_devaultvalues_usp default values
					     returning id, name
					     into :id, :name;
					     suspend;
					 end
					 """;
			await db.Database.ExecuteSqlRawAsync(sp);
			var entity = new DefaultValuesInsertEntity() { };
			await db.AddAsync(entity);
			await db.SaveChangesAsync();
			Assert.AreEqual(id, entity.Id);
			Assert.AreEqual($"{id}foobar", entity.Name);
		}
	}

	class TwoComputedInsertContext : FbTestDbContext
	{
		public TwoComputedInsertContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnTestModelCreating(ModelBuilder modelBuilder)
		{
			base.OnTestModelCreating(modelBuilder);

			var insertEntityConf = modelBuilder.Entity<TwoComputedInsertEntity>();
			insertEntityConf.Property(x => x.Id).HasColumnName("ID")
				.UseIdentityColumn();
			insertEntityConf.Property(x => x.Name).HasColumnName("NAME");
			insertEntityConf.Property(x => x.Computed1).HasColumnName("COMPUTED1")
				.ValueGeneratedOnAddOrUpdate();
			insertEntityConf.Property(x => x.Computed2).HasColumnName("COMPUTED2")
				.ValueGeneratedOnAddOrUpdate();
			insertEntityConf.ToTable("TEST_INSERT_2COMPUTED_USP");
			modelBuilder.Entity<TwoComputedInsertEntity>().InsertUsingStoredProcedure("SP_TEST_INSERT_2COMPUTED",
				storedProcedureBuilder =>
				{
					storedProcedureBuilder.HasParameter(x => x.Name);
					storedProcedureBuilder.HasResultColumn(x => x.Id);
					storedProcedureBuilder.HasResultColumn(x => x.Computed1);
					storedProcedureBuilder.HasResultColumn(x => x.Computed2);
				});
		}
	}
	class TwoComputedInsertEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Computed1 { get; set; }
		public string Computed2 { get; set; }
	}
	[Test]
	public async Task TwoComputedInsert()
	{
		if (!EnsureServerVersionAtLeast(new Version(3, 0, 0, 0)))
			return;

		await using (var db = await GetDbContext<TwoComputedInsertContext>())
		{
			await db.Database.ExecuteSqlRawAsync("create table test_insert_2computed_usp (id int generated by default as identity (start with 26) primary key, name varchar(20), computed1 generated always as ('1' || name), computed2 generated always as ('2' || name))");
			var sp = """
					 create procedure sp_test_insert_2computed (
					     pname varchar(20))
					 returns (id integer, computed1 varchar(25), computed2 varchar(25))
					 as
					 begin
					     insert into test_insert_2computed_usp (name)
					     values (:pname)
					     returning id, computed1, computed2
					     into :id, :computed1, :computed2;
					     suspend;
					 end
					 """;
			await db.Database.ExecuteSqlRawAsync(sp);
			var entity = new TwoComputedInsertEntity() { Name = "foobar" };
			await db.AddAsync(entity);
			await db.SaveChangesAsync();
			Assert.AreEqual("1foobar", entity.Computed1);
			Assert.AreEqual("2foobar", entity.Computed2);
		}
	}
}
