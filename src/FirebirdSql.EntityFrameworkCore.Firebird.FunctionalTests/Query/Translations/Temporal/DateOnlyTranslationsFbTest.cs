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

public class DateOnlyTranslationsFbTest : DateOnlyTranslationsTestBase<BasicTypesQueryFbFixture>
{
	public DateOnlyTranslationsFbTest(BasicTypesQueryFbFixture fixture, ITestOutputHelper testOutputHelper)
		: base(fixture)
	{
		Fixture.TestSqlLoggerFactory.Clear();
		Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
	}

	[NotSupportedByProviderFact]
	public override Task DayNumber()
	{
		return base.DayNumber();
	}

	[NotSupportedByProviderFact]
	public override Task DayNumber_subtraction()
	{
		return base.DayNumber_subtraction();
	}

	[NotSupportedByProviderFact]
	public override Task ToDateTime_constant_DateTime_with_property_TimeOnly()
	{
		return base.ToDateTime_constant_DateTime_with_property_TimeOnly();
	}

	[NotSupportedByProviderFact]
	public override Task ToDateTime_property_with_constant_TimeOnly()
	{
		return base.ToDateTime_property_with_constant_TimeOnly();
	}

	[NotSupportedByProviderFact]
	public override Task ToDateTime_property_with_property_TimeOnly()
	{
		return base.ToDateTime_property_with_property_TimeOnly();
	}

	[NotSupportedByProviderFact]
	public override Task ToDateTime_with_complex_DateTime()
	{
		return base.ToDateTime_with_complex_DateTime();
	}

	[NotSupportedByProviderFact]
	public override Task ToDateTime_with_complex_TimeOnly()
	{
		return base.ToDateTime_with_complex_TimeOnly();
	}
}
