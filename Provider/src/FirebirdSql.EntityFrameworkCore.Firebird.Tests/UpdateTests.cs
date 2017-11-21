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
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests
{
	public class UpdateTests : EntityFrameworkCoreTestsBase
	{
		class UpdateContext : FbTestDbContext
		{
			public UpdateContext(string connectionString)
				: base(connectionString)
			{ }

			protected override void OnModelCreating(ModelBuilder modelBuilder)
			{
				base.OnModelCreating(modelBuilder);

				var insertEntityConf = modelBuilder.Entity<UpdateEntity>();
				insertEntityConf.Property(x => x.Id).HasColumnName("ID");
				insertEntityConf.Property(x => x.Foo).HasColumnName("FOO");
				insertEntityConf.Property(x => x.Bar).HasColumnName("BAR");
				insertEntityConf.ToTable("TEST_UPDATE");
			}
		}
		class UpdateEntity
		{
			public int Id { get; set; }
			public string Foo { get; set; }
			public string Bar { get; set; }
		}
		[Test]
		public void Update()
		{
			using (var db = GetDbContext<UpdateContext>())
			{
				db.Database.ExecuteSqlCommand("recreate table test_update (id int primary key, foo varchar(20), bar varchar(20))");
				db.Database.ExecuteSqlCommand("update or insert into test_update values (66, 'foo', 'bar')");
				var entity = new UpdateEntity() { Id = 66, Foo = "test", Bar = "test" };
				var entry = db.Attach(entity);
				entry.Property(x => x.Foo).IsModified = true;
				db.SaveChanges();
				var value = db.Set<UpdateEntity>()
					.FromSql("select * from test_update where id = 66")
					.AsNoTracking()
					.First();
				Assert.AreEqual("test", value.Foo);
				Assert.AreNotEqual("test", value.Bar);
			}
		}
	}
}
