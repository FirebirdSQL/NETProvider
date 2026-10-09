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

public class DateTimeTranslationsFbTest : DateTimeTranslationsTestBase<BasicTypesQueryFbFixture>
{
	public DateTimeTranslationsFbTest(BasicTypesQueryFbFixture fixture, ITestOutputHelper testOutputHelper)
		: base(fixture)
	{
		Fixture.TestSqlLoggerFactory.Clear();
		Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
	}



	[NotSupportedOnFirebirdFact]
	public override Task UtcNow()
	{
		return base.UtcNow();
	}

	[NotSupportedByProviderFact]
	public override Task subtract_and_TotalDays()
	{
	 	return base.subtract_and_TotalDays();
	}

	[NotSupportedByProviderFact]
	public override Task TimeOfDay()
	{
		return base.TimeOfDay();
	}
}
