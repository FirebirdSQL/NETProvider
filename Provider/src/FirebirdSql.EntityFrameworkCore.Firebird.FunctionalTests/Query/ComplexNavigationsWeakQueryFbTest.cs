/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
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
using Xunit;
using Xunit.Abstractions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class ComplexNavigationsWeakQueryFbTest : ComplexNavigationsWeakQueryTestBase<ComplexNavigationsWeakQueryFbFixture>
	{
		public ComplexNavigationsWeakQueryFbTest(ComplexNavigationsWeakQueryFbFixture fixture, ITestOutputHelper testOutputHelper)
			: base(fixture)
		{ }

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Complex_multi_include_with_order_by_and_paging(bool isAsync)
		{
			return base.Complex_multi_include_with_order_by_and_paging(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(bool isAsync)
		{
			return base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(bool isAsync)
		{
			return base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(isAsync);
		}

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
		public override Task Correlated_nested_subquery_doesnt_project_unnecessary_columns_in_top_level(bool isAsync)
		{
			return base.Correlated_nested_subquery_doesnt_project_unnecessary_columns_in_top_level(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Correlated_nested_two_levels_up_subquery_doesnt_project_unnecessary_columns_in_top_level(bool isAsync)
		{
			return base.Correlated_nested_two_levels_up_subquery_doesnt_project_unnecessary_columns_in_top_level(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Include_after_SelectMany_and_reference_navigation(bool isAsync)
		{
			return base.Include_after_SelectMany_and_reference_navigation(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Include_after_SelectMany_and_reference_navigation_with_another_SelectMany_with_Distinct(bool isAsync)
		{
			return base.Include_after_SelectMany_and_reference_navigation_with_another_SelectMany_with_Distinct(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Include_collection_with_multiple_orderbys_complex(bool isAsync)
		{
			return base.Include_collection_with_multiple_orderbys_complex(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Include_collection_with_multiple_orderbys_complex_repeated(bool isAsync)
		{
			return base.Include_collection_with_multiple_orderbys_complex_repeated(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Include_collection_with_multiple_orderbys_member(bool isAsync)
		{
			return base.Include_collection_with_multiple_orderbys_member(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Include_collection_with_multiple_orderbys_methodcall(bool isAsync)
		{
			return base.Include_collection_with_multiple_orderbys_methodcall(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Include_collection_with_multiple_orderbys_property(bool isAsync)
		{
			return base.Include_collection_with_multiple_orderbys_property(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Include_nested_with_optional_navigation(bool isAsync)
		{
			return base.Include_nested_with_optional_navigation(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Include_reference_collection_order_by_reference_navigation(bool isAsync)
		{
			return base.Include_reference_collection_order_by_reference_navigation(isAsync);
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
		public override Task Key_equality_navigation_converted_to_FK(bool isAsync)
		{
			return base.Key_equality_navigation_converted_to_FK(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Key_equality_two_conditions_on_same_navigation2(bool isAsync)
		{
			return base.Key_equality_two_conditions_on_same_navigation2(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Key_equality_using_property_method_and_member_expression3(bool isAsync)
		{
			return base.Key_equality_using_property_method_and_member_expression3(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Key_equality_using_property_method_nested2(bool isAsync)
		{
			return base.Key_equality_using_property_method_nested2(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Key_equality_using_property_method_required2(bool isAsync)
		{
			return base.Key_equality_using_property_method_required2(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Manually_created_left_join_propagates_nullability_to_navigations(bool isAsync)
		{
			return base.Manually_created_left_join_propagates_nullability_to_navigations(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Member_doesnt_get_pushed_down_into_subquery_with_result_operator(bool isAsync)
		{
			return base.Member_doesnt_get_pushed_down_into_subquery_with_result_operator(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Multiple_complex_includes(bool isAsync)
		{
			return base.Multiple_complex_includes(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Multiple_complex_include_select(bool isAsync)
		{
			return base.Multiple_complex_include_select(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Multiple_include_with_multiple_optional_navigations(bool isAsync)
		{
			return base.Multiple_include_with_multiple_optional_navigations(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Multiple_optional_navigation_with_Include(bool isAsync)
		{
			return base.Multiple_optional_navigation_with_Include(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Multiple_optional_navigation_with_string_based_Include(bool isAsync)
		{
			return base.Multiple_optional_navigation_with_string_based_Include(isAsync);
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
		public override Task Multiple_SelectMany_navigation_property_followed_by_select_collection_navigation(bool isAsync)
		{
			return base.Multiple_SelectMany_navigation_property_followed_by_select_collection_navigation(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Multiple_SelectMany_with_Include(bool isAsync)
		{
			return base.Multiple_SelectMany_with_Include(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool isAsync)
		{
			return base.Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(isAsync);
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
		public override Task Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times(bool isAsync)
		{
			return base.Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql(bool isAsync)
		{
			return base.Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Multi_level_navigation_compared_to_null(bool isAsync)
		{
			return base.Multi_level_navigation_compared_to_null(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Multi_level_navigation_with_same_navigation_compared_to_null(bool isAsync)
		{
			return base.Multi_level_navigation_with_same_navigation_compared_to_null(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Navigations_compared_to_each_other1(bool isAsync)
		{
			return base.Navigations_compared_to_each_other1(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Navigations_compared_to_each_other2(bool isAsync)
		{
			return base.Navigations_compared_to_each_other2(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Navigations_compared_to_each_other4(bool isAsync)
		{
			return base.Navigations_compared_to_each_other4(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Navigations_compared_to_each_other5(bool isAsync)
		{
			return base.Navigations_compared_to_each_other5(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Navigation_inside_method_call_translated_to_join2(bool isAsync)
		{
			return base.Navigation_inside_method_call_translated_to_join2(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Navigation_key_access_optional_comparison(bool isAsync)
		{
			return base.Navigation_key_access_optional_comparison(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Navigation_key_access_required_comparison(bool isAsync)
		{
			return base.Navigation_key_access_required_comparison(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Navigation_with_same_navigation_compared_to_null(bool isAsync)
		{
			return base.Navigation_with_same_navigation_compared_to_null(isAsync);
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
		public override Task Optional_navigation_with_Include_and_order(bool isAsync)
		{
			return base.Optional_navigation_with_Include_and_order(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Optional_navigation_with_Include_ThenInclude(bool isAsync)
		{
			return base.Optional_navigation_with_Include_ThenInclude(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Optional_navigation_with_order_by_and_Include(bool isAsync)
		{
			return base.Optional_navigation_with_order_by_and_Include(isAsync);
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
		public override Task Projection_select_correct_table_in_subquery_when_materialization_is_not_required_in_multiple_joins(bool isAsync)
		{
			return base.Projection_select_correct_table_in_subquery_when_materialization_is_not_required_in_multiple_joins(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Projection_select_correct_table_with_anonymous_projection_in_subquery(bool isAsync)
		{
			return base.Projection_select_correct_table_with_anonymous_projection_in_subquery(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Project_collection_navigation_nested(bool isAsync)
		{
			return base.Project_collection_navigation_nested(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Project_collection_navigation_nested_anonymous(bool isAsync)
		{
			return base.Project_collection_navigation_nested_anonymous(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Project_collection_navigation_using_ef_property(bool isAsync)
		{
			return base.Project_collection_navigation_using_ef_property(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Project_navigation_and_collection(bool isAsync)
		{
			return base.Project_navigation_and_collection(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Query_source_materialization_bug_4547(bool isAsync)
		{
			return base.Query_source_materialization_bug_4547(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Required_navigation_on_a_subquery_with_complex_projection_and_First(bool isAsync)
		{
			return base.Required_navigation_on_a_subquery_with_complex_projection_and_First(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Required_navigation_with_Include(bool isAsync)
		{
			return base.Required_navigation_with_Include(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Required_navigation_with_Include_ThenInclude(bool isAsync)
		{
			return base.Required_navigation_with_Include_ThenInclude(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_navigation_comparison2(bool isAsync)
		{
			return base.SelectMany_navigation_comparison2(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_navigation_property_with_another_navigation_in_subquery(bool isAsync)
		{
			return base.SelectMany_navigation_property_with_another_navigation_in_subquery(isAsync);
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
		public override Task SelectMany_with_Include1(bool isAsync)
		{
			return base.SelectMany_with_Include1(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_with_Include2(bool isAsync)
		{
			return base.SelectMany_with_Include2(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_with_Include_and_order_by(bool isAsync)
		{
			return base.SelectMany_with_Include_and_order_by(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_with_Include_ThenInclude(bool isAsync)
		{
			return base.SelectMany_with_Include_ThenInclude(isAsync);
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
		public override Task SelectMany_with_order_by_and_Include(bool isAsync)
		{
			return base.SelectMany_with_order_by_and_Include(isAsync);
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
		public override Task Select_nav_prop_reference_optional3(bool isAsync)
		{
			return base.Select_nav_prop_reference_optional3(isAsync);
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
		public override Task Where_navigation_property_to_collection_of_original_entity_type(bool isAsync)
		{
			return base.Where_navigation_property_to_collection_of_original_entity_type(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_on_multilevel_reference_in_subquery_with_outer_projection(bool isAsync)
		{
			return base.Where_on_multilevel_reference_in_subquery_with_outer_projection(isAsync);
		}

		[GeneratedNameTooLongFact]
		public override void Include19()
		{
			base.Include19();
		}

		[GeneratedNameTooLongFact]
		public override void IncludeCollection6_2()
		{
			base.IncludeCollection6_2();
		}

		[GeneratedNameTooLongFact]
		public override void IncludeCollection6_3()
		{
			base.IncludeCollection6_3();
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Join_navigation_deeply_nested_required(bool isAsync)
		{
			return base.Join_navigation_deeply_nested_required(isAsync);
		}

		[GeneratedNameTooLongTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Join_navigations_in_inner_selector_translated_without_collision(bool isAsync)
		{
			return base.Join_navigations_in_inner_selector_translated_without_collision(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Project_collection_navigation_nested_with_take(bool isAsync)
		{
			return base.Project_collection_navigation_nested_with_take(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_with_outside_reference_to_joined_table_correctly_translated_to_apply(bool isAsync)
		{
			return base.SelectMany_with_outside_reference_to_joined_table_correctly_translated_to_apply(isAsync);
		}
	}
}
