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

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class NorthwindSelectQueryFbTest : NorthwindSelectQueryRelationalTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
	{
		public NorthwindSelectQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
			: base(fixture)
		{ }

		[Theory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Reverse_without_explicit_ordering_throws(bool async)
		{
			return AssertTranslationFailedWithDetails(
				() => base.Reverse_without_explicit_ordering_throws(async), RelationalStrings.MissingOrderingInSelectExpression);
		}

		[Theory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool async)
		{
			return AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(async));
		}

		[Theory]
		[MemberData(nameof(IsAsyncData))]
		public override async Task Projecting_after_navigation_and_distinct_throws(bool async)
		{
			var message = (await Assert.ThrowsAsync<InvalidOperationException>(
				() => base.Projecting_after_navigation_and_distinct_throws(async))).Message;

			Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyOuterElementOfCollectionJoin, message);
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
	}
}
