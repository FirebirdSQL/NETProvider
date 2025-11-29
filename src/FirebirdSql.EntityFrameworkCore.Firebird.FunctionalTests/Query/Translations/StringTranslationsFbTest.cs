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
using Microsoft.EntityFrameworkCore.Query.Translations;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query.Translations;

public class StringTranslationsFbTest : StringTranslationsRelationalTestBase<BasicTypesQueryFbFixture>
{
	public StringTranslationsFbTest(BasicTypesQueryFbFixture fixture, ITestOutputHelper testOutputHelper)
		: base(fixture)
	{
		Fixture.TestSqlLoggerFactory.Clear();
		Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
	}

	protected override void ClearLog()
	{
		Fixture.TestSqlLoggerFactory.Clear();
	}

	[NotSupportedOnFirebirdFact]
	public override Task TrimStart_with_char_array_argument()
	{
		return base.TrimStart_with_char_array_argument();
	}

	[NotSupportedOnFirebirdFact]
	public override Task TrimEnd_with_char_array_argument()
	{
		return base.TrimEnd_with_char_array_argument();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Trim_with_char_array_argument_in_predicate()
	{
		return base.Trim_with_char_array_argument_in_predicate();
	}

	[NotSupportedByProviderFact]
	public override Task Regex_IsMatch()
	{
		return base.Regex_IsMatch();
	}

	[NotSupportedByProviderFact]
	public override Task Regex_IsMatch_constant_input()
	{
		return base.Regex_IsMatch_constant_input();
	}

	[ConditionalFact]
	public override Task Join_non_aggregate()
	{
		return AssertTranslationFailed(() => base.Join_non_aggregate());
	}

	[NotSupportedByProviderFact]
	public override Task Contains_Literal_Char()
	{
		return base.Contains_Literal_Char();
	}

	[NotSupportedByProviderFact]
	public override Task Contains_with_StringComparison_Ordinal()
	{
		return base.Contains_with_StringComparison_Ordinal();
	}

	[NotSupportedByProviderFact]
	public override Task Contains_with_StringComparison_OrdinalIgnoreCase()
	{
		return base.Contains_with_StringComparison_OrdinalIgnoreCase();
	}

	[NotSupportedByProviderFact]
	public override Task EndsWith_Literal_Char()
	{
		return base.EndsWith_Literal_Char();
	}

	[NotSupportedByProviderFact]
	public override Task EndsWith_Parameter_Char()
	{
		return base.EndsWith_Parameter_Char();
	}

	[NotSupportedByProviderFact]
	public override Task EndsWith_with_StringComparison_Ordinal()
	{
		return base.EndsWith_with_StringComparison_Ordinal();
	}

	[NotSupportedByProviderFact]
	public override Task EndsWith_with_StringComparison_OrdinalIgnoreCase()
	{
		return base.EndsWith_with_StringComparison_OrdinalIgnoreCase();
	}

	[NotSupportedByProviderFact]
	public override Task Equals_with_Ordinal()
	{
		return base.Equals_with_Ordinal();
	}

	[NotSupportedByProviderFact]
	public override Task Equals_with_OrdinalIgnoreCase()
	{
		return base.Equals_with_OrdinalIgnoreCase();
	}

	[NotSupportedByProviderFact]
	public override Task IndexOf_Char()
	{
		return base.IndexOf_Char();
	}

	[NotSupportedByProviderFact]
	public override Task IndexOf_with_constant_starting_position_char()
	{
		return base.IndexOf_with_constant_starting_position_char();
	}

	[NotSupportedByProviderFact]
	public override Task IndexOf_with_one_parameter_arg_char()
	{
		return base.IndexOf_with_one_parameter_arg_char();
	}

	[NotSupportedByProviderFact]
	public override Task IndexOf_with_parameter_starting_position_char()
	{
		return base.IndexOf_with_parameter_starting_position_char();
	}

	[NotSupportedByProviderFact]
	public override Task Replace_Char()
	{
		return base.Replace_Char();
	}

	[NotSupportedByProviderFact]
	public override Task StartsWith_Literal_Char()
	{
		return base.StartsWith_Literal_Char();
	}

	[NotSupportedByProviderFact]
	public override Task StartsWith_Parameter_Char()
	{
		return base.StartsWith_Parameter_Char();
	}

	[NotSupportedByProviderFact]
	public override Task StartsWith_with_StringComparison_Ordinal()
	{
		return base.StartsWith_with_StringComparison_Ordinal();
	}

	[NotSupportedByProviderFact]
	public override Task StartsWith_with_StringComparison_OrdinalIgnoreCase()
	{
		return base.StartsWith_with_StringComparison_OrdinalIgnoreCase();
	}

	[NotSupportedByProviderFact]
	public override Task Static_Equals_with_Ordinal()
	{
		return base.Static_Equals_with_Ordinal();
	}

	[NotSupportedByProviderFact]
	public override Task Static_Equals_with_OrdinalIgnoreCase()
	{
		return base.Static_Equals_with_OrdinalIgnoreCase();
	}
}
