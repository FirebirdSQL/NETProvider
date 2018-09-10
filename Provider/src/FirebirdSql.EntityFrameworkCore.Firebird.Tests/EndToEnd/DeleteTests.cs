/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests.EndToEnd
{
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
		public void Delete()
		{
			using (var db = GetDbContext<DeleteContext>())
			{
				db.Database.ExecuteSqlCommand("create table test_delete (id int primary key, name varchar(20))");
				db.Database.ExecuteSqlCommand("insert into test_delete values (65, 'test')");
				db.Database.ExecuteSqlCommand("insert into test_delete values (66, 'test')");
				db.Database.ExecuteSqlCommand("insert into test_delete values (67, 'test')");
				var entity = new DeleteEntity() { Id = 66 };
				var entry = db.Attach(entity);
				entry.State = EntityState.Deleted;
				db.SaveChanges();
				var values = db.Set<DeleteEntity>()
					 .FromSql("select * from test_delete")
					 .AsNoTracking()
					 .OrderBy(x => x.Id)
					 .ToList();
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
		public void ConcurrencyDelete()
		{
			using (var db = GetDbContext<ConcurrencyDeleteContext>())
			{
				db.Database.ExecuteSqlCommand("create table test_delete_concurrency (id int primary key, name varchar(20), stamp timestamp)");
				db.Database.ExecuteSqlCommand("insert into test_delete_concurrency values (65, 'test', current_timestamp)");
				db.Database.ExecuteSqlCommand("insert into test_delete_concurrency values (66, 'test', current_timestamp)");
				db.Database.ExecuteSqlCommand("insert into test_delete_concurrency values (67, 'test', current_timestamp)");
				var entity = new ConcurrencyDeleteEntity() { Id = 66, Stamp = new DateTime(1970, 1, 1) };
				var entry = db.Attach(entity);
				entry.State = EntityState.Deleted;
				Assert.Throws<DbUpdateConcurrencyException>(() => db.SaveChanges());
			}
		}
	}
}
