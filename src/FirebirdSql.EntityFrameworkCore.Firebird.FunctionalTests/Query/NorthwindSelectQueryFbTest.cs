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
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class NorthwindSelectQueryFbTest : NorthwindSelectQueryRelationalTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
{
	public NorthwindSelectQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
		: base(fixture)
	{ }

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool async)
	{
		return AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(async));
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Projection_containing_DateTime_subtraction(bool async)
	{
		return base.Projection_containing_DateTime_subtraction(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(bool async)
	{
		return base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Select_nested_collection_deep(bool async)
	{
		return base.Select_nested_collection_deep(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_correlated_with_outer_1(bool async)
	{
		return base.SelectMany_correlated_with_outer_1(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_correlated_with_outer_2(bool async)
	{
		return base.SelectMany_correlated_with_outer_1(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_correlated_with_outer_3(bool async)
	{
		return base.SelectMany_correlated_with_outer_1(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_correlated_with_outer_4(bool async)
	{
		return base.SelectMany_correlated_with_outer_1(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_correlated_with_outer_5(bool async)
	{
		return base.SelectMany_correlated_with_outer_1(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_correlated_with_outer_6(bool async)
	{
		return base.SelectMany_correlated_with_outer_1(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_correlated_with_outer_7(bool async)
	{
		return base.SelectMany_correlated_with_outer_1(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(bool async)
	{
		return base.SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_whose_selector_references_outer_source(bool async)
	{
		return base.SelectMany_whose_selector_references_outer_source(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Collection_projection_selecting_outer_element_followed_by_take(bool async)
	{
		return base.Collection_projection_selecting_outer_element_followed_by_take(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Correlated_collection_after_distinct_not_containing_original_identifier(bool async)
	{
		return base.Correlated_collection_after_distinct_not_containing_original_identifier(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Correlated_collection_after_distinct_with_complex_projection_containing_original_identifier(bool async)
	{
		return base.Correlated_collection_after_distinct_with_complex_projection_containing_original_identifier(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Correlated_collection_after_groupby_with_complex_projection_containing_original_identifier(bool async)
	{
		return base.Correlated_collection_after_groupby_with_complex_projection_containing_original_identifier(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Projecting_after_navigation_and_distinct(bool async)
	{
		return base.Projecting_after_navigation_and_distinct(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Select_nested_collection_deep_distinct_no_identifiers(bool async)
	{
		return base.Select_nested_collection_deep_distinct_no_identifiers(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Take_on_correlated_collection_in_first(bool async)
	{
		return base.Take_on_correlated_collection_in_first(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Take_on_top_level_and_on_collection_projection_with_outer_apply(bool async)
	{
		return base.Take_on_top_level_and_on_collection_projection_with_outer_apply(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Correlated_collection_after_groupby_with_complex_projection_not_containing_original_identifier(bool async)
	{
		return base.Correlated_collection_after_groupby_with_complex_projection_not_containing_original_identifier(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Reverse_in_projection_subquery(bool async)
	{
		return base.Reverse_in_projection_subquery(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Reverse_in_projection_subquery_single_result(bool async)
	{
		return base.Reverse_in_projection_subquery_single_result(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Reverse_in_SelectMany_with_Take(bool async)
	{
		return base.Reverse_in_SelectMany_with_Take(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override async Task Correlated_collection_after_distinct_with_complex_projection_not_containing_original_identifier(bool async)
	{
		Assert.Equal(
			RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin,
			(await Assert.ThrowsAsync<InvalidOperationException>(
				() => base.Correlated_collection_after_distinct_with_complex_projection_not_containing_original_identifier(async)))
			.Message);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(bool async)
	{
		return AssertUnableToTranslateEFProperty(
			() => base
				.SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
					async));
	}
}
