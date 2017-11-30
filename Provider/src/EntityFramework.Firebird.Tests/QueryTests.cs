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
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace EntityFramework.Firebird.Tests
{
	public class QueryTests : EntityFrameworkTestsBase
	{
		class QueryTest1Context : FbTestDbContext
		{
			public QueryTest1Context(FbConnection conn)
				: base(conn)
			{ }

			protected override void OnModelCreating(DbModelBuilder modelBuilder)
			{
				base.OnModelCreating(modelBuilder);
				var queryTest1Entity = modelBuilder.Entity<QueryTest1Entity>();
				queryTest1Entity.Property(x => x.ID).HasColumnName("ID");
				queryTest1Entity.ToTable("TEST_QUERYTEST1ENTITY");
			}

			public IDbSet<QueryTest1Entity> QueryTest1Entity { get; set; }
		}
		[Test]
		public void QueryTest1()
		{
			using (var c = GetDbContext<QueryTest1Context>())
			{
				c.Database.ExecuteSqlCommand("create table test_querytest1entity (id int primary key)");
				Assert.DoesNotThrow(() => c.QueryTest1Entity.Max<QueryTest1Entity, int?>(x => x.ID));
			}
		}

		class QueryTest2Context : FbTestDbContext
		{
			public QueryTest2Context(FbConnection conn)
				: base(conn)
			{ }

			protected override void OnModelCreating(DbModelBuilder modelBuilder)
			{
				base.OnModelCreating(modelBuilder);
			}

			public IDbSet<Foo> Foos { get; set; }
		}
		[Test]
		public void QueryTest2()
		{
			using (var c = GetDbContext<QueryTest2Context>())
			{
				var q = c.Foos
					.OrderBy(x => x.ID)
					.Take(45).Skip(0)
					.Select(x => new
					{
						x.ID,
						x.BazID,
						BazID2 = x.Baz.ID,
						x.Baz.BazString,
					});
				Assert.DoesNotThrow(() =>
				{
					q.ToString();
				});
			}
		}

		class QueryTest3Context : FbTestDbContext
		{
			public QueryTest3Context(FbConnection conn)
				: base(conn)
			{ }

			protected override void OnModelCreating(DbModelBuilder modelBuilder)
			{
				base.OnModelCreating(modelBuilder);
			}

			public IDbSet<Foo> Foos { get; set; }
		}
		[Test]
		public void QueryTest3()
		{
			using (var c = GetDbContext<QueryTest3Context>())
			{
				var q = c.Foos
					 .OrderByDescending(m => m.Bars.Count())
					 .Skip(3)
					 .SelectMany(m => m.Bars);
				Assert.DoesNotThrow(() =>
				{
					q.ToString();
				});
			}
		}

		class ProperVarcharLengthForConstantContext : FbTestDbContext
		{
			public ProperVarcharLengthForConstantContext(FbConnection conn)
				: base(conn)
			{ }

			protected override void OnModelCreating(DbModelBuilder modelBuilder)
			{
				base.OnModelCreating(modelBuilder);
			}

			public IDbSet<Bar> Bars { get; set; }
		}
		[Test]
		public void ProperVarcharLengthForConstantTest()
		{
			using (var c = GetDbContext<ProperVarcharLengthForConstantContext>())
			{
				var q = c.Bars.Where(x => x.BarString == "TEST");
				StringAssert.Contains("CAST(_UTF8'TEST' AS VARCHAR(8191))", q.ToString());
			}
		}
	}

	class QueryTest1Entity
	{
		public int ID { get; set; }
	}

	class Foo
	{
		public int ID { get; set; }
		public int BazID { get; set; }
		public ICollection<Bar> Bars { get; set; }
		public Baz Baz { get; set; }
	}
	class Bar
	{
		public int ID { get; set; }
		public int FooID { get; set; }
		public string BarString { get; set; }
		public Foo Foo { get; set; }
	}
	class Baz
	{
		public int ID { get; set; }
		public string BazString { get; set; }
		public ICollection<Foo> Foos { get; set; }
	}
}
