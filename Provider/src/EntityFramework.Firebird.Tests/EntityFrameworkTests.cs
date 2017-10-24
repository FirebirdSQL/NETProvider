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
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace EntityFramework.Firebird.Tests
{
	[FbTestFixture(FbServerType.Default, false)]
	public class EntityFrameworkTests : FbTestsBase
	{
		public EntityFrameworkTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{ }

		[Test]
		public void DbProviderServicesTest()
		{
			object dbproviderservices = GetProviderServices();
			Assert.IsNotNull(dbproviderservices);
			Assert.IsInstanceOf<FbProviderServices>(dbproviderservices);
		}

		[Test]
		public void ProviderManifestTest()
		{
			DbProviderManifest manifest = GetProviderServices().GetProviderManifest("foobar");
			Assert.IsNotNull(manifest);
		}

		[Test]
		public void ProviderManifestTokenTest()
		{
			string token = GetProviderServices().GetProviderManifestToken(Connection);
			Assert.IsNotNull(token);
			Assert.IsNotEmpty(token);
			Version v = new Version(token);
			Assert.Greater(v.Major, 0);
			Assert.GreaterOrEqual(v.Minor, 0);
			Assert.AreEqual(v.Build, -1);
			Assert.AreEqual(v.Revision, -1);
		}

		class QueryTest1Context : FbTestDbContext
		{
			public QueryTest1Context(FbConnection conn)
				: base(conn)
			{ }

			protected override void OnModelCreating(DbModelBuilder modelBuilder)
			{
				base.OnModelCreating(modelBuilder);
				var queryTest1Entity = modelBuilder.Entity<QueryTest1Entity>();
				queryTest1Entity.Property(x => x.ID).HasColumnName("INT_FIELD");
				queryTest1Entity.ToTable("TEST");
			}

			public IDbSet<QueryTest1Entity> QueryTest1Entity { get; set; }
		}
		[Test]
		public void QueryTest1()
		{
			Database.SetInitializer<QueryTest1Context>(null);
			Connection.Close();
			using (var c = new QueryTest1Context(Connection))
			{
				Assert.DoesNotThrow(() => c.QueryTest1Entity.Max(x => x.ID));
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
			Database.SetInitializer<QueryTest2Context>(null);
			Connection.Close();
			using (var c = new QueryTest2Context(Connection))
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
			Database.SetInitializer<QueryTest3Context>(null);
			Connection.Close();
			using (var c = new QueryTest3Context(Connection))
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
			Database.SetInitializer<ProperVarcharLengthForConstantContext>(null);
			Connection.Close();
			using (var c = new ProperVarcharLengthForConstantContext(Connection))
			{
				var q = c.Bars.Where(x => x.BarString == "TEST");
				StringAssert.Contains("CAST(_UTF8'TEST' AS VARCHAR(8191))", q.ToString());
			}
		}

		DbProviderServices GetProviderServices()
		{
			return FbProviderServices.Instance;
		}

		class FbTestDbContext : DbContext
		{
			public FbTestDbContext(FbConnection conn)
				: base(conn, false)
			{ }
		}
	}

	class QueryTest1Entity
	{
		public int ID { get; set; }
	}

	public class Foo
	{
		public int ID { get; set; }
		public int BazID { get; set; }
		public ICollection<Bar> Bars { get; set; }
		public Baz Baz { get; set; }
	}
	public class Bar
	{
		public int ID { get; set; }
		public int FooID { get; set; }
		public string BarString { get; set; }
		public Foo Foo { get; set; }
	}
	public class Baz
	{
		public int ID { get; set; }
		public string BazString { get; set; }
		public ICollection<Foo> Foos { get; set; }
	}
}
