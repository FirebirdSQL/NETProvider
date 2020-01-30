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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class SimpleQueryFbTest : SimpleQueryTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
	{
		public SimpleQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
			: base(fixture)
		{ }

		[Theory(Skip = "Different DECIMAL handling on Firebird.")]
		[MemberData(nameof(IsAsyncData))]
		public override Task Sum_with_division_on_decimal(bool isAsync)
		{
			return base.Sum_with_division_on_decimal(isAsync);
		}

		[Theory(Skip = "Not handled directly into TimeSpan.")]
		[MemberData(nameof(IsAsyncData))]
		public override Task Projection_containing_DateTime_subtraction(bool isAsync)
		{
			return base.Projection_containing_DateTime_subtraction(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Except(bool isAsync)
		{
			return base.Except(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Except_nested(bool isAsync)
		{
			return base.Except_nested(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Except_non_entity(bool isAsync)
		{
			return base.Except_non_entity(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Except_simple_followed_by_projecting_constant(bool isAsync)
		{
			return base.Except_simple_followed_by_projecting_constant(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Intersect(bool isAsync)
		{
			return base.Intersect(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Intersect_nested(bool isAsync)
		{
			return base.Intersect_nested(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Intersect_non_entity(bool isAsync)
		{
			return base.Intersect_non_entity(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(bool isAsync)
		{
			return base.Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task AsQueryable_in_query_server_evals(bool isAsync)
		{
			return base.AsQueryable_in_query_server_evals(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(bool isAsync)
		{
			return base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_correlated_subquery_hard(bool isAsync)
		{
			return base.SelectMany_correlated_subquery_hard(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_correlated_with_outer_1(bool isAsync)
		{
			return base.SelectMany_correlated_with_outer_1(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_correlated_with_outer_2(bool isAsync)
		{
			return base.SelectMany_correlated_with_outer_2(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_correlated_with_outer_3(bool isAsync)
		{
			return base.SelectMany_correlated_with_outer_3(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_correlated_with_outer_4(bool isAsync)
		{
			return base.SelectMany_correlated_with_outer_4(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_whose_selector_references_outer_source(bool isAsync)
		{
			return base.SelectMany_whose_selector_references_outer_source(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(bool isAsync)
		{
			return base.SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Select_Except_reference_projection(bool isAsync)
		{
			return base.Select_Except_reference_projection(isAsync);
		}

		[NotSupportedOnFirebirdFact]
		public override void Select_nested_collection_multi_level()
		{
			base.Select_nested_collection_multi_level();
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Trim_with_char_array_argument_in_predicate(bool isAsync)
		{
			return base.Trim_with_char_array_argument_in_predicate(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task TrimEnd_with_char_array_argument_in_predicate(bool isAsync)
		{
			return base.TrimEnd_with_char_array_argument_in_predicate(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task TrimStart_with_char_array_argument_in_predicate(bool isAsync)
		{
			return base.TrimStart_with_char_array_argument_in_predicate(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Union_Intersect(bool isAsync)
		{
			return base.Union_Intersect(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Union_Select_scalar(bool isAsync)
		{
			return base.Union_Select_scalar(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_now_component(bool isAsync)
		{
			return base.Where_datetimeoffset_now_component(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_utcnow_component(bool isAsync)
		{
			return base.Where_datetimeoffset_utcnow_component(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetime_utcnow(bool isAsync)
		{
			return base.Where_datetime_utcnow(isAsync);
		}

		[Theory]
		[MemberData(nameof(IsAsyncData))]
		// See #19216 on EntityFrameworkCore.
		public override Task Projection_of_entity_type_into_object_list(bool isAsync)
		{
			return AssertQuery(
				isAsync,
				ss => ss.Set<Microsoft.EntityFrameworkCore.TestModels.Northwind.Customer>().OrderBy(c => c.CustomerID).Select(c => new List<object> { c }),
				entryCount: 91,
				assertOrder: true);
		}

		[Theory]
		[MemberData(nameof(IsAsyncData))]
		// See #19220 on EntityFrameworkCore.
		public override Task Where_subquery_anon_nested(bool isAsync)
		{
			return AssertQuery(
				isAsync,
				ss =>
					from t in (
						from e in ss.Set<Microsoft.EntityFrameworkCore.TestModels.Northwind.Employee>().OrderBy(ee => ee.EmployeeID).Take(3).Select(
							e => new { e }).Where(e => e.e.City == "Seattle")
						from o in ss.Set<Microsoft.EntityFrameworkCore.TestModels.Northwind.Order>().OrderBy(oo => oo.OrderID).Take(5).Select(
							o => new { o })
						select new { e, o })
						  from c in ss.Set<Microsoft.EntityFrameworkCore.TestModels.Northwind.Customer>().OrderBy(cc => cc.CustomerID).Take(2).Select(
							  c => new { c })
						  select new
						  {
							  t.e,
							  t.o,
							  c
						  },
				entryCount: 8);
		}

		[Theory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool isAsync)
		{
			return AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(isAsync));
		}

		[Theory(Skip = "Temp free space")]
		[MemberData(nameof(IsAsyncData))]
		public override Task Handle_materialization_properly_when_more_than_two_query_sources_are_involved(bool isAsync)
		{
			return base.Handle_materialization_properly_when_more_than_two_query_sources_are_involved(isAsync);
		}
	}
}
