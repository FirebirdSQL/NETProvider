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
 *  Copyright (c) 2016 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture]
	public class FbConnectionStringBuilderTests
	{
		[Test]
		public void EmptyCtorGeneratesEmptyString()
		{
			var b = new FbConnectionStringBuilder();
			Assert.IsEmpty(b.ToString());
		}

		[Test]
		public void NoValueProvidedReturnsDefault()
		{
			var b = new FbConnectionStringBuilder();
			Assert.AreEqual(b.MaxPoolSize, FbConnectionString.DefaultValueMaxPoolSize);
		}

		[Test]
		public void CryptKeyValueSetter()
		{
			var b = new FbConnectionStringBuilder();
			b.CryptKey = Encoding.ASCII.GetBytes("test");
			Assert.AreEqual("crypt key=\"dGVzdA==\"", b.ToString());
		}

		[Test]
		public void CryptKeyValueGetter()
		{
			var b = new FbConnectionStringBuilder("CryptKey=dGVzdA==");
			Assert.AreEqual("test", Encoding.ASCII.GetString(b.CryptKey));
		}
	}
}
