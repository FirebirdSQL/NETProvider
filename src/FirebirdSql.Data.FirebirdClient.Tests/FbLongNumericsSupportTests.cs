/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
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
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
public class FbLongNumericsSupportTests : FbTestsBase
{
	public FbLongNumericsSupportTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
		: base(serverType, compression, wireCrypt)
	{ }

	[SetUp]
	public override async Task SetUp()
	{
		await base.SetUp();

		if (!EnsureServerVersionAtLeast(new Version(4, 0, 0, 0)))
			return;
	}

	static readonly object[] TestValues = new[]
	{
			new object[] { 3.14159265359m, 20 },
			new object[] { 7465205690.61m, 4 },
		};

	[TestCaseSource(nameof(TestValues))]
	public async Task ReadsValueCorrectly(decimal value, int scale)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			var svalue = value.ToString(CultureInfo.InvariantCulture);
			cmd.CommandText = $"select cast({svalue} as decimal(20, {scale})) from rdb$database";
			var result = (decimal)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(value, result);
		}
	}

	[TestCaseSource(nameof(TestValues))]
	public async Task PassesValueCorrectly(decimal value, int scale)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = $"select cast(@value as decimal(20, {scale})) from rdb$database";
			cmd.Parameters.AddWithValue("value", value);
			var result = (decimal)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(value, result);
		}
	}

	[Test]
	public async Task ReadsValueNullCorrectly()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(null as decimal(20, 6)) from rdb$database";
			var result = (DBNull)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(DBNull.Value, result);
		}
	}

	[Test]
	public async Task PassesValueNullCorrectly()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(@value as decimal(20, 6)) from rdb$database";
			cmd.Parameters.AddWithValue("value", DBNull.Value);
			var result = (DBNull)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(DBNull.Value, result);
		}
	}

	[Test]
	public async Task SimpleSelectSchemaTableTest()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(null as decimal(20, 6)) from rdb$database";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				var schema = await reader.GetSchemaTableAsync();
				Assert.AreEqual(typeof(decimal), schema.Rows[0].ItemArray[5]);
			}
		}
	}
}
