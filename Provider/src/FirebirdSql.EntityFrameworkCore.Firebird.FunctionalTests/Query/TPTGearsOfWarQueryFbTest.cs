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

using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class TPTGearsOfWarQueryFbTest : TPTGearsOfWarQueryRelationalTestBase<TPTGearsOfWarQueryFbFixture>
	{
		public TPTGearsOfWarQueryFbTest(TPTGearsOfWarQueryFbFixture fixture)
			: base(fixture)
		{ }

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Byte_array_contains_parameter(bool async)
		{
			return base.Byte_array_contains_parameter(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Byte_array_contains_literal(bool async)
		{
			return base.Byte_array_contains_literal(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Contains_on_byte_array_property_using_byte_column(bool async)
		{
			return base.Contains_on_byte_array_property_using_byte_column(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool async)
		{
			return base.Correlated_collections_inner_subquery_predicate_references_outer_qsre(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Correlated_collections_inner_subquery_selector_references_outer_qsre(bool async)
		{
			return base.Correlated_collections_inner_subquery_selector_references_outer_qsre(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool async)
		{
			return base.Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool async)
		{
			return base.Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task DateTimeOffset_Contains_Less_than_Greater_than(bool isAsync)
		{
			return base.DateTimeOffset_Contains_Less_than_Greater_than(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task DateTimeOffset_Date_returns_datetime(bool async)
		{
			return base.DateTimeOffset_Date_returns_datetime(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_date_component(bool async)
		{
			return base.Where_datetimeoffset_date_component(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_day_component(bool async)
		{
			return base.Where_datetimeoffset_day_component(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_dayofyear_component(bool async)
		{
			return base.Where_datetimeoffset_dayofyear_component(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_hour_component(bool async)
		{
			return base.Where_datetimeoffset_hour_component(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_millisecond_component(bool async)
		{
			return base.Where_datetimeoffset_millisecond_component(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_minute_component(bool async)
		{
			return base.Where_datetimeoffset_minute_component(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_month_component(bool async)
		{
			return base.Where_datetimeoffset_month_component(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_now(bool async)
		{
			return base.Where_datetimeoffset_now(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_second_component(bool async)
		{
			return base.Where_datetimeoffset_second_component(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_utcnow(bool async)
		{
			return base.Where_datetimeoffset_utcnow(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_year_component(bool async)
		{
			return base.Where_datetimeoffset_year_component(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool async)
		{
			return base.Outer_parameter_in_group_join_with_DefaultIfEmpty(async);
		}
		
		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Outer_parameter_in_join_key(bool async)
		{
			return base.Outer_parameter_in_join_key(async);
		}
		
		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Outer_parameter_in_join_key_inner_and_outer(bool async)
		{
			return base.Outer_parameter_in_join_key_inner_and_outer(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(bool async)
		{
			return base.SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(bool async)
		{
			return base.Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(bool async)
		{
			return base.Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(bool async)
		{
			return base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(bool async)
		{
			return base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(async);
		}

		[Theory(Skip = "Different implicit ordering on Firebird.")]
		[MemberData(nameof(IsAsyncData))]
		public override Task Take_without_orderby_followed_by_orderBy_is_pushed_down1(bool async)
		{
			return base.Take_without_orderby_followed_by_orderBy_is_pushed_down1(async);
		}

		[Theory(Skip = "Different implicit ordering on Firebird.")]
		[MemberData(nameof(IsAsyncData))]
		public override Task Take_without_orderby_followed_by_orderBy_is_pushed_down2(bool async)
		{
			return base.Take_without_orderby_followed_by_orderBy_is_pushed_down2(async);
		}
	}
}
