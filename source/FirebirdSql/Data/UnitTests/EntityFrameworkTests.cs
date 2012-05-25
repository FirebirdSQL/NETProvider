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

using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture]
	public class EntityFrameworkTests : TestsBase
	{
		#region · Constructors ·

		public EntityFrameworkTests()
		{ }

		#endregion

		#region · Unit Tests ·

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

		class QueryTest1Context : DbContext
		{
			public QueryTest1Context(FbConnection conn)
				: base(conn, false)
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

		#endregion

		private DbProviderServices GetProviderServices()
		{
			return (DbProviderServices)(FirebirdClientFactory.Instance as IServiceProvider).GetService(typeof(DbProviderServices));
		}
	}

	class QueryTest1Entity
	{
		public int ID { get; set; }
	}
}
