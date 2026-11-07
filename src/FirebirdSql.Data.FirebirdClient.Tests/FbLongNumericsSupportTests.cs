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
using System.Numerics;
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

	static readonly object[] FitsDecimalValues = new[]
	{
			new object[] { "1.000000000000000000000000000000", 30, 1m },
			new object[] { "1234.500000", 6, 1234.5m },
			new object[] { "123", 0, 123m },
			new object[] { "79228162514264337593543950335", 0, decimal.MaxValue },
			new object[] { "-79228162514264337593543950335", 0, decimal.MinValue },
			new object[] { "0.0000000000000000000000000001", 28, 0.0000000000000000000000000001m },
			new object[] { "0.000000000000000000000000000100", 30, 0.0000000000000000000000000001m },
		};

	[TestCaseSource(nameof(FitsDecimalValues))]
	public async Task ReadsValueFittingDecimalExactly(string literal, int scale, decimal expected)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = $"select cast('{literal}' as numeric(38, {scale})) from rdb$database";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				await reader.ReadAsync();
				Assert.AreEqual(typeof(decimal), reader.GetFieldType(0));
				var result = reader.GetValue(0);
				Assert.IsInstanceOf<decimal>(result);
				Assert.AreEqual(expected, (decimal)result);
				Assert.AreEqual(expected.ToString(CultureInfo.InvariantCulture), ((decimal)result).ToString(CultureInfo.InvariantCulture));
			}
		}
	}

	static readonly object[] DoesNotFitDecimalValues = new[]
	{
			new object[] { "12345678901234567890123456789012345678", 0 },
			new object[] { "-12345678901234567890123456789012345678", 0 },
			new object[] { "79228162514264337593543950336", 0 },
			new object[] { "12345678901234567890123456789012.345678", 6 },
			new object[] { "0.000000000000000000000000000001", 30 },
			new object[] { "-0.000123456789012345678901234567", 30 },
		};

	[TestCaseSource(nameof(DoesNotFitDecimalValues))]
	public async Task ReadsValueNotFittingDecimalWithoutFailingTheRow(string literal, int scale)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = $"select cast('{literal}' as numeric(38, {scale})), 1 from rdb$database";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				Assert.IsTrue(await reader.ReadAsync());
				Assert.AreEqual(1, reader.GetInt32(1));
				var format = NumberFormatInfo.CurrentInfo;
				Assert.AreEqual(literal.Replace("-", format.NegativeSign).Replace(".", format.NumberDecimalSeparator), reader.GetString(0));
				Assert.AreEqual(double.Parse(literal, CultureInfo.InvariantCulture), reader.GetDouble(0));
				Assert.Throws<OverflowException>(() => reader.GetValue(0));
				Assert.Throws<OverflowException>(() => reader.GetDecimal(0));
				Assert.Throws<OverflowException>(() => reader.GetFieldValue<decimal>(0));
			}
		}
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast('1234.5' as numeric(38, 6)) from rdb$database";
			Assert.AreEqual(1234.5m, await cmd.ExecuteScalarAsync(), "connection must remain usable");
		}
	}

	static readonly object[] BigIntegerValues = new[]
	{
			new object[] { "12345678901234567890123456789012345678", 0, "12345678901234567890123456789012345678" },
			new object[] { "-12345678901234567890123456789012345678", 0, "-12345678901234567890123456789012345678" },
			new object[] { "123", 0, "123" },
			new object[] { "12345678901234567890123456789012.000000", 6, "12345678901234567890123456789012" },
			new object[] { "1234.000000", 6, "1234" },
		};

	[TestCaseSource(nameof(BigIntegerValues))]
	public async Task ReadsIntegralValueAsBigInteger(string literal, int scale, string expected)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = $"select cast('{literal}' as numeric(38, {scale})) from rdb$database";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				await reader.ReadAsync();
				Assert.AreEqual(BigInteger.Parse(expected, CultureInfo.InvariantCulture), reader.GetFieldValue<BigInteger>(0));
			}
		}
	}

	[TestCase("1234.5", 6)]
	[TestCase("12345678901234567890123456789012.5", 6)]
	public async Task ReadsFractionalValueAsBigIntegerThrows(string literal, int scale)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = $"select cast('{literal}' as numeric(38, {scale})) from rdb$database";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				await reader.ReadAsync();
				Assert.Throws<OverflowException>(() => reader.GetFieldValue<BigInteger>(0));
			}
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
