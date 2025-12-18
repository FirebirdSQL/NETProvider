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
using System.Linq;
using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexTypeModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class ComplexTypeQueryFbTest : ComplexTypeQueryRelationalTestBase<ComplexTypeQueryFbTest.ComplexTypeQueryFbFixture>
{
	public ComplexTypeQueryFbTest(ComplexTypeQueryFbFixture fixture)
		: base(fixture)
	{ }

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Union_property_in_complex_type(bool async)
	{
		return AssertQuery(
			async,
			ss => ss.Set<Customer>().Select(c => c.ShippingAddress.AddressLine1)
				.Union(ss.Set<Customer>().Select(c => c.BillingAddress.AddressLine1)));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_same_entity_with_nested_complex_type_twice_with_pushdown(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Project_same_entity_with_nested_complex_type_twice_with_pushdown(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_same_entity_with_nested_complex_type_twice_with_double_pushdown(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Project_same_entity_with_nested_complex_type_twice_with_double_pushdown(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_same_entity_with_struct_nested_complex_type_twice_with_double_pushdown(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Project_same_entity_with_struct_nested_complex_type_twice_with_double_pushdown(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_same_entity_with_struct_nested_complex_type_twice_with_pushdown(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Project_same_entity_with_struct_nested_complex_type_twice_with_pushdown(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_same_nested_complex_type_twice_with_double_pushdown(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Project_same_nested_complex_type_twice_with_double_pushdown(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_same_nested_complex_type_twice_with_pushdown(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Project_same_nested_complex_type_twice_with_pushdown(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_same_struct_nested_complex_type_twice_with_double_pushdown(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Project_same_struct_nested_complex_type_twice_with_double_pushdown(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_same_struct_nested_complex_type_twice_with_pushdown(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Project_same_struct_nested_complex_type_twice_with_pushdown(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Union_of_same_entity_with_nested_complex_type_projected_twice_with_double_pushdown(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Union_of_same_entity_with_nested_complex_type_projected_twice_with_double_pushdown(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Union_of_same_entity_with_nested_complex_type_projected_twice_with_pushdown(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Union_of_same_entity_with_nested_complex_type_projected_twice_with_pushdown(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Union_of_same_nested_complex_type_projected_twice_with_double_pushdown(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Union_of_same_nested_complex_type_projected_twice_with_double_pushdown(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Union_of_same_nested_complex_type_projected_twice_with_pushdown(bool async)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Union_of_same_nested_complex_type_projected_twice_with_pushdown(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Same_entity_with_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(bool async)
	{
		return base.Same_entity_with_complex_type_projected_twice_with_pushdown_as_part_of_another_projection(async);
	}

	public class ComplexTypeQueryFbFixture : ComplexTypeQueryRelationalFixtureBase
	{
		protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

		protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
		{
			base.OnModelCreating(modelBuilder, context);
			ModelHelpers.SetStringLengths(modelBuilder);
		}
	}
}
