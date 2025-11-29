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

public class MiscellaneousTranslationsFbTest : MiscellaneousTranslationsRelationalTestBase<BasicTypesQueryFbFixture>
{
	public MiscellaneousTranslationsFbTest(BasicTypesQueryFbFixture fixture, ITestOutputHelper testOutputHelper)
		: base(fixture)
	{
		Fixture.TestSqlLoggerFactory.Clear();
		Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
	}



	[NotSupportedOnFirebirdFact]
	public override Task Convert_ToBoolean()
	{
		return base.Convert_ToBoolean();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Convert_ToByte()
	{
		return base.Convert_ToByte();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Convert_ToDecimal()
	{
		return base.Convert_ToDecimal();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Convert_ToDouble()
	{
		return base.Convert_ToDouble();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Convert_ToInt16()
	{
		return base.Convert_ToInt16();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Convert_ToInt32()
	{
		return base.Convert_ToInt32();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Convert_ToInt64()
	{
		return base.Convert_ToInt64();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Convert_ToString()
	{
		return base.Convert_ToString();
	}
}
