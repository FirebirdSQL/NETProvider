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

public class MathTranslationsFbTest : MathTranslationsTestBase<BasicTypesQueryFbFixture>
{
	public MathTranslationsFbTest(BasicTypesQueryFbFixture fixture, ITestOutputHelper testOutputHelper)
		: base(fixture)
	{
		Fixture.TestSqlLoggerFactory.Clear();
		Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
	}

	[NotSupportedByProviderFact]
	public override Task Acos_float()
	{
		return base.Acos_float();
	}

	[NotSupportedByProviderFact]
	public override Task Acosh()
	{
		return base.Acosh();
	}

	[NotSupportedByProviderFact]
	public override Task Asin_float()
	{
		return base.Asin_float();
	}

	[NotSupportedByProviderFact]
	public override Task Asinh()
	{
		return base.Asinh();
	}

	[NotSupportedByProviderFact]
	public override Task Atan_float()
	{
		return base.Atan_float();
	}

	[NotSupportedByProviderFact]
	public override Task Atanh()
	{
		return base.Atanh();
	}

	[NotSupportedByProviderFact]
	public override Task Atan2_float()
	{
		return base.Atan2_float();
	}

	[NotSupportedByProviderFact]
	public override Task Ceiling_float()
	{
		return base.Ceiling_float();
	}

	[NotSupportedByProviderFact]
	public override Task Cos_float()
	{
		return base.Cos_float();
	}

	[NotSupportedByProviderFact]
	public override Task Cosh()
	{
		return base.Cosh();
	}

	[NotSupportedByProviderFact]
	public override Task Degrees()
	{
		return base.Degrees();
	}

	[NotSupportedByProviderFact]
	public override Task Degrees_float()
	{
		return base.Degrees_float();
	}

	[NotSupportedByProviderFact]
	public override Task Exp_float()
	{
		return base.Exp_float();
	}

	[NotSupportedByProviderFact]
	public override Task Floor_float()
	{
		return base.Floor_float();
	}

	[NotSupportedByProviderFact]
	public override Task Log_float()
	{
		return base.Log_float();
	}

	[NotSupportedByProviderFact]
	public override Task Log_with_newBase_float()
	{
		return base.Log_with_newBase_float();
	}

	[NotSupportedByProviderFact]
	public override Task Log10_float()
	{
		return base.Log10_float();
	}

	[NotSupportedByProviderFact]
	public override Task Log2()
	{
		return base.Log2();
	}

	[NotSupportedByProviderFact]
	public override Task Power_float()
	{
		return base.Power_float();
	}

	[NotSupportedByProviderFact]
	public override Task Radians()
	{
		return base.Radians();
	}

	[NotSupportedByProviderFact]
	public override Task Radians_float()
	{
		return base.Radians_float();
	}

	[NotSupportedByProviderFact]
	public override Task Round_float()
	{
		return base.Round_float();
	}

	[NotSupportedByProviderFact]
	public override Task Round_with_digits_float()
	{
		return base.Round_with_digits_float();
	}

	[NotSupportedByProviderFact]
	public override Task Sign_float()
	{
		return base.Sign_float();
	}

	[NotSupportedByProviderFact]
	public override Task Sin_float()
	{
		return base.Sin_float();
	}

	[NotSupportedByProviderFact]
	public override Task Sinh()
	{
		return base.Sinh();
	}

	[NotSupportedByProviderFact]
	public override Task Sqrt_float()
	{
		return base.Sqrt_float();
	}

	[NotSupportedByProviderFact]
	public override Task Tan_float()
	{
		return base.Tan_float();
	}

	[NotSupportedByProviderFact]
	public override Task Tanh()
	{
		return base.Tanh();
	}

	[NotSupportedByProviderFact]
	public override Task Truncate_float()
	{
		return base.Truncate_float();
	}

	// Round itself works here, but the where clause (255.1) is not translated correctly (255.09999) due to floating point error
	[NotSupportedByProviderFact]
	public override Task Round_with_digits_double()
	{
		return base.Round_with_digits_double();
	}
}
