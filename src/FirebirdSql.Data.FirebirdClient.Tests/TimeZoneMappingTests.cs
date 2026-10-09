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

using FirebirdSql.Data.Common;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[NoServerCategory]
public class TimeZoneMappingTests
{
	[TestCase((ushort)599, "-14:00")]
	[TestCase((ushort)1439, "+00:00")]
	[TestCase((ushort)1499, "+01:00")]
	[TestCase((ushort)2279, "+14:00")]
	[TestCase((ushort)65059, "Europe/Prague")]
	public void TryGetById(ushort id, string expected)
	{
		Assert.IsTrue(TimeZoneMapping.TryGetById(id, out var name));
		Assert.AreEqual(expected, name);
	}

	[TestCase("+01:00", (ushort)1499)]
	[TestCase("Europe/Prague", (ushort)65059)]
	[TestCase("europe/prague", (ushort)65059)]
	public void TryGetByName(string name, ushort expected)
	{
		Assert.IsTrue(TimeZoneMapping.TryGetByName(name, out var id));
		Assert.AreEqual(expected, id);
	}

	[TestCase("+01:00", "+01:00")]
	[TestCase("+1:00", "+01:00")]
	[TestCase("+1", "+01:00")]
	[TestCase("-5:0", "-05:00")]
	[TestCase(" - 05 : 30 ", "-05:30")]
	[TestCase("-00:00", "+00:00")]
	[TestCase("+001:00", "+01:00")]
	[TestCase("-14:00", "-14:00")]
	[TestCase("europe/prague", "Europe/Prague")]
	[TestCase("+14:01", "+14:01")]
	[TestCase("+15:00", "+15:00")]
	[TestCase("+01:60", "+01:60")]
	[TestCase("+99999999999:00", "+99999999999:00")]
	[TestCase("+01:00x", "+01:00x")]
	[TestCase("Europe/Nowhere", "Europe/Nowhere")]
	public void Normalize(string name, string expected)
	{
		Assert.AreEqual(expected, TimeZoneMapping.Normalize(name));
	}
}
