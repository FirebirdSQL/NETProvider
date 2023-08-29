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
using System.Collections.Generic;
using System.Linq;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class QueryFilterFuncletizationFbTest : QueryFilterFuncletizationTestBase<QueryFilterFuncletizationFbTest.QueryFilterFuncletizationFbFixture>
{
	public QueryFilterFuncletizationFbTest(QueryFilterFuncletizationFbFixture fixture)
		: base(fixture)
	{ }

	[Fact]
	public override void DbContext_complex_expression_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.DbContext_complex_expression_is_parameterized();
	}

	[Fact]
	public override void DbContext_field_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.DbContext_field_is_parameterized();
	}

	[Fact]
	public override void DbContext_list_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;

		using var context = CreateContext();
		// Default value of TenantIds is null InExpression over null values throws
		Assert.Throws<NullReferenceException>(() => context.Set<ListFilter>().ToList());

		context.TenantIds = new List<int>();
		var query = context.Set<ListFilter>().ToList();
		Assert.Empty(query);

		context.TenantIds = new List<int> { 1 };
		query = context.Set<ListFilter>().ToList();
		Assert.Single(query);

		context.TenantIds = new List<int> { 2, 3 };
		query = context.Set<ListFilter>().ToList();
		Assert.Equal(2, query.Count);
	}

	[Fact]
	public override void DbContext_method_call_chain_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.DbContext_method_call_chain_is_parameterized();
	}

	[Fact]
	public override void DbContext_method_call_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.DbContext_method_call_is_parameterized();
	}

	[Fact]
	public override void DbContext_property_based_filter_does_not_short_circuit()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.DbContext_property_based_filter_does_not_short_circuit();
	}

	[Fact]
	public override void DbContext_property_chain_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.DbContext_property_chain_is_parameterized();
	}

	[Fact]
	public override void DbContext_property_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.DbContext_property_is_parameterized();
	}

	[Fact]
	public override void DbContext_property_method_call_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.DbContext_property_method_call_is_parameterized();
	}

	[Fact]
	public override void DbContext_property_parameter_does_not_clash_with_closure_parameter_name()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.DbContext_property_parameter_does_not_clash_with_closure_parameter_name();
	}

	[Fact]
	public override void EntityTypeConfiguration_DbContext_field_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.EntityTypeConfiguration_DbContext_field_is_parameterized();
	}

	[Fact]
	public override void EntityTypeConfiguration_DbContext_method_call_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.EntityTypeConfiguration_DbContext_method_call_is_parameterized();
	}

	[Fact]
	public override void EntityTypeConfiguration_DbContext_property_chain_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.EntityTypeConfiguration_DbContext_property_chain_is_parameterized();
	}

	[Fact]
	public override void EntityTypeConfiguration_DbContext_property_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.EntityTypeConfiguration_DbContext_property_is_parameterized();
	}

	[Fact]
	public override void Extension_method_DbContext_field_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.Extension_method_DbContext_field_is_parameterized();
	}

	[Fact]
	public override void Extension_method_DbContext_property_chain_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.Extension_method_DbContext_property_chain_is_parameterized();
	}

	[Fact]
	public override void Local_method_DbContext_field_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.Local_method_DbContext_field_is_parameterized();
	}

	[Fact]
	public override void Local_static_method_DbContext_property_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.Local_static_method_DbContext_property_is_parameterized();
	}

	[Fact]
	public override void Local_variable_from_OnModelCreating_can_throw_exception()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.Local_variable_from_OnModelCreating_can_throw_exception();
	}

	[Fact]
	public override void Local_variable_from_OnModelCreating_is_inlined()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.Local_variable_from_OnModelCreating_is_inlined();
	}

	[Fact]
	public override void Method_parameter_is_inlined()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.Method_parameter_is_inlined();
	}

	[Fact]
	public override void Remote_method_DbContext_property_method_call_is_parameterized()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.Remote_method_DbContext_property_method_call_is_parameterized();
	}

	[Fact]
	public override void Static_member_from_dbContext_is_inlined()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.Static_member_from_dbContext_is_inlined();
	}

	[Fact]
	public override void Static_member_from_non_dbContext_is_inlined()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.Static_member_from_non_dbContext_is_inlined();
	}

	[Fact]
	public override void Using_Context_set_method_in_filter_works()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.Using_Context_set_method_in_filter_works();
	}

	[Fact]
	public override void Using_DbSet_in_filter_works()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.Using_DbSet_in_filter_works();
	}

	[Fact]
	public override void Using_multiple_context_in_filter_parametrize_only_current_context()
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		base.Using_multiple_context_in_filter_parametrize_only_current_context();
	}

	public class QueryFilterFuncletizationFbFixture : QueryFilterFuncletizationRelationalFixture
	{
		protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

		protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
		{
			base.OnModelCreating(modelBuilder, context);
			ModelHelpers.SimpleTableNames(modelBuilder);
			ModelHelpers.SetPrimaryKeyGeneration(modelBuilder, FbValueGenerationStrategy.IdentityColumn);
		}
	}
}
