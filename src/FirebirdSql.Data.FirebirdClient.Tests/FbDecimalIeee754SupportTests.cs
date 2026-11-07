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

//$Authors = F.D.Castel

#if NET11_0_OR_GREATER

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
public class FbDecimalIeee754SupportTests : FbTestsBase
{
	public FbDecimalIeee754SupportTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
		: base(serverType, compression, wireCrypt, false)
	{ }

	[SetUp]
	public override async Task SetUp()
	{
		await base.SetUp();

		if (!EnsureServerVersionAtLeast(new Version(4, 0, 0, 0)))
			return;
	}

	static Decimal128 D128(string s) => Decimal128.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
	static Decimal64 D64(string s) => Decimal64.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);

	static void AssertSameValueAndExponent(Decimal128 expected, Decimal128 actual)
	{
		Assert.AreEqual(Decimal128.EncodeDecimal(expected), Decimal128.EncodeDecimal(actual), $"expected {expected.ToString(CultureInfo.InvariantCulture)}, was {actual.ToString(CultureInfo.InvariantCulture)}");
	}

	static void AssertSameValueAndExponent(Decimal64 expected, Decimal64 actual)
	{
		Assert.AreEqual(Decimal64.EncodeDecimal(expected), Decimal64.EncodeDecimal(actual), $"expected {expected.ToString(CultureInfo.InvariantCulture)}, was {actual.ToString(CultureInfo.InvariantCulture)}");
	}

	async Task<T> SelectAsync<T>(string sql)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = sql;
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				Assert.IsTrue(await reader.ReadAsync());
				return await reader.GetFieldValueAsync<T>(0);
			}
		}
	}

	async Task<T> RoundTripAsync<T>(string castAs, object value)
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = $"select cast(@value as {castAs}) from rdb$database";
			cmd.Parameters.AddWithValue("value", value);
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				Assert.IsTrue(await reader.ReadAsync());
				return reader.GetFieldValue<T>(0);
			}
		}
	}

	[TestCase("0")]
	[TestCase("1.10")]
	[TestCase("-123.456")]
	[TestCase("1234567890123456789012345678901234")]
	[TestCase("1.234567890123456789012345678901234E-100")]
	[TestCase("9.999999999999999999999999999999999E+6144")]
	[TestCase("1E-6176")]
	[TestCase("0.00000")]
	[TestCase("-0")]
	[TestCase("Infinity")]
	[TestCase("-Infinity")]
	public async Task ReadsDecFloat34AsDecimal128(string literal)
	{
		var result = await SelectAsync<Decimal128>($"select cast('{literal}' as decfloat(34)) from rdb$database");
		AssertSameValueAndExponent(D128(literal), result);
	}

	[Test]
	public async Task ReadsDecFloat34NaNAsDecimal128()
	{
		Assert.IsTrue(Decimal128.IsNaN(await SelectAsync<Decimal128>("select cast('NaN' as decfloat(34)) from rdb$database")));
		Assert.IsTrue(Decimal128.IsNaN(await SelectAsync<Decimal128>("select cast('-sNaN' as decfloat(34)) from rdb$database")));
	}

	[TestCase("1.10")]
	[TestCase("1234567890123456")]
	[TestCase("9.999999999999999E+384")]
	[TestCase("1E-398")]
	[TestCase("-0")]
	[TestCase("-Infinity")]
	public async Task ReadsDecFloat16AsDecimal64AndDecimal128(string literal)
	{
		AssertSameValueAndExponent(D64(literal), await SelectAsync<Decimal64>($"select cast('{literal}' as decfloat(16)) from rdb$database"));
		AssertSameValueAndExponent(D128(literal), await SelectAsync<Decimal128>($"select cast('{literal}' as decfloat(16)) from rdb$database"));
	}

	[TestCase("1.5")]
	[TestCase("1234567890123456")]
	[TestCase("12345678901234560")]
	public async Task ReadsDecFloat34AsDecimal64WhenExact(string literal)
	{
		var result = await SelectAsync<Decimal64>($"select cast('{literal}' as decfloat(34)) from rdb$database");
		Assert.AreEqual(D64(literal), result);
	}

	[TestCase("12345678901234567")]
	[TestCase("1E+385")]
	[TestCase("1E-399")]
	public void ReadsDecFloat34AsDecimal64ThrowsWhenNotExact(string literal)
	{
		Assert.ThrowsAsync<OverflowException>(() => SelectAsync<Decimal64>($"select cast('{literal}' as decfloat(34)) from rdb$database"));
	}

	[TestCase("cast(1.10 as numeric(18, 2))", "1.1")]
	[TestCase("cast(-123456789012.345678 as numeric(18, 6))", "-123456789012.345678")]
	[TestCase("cast(42 as integer)", "42")]
	[TestCase("cast(-9223372036854775808 as bigint)", "-9223372036854775808")]
	[TestCase("cast(0.1 as double precision)", "0.1")]
	public async Task ReadsOtherNumericTypesAsDecimal128(string expression, string expected)
	{
		Assert.AreEqual(D128(expected), await SelectAsync<Decimal128>($"select {expression} from rdb$database"));
	}

	[Test]
	public void ReadsInt128BeyondDecimal128Throws()
	{
		Assert.ThrowsAsync<OverflowException>(() => SelectAsync<Decimal128>("select cast(170141183460469231731687303715884105727 as int128) from rdb$database"));
	}

	[TestCase("1.10")]
	[TestCase("-123.456")]
	[TestCase("1234567890123456789012345678901234")]
	[TestCase("9.999999999999999999999999999999999E+6144")]
	[TestCase("1E-6176")]
	[TestCase("0.00000")]
	[TestCase("-0")]
	[TestCase("Infinity")]
	[TestCase("-Infinity")]
	public async Task PassesDecimal128ToDecFloat34(string literal)
	{
		var value = D128(literal);
		Assert.AreEqual(FbDbType.Dec34, new FbParameter("value", value).FbDbType);
		AssertSameValueAndExponent(value, await RoundTripAsync<Decimal128>("decfloat(34)", value));
	}

	[Test]
	public async Task PassesDecimal128NaNToDecFloat34()
	{
		Assert.IsTrue(Decimal128.IsNaN(await RoundTripAsync<Decimal128>("decfloat(34)", Decimal128.NaN)));
	}

	[TestCase("1.10")]
	[TestCase("1234567890123456")]
	[TestCase("-9.999999999999999E+384")]
	[TestCase("1E-398")]
	[TestCase("-0")]
	public async Task PassesDecimal64ToDecFloat16(string literal)
	{
		var value = D64(literal);
		Assert.AreEqual(FbDbType.Dec16, new FbParameter("value", value).FbDbType);
		AssertSameValueAndExponent(value, await RoundTripAsync<Decimal64>("decfloat(16)", value));
	}

	[Test]
	public async Task PassesDecimal32ToDecFloat16()
	{
		var value = Decimal32.Parse("1.10", CultureInfo.InvariantCulture);
		Assert.AreEqual(FbDbType.Dec16, new FbParameter("value", value).FbDbType);
		AssertSameValueAndExponent(D64("1.10"), await RoundTripAsync<Decimal64>("decfloat(16)", value));
	}

	[TestCase("1.5")]
	[TestCase("1234567890123456")]
	[TestCase("12345678901234560")]
	public async Task PassesDecimal128ToDecFloat16WhenExact(string literal)
	{
		var result = await RoundTripAsync<Decimal128>("decfloat(16)", D128(literal));
		Assert.AreEqual(D128(literal), result);
	}

	[TestCase("12345678901234567")]
	[TestCase("1234567890123456789012345678901234")]
	[TestCase("1E+385")]
	[TestCase("1E-399")]
	public async Task PassesDecimal128ToDecFloat16ThrowsWhenNotExact(string literal)
	{
		Assert.ThrowsAsync<OverflowException>(() => RoundTripAsync<Decimal128>("decfloat(16)", D128(literal)));
		Assert.AreEqual(D128("1"), await SelectAsync<Decimal128>("select cast(1 as decfloat(16)) from rdb$database"), "connection must remain usable");
	}

	[TestCase("1.2345", "numeric(18, 4)", "1.2345")]
	[TestCase("42", "integer", "42")]
	[TestCase("0.5", "double precision", "0.5")]
	public async Task PassesDecimal128ToOtherNumericTypes(string literal, string type, string expected)
	{
		Assert.AreEqual(D128(expected), await RoundTripAsync<Decimal128>(type, D128(literal)));
	}

	[Test]
	public async Task InsertsAndReadsBackDecimal128()
	{
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "recreate table decimal128_test (id integer, d16 decfloat(16), d34 decfloat(34))";
			await cmd.ExecuteNonQueryAsync();
		}
		var d34 = D128("1234567890123456789012345678.901234");
		var d16 = D64("-1.100");
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "insert into decimal128_test (id, d16, d34) values (1, @d16, @d34)";
			cmd.Parameters.AddWithValue("d16", d16);
			cmd.Parameters.AddWithValue("d34", d34);
			Assert.AreEqual(1, await cmd.ExecuteNonQueryAsync());
		}
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "select d16, d34 from decimal128_test where id = 1";
			await using (var reader = await cmd.ExecuteReaderAsync())
			{
				Assert.IsTrue(await reader.ReadAsync());
				AssertSameValueAndExponent(d16, reader.GetFieldValue<Decimal64>(0));
				AssertSameValueAndExponent(d34, reader.GetFieldValue<Decimal128>(1));
				// the default CLR type is unchanged
				Assert.AreEqual(typeof(FbDecFloat), reader.GetFieldType(1));
				Assert.IsInstanceOf<FbDecFloat>(reader.GetValue(1));
			}
		}
		await using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = "drop table decimal128_test";
			await cmd.ExecuteNonQueryAsync();
		}
	}
}

#endif
