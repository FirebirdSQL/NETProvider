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

using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class NorthwindIncludeNoTrackingQueryFbTest : NorthwindIncludeNoTrackingQueryTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
{
	public NorthwindIncludeNoTrackingQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
		: base(fixture)
	{ }

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Filtered_include_with_multiple_ordering(bool async)
	{
		return base.Filtered_include_with_multiple_ordering(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_with_outer_apply_with_filter(bool async)
	{
		return base.Include_collection_with_outer_apply_with_filter(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_with_outer_apply_with_filter_non_equality(bool async)
	{
		return base.Include_collection_with_outer_apply_with_filter_non_equality(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_with_cross_apply_with_filter(bool async)
	{
		return base.Include_collection_with_cross_apply_with_filter(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_with_last_no_orderby(bool async)
	{
		return Assert.ThrowsAsync<InvalidOperationException>(() => base.Include_collection_with_last_no_orderby(async));
	}

	[Theory(Skip = "Different ordering on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_OrderBy_list_contains(bool async)
	{
		return base.Include_collection_OrderBy_list_contains(async);
	}

	[Theory(Skip = "Different ordering on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_with_multiple_conditional_order_by(bool async)
	{
		return base.Include_collection_with_multiple_conditional_order_by(async);
	}

	[Theory(Skip = "Different implicit ordering on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_duplicate_collection_result_operator2(bool async)
	{
		return base.Include_duplicate_collection_result_operator2(async);
	}

	[Theory(Skip = "Different implicit ordering on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_take_no_order_by(bool async)
	{
		return base.Include_collection_take_no_order_by(async);
	}

	[Theory(Skip = "Different implicit ordering on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_skip_no_order_by(bool async)
	{
		return base.Include_collection_skip_no_order_by(async);
	}

	[Theory(Skip = "Different implicit ordering on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_skip_take_no_order_by(bool async)
	{
		return base.Include_collection_skip_take_no_order_by(async);
	}

	[Theory(Skip = "Different implicit ordering on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_duplicate_collection_result_operator(bool async)
	{
		return base.Include_duplicate_collection_result_operator(async);
	}

	[Theory(Skip = "Different implicit ordering on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Repro9735(bool async)
	{
		return base.Repro9735(async);
	}

	[Theory(Skip = "Different implicit ordering on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_OrderBy_empty_list_contains(bool async)
	{
		return base.Include_collection_OrderBy_empty_list_contains(async);
	}

	[Theory(Skip = "Different implicit ordering on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_OrderBy_empty_list_does_not_contains(bool async)
	{
		return base.Include_collection_OrderBy_empty_list_does_not_contains(async);
	}

	[LongExecutionTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_Include_reference_GroupBy_Select(bool async)
	{
		return base.SelectMany_Include_reference_GroupBy_Select(async);
	}

	[LongExecutionTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_Include_collection_GroupBy_Select(bool async)
	{
		return base.SelectMany_Include_collection_GroupBy_Select(async);
	}

	[LongExecutionTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_Include_reference_GroupBy_Select(bool async)
	{
		return base.Join_Include_reference_GroupBy_Select(async);
	}
}
