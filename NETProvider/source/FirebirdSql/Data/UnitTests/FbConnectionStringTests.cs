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
 *  Copyright (c) 2012 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

using System;
using System.Configuration;
using System.Data;
using System.Reflection;

using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture]
	public class FbConnectionStringTests
	{
		[Test]
		public void ParsingNormalConnectionStringTest()
		{
			const string ConnectionString = "datasource=testserver;database=testdb.fdb;user=testuser;password=testpwd";
			var cs = new FbConnectionString(ConnectionString);
			Assert.AreEqual("testserver", cs.DataSource);
			Assert.AreEqual("testdb.fdb", cs.Database);
			Assert.AreEqual("testuser", cs.UserID);
			Assert.AreEqual("testpwd", cs.Password);
		}

		[Test]
		public void ParsingFullDatabaseConnectionStringTest()
		{
			const string ConnectionString = "database=testserver/1234:testdb.fdb;user=testuser;password=testpwd";
			var cs = new FbConnectionString(ConnectionString);
			Assert.AreEqual("testserver", cs.DataSource);
			Assert.AreEqual("testdb.fdb", cs.Database);
			Assert.AreEqual("testuser", cs.UserID);
			Assert.AreEqual("testpwd", cs.Password);
			Assert.AreEqual(1234, cs.Port);
		}

		[Test]
		public void ParsingSingleQuotedConnectionStringTest()
		{
			const string ConnectionString = "datasource=testserver;database=testdb.fdb;user=testuser;password=test'pwd";
			var cs = new FbConnectionString(ConnectionString);
			Assert.AreEqual("test'pwd", cs.Password);
		}

		[Test]
		public void ParsingDoubleQuotedConnectionStringTest()
		{
			const string ConnectionString = "datasource=testserver;database=testdb.fdb;user=testuser;password=test\"pwd";
			var cs = new FbConnectionString(ConnectionString);
			Assert.AreEqual("test\"pwd", cs.Password);
		}
	}
}
