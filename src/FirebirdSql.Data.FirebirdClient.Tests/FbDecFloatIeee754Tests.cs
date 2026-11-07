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
using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.TestsBase;
using FirebirdSql.Data.Types;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[NoServerCategory]
public class FbDecFloatIeee754Tests
{
	static BigInteger RandomCoefficient(Random random, int maxDigits)
	{
		var digits = random.Next(0, maxDigits + 1);
		if (digits == 0)
			return BigInteger.Zero;
		var chars = new char[digits];
		for (var i = 0; i < digits; i++)
			chars[i] = (char)('0' + random.Next(10));
		var result = BigInteger.Parse(new string(chars), CultureInfo.InvariantCulture);
		return random.Next(2) == 0 ? result : -result;
	}

	// The provider's DECFLOAT codec and .NET's IEEE 754 decimal interchange encoding (EncodeDecimal/DecodeDecimal)
	// must agree bit for bit; the Decimal64/Decimal128 support relies on it.
	[Test]
	public void DecFloat34CodecMatchesDecimal128Encoding()
	{
		var random = new Random(20261107);
		for (var i = 0; i < 20_000; i++)
		{
			var value = new FbDecFloat(RandomCoefficient(random, 34), random.Next(-6176, 6111 + 1));
			var bits = BinaryPrimitives.ReadUInt128LittleEndian(DecimalCodec.DecFloat34.EncodeDecimal(value));
			var d = (Decimal128)value;
			Assert.AreEqual(bits, Decimal128.EncodeDecimal(d), value.ToString());
			var expected = Decimal128.Parse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
			Assert.IsTrue(d.Equals(expected) && Decimal128.HaveSameQuantum(d, expected), value.ToString());
			var back = (FbDecFloat)d;
			Assert.AreEqual(value.Coefficient, back.Coefficient, value.ToString());
			Assert.AreEqual(value.Exponent, back.Exponent, value.ToString());
		}
	}

	[Test]
	public void DecFloat16CodecMatchesDecimal64Encoding()
	{
		var random = new Random(20261107);
		for (var i = 0; i < 20_000; i++)
		{
			var value = new FbDecFloat(RandomCoefficient(random, 16), random.Next(-398, 369 + 1));
			var bits = BinaryPrimitives.ReadUInt64LittleEndian(DecimalCodec.DecFloat16.EncodeDecimal(value));
			var d = (Decimal64)value;
			Assert.AreEqual(bits, Decimal64.EncodeDecimal(d), value.ToString());
			var expected = Decimal64.Parse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
			Assert.IsTrue(d.Equals(expected) && Decimal64.HaveSameQuantum(d, expected), value.ToString());
			var back = (FbDecFloat)d;
			Assert.AreEqual(value.Coefficient, back.Coefficient, value.ToString());
			Assert.AreEqual(value.Exponent, back.Exponent, value.ToString());
		}
	}

	static readonly object[] SpecialValues = new object[]
	{
		FbDecFloat.NegativeZero,
		FbDecFloat.PositiveInfinity,
		FbDecFloat.NegativeInfinity,
		FbDecFloat.PositiveNaN,
		FbDecFloat.NegativeNaN,
		FbDecFloat.PositiveSignalingNaN,
		FbDecFloat.NegativeSignalingNaN,
	};
	[TestCaseSource(nameof(SpecialValues))]
	public void ConvertsSpecialValuesBitExactly(FbDecFloat value)
	{
		var bits128 = BinaryPrimitives.ReadUInt128LittleEndian(DecimalCodec.DecFloat34.EncodeDecimal(value));
		Assert.AreEqual(bits128, Decimal128.EncodeDecimal((Decimal128)value));
		Assert.AreEqual(value, (FbDecFloat)(Decimal128)value);
		var bits64 = BinaryPrimitives.ReadUInt64LittleEndian(DecimalCodec.DecFloat16.EncodeDecimal(value));
		Assert.AreEqual(bits64, Decimal64.EncodeDecimal((Decimal64)value));
		Assert.AreEqual(value, (FbDecFloat)(Decimal64)value);
	}

	[Test]
	public void ConvertsSpecialDecimal128Values()
	{
		Assert.AreEqual(FbDecFloat.NegativeZero, (FbDecFloat)Decimal128.NegativeZero);
		Assert.AreEqual(FbDecFloat.PositiveInfinity, (FbDecFloat)Decimal128.PositiveInfinity);
		Assert.AreEqual(FbDecFloat.NegativeInfinity, (FbDecFloat)Decimal128.NegativeInfinity);
		Assert.IsTrue(Decimal128.IsNaN((Decimal128)FbDecFloat.PositiveNaN));
		Assert.IsTrue(Decimal128.IsNegative((Decimal128)FbDecFloat.NegativeZero));
	}

	[Test]
	public void ConvertsDecimal32ExactlyThroughDecimal64()
	{
		var value = Decimal32.Parse("1.10", CultureInfo.InvariantCulture);
		var result = (FbDecFloat)value;
		Assert.AreEqual(new BigInteger(110), result.Coefficient);
		Assert.AreEqual(-2, result.Exponent);
	}

	static readonly object[] NotRepresentableAsDecimal64 = new object[]
	{
		new FbDecFloat(BigInteger.Parse("12345678901234567"), 0),
		new FbDecFloat(1, 385),
		new FbDecFloat(1, -399),
	};
	[TestCaseSource(nameof(NotRepresentableAsDecimal64))]
	public void ConvertingToDecimal64ThrowsInsteadOfRounding(FbDecFloat value)
	{
		Assert.Throws<OverflowException>(() => _ = (Decimal64)value);
	}

	static readonly object[] NotRepresentableAsDecimal128 = new object[]
	{
		new FbDecFloat(BigInteger.Parse("12345678901234567890123456789012345"), 0),
		new FbDecFloat(1, 6145),
		new FbDecFloat(1, -6177),
	};
	[TestCaseSource(nameof(NotRepresentableAsDecimal128))]
	public void ConvertingToDecimal128ThrowsInsteadOfRounding(FbDecFloat value)
	{
		Assert.Throws<OverflowException>(() => _ = (Decimal128)value);
	}

	[Test]
	public void ConvertsRepresentableValueToDecimal64AfterNormalization()
	{
		// 17 digits, but the last one is a trailing zero
		var value = new FbDecFloat(BigInteger.Parse("12345678901234560"), 0);
		Assert.AreEqual(Decimal64.Parse("12345678901234560", CultureInfo.InvariantCulture), (Decimal64)value);
	}
}

#endif
