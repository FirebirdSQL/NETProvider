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
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class OwnedQueryFbTest : RelationalOwnedQueryTestBase<OwnedQueryFbTest.OwnedQueryFbFixture>
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
		public override Task Query_when_group_by(bool isAsync) => base.Query_when_group_by(isAsync);

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

		public class OwnedQueryFbFixture : RelationalOwnedQueryFixture
		{
			protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;
		}
	}
}
