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
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class ComplexNavigationsQueryFbTest : ComplexNavigationsQueryTestBase<ComplexNavigationsQueryFbFixture>
{
	public ComplexNavigationsQueryFbTest(ComplexNavigationsQueryFbFixture fixture)
		: base(fixture)
	{ }

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Complex_navigations_with_predicate_projected_into_anonymous_type(bool isAsync)
	{
		return base.Complex_navigations_with_predicate_projected_into_anonymous_type(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Complex_navigations_with_predicate_projected_into_anonymous_type2(bool isAsync)
	{
		return base.Complex_navigations_with_predicate_projected_into_anonymous_type2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Complex_query_with_optional_navigations_and_client_side_evaluation(bool isAsync)
	{
		return base.Complex_query_with_optional_navigations_and_client_side_evaluation(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_navigation_in_outer_selector_translated_to_extra_join_nested(bool isAsync)
	{
		return base.Join_navigation_in_outer_selector_translated_to_extra_join_nested(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_navigation_in_outer_selector_translated_to_extra_join_nested2(bool isAsync)
	{
		return base.Join_navigation_in_outer_selector_translated_to_extra_join_nested2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Level4_Include(bool isAsync)
	{
		return base.Level4_Include(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Manually_created_left_join_propagates_nullability_to_navigations(bool isAsync)
	{
		return base.Manually_created_left_join_propagates_nullability_to_navigations(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multiple_required_navigations_with_Include(bool isAsync)
	{
		return base.Multiple_required_navigations_with_Include(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multiple_required_navigation_using_multiple_selects_with_Include(bool isAsync)
	{
		return base.Multiple_required_navigation_using_multiple_selects_with_Include(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multiple_required_navigation_using_multiple_selects_with_string_based_Include(bool isAsync)
	{
		return base.Multiple_required_navigation_using_multiple_selects_with_string_based_Include(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multiple_required_navigation_with_string_based_Include(bool isAsync)
	{
		return base.Multiple_required_navigation_with_string_based_Include(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multiple_SelectMany_calls(bool isAsync)
	{
		return base.Multiple_SelectMany_calls(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool isAsync)
	{
		return base.Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(bool isAsync)
	{
		return base.Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multiple_SelectMany_with_string_based_Include(bool isAsync)
	{
		return base.Multiple_SelectMany_with_string_based_Include(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multi_include_with_groupby_in_subquery(bool isAsync)
	{
		return base.Multi_include_with_groupby_in_subquery(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multi_level_navigation_with_same_navigation_compared_to_null(bool isAsync)
	{
		return base.Multi_level_navigation_with_same_navigation_compared_to_null(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Navigation_inside_method_call_translated_to_join2(bool isAsync)
	{
		return base.Navigation_inside_method_call_translated_to_join2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Optional_navigation_take_optional_navigation(bool isAsync)
	{
		return base.Optional_navigation_take_optional_navigation(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Optional_navigation_with_Include(bool isAsync)
	{
		return base.Optional_navigation_with_Include(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Order_by_key_of_anonymous_type_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(bool isAsync)
	{
		return base.Order_by_key_of_anonymous_type_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Order_by_key_of_navigation_similar_to_projected_gets_optimized_into_FK_access(bool isAsync)
	{
		return base.Order_by_key_of_navigation_similar_to_projected_gets_optimized_into_FK_access(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access1(bool isAsync)
	{
		return base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access1(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access2(bool isAsync)
	{
		return base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access3(bool isAsync)
	{
		return base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access3(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(bool isAsync)
	{
		return base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Projection_select_correct_table_from_subquery_when_materialization_is_not_required(bool isAsync)
	{
		return base.Projection_select_correct_table_from_subquery_when_materialization_is_not_required(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Required_navigation_on_a_subquery_with_complex_projection_and_First(bool isAsync)
	{
		return base.Required_navigation_on_a_subquery_with_complex_projection_and_First(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Required_navigation_on_a_subquery_with_First_in_predicate(bool isAsync)
	{
		return base.Required_navigation_on_a_subquery_with_First_in_predicate(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Required_navigation_on_a_subquery_with_First_in_projection(bool isAsync)
	{
		return base.Required_navigation_on_a_subquery_with_First_in_projection(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_nested_navigation_property_optional_and_projection(bool isAsync)
	{
		return base.SelectMany_nested_navigation_property_optional_and_projection(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_nested_navigation_property_required(bool isAsync)
	{
		return base.SelectMany_nested_navigation_property_required(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_nested_navigations_and_additional_joins_outside_of_SelectMany(bool isAsync)
	{
		return base.SelectMany_with_nested_navigations_and_additional_joins_outside_of_SelectMany(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(bool isAsync)
	{
		return base.SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_same_navs(bool isAsync)
	{
		return base.SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_same_navs(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(bool isAsync)
	{
		return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(bool isAsync)
	{
		return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(bool isAsync)
	{
		return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(bool isAsync)
	{
		return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty(bool isAsync)
	{
		return base.SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(bool isAsync)
	{
		return base.SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_nested_required_navigation_filter_and_explicit_DefaultIfEmpty(bool isAsync)
	{
		return base.SelectMany_with_nested_required_navigation_filter_and_explicit_DefaultIfEmpty(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_string_based_Include1(bool isAsync)
	{
		return base.SelectMany_with_string_based_Include1(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_string_based_Include2(bool isAsync)
	{
		return base.SelectMany_with_string_based_Include2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Select_multiple_nav_prop_optional_required(bool isAsync)
	{
		return base.Select_multiple_nav_prop_optional_required(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Select_multiple_nav_prop_reference_optional(bool isAsync)
	{
		return base.Select_multiple_nav_prop_reference_optional(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Select_multiple_nav_prop_reference_required(bool isAsync)
	{
		return base.Select_multiple_nav_prop_reference_required(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Select_multiple_nav_prop_reference_required2(bool isAsync)
	{
		return base.Select_multiple_nav_prop_reference_required2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Select_subquery_with_client_eval_and_navigation1(bool isAsync)
	{
		return base.Select_subquery_with_client_eval_and_navigation1(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Select_subquery_with_client_eval_and_navigation2(bool isAsync)
	{
		return base.Select_subquery_with_client_eval_and_navigation2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Simple_level1_level2_GroupBy_Count(bool isAsync)
	{
		return base.Simple_level1_level2_GroupBy_Count(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Simple_level1_level2_GroupBy_Having_Count(bool isAsync)
	{
		return base.Simple_level1_level2_GroupBy_Having_Count(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Simple_level1_level2_include(bool isAsync)
	{
		return base.Simple_level1_level2_include(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Simple_level1_level2_level3_include(bool isAsync)
	{
		return base.Simple_level1_level2_level3_include(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task String_include_multiple_derived_collection_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(bool isAsync)
	{
		return base.String_include_multiple_derived_collection_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task String_include_multiple_derived_navigations_complex(bool isAsync)
	{
		return base.String_include_multiple_derived_navigations_complex(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task String_include_multiple_derived_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(bool isAsync)
	{
		return base.String_include_multiple_derived_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_complex_predicate_with_with_nav_prop_and_OrElse1(bool isAsync)
	{
		return base.Where_complex_predicate_with_with_nav_prop_and_OrElse1(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_complex_predicate_with_with_nav_prop_and_OrElse2(bool isAsync)
	{
		return base.Where_complex_predicate_with_with_nav_prop_and_OrElse2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_complex_predicate_with_with_nav_prop_and_OrElse3(bool isAsync)
	{
		return base.Where_complex_predicate_with_with_nav_prop_and_OrElse3(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_complex_predicate_with_with_nav_prop_and_OrElse4(bool isAsync)
	{
		return base.Where_complex_predicate_with_with_nav_prop_and_OrElse4(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_multiple_nav_prop_optional_required(bool isAsync)
	{
		return base.Where_multiple_nav_prop_optional_required(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_multiple_nav_prop_reference_optional_compared_to_null1(bool isAsync)
	{
		return base.Where_multiple_nav_prop_reference_optional_compared_to_null1(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_multiple_nav_prop_reference_optional_compared_to_null2(bool isAsync)
	{
		return base.Where_multiple_nav_prop_reference_optional_compared_to_null2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_multiple_nav_prop_reference_optional_compared_to_null3(bool isAsync)
	{
		return base.Where_multiple_nav_prop_reference_optional_compared_to_null3(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_multiple_nav_prop_reference_optional_compared_to_null4(bool isAsync)
	{
		return base.Where_multiple_nav_prop_reference_optional_compared_to_null4(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_multiple_nav_prop_reference_optional_compared_to_null5(bool isAsync)
	{
		return base.Where_multiple_nav_prop_reference_optional_compared_to_null5(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_multiple_nav_prop_reference_optional_member_compared_to_null(bool isAsync)
	{
		return base.Where_multiple_nav_prop_reference_optional_member_compared_to_null(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_multiple_nav_prop_reference_optional_member_compared_to_value(bool isAsync)
	{
		return base.Where_multiple_nav_prop_reference_optional_member_compared_to_value(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_navigation_property_to_collection2(bool isAsync)
	{
		return base.Where_navigation_property_to_collection2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_on_multilevel_reference_in_subquery_with_outer_projection(bool isAsync)
	{
		return base.Where_on_multilevel_reference_in_subquery_with_outer_projection(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level_join(bool isAsync)
	{
		return base.Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level_join(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened(bool isAsync)
	{
		return base.GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened2(bool isAsync)
	{
		return base.GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened3(bool isAsync)
	{
		return base.GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened3(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include18_1_1(bool isAsync)
	{
		return base.Include18_1_1(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include19(bool isAsync)
	{
		return base.Include19(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_navigation_deeply_nested_non_key_join(bool isAsync)
	{
		return base.Join_navigation_deeply_nested_non_key_join(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_navigation_deeply_nested_required(bool isAsync)
	{
		return base.Join_navigation_deeply_nested_required(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_navigation_in_inner_selector(bool isAsync)
	{
		return base.Join_navigation_in_inner_selector(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_navigation_key_access_optional(bool isAsync)
	{
		return base.Join_navigation_key_access_optional(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_navigation_key_access_required(bool isAsync)
	{
		return base.Join_navigation_key_access_required(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_navigation_nested(bool isAsync)
	{
		return base.Join_navigation_nested(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_navigation_nested2(bool isAsync)
	{
		return base.Join_navigation_nested2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_navigation_non_key_join(bool isAsync)
	{
		return base.Join_navigation_non_key_join(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_navigation_self_ref(bool isAsync)
	{
		return base.Join_navigation_self_ref(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_navigations_in_inner_selector_translated_without_collision(bool isAsync)
	{
		return base.Join_navigations_in_inner_selector_translated_without_collision(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_with_orderby_on_inner_sequence_navigation_non_key_join(bool isAsync)
	{
		return base.Join_with_orderby_on_inner_sequence_navigation_non_key_join(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Null_reference_protection_complex(bool isAsync)
	{
		return base.Null_reference_protection_complex(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Null_reference_protection_complex_client_eval(bool isAsync)
	{
		return base.Null_reference_protection_complex_client_eval(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Null_reference_protection_complex_materialization(bool isAsync)
	{
		return base.Null_reference_protection_complex_materialization(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Optional_navigation_propagates_nullability_to_manually_created_left_join2(bool isAsync)
	{
		return base.Optional_navigation_propagates_nullability_to_manually_created_left_join2(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Union_over_entities_with_different_nullability(bool isAsync)
	{
		return base.Union_over_entities_with_different_nullability(isAsync);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_with_groupby_in_subquery(bool async)
	{
		return base.Include_reference_with_groupby_in_subquery(async);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_shadow_properties1(bool async)
	{
		return base.Project_shadow_properties1(async);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_shadow_properties2(bool async)
	{
		return base.Project_shadow_properties2(async);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_shadow_properties3(bool async)
	{
		return base.Project_shadow_properties3(async);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_shadow_properties4(bool async)
	{
		return base.Project_shadow_properties4(async);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_shadow_properties9(bool async)
	{
		return base.Project_shadow_properties9(async);
	}

	[GeneratedNameTooLongTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_shadow_properties10(bool async)
	{
		return base.Project_shadow_properties10(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SelectMany_with_outside_reference_to_joined_table_correctly_translated_to_apply(bool isAsync)
	{
		return base.SelectMany_with_outside_reference_to_joined_table_correctly_translated_to_apply(isAsync);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Let_let_contains_from_outer_let(bool async)
	{
		return base.Let_let_contains_from_outer_let(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Prune_does_not_throw_null_ref(bool async)
	{
		return base.Prune_does_not_throw_null_ref(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task GroupJoin_with_subquery_on_inner(bool async)
	{
		return base.GroupJoin_with_subquery_on_inner(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty(bool async)
	{
		return base.GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(bool async)
	{
		return base.Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task GroupJoin_client_method_in_OrderBy(bool async)
	{
		return AssertTranslationFailed(() => base.GroupJoin_client_method_in_OrderBy(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_with_result_selector_returning_queryable_throws_validation_error(bool async)
	{
		return Assert.ThrowsAsync<ArgumentException>(() => base.Join_with_result_selector_returning_queryable_throws_validation_error(async));
	}
}
