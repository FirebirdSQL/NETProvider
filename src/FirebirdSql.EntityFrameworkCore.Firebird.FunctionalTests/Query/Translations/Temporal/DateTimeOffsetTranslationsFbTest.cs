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

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query.Translations.Temporal;

public class DateTimeOffsetTranslationsFbTest : DateTimeOffsetTranslationsTestBase<BasicTypesQueryFbFixture>
{
	public DateTimeOffsetTranslationsFbTest(BasicTypesQueryFbFixture fixture, ITestOutputHelper testOutputHelper)
		: base(fixture)
	{
		Fixture.TestSqlLoggerFactory.Clear();
		Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
	}



	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Now()
	{
		return base.Now();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task UtcNow()
	{
		return base.UtcNow();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task AddMilliseconds()
	{
		return base.AddMilliseconds();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task AddMinutes()
	{
		return base.AddMinutes();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Date()
	{
		return base.Date();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Day()
	{
		return base.Day();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task DayOfYear()
	{
		return base.DayOfYear();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Hour()
	{
		return base.Hour();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Microsecond()
	{
		return base.Microsecond();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Millisecond()
	{
		return base.Millisecond();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Minute()
	{
		return base.Minute();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Month()
	{
		return base.Month();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Nanosecond()
	{
		return base.Nanosecond();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Second()
	{
		return base.Second();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task ToUnixTimeMilliseconds()
	{
		return base.ToUnixTimeMilliseconds();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task ToUnixTimeSecond()
	{
		return base.ToUnixTimeSecond();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Year()
	{
		return base.Year();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Ctor_DateTime_TimeSpan()
	{
		return base.Ctor_DateTime_TimeSpan();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Ctor_DateTime()
	{
		return base.Ctor_DateTime();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task DateTime()
	{
		return base.DateTime();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task LocalDateTime()
	{
		return base.LocalDateTime();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task ToOffset()
	{
		return base.ToOffset();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task UtcDateTime()
	{
		return base.UtcDateTime();
	}
}
