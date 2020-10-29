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

using System;
using System.Numerics;
using FirebirdSql.Data.TestsBase;
using FirebirdSql.Data.Types;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbDefaultServerTypeTestFixtureSource))]
	[TestFixtureSource(typeof(FbEmbeddedServerTypeTestFixtureSource))]
	public class FbDecFloat34SupportTests : FbTestsBase
	{
		public FbDecFloat34SupportTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			if (!EnsureVersion(new Version(4, 0, 0, 0)))
				return;
		}

		static readonly object[] TestValues = new[]
		{
			new object[] { (FbDecFloat)0, "0" },
			new object[] { (FbDecFloat)1, "1" },
			new object[] { (FbDecFloat)(-1), "-1" },
			new object[] { (FbDecFloat)6, "6" },
			new object[] { (FbDecFloat)(-6), "-6" },
			new object[] { FbDecFloat.NegativeZero, "-0" },
			new object[] { FbDecFloat.PositiveInfinity, "inf" },
			new object[] { FbDecFloat.NegativeInfinity, "-inf" },
			new object[] { FbDecFloat.PositiveNaN, "nan" },
			new object[] { FbDecFloat.NegativeNaN, "-nan" },
			new object[] { FbDecFloat.PositiveSignalingNaN, "snan" },
			new object[] { FbDecFloat.NegativeSignalingNaN, "-snan" },
			new object[] { (FbDecFloat)0.1, "0.1" },
			new object[] { (FbDecFloat)(-0.1), "-0.1" },
			new object[] { (FbDecFloat)6.6, "6.6" },
			new object[] { (FbDecFloat)(-6.6), "-6.6" },
			new object[] { new FbDecFloat(10, 34), "100000000000000000000000000000000000" },
			new object[] { new FbDecFloat(-10, 34), "-100000000000000000000000000000000000" },
			new object[] { new FbDecFloat(BigInteger.Parse("123000000001"), -10), "123.000000001E-1" },
			new object[] { new FbDecFloat(BigInteger.Parse("-123000000001"), -10), "-123.000000001E-1" },
		};

		[TestCaseSource(nameof(TestValues))]
		public void ReadsValueCorrectly(FbDecFloat value, string castValue)
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = $"select cast('{castValue}' as decfloat(34)) from rdb$database";
				var result = (FbDecFloat)cmd.ExecuteScalar();
				Assert.AreEqual(value, result);
			}
		}

		[TestCaseSource(nameof(TestValues))]
		public void PassesValueCorrectly(FbDecFloat value, string dummy)
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as decfloat(34)) from rdb$database";
				cmd.Parameters.AddWithValue("value", value);
				var result = (FbDecFloat)cmd.ExecuteScalar();
				Assert.AreEqual(value, result);
			}
		}

		[Test]
		public void ReadsValueNullCorrectly()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(null as decfloat(34)) from rdb$database";
				var result = (DBNull)cmd.ExecuteScalar();
				Assert.AreEqual(DBNull.Value, result);
			}
		}

		[Test]
		public void PassesValueNullCorrectly()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as decfloat(34)) from rdb$database";
				cmd.Parameters.AddWithValue("value", DBNull.Value);
				var result = (DBNull)cmd.ExecuteScalar();
				Assert.AreEqual(DBNull.Value, result);
			}
		}

		[Test]
		public void SimpleSelectSchemaTableTest()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(null as decfloat(34)) from rdb$database";
				using (var reader = cmd.ExecuteReader())
				{
					var schema = reader.GetSchemaTable();
					Assert.AreEqual(typeof(FbDecFloat), schema.Rows[0].ItemArray[5]);
				}
			}
		}
	}
}
