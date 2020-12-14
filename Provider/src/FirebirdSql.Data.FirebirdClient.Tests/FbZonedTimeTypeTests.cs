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
using FirebirdSql.Data.Types;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	public class FbZonedTimeTypeTests
	{
		static readonly object[] SimpleEqualityTrueSource = new object[]
		{
			new object[] { new FbZonedTime(TimeSpan.FromMinutes(142), "UTC"), new FbZonedTime(TimeSpan.FromMinutes(142), "UTC") },
			new object[] { new FbZonedTime(TimeSpan.FromMinutes(142), "UTC"), new FbZonedTime(TimeSpan.FromMinutes(142), "utc") },
		};
		[TestCaseSource(nameof(SimpleEqualityTrueSource))]
		public void EqualityTrue(FbZonedTime expected, FbZonedTime actual)
		{
			Assert.AreEqual(expected, actual);
		}

		static readonly object[] SimpleEqualityFalseSource = new object[]
		{
			new object[] { new FbZonedTime(TimeSpan.FromMinutes(142), "UTC"), new FbZonedTime(TimeSpan.FromMinutes(141), "UTC") },
			new object[] { new FbZonedTime(TimeSpan.FromMinutes(142), "foo"), new FbZonedTime(TimeSpan.FromMinutes(142), "bar") },
		};
		[TestCaseSource(nameof(SimpleEqualityFalseSource))]
		public void EqualityFalse(FbZonedTime expected, FbZonedTime actual)
		{
			Assert.AreNotEqual(expected, actual);
		}
	}
}
