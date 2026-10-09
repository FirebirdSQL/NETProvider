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

public class NorthwindJoinQueryFbTest : NorthwindJoinQueryRelationalTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
{
	public NorthwindJoinQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
		: base(fixture)
	{ }

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task GroupJoin_as_final_operator(bool async)
	{
		return base.GroupJoin_as_final_operator(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task GroupJoin_SelectMany_subquery_with_filter_orderby(bool async)
	{
		return base.GroupJoin_SelectMany_subquery_with_filter_orderby(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty(bool async)
	{
		return base.GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_client_eval(bool async)
	{
		return base.SelectMany_with_client_eval(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_client_eval_with_collection_shaper(bool async)
	{
		return base.SelectMany_with_client_eval_with_collection_shaper(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_client_eval_with_collection_shaper_ignored(bool async)
	{
		return base.SelectMany_with_client_eval_with_collection_shaper_ignored(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_selecting_outer_element(bool async)
	{
		return base.SelectMany_with_selecting_outer_element(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_selecting_outer_entity(bool async)
	{
		return base.SelectMany_with_selecting_outer_entity(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_selecting_outer_entity_column_and_inner_column(bool async)
	{
		return base.SelectMany_with_selecting_outer_entity_column_and_inner_column(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Take_in_collection_projection_with_FirstOrDefault_on_top_level(bool async)
	{
		return base.Take_in_collection_projection_with_FirstOrDefault_on_top_level(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Unflattened_GroupJoin_composed(bool async)
	{
		return base.Unflattened_GroupJoin_composed(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Unflattened_GroupJoin_composed_2(bool async)
	{
		return base.Unflattened_GroupJoin_composed_2(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_local_collection_int_closure_is_cached_correctly(bool async)
	{
		return base.Join_local_collection_int_closure_is_cached_correctly(async);
	}
}
