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

using System;
using System.Globalization;
using System.Numerics;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[NoServerCategory]
public class BigDecimalTests
{
	static readonly object[] FitsDecimalSource = new object[]
	{
		new object[] { "0", 0, 0m },
		new object[] { "0", 30, 0m },
		new object[] { "1234500000", 6, 1234.5m },
		new object[] { "1234567", 6, 1.234567m },
		new object[] { "-1234567", 6, -1.234567m },
		new object[] { "1000000000000000000000000000000", 30, 1m },
		new object[] { "79228162514264337593543950335", 0, decimal.MaxValue },
		new object[] { "-79228162514264337593543950335", 0, decimal.MinValue },
		new object[] { "792281625142643375935439503350", 1, decimal.MaxValue },
		new object[] { "1", 28, 0.0000000000000000000000000001m },
		new object[] { "79228162514264337593543950335", 28, 7.9228162514264337593543950335m },
	};
	[TestCaseSource(nameof(FitsDecimalSource))]
	public void CreatesDecimalWithSmallestScaleWhenExact(string unscaled, int scale, decimal expected)
	{
		var result = BigDecimal.Create(BigInteger.Parse(unscaled, CultureInfo.InvariantCulture), scale);
		Assert.IsInstanceOf<decimal>(result);
		Assert.AreEqual(expected, (decimal)result);
		Assert.AreEqual(expected.Scale, ((decimal)result).Scale);
	}

	static readonly object[] DoesNotFitDecimalSource = new object[]
	{
		new object[] { "79228162514264337593543950336", 0, "79228162514264337593543950336" },
		new object[] { "-79228162514264337593543950336", 0, "-79228162514264337593543950336" },
		new object[] { "1", 29, "0.00000000000000000000000000001" },
		new object[] { "-123", 30, "-0.000000000000000000000000000123" },
		new object[] { "12345678901234567890123456789012345678", 6, "12345678901234567890123456789012.345678" },
		new object[] { "123456789012345678901234567890000", 3, "123456789012345678901234567890" },
	};
	[TestCaseSource(nameof(DoesNotFitDecimalSource))]
	public void CreatesBigDecimalWhenNotExact(string unscaled, int scale, string expected)
	{
		var result = BigDecimal.Create(BigInteger.Parse(unscaled, CultureInfo.InvariantCulture), scale);
		Assert.IsInstanceOf<BigDecimal>(result);
		Assert.AreEqual(expected, ((BigDecimal)result).ToString(CultureInfo.InvariantCulture));
		Assert.Throws<OverflowException>(() => Convert.ToDecimal(result, CultureInfo.InvariantCulture));
		Assert.AreEqual(double.Parse(expected, CultureInfo.InvariantCulture), Convert.ToDouble(result, CultureInfo.InvariantCulture));
		Assert.AreEqual(expected, Convert.ToString(result, CultureInfo.InvariantCulture));
	}

	[Test]
	public void FormatsWithCulture()
	{
		var value = new BigDecimal(BigInteger.Parse("-12345678901234567890123456789012345678"), 6);
		Assert.AreEqual("-12345678901234567890123456789012,345678", value.ToString(new CultureInfo("pt-BR")));
		Assert.AreEqual("-12345678901234567890123456789012.345678", value.ToString(new CultureInfo("en-US")));
	}

	[TestCase("25", 1, 2)]
	[TestCase("35", 1, 4)]
	[TestCase("-25", 1, -2)]
	[TestCase("-35", 1, -4)]
	[TestCase("26", 1, 3)]
	[TestCase("24", 1, 2)]
	[TestCase("123", 0, 123)]
	public void ConvertsToIntegerRoundingToEven(string unscaled, int scale, long expected)
	{
		var value = new BigDecimal(BigInteger.Parse(unscaled, CultureInfo.InvariantCulture), scale);
		Assert.AreEqual(expected, Convert.ToInt64(value, CultureInfo.InvariantCulture));
		Assert.AreEqual((int)expected, Convert.ToInt32(value, CultureInfo.InvariantCulture));
	}

	[Test]
	public void ConvertsToIntegerThrowsOnOverflow()
	{
		var value = new BigDecimal(BigInteger.Parse("12345678901234567890123456789012345678"), 0);
		Assert.Throws<OverflowException>(() => Convert.ToInt64(value, CultureInfo.InvariantCulture));
	}

	[TestCase("12345678901234567890123456789012345678", 0, true, "12345678901234567890123456789012345678")]
	[TestCase("12345678901234567890123456789012000000", 6, true, "12345678901234567890123456789012")]
	[TestCase("12345678901234567890123456789012345678", 6, false, null)]
	public void ConvertsToBigIntegerOnlyWhenIntegral(string unscaled, int scale, bool expectedSuccess, string expected)
	{
		var value = new BigDecimal(BigInteger.Parse(unscaled, CultureInfo.InvariantCulture), scale);
		Assert.AreEqual(expectedSuccess, value.TryToBigInteger(out var result));
		if (expectedSuccess)
		{
			Assert.AreEqual(BigInteger.Parse(expected, CultureInfo.InvariantCulture), result);
		}
	}
}
