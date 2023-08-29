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
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class OwnedQueryFbTest : OwnedQueryRelationalTestBase<OwnedQueryFbTest.OwnedQueryFbFixture>
{
	public OwnedQueryFbTest(OwnedQueryFbFixture fixture)
		: base(fixture)
	{ }

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_collection(bool isAsync) => base.Navigation_rewrite_on_owned_collection(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity(bool isAsync) => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(bool isAsync) => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(bool isAsync) => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(bool isAsync) => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(bool isAsync) => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count(bool isAsync) => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(bool isAsync) => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task No_ignored_include_warning_when_implicit_load(bool isAsync) => base.No_ignored_include_warning_when_implicit_load(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_for_base_type_loads_all_owned_navs(bool isAsync) => base.Query_for_base_type_loads_all_owned_navs(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_for_branch_type_loads_all_owned_navs(bool isAsync) => base.Query_for_branch_type_loads_all_owned_navs(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_for_leaf_type_loads_all_owned_navs(bool isAsync) => base.Query_for_leaf_type_loads_all_owned_navs(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_when_subquery(bool isAsync) => base.Query_when_subquery(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_with_OfType_eagerly_loads_correct_owned_navigations(bool isAsync) => base.Query_with_OfType_eagerly_loads_correct_owned_navigations(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_with_owned_entity_equality_method(bool isAsync) => base.Query_with_owned_entity_equality_method(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_with_owned_entity_equality_object_method(bool isAsync) => base.Query_with_owned_entity_equality_object_method(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_with_owned_entity_equality_operator(bool isAsync) => base.Query_with_owned_entity_equality_operator(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(bool isAsync) => base.SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(bool isAsync) => base.SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_collection_with_composition(bool isAsync) => base.Navigation_rewrite_on_owned_collection_with_composition(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(bool isAsync) => base.Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_collection_with_composition_complex(bool isAsync) => base.Navigation_rewrite_on_owned_collection_with_composition_complex(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(bool isAsync) => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_reference_projecting_entity(bool isAsync) => base.Navigation_rewrite_on_owned_reference_projecting_entity(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_reference_projecting_scalar(bool isAsync) => base.Navigation_rewrite_on_owned_reference_projecting_scalar(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Preserve_includes_when_applying_skip_take_after_anonymous_type_select(bool isAsync) => base.Preserve_includes_when_applying_skip_take_after_anonymous_type_select(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_multiple_owned_navigations(bool isAsync) => base.Project_multiple_owned_navigations(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_multiple_owned_navigations_with_expansion_on_owned_collections(bool isAsync) => base.Project_multiple_owned_navigations_with_expansion_on_owned_collections(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_for_branch_type_loads_all_owned_navs_tracking(bool isAsync) => base.Query_for_branch_type_loads_all_owned_navs_tracking(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_loads_reference_nav_automatically_in_projection(bool isAsync) => base.Query_loads_reference_nav_automatically_in_projection(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_on_owned_collection(bool isAsync) => base.SelectMany_on_owned_collection(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Set_throws_for_owned_type(bool isAsync) => base.Set_throws_for_owned_type(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Throw_for_owned_entities_without_owner_in_tracking_query(bool isAsync) => base.Throw_for_owned_entities_without_owner_in_tracking_query(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Unmapped_property_projection_loads_owned_navigations(bool isAsync) => base.Unmapped_property_projection_loads_owned_navigations(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Client_method_take_loads_owned_navigations_variation_2(bool isAsync) => base.Client_method_take_loads_owned_navigations_variation_2(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Trying_to_access_non_existent_indexer_property_throws_meaningful_exception(bool isAsync) => base.Trying_to_access_non_existent_indexer_property_throws_meaningful_exception(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Client_method_skip_loads_owned_navigations(bool isAsync) => base.Client_method_skip_loads_owned_navigations(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Indexer_property_is_pushdown_into_subquery(bool isAsync) => base.Indexer_property_is_pushdown_into_subquery(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_with_OfType_eagerly_loads_correct_owned_navigations_split(bool isAsync) => base.Query_with_OfType_eagerly_loads_correct_owned_navigations_split(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_OrderBy_owned_indexer_properties(bool isAsync) => base.Can_OrderBy_owned_indexer_properties(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_query_on_indexer_property_when_property_name_from_closure(bool isAsync) => base.Can_query_on_indexer_property_when_property_name_from_closure(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_group_by_converted_indexer_property(bool isAsync) => base.Can_group_by_converted_indexer_property(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_collection_navigation_AsEnumerable_Count(bool isAsync) => base.Where_collection_navigation_AsEnumerable_Count(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_project_owned_indexer_properties_converted(bool isAsync) => base.Can_project_owned_indexer_properties_converted(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_group_by_owned_indexer_property(bool isAsync) => base.Can_group_by_owned_indexer_property(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_multiple_owned_navigations_split(bool isAsync) => base.Project_multiple_owned_navigations_split(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_project_owned_indexer_properties(bool isAsync) => base.Can_project_owned_indexer_properties(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task NoTracking_Include_with_cycles_throws(bool isAsync) => base.NoTracking_Include_with_cycles_throws(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Projecting_indexer_property_ignores_include(bool isAsync) => base.Projecting_indexer_property_ignores_include(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task GroupBy_with_multiple_aggregates_on_owned_navigation_properties(bool isAsync) => base.GroupBy_with_multiple_aggregates_on_owned_navigation_properties(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(bool isAsync) => base.Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_when_subquery_split(bool isAsync) => base.Query_when_subquery_split(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_group_by_converted_owned_indexer_property(bool isAsync) => base.Can_group_by_converted_owned_indexer_property(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_project_indexer_properties_converted(bool isAsync) => base.Can_project_indexer_properties_converted(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_for_base_type_loads_all_owned_navs_split(bool isAsync) => base.Query_for_base_type_loads_all_owned_navs_split(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_query_on_indexer_properties(bool isAsync) => base.Can_query_on_indexer_properties(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Non_nullable_property_through_optional_navigation(bool isAsync) => base.Non_nullable_property_through_optional_navigation(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_collection_navigation_ToArray_Count(bool isAsync) => base.Where_collection_navigation_ToArray_Count(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_OrderBy_indexer_properties_converted(bool isAsync) => base.Can_OrderBy_indexer_properties_converted(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_join_on_indexer_property_on_query(bool isAsync) => base.Can_join_on_indexer_property_on_query(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_OrderBy_owened_indexer_properties_converted(bool isAsync) => base.Can_OrderBy_owened_indexer_properties_converted(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_collection_navigation_ToList_Count_member(bool isAsync) => base.Where_collection_navigation_ToList_Count_member(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_query_on_owned_indexer_properties(bool isAsync) => base.Can_query_on_owned_indexer_properties(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Projecting_indexer_property_ignores_include_converted(bool isAsync) => base.Projecting_indexer_property_ignores_include_converted(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Client_method_skip_loads_owned_navigations_variation_2(bool isAsync) => base.Client_method_skip_loads_owned_navigations_variation_2(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Client_method_skip_take_loads_owned_navigations_variation_2(bool isAsync) => base.Client_method_skip_take_loads_owned_navigations_variation_2(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Projecting_collection_correlated_with_keyless_entity_after_navigation_works_using_parent_identifiers(bool isAsync) => base.Projecting_collection_correlated_with_keyless_entity_after_navigation_works_using_parent_identifiers(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_query_on_indexer_properties_split(bool isAsync) => base.Can_query_on_indexer_properties_split(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_split(bool isAsync) => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_split(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_collection_navigation_ToArray_Length_member(bool isAsync) => base.Where_collection_navigation_ToArray_Length_member(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_OrderBy_indexer_properties(bool isAsync) => base.Can_OrderBy_indexer_properties(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_owned_collection_navigation_ToList_Count(bool isAsync) => base.Where_owned_collection_navigation_ToList_Count(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_on_collection_entry_works_for_owned_collection(bool isAsync) => base.Query_on_collection_entry_works_for_owned_collection(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Client_method_take_loads_owned_navigations(bool isAsync) => base.Client_method_take_loads_owned_navigations(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_group_by_indexer_property(bool isAsync) => base.Can_group_by_indexer_property(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Query_for_branch_type_loads_all_owned_navs_split(bool isAsync) => base.Query_for_branch_type_loads_all_owned_navs_split(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_query_indexer_property_on_owned_collection(bool isAsync) => base.Can_query_indexer_property_on_owned_collection(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Client_method_skip_take_loads_owned_navigations(bool isAsync) => base.Client_method_skip_take_loads_owned_navigations(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Unmapped_property_projection_loads_owned_navigations_split(bool isAsync) => base.Unmapped_property_projection_loads_owned_navigations_split(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_project_indexer_properties(bool isAsync) => base.Can_project_indexer_properties(isAsync);

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Ordering_by_identifying_projection(bool isAsync) => base.Ordering_by_identifying_projection(isAsync);

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Filter_on_indexer_using_closure(bool async)
	{
		return base.Filter_on_indexer_using_closure(async);
	}

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Filter_on_indexer_using_function_argument(bool async)
	{
		return base.Filter_on_indexer_using_function_argument(async);
	}

	[HasDataInTheSameTransactionAsDDLTheory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public override Task NoTracking_Include_with_cycles_does_not_throw_when_performing_identity_resolution(bool async, bool useAsTracking)
	{
		return base.NoTracking_Include_with_cycles_does_not_throw_when_performing_identity_resolution(async, useAsTracking);
	}

	[HasDataInTheSameTransactionAsDDLTheory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public override Task Owned_entity_without_owner_does_not_throw_for_identity_resolution(bool async, bool useAsTracking)
	{
		return base.Owned_entity_without_owner_does_not_throw_for_identity_resolution(async, useAsTracking);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Can_query_owner_with_different_owned_types_having_same_property_name_in_hierarchy(bool async)
	{
		return base.Can_query_owner_with_different_owned_types_having_same_property_name_in_hierarchy(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task GroupBy_aggregate_on_owned_navigation_in_aggregate_selector(bool async)
	{
		return base.GroupBy_aggregate_on_owned_navigation_in_aggregate_selector(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Left_join_on_entity_with_owned_navigations(bool async)
	{
		return base.Left_join_on_entity_with_owned_navigations(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Left_join_on_entity_with_owned_navigations_complex(bool async)
	{
		return base.Left_join_on_entity_with_owned_navigations_complex(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Simple_query_entity_with_owned_collection(bool async)
	{
		return base.Simple_query_entity_with_owned_collection(async);
	}

	public class OwnedQueryFbFixture : RelationalOwnedQueryFixture
	{
		protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;
	}
}
