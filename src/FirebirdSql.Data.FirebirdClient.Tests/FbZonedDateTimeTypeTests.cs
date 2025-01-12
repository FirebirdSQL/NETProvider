﻿/*
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
using FirebirdSql.Data.TestsBase;
using FirebirdSql.Data.Types;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[NoServerCategory]
public class FbZonedDateTimeTypeTests
{
	static readonly object[] SimpleEqualityTrueSource = new object[]
	{
			new object[] { new FbZonedDateTime(new DateTime(2020, 12, 4, 10, 38, 0, DateTimeKind.Utc), "UTC"), new FbZonedDateTime(new DateTime(2020, 12, 4, 10, 38, 0, DateTimeKind.Utc), "UTC") },
			new object[] { new FbZonedDateTime(new DateTime(2020, 12, 4, 10, 38, 0, DateTimeKind.Utc), "UTC"), new FbZonedDateTime(new DateTime(2020, 12, 4, 10, 38, 0, DateTimeKind.Utc), "utc") },
	};
	[TestCaseSource(nameof(SimpleEqualityTrueSource))]
	public void EqualityTrue(FbZonedDateTime expected, FbZonedDateTime actual)
	{
		Assert.AreEqual(expected, actual);
	}

	static readonly object[] SimpleEqualityFalseSource = new object[]
	{
			new object[] { new FbZonedDateTime(new DateTime(2020, 12, 4, 10, 38, 0, DateTimeKind.Utc), "UTC"), new FbZonedDateTime(new DateTime(2020, 12, 4, 10, 38, 1, DateTimeKind.Utc), "UTC") },
			new object[] { new FbZonedDateTime(new DateTime(2020, 12, 4, 10, 38, 0, DateTimeKind.Utc), "foo"), new FbZonedDateTime(new DateTime(2020, 12, 4, 10, 38, 0, DateTimeKind.Utc), "bar") },
	};
	[TestCaseSource(nameof(SimpleEqualityFalseSource))]
	public void EqualityFalse(FbZonedDateTime expected, FbZonedDateTime actual)
	{
		Assert.AreNotEqual(expected, actual);
	}

	[Test]
	public void ConvertToDateTimeShouldNotThrow()
	{
		var fbZonedDateTime = new FbZonedDateTime(new DateTime(2020, 12, 4, 10, 38, 0, DateTimeKind.Utc), "UTC");
		
		Assert.DoesNotThrow(() => Convert.ChangeType(fbZonedDateTime, typeof(DateTime)));
	}

	public void DateTimeShouldBeUtc()
	{
		Assert.Throws<ArgumentException>(() =>
		{
			new FbZonedDateTime(new DateTime(2020, 12, 4, 10, 38, 0, DateTimeKind.Local), "foo");
		});
	}
}
