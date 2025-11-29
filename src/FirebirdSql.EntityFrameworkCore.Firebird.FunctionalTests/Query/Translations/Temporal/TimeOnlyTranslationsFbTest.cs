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

public class TimeOnlyTranslationsFbTest : TimeOnlyTranslationsTestBase<BasicTypesQueryFbFixture>
{
	public TimeOnlyTranslationsFbTest(BasicTypesQueryFbFixture fixture, ITestOutputHelper testOutputHelper)
		: base(fixture)
	{
		Fixture.TestSqlLoggerFactory.Clear();
		Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
	}

	[NotSupportedByProviderFact]
	public override Task Add_TimeSpan()
	{
		return base.Add_TimeSpan();
	}

	[NotSupportedByProviderFact]
	public override Task IsBetween()
	{
		return base.IsBetween();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Microsecond()
	{
		return base.Microsecond();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Nanosecond()
	{
		return base.Nanosecond();
	}

	[NotSupportedByProviderFact]
	public override Task Subtract()
	{
		return base.Subtract();
	}
}
