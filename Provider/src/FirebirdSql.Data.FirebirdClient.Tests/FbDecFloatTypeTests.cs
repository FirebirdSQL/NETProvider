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

using System.Numerics;
using FirebirdSql.Data.Types;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
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
			new object[] { FbDecFloat.PositiveInfinity, FbDecFloat.PositiveInfinity },
			new object[] { FbDecFloat.NegativeInfinity, FbDecFloat.NegativeInfinity },
			new object[] { FbDecFloat.PositiveNaN, FbDecFloat.PositiveNaN },
		};
		[TestCaseSource(nameof(SimpleEqualityTrueSource))]
		public void EqualityTrue(FbDecFloat expected, FbDecFloat actual)
		{
			Assert.AreEqual(expected, actual);
		}

		static readonly object[] SimpleEqualityFalseSource = new object[]
		{
			new object[] { new FbDecFloat(0), new FbDecFloat(BigInteger.Parse("986767875675879890678765756798079808709")) },
			new object[] { new FbDecFloat(6, 3), new FbDecFloat(-6, 3) },
			new object[] { new FbDecFloat(6, 3), new FbDecFloat(6, -3) },
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
	}
}
