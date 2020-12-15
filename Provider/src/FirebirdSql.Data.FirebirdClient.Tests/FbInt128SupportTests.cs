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
using System.Globalization;
using System.Numerics;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbDefaultServerTypeTestFixtureSource))]
	[TestFixtureSource(typeof(FbEmbeddedServerTypeTestFixtureSource))]
	public class FbInt128SupportTests : FbTestsBase
	{
		public FbInt128SupportTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			if (!EnsureVersion(new Version(4, 0, 0, 0)))
				return;
		}

		static readonly BigInteger[] TestValues = new[]
		{
			BigInteger.Parse("0"),
			BigInteger.Parse("1"),
			BigInteger.Parse("-1"),
			BigInteger.Parse("6"),
			BigInteger.Parse("-6"),
			BigInteger.Parse("184467440737095516190874"),
			BigInteger.Parse("-184467440737095516190874"),
		};

		[TestCaseSource(nameof(TestValues))]
		public void ReadsValueCorrectly(BigInteger value)
		{
			using (var cmd = Connection.CreateCommand())
			{
				var svalue = value.ToString(CultureInfo.InvariantCulture);
				cmd.CommandText = $"select cast({svalue} as int128) from rdb$database";
				var result = (BigInteger)cmd.ExecuteScalar();
				Assert.AreEqual(value, result);
			}
		}

		[TestCaseSource(nameof(TestValues))]
		public void PassesValueCorrectly(BigInteger value)
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as int128) from rdb$database";
				cmd.Parameters.AddWithValue("value", value);
				var result = (BigInteger)cmd.ExecuteScalar();
				Assert.AreEqual(value, result);
			}
		}

		[Test]
		public void ReadsValueNullCorrectly()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(null as int128) from rdb$database";
				var result = (DBNull)cmd.ExecuteScalar();
				Assert.AreEqual(DBNull.Value, result);
			}
		}

		[Test]
		public void PassesValueNullCorrectly()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@value as int128) from rdb$database";
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
				cmd.CommandText = "select cast(null as int128) from rdb$database";
				using (var reader = cmd.ExecuteReader())
				{
					var schema = reader.GetSchemaTable();
					Assert.AreEqual(typeof(BigInteger), schema.Rows[0].ItemArray[5]);
				}
			}
		}
	}
}
