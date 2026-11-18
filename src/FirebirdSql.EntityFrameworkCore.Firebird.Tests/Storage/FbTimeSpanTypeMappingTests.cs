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

//$Authors = Niek Schoemaker

using System;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using NUnit.Framework;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests.Storage;

public class FbTimeSpanTypeMappingTests
{
	[Test]
	public void GeneratesTimeLiteral()
	{
		var mapping = new FbTimeSpanTypeMapping("TIME", FbDbType.Time);

		Assert.AreEqual("CAST('07:08:09.1234' AS TIME)", mapping.GenerateSqlLiteral(new TimeSpan(0, 7, 8, 9, 123) + TimeSpan.FromTicks(4000)));
	}
}
