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

using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class InheritanceRelationshipsQueryFbTest : InheritanceRelationshipsQueryTestBase<InheritanceRelationshipsQueryFbTest.InheritanceRelationshipsQueryFbFixture>
{
	public InheritanceRelationshipsQueryFbTest(InheritanceRelationshipsQueryFbFixture fixture)
		: base(fixture)
	{ }

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_without_inheritance_on_derived1(bool async)
	{
		return base.Include_reference_without_inheritance_on_derived1(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_without_inheritance_with_filter_reverse(bool async)
	{
		return base.Include_collection_without_inheritance_with_filter_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Nested_include_with_inheritance_reference_reference_reverse(bool async)
	{
		return base.Nested_include_with_inheritance_reference_reference_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Nested_include_with_inheritance_reference_collection(bool async)
	{
		return base.Nested_include_with_inheritance_reference_collection(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_with_inheritance_with_filter(bool async)
	{
		return base.Include_reference_with_inheritance_with_filter(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_with_inheritance_on_derived2(bool async)
	{
		return base.Include_reference_with_inheritance_on_derived2(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_without_inheritance_on_derived_reverse(bool async)
	{
		return base.Include_reference_without_inheritance_on_derived_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_with_inheritance_on_derived_with_filter_reverse(bool async)
	{
		return base.Include_reference_with_inheritance_on_derived_with_filter_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_with_inheritance_reverse(bool async)
	{
		return base.Include_collection_with_inheritance_reverse(async);
	}

	[Fact(Skip = SkipReasons.GeneratedNameTooLong)]
	public override void Entity_can_make_separate_relationships_with_base_type_and_derived_type_both()
	{
		base.Entity_can_make_separate_relationships_with_base_type_and_derived_type_both();
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Nested_include_with_inheritance_collection_reference(bool async)
	{
		return base.Nested_include_with_inheritance_collection_reference(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_without_inheritance_reverse(bool async)
	{
		return base.Include_reference_without_inheritance_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Nested_include_with_inheritance_reference_collection_on_base(bool async)
	{
		return base.Nested_include_with_inheritance_reference_collection_on_base(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_with_inheritance_reverse(bool async)
	{
		return base.Include_reference_with_inheritance_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_with_inheritance_on_derived1(bool async)
	{
		return base.Include_collection_with_inheritance_on_derived1(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_self_reference_with_inheritance(bool async)
	{
		return base.Include_self_reference_with_inheritance(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_with_inheritance_on_derived_with_filter1(bool async)
	{
		return base.Include_reference_with_inheritance_on_derived_with_filter1(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Nested_include_with_inheritance_collection_reference_reverse(bool async)
	{
		return base.Nested_include_with_inheritance_collection_reference_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_with_inheritance_on_derived3(bool async)
	{
		return base.Include_collection_with_inheritance_on_derived3(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_without_inheritance_reverse(bool async)
	{
		return base.Include_collection_without_inheritance_reverse(async);
	}

	[Fact(Skip = SkipReasons.GeneratedNameTooLong)]
	public override void Changes_in_derived_related_entities_are_detected()
	{
		base.Changes_in_derived_related_entities_are_detected();
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_with_inheritance_with_filter(bool async)
	{
		return base.Include_collection_with_inheritance_with_filter(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_without_inheritance_on_derived2(bool async)
	{
		return base.Include_reference_without_inheritance_on_derived2(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_with_inheritance_on_derived4(bool async)
	{
		return base.Include_reference_with_inheritance_on_derived4(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Nested_include_with_inheritance_reference_reference(bool async)
	{
		return base.Nested_include_with_inheritance_reference_reference(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_with_inheritance_on_derived_with_filter2(bool async)
	{
		return base.Include_reference_with_inheritance_on_derived_with_filter2(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_with_inheritance_with_filter_reverse(bool async)
	{
		return base.Include_reference_with_inheritance_with_filter_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_with_inheritance_on_derived_reverse(bool async)
	{
		return base.Include_reference_with_inheritance_on_derived_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_self_reference_with_inheritance_reverse(bool async)
	{
		return base.Include_self_reference_with_inheritance_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_without_inheritance(bool async)
	{
		return base.Include_collection_without_inheritance(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_without_inheritance_with_filter(bool async)
	{
		return base.Include_reference_without_inheritance_with_filter(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Nested_include_with_inheritance_collection_collection(bool async)
	{
		return base.Nested_include_with_inheritance_collection_collection(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Nested_include_with_inheritance_reference_reference_on_base(bool async)
	{
		return base.Nested_include_with_inheritance_reference_reference_on_base(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_without_inheritance_with_filter_reverse(bool async)
	{
		return base.Include_reference_without_inheritance_with_filter_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_with_inheritance(bool async)
	{
		return base.Include_collection_with_inheritance(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Nested_include_with_inheritance_reference_collection_reverse(bool async)
	{
		return base.Nested_include_with_inheritance_reference_collection_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_with_inheritance_on_derived_reverse(bool async)
	{
		return base.Include_collection_with_inheritance_on_derived_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_with_inheritance_with_filter_reverse(bool async)
	{
		return base.Include_collection_with_inheritance_with_filter_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Collection_projection_on_base_type(bool async)
	{
		return base.Collection_projection_on_base_type(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_with_inheritance_on_derived2(bool async)
	{
		return base.Include_collection_with_inheritance_on_derived2(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Nested_include_collection_reference_on_non_entity_base(bool async)
	{
		return base.Nested_include_collection_reference_on_non_entity_base(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_without_inheritance(bool async)
	{
		return base.Include_reference_without_inheritance(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_with_inheritance_on_derived_with_filter4(bool async)
	{
		return base.Include_reference_with_inheritance_on_derived_with_filter4(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_with_inheritance_on_derived1(bool async)
	{
		return base.Include_reference_with_inheritance_on_derived1(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Nested_include_with_inheritance_collection_collection_reverse(bool async)
	{
		return base.Nested_include_with_inheritance_collection_collection_reverse(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_reference_with_inheritance(bool async)
	{
		return base.Include_reference_with_inheritance(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_collection_without_inheritance_with_filter(bool async)
	{
		return base.Include_collection_without_inheritance_with_filter(async);
	}

	[Theory(Skip = SkipReasons.GeneratedNameTooLong)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Include_on_derived_type_with_queryable_Cast(bool async)
	{
		return base.Include_on_derived_type_with_queryable_Cast(async);
	}

	public class InheritanceRelationshipsQueryFbFixture : InheritanceRelationshipsQueryRelationalFixture
	{
		protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

		protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
		{
			base.OnModelCreating(modelBuilder, context);
			ModelHelpers.SetPrimaryKeyGeneration(modelBuilder);
			ModelHelpers.SimpleTableNames(modelBuilder);
		}
	}
}
