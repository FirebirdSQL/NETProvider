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
using System.Globalization;
using System.Reflection;
using System.Threading;
using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture]
	public class FbConnectionStringTests : TestsBase
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

		[Test]
		public void ParsingSpacesInKeyConnectionStringTest()
		{
			const string ConnectionString = "data source=testserver";
			var cs = new FbConnectionString(ConnectionString);
			Assert.AreEqual("testserver", cs.DataSource);
		}

		[Test]
		public void ParsingOneCharValueConnectionStringTest()
		{
			const string ConnectionString = "connection lifetime=6";
			var cs = new FbConnectionString(ConnectionString);
			Assert.AreEqual(6, cs.ConnectionLifeTime);
		}

		[Test]
		public void ParsingWithEndingSemicolonConnectionStringTest()
		{
			const string ConnectionString = "user=testuser;password=testpwd;";
			var cs = new FbConnectionString(ConnectionString);
			Assert.AreEqual("testuser", cs.UserID);
			Assert.AreEqual("testpwd", cs.Password);
		}

		[Test]
		public void ParsingWithoutEndingSemicolonConnectionStringTest()
		{
			const string ConnectionString = "user=testuser;password=testpwd";
			var cs = new FbConnectionString(ConnectionString);
			Assert.AreEqual("testuser", cs.UserID);
			Assert.AreEqual("testpwd", cs.Password);
		}

		[Test]
		public void ParsingMultilineConnectionStringTest()
		{
			const string ConnectionString = @"DataSource=S05-04; 
 User= SYSDBA; 
 Password= masterkey; 
 Role= ; 
 Database=Termine; 
 Port=3050; 
 Dialect=3; 
 Charset=ISO8859_1; 
 Connection lifetime=0; 
 Connection timeout=15; 
 Pooling=True; 
 Packet Size=8192;";
			var cs = new FbConnectionString(ConnectionString);
			Assert.AreEqual("Termine", cs.Database);
			Assert.AreEqual("", cs.Role);
		}

		[Test]
		public void NormalizedConnectionStringIgnoresCultureTest()
		{
			const string ConnectionString = "datasource=testserver;database=testdb.fdb;user=testuser;password=testpwd";
			var cs = new FbConnectionString(ConnectionString);
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
			var s1 = cs.NormalizedConnectionString;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("cs-CZ");
			var s2 = cs.NormalizedConnectionString;

			Assert.AreEqual(s1, s2);
		}

		[Test]
		public void ParsingWithEmptyKeyConnectionStringTest()
		{
			const string ConnectionString = "user=;password=testpwd";
			var cs = new FbConnectionString(ConnectionString);
			Assert.AreEqual("", cs.UserID);
			Assert.AreEqual("testpwd", cs.Password);
		}

		[Test]
		public void ParsingWithWhiteSpacesKeyConnectionStringTest()
		{
			const string ConnectionString = "user= \t;password=testpwd";
			var cs = new FbConnectionString(ConnectionString);
			Assert.AreEqual("", cs.UserID);
			Assert.AreEqual("testpwd", cs.Password);
		}
	}
}
