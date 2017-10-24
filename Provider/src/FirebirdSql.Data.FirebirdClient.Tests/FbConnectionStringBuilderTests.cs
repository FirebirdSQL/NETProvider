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

using System.Text;
using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
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
