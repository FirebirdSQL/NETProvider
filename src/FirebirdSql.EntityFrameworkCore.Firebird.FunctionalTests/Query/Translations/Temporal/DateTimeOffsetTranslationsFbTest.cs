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

//$Authors = Niek Schoemaker (@niekschoemaker)

using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using Microsoft.EntityFrameworkCore.Query.Translations.Temporal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query.Translations.Temporal;

public class DateTimeOffsetTranslationsFbTest : DateTimeOffsetTranslationsTestBase<BasicTypesQueryFbFixture>
{
	public DateTimeOffsetTranslationsFbTest(BasicTypesQueryFbFixture fixture, ITestOutputHelper testOutputHelper)
		: base(fixture)
	{
		Fixture.TestSqlLoggerFactory.Clear();
		Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
	}



	[NotSupportedOnFirebirdFact]
	public override Task Now()
	{
		return base.Now();
	}

	[NotSupportedOnFirebirdFact]
	public override Task UtcNow()
	{
		return base.UtcNow();
	}

	[NotSupportedOnFirebirdFact]
	public override Task AddMilliseconds()
	{
		return base.AddMilliseconds();
	}

	[NotSupportedOnFirebirdFact]
	public override Task AddMinutes()
	{
		return base.AddMinutes();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Date()
	{
		return base.Date();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Day()
	{
		return base.Day();
	}

	[NotSupportedOnFirebirdFact]
	public override Task DayOfYear()
	{
		return base.DayOfYear();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Hour()
	{
		return base.Hour();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Microsecond()
	{
		return base.Microsecond();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Millisecond()
	{
		return base.Millisecond();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Minute()
	{
		return base.Minute();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Month()
	{
		return base.Month();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Nanosecond()
	{
		return base.Nanosecond();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Second()
	{
		return base.Second();
	}

	[NotSupportedOnFirebirdFact]
	public override Task ToUnixTimeMilliseconds()
	{
		return base.ToUnixTimeMilliseconds();
	}

	[NotSupportedOnFirebirdFact]
	public override Task ToUnixTimeSecond()
	{
		return base.ToUnixTimeSecond();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Year()
	{
		return base.Year();
	}
}
