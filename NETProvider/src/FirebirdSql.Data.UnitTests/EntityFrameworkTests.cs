/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2009 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;

using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture]
	public class EntityFrameworkTests : TestsBase
	{
		#region Constructors

		public EntityFrameworkTests()
		{ }

		#endregion

		#region Unit Tests

		[Test]
		public void DbProviderServicesTest()
		{
			object dbproviderservices = (FirebirdClientFactory.Instance as IServiceProvider).GetService(typeof(DbProviderServices));
			Assert.IsNotNull(dbproviderservices);
			Assert.IsInstanceOf<FbProviderServices>(dbproviderservices);
		}

		[Test]
		public void ProviderManifestTest()
		{
			DbProviderManifest manifest = this.GetProviderServices().GetProviderManifest("foobar");
			Assert.IsNotNull(manifest);
		}

		[Test]
		public void ProviderManifestTokenTest()
		{
			string token = this.GetProviderServices().GetProviderManifestToken(Connection);
			Assert.IsNotNullOrEmpty(token);
			Console.WriteLine(token);
			Version v = new Version(token);
			Assert.Greater(v.Major, 0);
			Assert.GreaterOrEqual(v.Minor, 0);
			Assert.AreEqual(v.Build, -1);
			Assert.AreEqual(v.Revision, -1);
		}

		#region Query1

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

		#endregion

		#region Query2

		[Test]
		public void QueryTest2()
		{
			Database.SetInitializer<QueryTest1Context>(null);
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
					Console.WriteLine(q.ToString());
				});
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

		#endregion

		#region Query3

		[Test]
		public void QueryTest3()
		{
			Database.SetInitializer<QueryTest1Context>(null);
			Connection.Close();
			using (var c = new QueryTest3Context(Connection))
			{
				var q = c.Foos
					 .OrderByDescending(m => m.Bars.Count())
					 .Skip(3)
					 .SelectMany(m => m.Bars);
				Assert.DoesNotThrow(() =>
				{
					Console.WriteLine(q.ToString());
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

		#endregion

		#region ProperVarcharLengthForConstant

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

		#endregion

		#endregion

		private DbProviderServices GetProviderServices()
		{
			return (DbProviderServices)(FirebirdClientFactory.Instance as IServiceProvider).GetService(typeof(DbProviderServices));
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
