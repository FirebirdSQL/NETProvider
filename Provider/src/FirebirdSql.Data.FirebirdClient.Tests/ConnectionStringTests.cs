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

using System.Globalization;
using System.Threading;
using FirebirdSql.Data.Common;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	public class ConnectionStringTests
	{
		[Test]
		public void ParsingNormalConnectionStringTest()
		{
			const string ConnectionString = "datasource=testserver;database=testdb.fdb;user=testuser;password=testpwd";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("testserver", cs.DataSource);
			Assert.AreEqual("testdb.fdb", cs.Database);
			Assert.AreEqual("testuser", cs.UserID);
			Assert.AreEqual("testpwd", cs.Password);
		}

		[Test]
		public void ParsingFullDatabaseConnectionStringTest()
		{
			const string ConnectionString = "database=testserver/1234:testdb.fdb;user=testuser;password=testpwd";
			var cs = new ConnectionString(ConnectionString);
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
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("test'pwd", cs.Password);
		}

		[Test]
		public void ParsingDoubleQuotedConnectionStringTest()
		{
			const string ConnectionString = "datasource=testserver;database=testdb.fdb;user=testuser;password=test\"pwd";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("test\"pwd", cs.Password);
		}

		[Test]
		public void ParsingSpacesInKeyConnectionStringTest()
		{
			const string ConnectionString = "data source=testserver";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("testserver", cs.DataSource);
		}

		[Test]
		public void ParsingOneCharValueConnectionStringTest()
		{
			const string ConnectionString = "connection lifetime=6";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual(6, cs.ConnectionLifetime);
		}

		[Test]
		public void ParsingWithEndingSemicolonConnectionStringTest()
		{
			const string ConnectionString = "user=testuser;password=testpwd;";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("testuser", cs.UserID);
			Assert.AreEqual("testpwd", cs.Password);
		}

		[Test]
		public void ParsingWithoutEndingSemicolonConnectionStringTest()
		{
			const string ConnectionString = "user=testuser;password=testpwd";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("testuser", cs.UserID);
			Assert.AreEqual("testpwd", cs.Password);
		}

		[Test]
		public void ParsingMultilineConnectionStringTest()
		{
			const string ConnectionString = @"DataSource=S05-04;
 User=SYSDBA;
 Password=masterkey;
 Role=;
 Database=Termine;
 Port=3050;
 Dialect=3;
 Charset=ISO8859_1;
 Connection lifetime=0;
 Connection timeout=15;
 Pooling=True;
 Packet Size=8192;";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("Termine", cs.Database);
			Assert.AreEqual("", cs.Role);
		}

		[Test]
		public void NormalizedConnectionStringIgnoresCultureTest()
		{
			const string ConnectionString = "datasource=testserver;database=testdb.fdb;user=testuser;password=testpwd";
			var cs = new ConnectionString(ConnectionString);
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
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("", cs.UserID);
			Assert.AreEqual("testpwd", cs.Password);
		}

		[Test]
		public void ParsingWithWhiteSpacesKeyConnectionStringTest()
		{
			const string ConnectionString = "user= \t;password=testpwd";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("", cs.UserID);
			Assert.AreEqual("testpwd", cs.Password);
		}

		[Test]
		public void CryptKeyWithBase64FullPadding()
		{
			const string ConnectionString = "user=u;cryptkey=dGVzdA==;password=p";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("test", cs.CryptKey);
		}

		[Test]
		public void CryptKeyWithBase64SinglePadding()
		{
			const string ConnectionString = "user=u;cryptkey=YWE=;password=p";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("aa", cs.CryptKey);
		}

		[Test]
		public void CryptKeyWithBase64NoPadding()
		{
			const string ConnectionString = "user=u;cryptkey=YWFh;password=p";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("aaa", cs.CryptKey);
		}

		[Test]
		public void WireCryptMixedCase()
		{
			const string ConnectionString = "wire crYpt=reQUIREd";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual(FbWireCrypt.Required, cs.WireCrypt);
		}

		[Test]
		public void ParsingDatabaseOldStyleHostnameWithoutPortWithoutPath()
		{
			const string ConnectionString = "database=hostname:test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseOldStyleHostnameWithoutPortRootPath()
		{
			const string ConnectionString = "database=hostname:/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseOldStyleHostnameWithoutPortDrivePath()
		{
			const string ConnectionString = "database=hostname:C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseOldStyleIP4WithoutPortWithoutPath()
		{
			const string ConnectionString = "database=127.0.0.1:test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseOldStyleIP4WithoutPortRootPath()
		{
			const string ConnectionString = "database=127.0.0.1:/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseOldStyleIP4WithoutPortDrivePath()
		{
			const string ConnectionString = "database=127.0.0.1:C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseOldStyleIP6WithoutPortWithoutPath()
		{
			const string ConnectionString = "database=::1:test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseOldStyleIP6WithoutPortRootPath()
		{
			const string ConnectionString = "database=::1:/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseOldStyleIP6WithoutPortDrivePath()
		{
			const string ConnectionString = "database=::1:C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseOldStyleHostnameWithPortWithoutPath()
		{
			const string ConnectionString = "database=hostname/6666:test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseOldStyleHostnameWithPortRootPath()
		{
			const string ConnectionString = "database=hostname/6666:/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseOldStyleHostnameWithPortDrivePath()
		{
			const string ConnectionString = "database=hostname/6666:C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseOldStyleIP4WithPortWithoutPath()
		{
			const string ConnectionString = "database=127.0.0.1/6666:test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseOldStyleIP4WithPortRootPath()
		{
			const string ConnectionString = "database=127.0.0.1/6666:/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseOldStyleIP4WithPortDrivePath()
		{
			const string ConnectionString = "database=127.0.0.1/6666:C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseOldStyleIP6WithPortWithoutPath()
		{
			const string ConnectionString = "database=::1/6666:test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseOldStyleIP6WithPortRootPath()
		{
			const string ConnectionString = "database=::1/6666:/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseOldStyleIP6WithPortDrivePath()
		{
			const string ConnectionString = "database=::1/6666:C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseNewStyleHostnameWithoutPortWithoutPath()
		{
			const string ConnectionString = "database=//hostname/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseNewStyleHostnameWithoutPortRootPath()
		{
			const string ConnectionString = "database=//hostname//test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseNewStyleHostnameWithoutPortDrivePath()
		{
			const string ConnectionString = "database=//hostname/C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseNewStyleIP4WithoutPortWithoutPath()
		{
			const string ConnectionString = "database=//127.0.0.1/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseNewStyleIP4WithoutPortRootPath()
		{
			const string ConnectionString = "database=//127.0.0.1//test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseNewStyleIP4WithoutPortDrivePath()
		{
			const string ConnectionString = "database=//127.0.0.1/C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseNewStyleIP6WithoutPortWithoutPath()
		{
			const string ConnectionString = "database=//::1/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseNewStyleIP6WithoutPortRootPath()
		{
			const string ConnectionString = "database=//::1//test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseNewStyleIP6WithoutPortDrivePath()
		{
			const string ConnectionString = "database=//::1/C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseNewStyleHostnameWithPortWithoutPath()
		{
			const string ConnectionString = "database=//hostname:6666/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseNewStyleHostnameWithPortRootPath()
		{
			const string ConnectionString = "database=//hostname:6666//test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseNewStyleHostnameWithPortDrivePath()
		{
			const string ConnectionString = "database=//hostname:6666/C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseNewStyleIP4WithPortWithoutPath()
		{
			const string ConnectionString = "database=//127.0.0.1:6666/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseNewStyleIP4WithPortRootPath()
		{
			const string ConnectionString = "database=//127.0.0.1:6666//test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseNewStyleIP4WithPortDrivePath()
		{
			const string ConnectionString = "database=//127.0.0.1:6666/C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseNewStyleIP6WithPortWithoutPath()
		{
			const string ConnectionString = "database=//[::1]:6666/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseNewStyleIP6WithPortRootPath()
		{
			const string ConnectionString = "database=//[::1]:6666//test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseNewStyleIP6WithPortDrivePath()
		{
			const string ConnectionString = "database=//[::1]:6666/C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseURLStyleHostnameWithoutPortWithoutPath()
		{
			const string ConnectionString = "database=inet://hostname/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseURLStyleHostnameWithoutPortRootPath()
		{
			const string ConnectionString = "database=inet://hostname//test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseURLStyleHostnameWithoutPortDrivePath()
		{
			const string ConnectionString = "database=inet://hostname/C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseURLStyleIP4WithoutPortWithoutPath()
		{
			const string ConnectionString = "database=inet://127.0.0.1/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseURLStyleIP4WithoutPortRootPath()
		{
			const string ConnectionString = "database=inet://127.0.0.1//test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseURLStyleIP4WithoutPortDrivePath()
		{
			const string ConnectionString = "database=inet://127.0.0.1/C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseURLStyleIP6WithoutPortWithoutPath()
		{
			const string ConnectionString = "database=inet://::1/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseURLStyleIP6WithoutPortRootPath()
		{
			const string ConnectionString = "database=inet://::1//test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseURLStyleIP6WithoutPortDrivePath()
		{
			const string ConnectionString = "database=inet://::1/C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseURLStyleHostnameWithPortWithoutPath()
		{
			const string ConnectionString = "database=inet://hostname:6666/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseURLStyleHostnameWithPortRootPath()
		{
			const string ConnectionString = "database=inet://hostname:6666//test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseURLStyleHostnameWithPortDrivePath()
		{
			const string ConnectionString = "database=inet://hostname:6666/C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("hostname", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseURLStyleIP4WithPortWithoutPath()
		{
			const string ConnectionString = "database=inet://127.0.0.1:6666/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseURLStyleIP4WithPortRootPath()
		{
			const string ConnectionString = "database=inet://127.0.0.1:6666//test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseURLStyleIP4WithPortDrivePath()
		{
			const string ConnectionString = "database=inet://127.0.0.1:6666/C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("127.0.0.1", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseURLStyleIP6WithPortWithoutPath()
		{
			const string ConnectionString = "database=inet://[::1]:6666/test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseURLStyleIP6WithPortRootPath()
		{
			const string ConnectionString = "database=inet://[::1]:6666//test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseURLStyleIP6WithPortDrivePath()
		{
			const string ConnectionString = "database=inet://[::1]:6666/C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("::1", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
			Assert.AreEqual(6666, cs.Port);
		}

		[Test]
		public void ParsingDatabaseURLStyleWithoutHostnameWithoutPath()
		{
			const string ConnectionString = "database=inet:///test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("localhost", cs.DataSource);
			Assert.AreEqual("test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseURLStyleWithoutHostnameRootPath()
		{
			const string ConnectionString = "database=inet:////test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("localhost", cs.DataSource);
			Assert.AreEqual("/test.fdb", cs.Database);
		}

		[Test]
		public void ParsingDatabaseURLStyleWithoutHostnameDrivePath()
		{
			const string ConnectionString = "database=inet:///C:\\test.fdb";
			var cs = new ConnectionString(ConnectionString);
			Assert.AreEqual("localhost", cs.DataSource);
			Assert.AreEqual("C:\\test.fdb", cs.Database);
		}
	}
}
