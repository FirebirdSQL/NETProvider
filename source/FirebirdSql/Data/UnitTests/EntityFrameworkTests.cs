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

using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;
using System.Reflection;

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
			Assert.Greater(v.Minor, 0);
			Assert.AreEqual(v.Build, -1);
			Assert.AreEqual(v.Revision, -1);
		}

		#endregion

		private DbProviderServices GetProviderServices()
		{
			return (DbProviderServices)(FirebirdClientFactory.Instance as IServiceProvider).GetService(typeof(DbProviderServices));
		}
	}
}
