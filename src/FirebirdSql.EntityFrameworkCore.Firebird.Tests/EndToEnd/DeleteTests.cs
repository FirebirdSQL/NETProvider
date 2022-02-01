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

public class DeleteTests : EntityFrameworkCoreTestsBase
{
	class DeleteContext : FbTestDbContext
	{
		public DeleteContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnTestModelCreating(ModelBuilder modelBuilder)
		{
			base.OnTestModelCreating(modelBuilder);

			var insertEntityConf = modelBuilder.Entity<DeleteEntity>();
			insertEntityConf.Property(x => x.Id).HasColumnName("ID");
			insertEntityConf.Property(x => x.Name).HasColumnName("NAME");
			insertEntityConf.ToTable("TEST_DELETE");
		}
	}
	class DeleteEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
	[Test]
	public async Task Delete()
	{
		await using (var db = await GetDbContext<DeleteContext>())
		{
			await db.Database.ExecuteSqlRawAsync("create table test_delete (id int primary key, name varchar(20))");
			await db.Database.ExecuteSqlRawAsync("insert into test_delete values (65, 'test')");
			await db.Database.ExecuteSqlRawAsync("insert into test_delete values (66, 'test')");
			await db.Database.ExecuteSqlRawAsync("insert into test_delete values (67, 'test')");
			var entity = new DeleteEntity() { Id = 66 };
			var entry = db.Attach(entity);
			entry.State = EntityState.Deleted;
			await db.SaveChangesAsync();
			var values = await db.Set<DeleteEntity>()
				 .FromSqlRaw("select * from test_delete")
				 .AsNoTracking()
				 .OrderBy(x => x.Id)
				 .ToListAsync();
			Assert.AreEqual(2, values.Count());
			Assert.AreEqual(65, values[0].Id);
			Assert.AreEqual(67, values[1].Id);
		}
	}

	class ConcurrencyDeleteContext : FbTestDbContext
	{
		public ConcurrencyDeleteContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnTestModelCreating(ModelBuilder modelBuilder)
		{
			base.OnTestModelCreating(modelBuilder);

			var insertEntityConf = modelBuilder.Entity<ConcurrencyDeleteEntity>();
			insertEntityConf.Property(x => x.Id).HasColumnName("ID");
			insertEntityConf.Property(x => x.Name).HasColumnName("NAME");
			insertEntityConf.Property(x => x.Stamp).HasColumnName("STAMP")
				.ValueGeneratedOnAddOrUpdate()
				.IsConcurrencyToken();
			insertEntityConf.ToTable("TEST_DELETE_CONCURRENCY");
		}
	}
	class ConcurrencyDeleteEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime Stamp { get; set; }
	}
	[Test]
	public async Task ConcurrencyDelete()
	{
		await using (var db = await GetDbContext<ConcurrencyDeleteContext>())
		{
			await db.Database.ExecuteSqlRawAsync("create table test_delete_concurrency (id int primary key, name varchar(20), stamp timestamp)");
			await db.Database.ExecuteSqlRawAsync("insert into test_delete_concurrency values (65, 'test', current_timestamp)");
			await db.Database.ExecuteSqlRawAsync("insert into test_delete_concurrency values (66, 'test', current_timestamp)");
			await db.Database.ExecuteSqlRawAsync("insert into test_delete_concurrency values (67, 'test', current_timestamp)");
			var entity = new ConcurrencyDeleteEntity() { Id = 66, Stamp = new DateTime(1970, 1, 1) };
			var entry = db.Attach(entity);
			entry.State = EntityState.Deleted;
			Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => db.SaveChangesAsync());
		}
	}
}
