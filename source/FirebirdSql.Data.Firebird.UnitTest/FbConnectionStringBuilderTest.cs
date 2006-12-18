/*
 *	Firebird ADO.NET Data provider for .NET	and	Mono 
 * 
 *	   The contents	of this	file are subject to	the	Initial	
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this	file except	in compliance with the 
 *	   License.	You	may	obtain a copy of the License at	
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software	distributed	under the License is distributed on	
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied.	See	the	License	for	the	specific 
 *	   language	governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002,	2004 Carlos	Guzman Alvarez
 *	All	Rights Reserved.
 */

using System;
using System.Configuration;

using NUnit.Framework;
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture()]
	public class FbConnectionStringBuilderTest : BaseTest
	{		
		public FbConnectionStringBuilderTest() : base()
		{
		}

		[Test]
		public void BuildFromConnectionStringTest()
		{
			string cs = this.BuildConnectionString().Replace(";", "; ");

			FbConnectionStringBuilder csb = new FbConnectionStringBuilder(cs);

			Assert.AreEqual(csb.UserID		, ConfigurationSettings.AppSettings["User"]);
			Assert.AreEqual(csb.Password	, ConfigurationSettings.AppSettings["Password"]);
			Assert.AreEqual(csb.Database	, ConfigurationSettings.AppSettings["Database"]);
			Assert.AreEqual(csb.DataSource	, ConfigurationSettings.AppSettings["DataSource"]);
			Assert.AreEqual(csb.Port		, Convert.ToInt32(ConfigurationSettings.AppSettings["Port"]));
			Assert.AreEqual(csb.Charset		, ConfigurationSettings.AppSettings["Charset"]);
			Assert.AreEqual(csb.Pooling		, Convert.ToBoolean(ConfigurationSettings.AppSettings["Pooling"]));
			Assert.AreEqual(csb.ServerType	, Convert.ToInt32(ConfigurationSettings.AppSettings["ServerType"]));
		}
	}
}
