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
using FirebirdSql.Data.Types;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
public class FbDecFloat16SupportTests : FbTestsBase
{
	public FbDecFloat16SupportTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
		: base(serverType, compression, wireCrypt, false)
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
			new object[] { new FbDecFloat(1, 384), "1E384" },
			new object[] { new FbDecFloat(BigInteger.Parse("100000000000000000000"), -20), "1" },
			new object[] { new FbDecFloat(100, -400), "1E-398" },
			new object[] { (FbDecFloat)2.0, "2" },
			new object[] { (FbDecFloat)1e-20, "1E-20" },
			new object[] { (FbDecFloat)2.5f, "2.5" },
			new object[] { (FbDecFloat)100m, "100" },
			new object[] { (FbDecFloat)1.10m, "1.10" },
		};

	static readonly object[] UnrepresentableValues = new object[]
	{
		new FbDecFloat(BigInteger.Parse("30000000000000004"), -17),
		new FbDecFloat(1, 385),
		new FbDecFloat(1, -399),
		(FbDecFloat)(0.1 + 0.2),
		(FbDecFloat)decimal.MaxValue,
	};

	[TestCaseSource(nameof(TestValues))]
	public async Task ReadsValueCorrectly(FbDecFloat value, string castValue)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = $"select cast('{castValue}' as decfloat(16)) from rdb$database";
			var result = (FbDecFloat)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(value, result);
		}
	}

	[TestCaseSource(nameof(TestValues))]
	public async Task PassesValueCorrectly(FbDecFloat value, string dummy)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(@value as decfloat(16)) from rdb$database";
			cmd.Parameters.AddWithValue("value", value);
			var result = (FbDecFloat)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(value, result);
		}
	}

	[TestCaseSource(nameof(UnrepresentableValues))]
	public async Task PassesUnrepresentableValueThrows(FbDecFloat value)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(@value as decfloat(16)) from rdb$database";
			cmd.Parameters.AddWithValue("value", value);
			Assert.ThrowsAsync<OverflowException>(() => cmd.ExecuteScalarAsync());
		}
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(1 as decfloat(16)) from rdb$database";
			Assert.AreEqual((FbDecFloat)1, (FbDecFloat)await cmd.ExecuteScalarAsync(), "connection must remain usable");
		}
	}

	[Test]
	public async Task ReadsValueNullCorrectly()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(null as decfloat(16)) from rdb$database";
			var result = (DBNull)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(DBNull.Value, result);
		}
	}

	[Test]
	public async Task PassesValueNullCorrectly()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(@value as decfloat(16)) from rdb$database";
			cmd.Parameters.AddWithValue("value", DBNull.Value);
			var result = (DBNull)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(DBNull.Value, result);
		}
	}

	static readonly object[] ClrValues = new[]
	{
			new object[] { 2.5m, "25", -1 },
			new object[] { 1.10m, "110", -2 },
			new object[] { -100m, "-100", 0 },
			new object[] { 0.1, "1", -1 },
			new object[] { -6.6, "-66", -1 },
			new object[] { 2.5f, "25", -1 },
			new object[] { 42, "42", 0 },
			new object[] { -42L, "-42", 0 },
			new object[] { (short)7, "7", 0 },
			new object[] { (byte)7, "7", 0 },
			new object[] { 1234567890123456UL, "1234567890123456", 0 },
			new object[] { new BigInteger(123456789), "123456789", 0 },
		};

	[TestCaseSource(nameof(ClrValues))]
	public async Task PassesClrValueCorrectly(object value, string coefficient, int exponent)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(@value as decfloat(16)) from rdb$database";
			cmd.Parameters.AddWithValue("value", value);
			var result = (FbDecFloat)await cmd.ExecuteScalarAsync();
			Assert.AreEqual(new FbDecFloat(BigInteger.Parse(coefficient, CultureInfo.InvariantCulture), exponent), result);
		}
	}

	static readonly object[] UnrepresentableClrValues = new object[]
	{
		ulong.MaxValue,
		decimal.MaxValue,
		0.1 + 0.2,
		1.0 / 3.0 * 1e-300,
	};

	[TestCaseSource(nameof(UnrepresentableClrValues))]
	public async Task PassesUnrepresentableClrValueThrows(object value)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(@value as decfloat(16)) from rdb$database";
			cmd.Parameters.AddWithValue("value", value);
			Assert.ThrowsAsync<OverflowException>(() => cmd.ExecuteScalarAsync());
		}
	}

	[TestCase("1.10", "1.10")]
	[TestCase("-123.456", "-123.456")]
	[TestCase("1E+20", "100000000000000000000")]
	[TestCase("0", "0")]
	[TestCase("0.0000000000009999999999999999", "0.0000000000009999999999999999")]
	public async Task ReadsAsDecimalExactly(string castValue, string expected)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = $"select cast('{castValue}' as decfloat(16)) from rdb$database";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				await reader.ReadAsync();
				Assert.AreEqual(expected, reader.GetDecimal(0).ToString(CultureInfo.InvariantCulture));
				Assert.AreEqual(expected, reader.GetFieldValue<decimal>(0).ToString(CultureInfo.InvariantCulture));
			}
		}
	}

	[TestCase("1E+29")]
	[TestCase("-1E+29")]
	[TestCase("1E-29")]
	[TestCase("nan")]
	[TestCase("inf")]
	[TestCase("1E+300")]
	public async Task ReadsAsDecimalThrowsWhenNotExact(string castValue)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = $"select cast('{castValue}' as decfloat(16)) from rdb$database";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				await reader.ReadAsync();
				Assert.Throws<OverflowException>(() => reader.GetDecimal(0));
			}
		}
	}

	[TestCase("1.10", 1.1)]
	[TestCase("-6.6", -6.6)]
	[TestCase("0.1", 0.1)]
	[TestCase("nan", double.NaN)]
	[TestCase("inf", double.PositiveInfinity)]
	[TestCase("-inf", double.NegativeInfinity)]
	[TestCase("1E-398", 0d)]
	[TestCase("1E+300", 1E+300)]
	public async Task ReadsAsDoubleNearest(string castValue, double expected)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = $"select cast('{castValue}' as decfloat(16)) from rdb$database";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				await reader.ReadAsync();
				Assert.AreEqual(expected, reader.GetDouble(0));
				Assert.AreEqual((float)expected, reader.GetFloat(0));
			}
		}
	}

	[TestCase("123", 123)]
	[TestCase("1E+3", 1000)]
	[TestCase("2.5", 2)]
	[TestCase("3.5", 4)]
	[TestCase("-2.5", -2)]
	[TestCase("-3.5", -4)]
	[TestCase("2.51", 3)]
	[TestCase("1E-300", 0)]
	public async Task ReadsAsIntegerRoundingToEven(string castValue, long expected)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = $"select cast('{castValue}' as decfloat(16)) from rdb$database";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				await reader.ReadAsync();
				Assert.AreEqual(expected, reader.GetInt64(0));
				Assert.AreEqual((int)expected, reader.GetInt32(0));
				Assert.AreEqual((short)expected, reader.GetInt16(0));
			}
		}
	}

	[TestCase("1E+19")]
	[TestCase("nan")]
	[TestCase("-inf")]
	public async Task ReadsAsIntegerThrowsOnOverflow(string castValue)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = $"select cast('{castValue}' as decfloat(16)) from rdb$database";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				await reader.ReadAsync();
				Assert.Throws<OverflowException>(() => reader.GetInt64(0));
			}
		}
	}

	[Test]
	public async Task SelectEmptyResultSet()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(null as decfloat(16)) from rdb$database where 0=1";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				Assert.DoesNotThrowAsync(reader.ReadAsync);
			}
		}
	}

	[Test]
	public async Task SimpleSelectSchemaTableTest()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select cast(null as decfloat(16)) from rdb$database";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				var schema = await reader.GetSchemaTableAsync();
				Assert.AreEqual(typeof(FbDecFloat), schema.Rows[0].ItemArray[5]);
			}
		}
	}
}
