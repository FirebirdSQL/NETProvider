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

using System;
using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class NorthwindMiscellaneousQueryFbTest : NorthwindMiscellaneousQueryRelationalTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
{
	public NorthwindMiscellaneousQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
		: base(fixture)
	{ }

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Select_DTO_constructor_distinct_with_collection_projection_translated_to_server_with_binding_after_client_eval(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Select_DTO_constructor_distinct_with_collection_projection_translated_to_server_with_binding_after_client_eval(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override async Task Client_code_unknown_method(bool async)
	{
		await AssertTranslationFailed(() => base.Client_code_unknown_method(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override async Task Client_code_using_instance_in_anonymous_type(bool async)
	{
		await Assert.ThrowsAsync<InvalidOperationException>(() => base.Client_code_using_instance_in_anonymous_type(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override async Task Client_code_using_instance_in_static_method(bool async)
	{
		await Assert.ThrowsAsync<InvalidOperationException>(() => base.Client_code_using_instance_in_static_method(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override async Task Client_code_using_instance_method_throws(bool async)
	{
		await Assert.ThrowsAsync<InvalidOperationException>(() => base.Client_code_using_instance_method_throws(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override async Task Entity_equality_through_subquery_composite_key(bool async)
	{
		await Assert.ThrowsAsync<InvalidOperationException>(() => base.Entity_equality_through_subquery_composite_key(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override async Task Max_on_empty_sequence_throws(bool async)
	{
		await Assert.ThrowsAsync<InvalidOperationException>(() => base.Max_on_empty_sequence_throws(async));
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(bool async)
	{
		return base.Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Correlated_collection_with_distinct_without_default_identifiers_projecting_columns(bool async)
	{
		return base.Correlated_collection_with_distinct_without_default_identifiers_projecting_columns(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Correlated_collection_with_distinct_without_default_identifiers_projecting_columns_with_navigation(bool async)
	{
		return base.Correlated_collection_with_distinct_without_default_identifiers_projecting_columns_with_navigation(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task DefaultIfEmpty_in_subquery_nested_filter_order_comparison(bool async)
	{
		return base.DefaultIfEmpty_in_subquery_nested_filter_order_comparison(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Select_correlated_subquery_ordered(bool async)
	{
		return base.Select_correlated_subquery_ordered(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Select_subquery_recursive_trivial(bool async)
	{
		return base.Select_subquery_recursive_trivial(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_correlated_subquery_hard(bool async)
	{
		return base.SelectMany_correlated_subquery_hard(async);
	}

	[Theory(Skip = "Different implicit ordering on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Entity_equality_orderby_subquery(bool async)
	{
		return base.Entity_equality_orderby_subquery(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Subquery_with_navigation_inside_inline_collection(bool async)
	{
		return base.Subquery_with_navigation_inside_inline_collection(async);
	}

	[NotSupportedByProviderTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_correlated_with_Select_value_type_and_DefaultIfEmpty_in_selector(bool async)
	{
		return base.SelectMany_correlated_with_Select_value_type_and_DefaultIfEmpty_in_selector(async);
	}

	[NotSupportedByProviderTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_nanosecond_and_microsecond_component(bool async)
	{
		return base.Where_nanosecond_and_microsecond_component(async);
	}
}
