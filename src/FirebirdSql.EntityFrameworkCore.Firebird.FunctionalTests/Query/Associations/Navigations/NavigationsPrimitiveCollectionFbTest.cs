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
using Microsoft.EntityFrameworkCore.Query.Associations.Navigations;
using Xunit.Abstractions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query.Associations.Navigations;

public class NavigationsPrimitiveCollectionFbTest : NavigationsPrimitiveCollectionRelationalTestBase<NavigationsFbFixture>
{
	public NavigationsPrimitiveCollectionFbTest(NavigationsFbFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture, testOutputHelper)
	{
		Fixture.TestSqlLoggerFactory.Clear();
		Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
	}

	[NotSupportedByProviderFact]
	public override Task Any_predicate()
	{
		return base.Any_predicate();
	}

	[NotSupportedByProviderFact]
	public override Task Contains()
	{
		return base.Contains();
	}

	[NotSupportedByProviderFact]
	public override Task Count()
	{
		return base.Count();
	}

	[NotSupportedByProviderFact]
	public override Task Index()
	{
		return base.Index();
	}

	[NotSupportedByProviderFact]
	public override Task Nested_Count()
	{
		return base.Nested_Count();
	}

	[NotSupportedByProviderFact]
	public override Task Select_Sum()
	{
		return base.Select_Sum();
	}
}
