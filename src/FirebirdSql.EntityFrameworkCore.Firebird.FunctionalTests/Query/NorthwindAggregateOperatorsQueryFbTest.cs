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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class NorthwindAggregateOperatorsQueryFbTest : NorthwindAggregateOperatorsQueryRelationalTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
{
	public NorthwindAggregateOperatorsQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
		: base(fixture)
	{ }

	protected override bool CanExecuteQueryString => false;

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multiple_collection_navigation_with_FirstOrDefault_chained(bool async)
	{
		return base.Multiple_collection_navigation_with_FirstOrDefault_chained(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Contains_with_local_anonymous_type_array_closure(bool async)
	{
		return AssertTranslationFailed(() => base.Contains_with_local_anonymous_type_array_closure(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Contains_with_local_tuple_array_closure(bool async)
	{
		return AssertTranslationFailed(() => base.Contains_with_local_tuple_array_closure(async));
	}

	[Theory(Skip = "Different math on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Average_over_max_subquery_is_client_eval(bool async)
	{
		return base.Average_over_max_subquery_is_client_eval(async);
	}

	[Theory(Skip = "Different math on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Average_over_nested_subquery_is_client_eval(bool async)
	{
		return base.Average_over_nested_subquery_is_client_eval(async);
	}

	[Theory(Skip = "Different math on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Sum_with_division_on_decimal(bool async)
	{
		return base.Sum_with_division_on_decimal(async);
	}
}
