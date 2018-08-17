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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	[FbTestFixture(FbServerType.Embedded, default)]
	public class FbParameterTests : FbTestsBase
	{
		#region Constructors

		public FbParameterTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{ }

		#endregion

		#region Unit Tests

		[Test]
		public void ConstructorsTest()
		{
			var ctor01 = new FbParameter();
			var ctor02 = new FbParameter("ctor2", 10);
			var ctor03 = new FbParameter("ctor3", FbDbType.Char);
			var ctor04 = new FbParameter("ctor4", FbDbType.Integer, 4);
			var ctor05 = new FbParameter("ctor5", FbDbType.Integer, 4, "int_field");
			var ctor06 = new FbParameter(
				"ctor6",
				FbDbType.Integer,
				4,
				ParameterDirection.Input,
				false,
				0,
				0,
				"int_field",
				DataRowVersion.Original,
				100);

			ctor01 = null;
			ctor02 = null;
			ctor03 = null;
			ctor04 = null;
			ctor05 = null;
			ctor06 = null;
		}

		[Test]
		public void CloneTest()
		{
			var p = new FbParameter("@p1", FbDbType.Integer);
			p.Value = 1;
			p.Charset = FbCharset.Dos850;

			var p1 = ((ICloneable)p).Clone() as FbParameter;

			Assert.AreEqual(p1.ParameterName, p.ParameterName);
			Assert.AreEqual(p1.FbDbType, p.FbDbType);
			Assert.AreEqual(p1.DbType, p.DbType);
			Assert.AreEqual(p1.Direction, p.Direction);
			Assert.AreEqual(p1.SourceColumn, p.SourceColumn);
			Assert.AreEqual(p1.SourceVersion, p.SourceVersion);
			Assert.AreEqual(p1.Charset, p.Charset);
			Assert.AreEqual(p1.IsNullable, p.IsNullable);
			Assert.AreEqual(p1.Size, p.Size);
			Assert.AreEqual(p1.Scale, p.Scale);
			Assert.AreEqual(p1.Precision, p.Precision);
			Assert.AreEqual(p1.Value, p.Value);
		}

		#endregion
	}
}
