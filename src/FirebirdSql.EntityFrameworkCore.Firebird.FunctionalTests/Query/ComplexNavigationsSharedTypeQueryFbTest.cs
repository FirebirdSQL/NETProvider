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
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class ComplexNavigationsSharedTypeQueryFbTest : ComplexNavigationsSharedTypeQueryRelationalTestBase<ComplexNavigationsSharedTypeQueryFbFixture>
{
	public ComplexNavigationsSharedTypeQueryFbTest(ComplexNavigationsSharedTypeQueryFbFixture fixture)
		: base(fixture)
	{ }

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Join_with_result_selector_returning_queryable_throws_validation_error(bool async)
	{
		return Assert.ThrowsAsync<ArgumentException>(() => base.Join_with_result_selector_returning_queryable_throws_validation_error(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task GroupJoin_client_method_in_OrderBy(bool async)
	{
		return AssertTranslationFailed(() => base.GroupJoin_client_method_in_OrderBy(async));
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
	public override Task Let_let_contains_from_outer_let(bool async)
	{
		return base.Let_let_contains_from_outer_let(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(bool async)
	{
		return base.Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Prune_does_not_throw_null_ref(bool async)
	{
		return base.Prune_does_not_throw_null_ref(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_shadow_properties10(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Project_shadow_properties10(async);
	}
}
