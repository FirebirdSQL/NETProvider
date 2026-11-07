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
using FirebirdSql.Data.TestsBase;
using FirebirdSql.Data.Types;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[NoServerCategory]
public class FbDecFloatTypeTests
{
	static readonly object[] SimpleEqualityTrueSource = new object[]
	{
			new object[] { new FbDecFloat(0), new FbDecFloat(0) },
			new object[] { new FbDecFloat(10, 0), new FbDecFloat(10, 0) },
			new object[] { new FbDecFloat(6, 3), new FbDecFloat(6, 3) },
			new object[] { new FbDecFloat(6, -3), new FbDecFloat(6, -3) },
			new object[] { new FbDecFloat(-6, 3), new FbDecFloat(-6, 3) },
			new object[] { new FbDecFloat(-6, -3), new FbDecFloat(-6, -3) },
			new object[] { new FbDecFloat(BigInteger.Parse("986767875675879890678765756798079808709"), 1), new FbDecFloat(BigInteger.Parse("986767875675879890678765756798079808709"), 1) },
			new object[] { new FbDecFloat(10, 3), new FbDecFloat(100, 2) },
			new object[] { new FbDecFloat(-10, 3), new FbDecFloat(-100, 2) },
			new object[] { new FbDecFloat(10, -3), new FbDecFloat(1, -2) },
			new object[] { new FbDecFloat(-10, -3), new FbDecFloat(-1, -2) },
			new object[] { new FbDecFloat(0, 0), new FbDecFloat(0, 5) },
			new object[] { FbDecFloat.PositiveInfinity, FbDecFloat.PositiveInfinity },
			new object[] { FbDecFloat.NegativeInfinity, FbDecFloat.NegativeInfinity },
			new object[] { FbDecFloat.PositiveNaN, FbDecFloat.PositiveNaN },
	};
	[TestCaseSource(nameof(SimpleEqualityTrueSource))]
	public void EqualityTrue(FbDecFloat expected, FbDecFloat actual)
	{
		Assert.AreEqual(expected, actual);
	}

	[TestCaseSource(nameof(SimpleEqualityTrueSource))]
	public void HashCodeEqual(FbDecFloat expected, FbDecFloat actual)
	{
		Assert.AreEqual(expected.GetHashCode(), actual.GetHashCode());
	}

	static readonly object[] SimpleEqualityFalseSource = new object[]
	{
			new object[] { new FbDecFloat(0), new FbDecFloat(BigInteger.Parse("986767875675879890678765756798079808709")) },
			new object[] { new FbDecFloat(6, 3), new FbDecFloat(-6, 3) },
			new object[] { new FbDecFloat(6, 3), new FbDecFloat(6, -3) },
			new object[] { new FbDecFloat(1, 0), new FbDecFloat(1, 1_000_000_000) },
			new object[] { new FbDecFloat(1, int.MaxValue), new FbDecFloat(1, int.MinValue) },
			new object[] { FbDecFloat.PositiveInfinity, FbDecFloat.NegativeInfinity },
			new object[] { FbDecFloat.PositiveNaN, FbDecFloat.NegativeNaN },
			new object[] { FbDecFloat.PositiveInfinity, FbDecFloat.PositiveNaN },
			new object[] { FbDecFloat.NegativeInfinity, FbDecFloat.NegativeNaN },
	};
	[TestCaseSource(nameof(SimpleEqualityFalseSource))]
	public void EqualityFalse(FbDecFloat expected, FbDecFloat actual)
	{
		Assert.AreNotEqual(expected, actual);
	}

	static readonly object[] DecimalConversionSource = new object[]
	{
			new object[] { 0m, "0", 0 },
			new object[] { 1m, "1", 0 },
			new object[] { 100m, "100", 0 },
			new object[] { -5m, "-5", 0 },
			new object[] { 1.5m, "15", -1 },
			new object[] { 1.10m, "110", -2 },
			new object[] { -0.001m, "-1", -3 },
			new object[] { decimal.MaxValue, "79228162514264337593543950335", 0 },
			new object[] { decimal.MinValue, "-79228162514264337593543950335", 0 },
			new object[] { 0.0000000000000000000000000001m, "1", -28 },
			new object[] { -7.9228162514264337593543950335m, "-79228162514264337593543950335", -28 },
	};
	[TestCaseSource(nameof(DecimalConversionSource))]
	public void ConvertsDecimalExactly(decimal value, string coefficient, int exponent)
	{
		var result = (FbDecFloat)value;
		Assert.AreEqual(BigInteger.Parse(coefficient), result.Coefficient);
		Assert.AreEqual(exponent, result.Exponent);
	}

	static readonly object[] DoubleConversionSource = new object[]
	{
			new object[] { 0d, "0", 0 },
			new object[] { 2d, "2", 0 },
			new object[] { 100d, "100", 0 },
			new object[] { 0.1, "1", -1 },
			new object[] { -6.6, "-66", -1 },
			new object[] { 1e-20, "1", -20 },
			new object[] { 1e300, "1", 300 },
			new object[] { 0.1 + 0.2, "30000000000000004", -17 },
			new object[] { 123456789.123456789, "12345678912345679", -8 },
			new object[] { double.MaxValue, "17976931348623157", 292 },
			new object[] { double.MinValue, "-17976931348623157", 292 },
			new object[] { double.Epsilon, "5", -324 },
	};
	[TestCaseSource(nameof(DoubleConversionSource))]
	public void ConvertsDoubleToShortestRoundTripValue(double value, string coefficient, int exponent)
	{
		var result = (FbDecFloat)value;
		Assert.AreEqual(BigInteger.Parse(coefficient), result.Coefficient);
		Assert.AreEqual(exponent, result.Exponent);
	}

	static readonly object[] SingleConversionSource = new object[]
	{
			new object[] { 2f, "2", 0 },
			new object[] { 0.1f, "1", -1 },
			new object[] { -6.6f, "-66", -1 },
			new object[] { 1e-20f, "1", -20 },
			new object[] { float.MaxValue, "34028235", 31 },
			new object[] { float.Epsilon, "1", -45 },
	};
	[TestCaseSource(nameof(SingleConversionSource))]
	public void ConvertsSingleToShortestRoundTripValue(float value, string coefficient, int exponent)
	{
		var result = (FbDecFloat)value;
		Assert.AreEqual(BigInteger.Parse(coefficient), result.Coefficient);
		Assert.AreEqual(exponent, result.Exponent);
	}

	[Test]
	public void ConvertsNegativeZero()
	{
		Assert.AreEqual(FbDecFloat.NegativeZero, (FbDecFloat)(-0d));
		Assert.AreEqual(FbDecFloat.NegativeZero, (FbDecFloat)(-0f));
		Assert.AreEqual(FbDecFloat.NegativeZero, (FbDecFloat)decimal.Negate(0m));
		Assert.AreNotEqual(FbDecFloat.NegativeZero, (FbDecFloat)0d);
		Assert.AreNotEqual(FbDecFloat.NegativeZero, (FbDecFloat)0m);
	}

	[Test]
	public void ConvertsSpecialValues()
	{
		Assert.AreEqual(FbDecFloat.PositiveNaN, (FbDecFloat)double.NaN);
		Assert.AreEqual(FbDecFloat.PositiveInfinity, (FbDecFloat)double.PositiveInfinity);
		Assert.AreEqual(FbDecFloat.NegativeInfinity, (FbDecFloat)double.NegativeInfinity);
		Assert.AreEqual(FbDecFloat.PositiveNaN, (FbDecFloat)float.NaN);
		Assert.AreEqual(FbDecFloat.PositiveInfinity, (FbDecFloat)float.PositiveInfinity);
		Assert.AreEqual(FbDecFloat.NegativeInfinity, (FbDecFloat)float.NegativeInfinity);
	}

	[Test]
	public void DoubleAndSingleConversionsRoundTrip()
	{
		var random = new Random(20261107);
		var negativeDoubles = 0;
		var negativeSingles = 0;
		for (var i = 0; i < 10_000; i++)
		{
			// random bit patterns, so all exponents and both signs are covered
			var d = BitConverter.Int64BitsToDouble(random.NextInt64(long.MinValue, long.MaxValue));
			if (double.IsFinite(d))
			{
				var result = (FbDecFloat)d;
				Assert.AreEqual(d, double.Parse(result.ToString(), CultureInfo.InvariantCulture), $"{d:R} -> {result}");
				negativeDoubles += d < 0 ? 1 : 0;
			}
			var f = BitConverter.Int32BitsToSingle(random.Next(int.MinValue, int.MaxValue));
			if (float.IsFinite(f))
			{
				var result = (FbDecFloat)f;
				Assert.AreEqual(f, float.Parse(result.ToString(), CultureInfo.InvariantCulture), $"{f:R} -> {result}");
				negativeSingles += f < 0 ? 1 : 0;
			}
		}
		Assert.Greater(negativeDoubles, 1000);
		Assert.Greater(negativeSingles, 1000);
	}
}
