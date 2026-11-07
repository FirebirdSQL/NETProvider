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
using System.Numerics;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.TestsBase;
using FirebirdSql.Data.Types;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[NoServerCategory]
public class DecimalCodecTests
{
	static readonly object[] RepresentableDecFloat16Values = new object[]
	{
		new FbDecFloat(BigInteger.Parse("9999999999999999"), 369),
		new FbDecFloat(BigInteger.Parse("-9999999999999999"), 369),
		new FbDecFloat(1, -398),
		new FbDecFloat(-1, -398),
		// representable only after normalization
		new FbDecFloat(1, 384),
		new FbDecFloat(-1, 384),
		new FbDecFloat(BigInteger.Parse("100000000000000000000"), -20),
		new FbDecFloat(BigInteger.Parse("12345678901234560"), 0),
		new FbDecFloat(100, -400),
		new FbDecFloat(0, 1000),
		new FbDecFloat(0, -1000),
	};
	[TestCaseSource(nameof(RepresentableDecFloat16Values))]
	public void DecFloat16EncodesRepresentableValueExactly(FbDecFloat value)
	{
		var result = DecimalCodec.DecFloat16.ParseBytes(DecimalCodec.DecFloat16.EncodeDecimal(value));
		Assert.AreEqual(value, result);
	}

	static readonly object[] RepresentableDecFloat34Values = new object[]
	{
		new FbDecFloat(BigInteger.Parse("9999999999999999999999999999999999"), 6111),
		new FbDecFloat(1, -6176),
		// representable only after normalization
		new FbDecFloat(1, 6144),
		new FbDecFloat(BigInteger.Parse("12345678901234567890123456789012340"), 0),
		new FbDecFloat(BigInteger.Parse("-12345678901234567890123456789012340000"), -40),
		new FbDecFloat(1000, -6179),
		new FbDecFloat(0, int.MaxValue),
		new FbDecFloat(0, int.MinValue),
	};
	[TestCaseSource(nameof(RepresentableDecFloat34Values))]
	public void DecFloat34EncodesRepresentableValueExactly(FbDecFloat value)
	{
		var result = DecimalCodec.DecFloat34.ParseBytes(DecimalCodec.DecFloat34.EncodeDecimal(value));
		Assert.AreEqual(value, result);
	}

	[Test]
	public void ZeroExponentIsClamped()
	{
		var result = DecimalCodec.DecFloat16.ParseBytes(DecimalCodec.DecFloat16.EncodeDecimal(new FbDecFloat(0, 1000)));
		Assert.AreEqual(369, result.Exponent);
	}

	static readonly object[] UnrepresentableDecFloat16Values = new object[]
	{
		new FbDecFloat(BigInteger.Parse("30000000000000004"), -17),
		new FbDecFloat(BigInteger.Parse("-12345678901234567"), 0),
		new FbDecFloat(1, 385),
		new FbDecFloat(BigInteger.Parse("9999999999999999"), 370),
		new FbDecFloat(1, -399),
		new FbDecFloat(15, -400),
		new FbDecFloat(1, int.MaxValue),
		new FbDecFloat(1, int.MinValue),
	};
	[TestCaseSource(nameof(UnrepresentableDecFloat16Values))]
	public void DecFloat16ThrowsInsteadOfRounding(FbDecFloat value)
	{
		Assert.Throws<OverflowException>(() => DecimalCodec.DecFloat16.EncodeDecimal(value));
	}

	static readonly object[] UnrepresentableDecFloat34Values = new object[]
	{
		new FbDecFloat(BigInteger.Parse("12345678901234567890123456789012345"), 0),
		new FbDecFloat(1, 6145),
		new FbDecFloat(1, -6177),
	};
	[TestCaseSource(nameof(UnrepresentableDecFloat34Values))]
	public void DecFloat34ThrowsInsteadOfRounding(FbDecFloat value)
	{
		Assert.Throws<OverflowException>(() => DecimalCodec.DecFloat34.EncodeDecimal(value));
	}
}
