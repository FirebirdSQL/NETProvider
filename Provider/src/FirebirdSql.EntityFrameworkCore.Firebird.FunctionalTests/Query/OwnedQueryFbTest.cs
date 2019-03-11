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

		[HasDataInTheSameTransactionAsDDL]
		public override void Navigation_rewrite_on_owned_collection()
		{
			base.Navigation_rewrite_on_owned_collection();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Navigation_rewrite_on_owned_reference()
		{
			base.Navigation_rewrite_on_owned_reference();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity()
		{
			base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference()
		{
			base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar()
		{
			base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection()
		{
			base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection()
		{
			base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count()
		{
			base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property()
		{
			base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void No_ignored_include_warning_when_implicit_load()
		{
			base.No_ignored_include_warning_when_implicit_load();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Query_for_base_type_loads_all_owned_navs()
		{
			base.Query_for_base_type_loads_all_owned_navs();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Query_for_branch_type_loads_all_owned_navs()
		{
			base.Query_for_branch_type_loads_all_owned_navs();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Query_for_leaf_type_loads_all_owned_navs()
		{
			base.Query_for_leaf_type_loads_all_owned_navs();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Query_when_group_by()
		{
			base.Query_when_group_by();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Query_when_subquery()
		{
			base.Query_when_subquery();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Query_with_OfType_eagerly_loads_correct_owned_navigations()
		{
			base.Query_with_OfType_eagerly_loads_correct_owned_navigations();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Query_with_owned_entity_equality_method()
		{
			base.Query_with_owned_entity_equality_method();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Query_with_owned_entity_equality_object_method()
		{
			base.Query_with_owned_entity_equality_object_method();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Query_with_owned_entity_equality_operator()
		{
			base.Query_with_owned_entity_equality_operator();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void SelectMany_on_owned_reference_followed_by_regular_entity_and_collection()
		{
			base.SelectMany_on_owned_reference_followed_by_regular_entity_and_collection();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection()
		{
			base.SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection();
		}

		[HasDataInTheSameTransactionAsDDL]
		public override void Select_many_on_owned_collection()
		{
			base.Select_many_on_owned_collection();
		}

		public class HasDataInTheSameTransactionAsDDLAttribute : FactAttribute
		{
			public HasDataInTheSameTransactionAsDDLAttribute()
			{
				Skip = "HasData is called in the same transaction as DDL commands.";
			}
		}

		public class OwnedQueryFbFixture : RelationalOwnedQueryFixture
		{
			protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;
		}
	}
}
