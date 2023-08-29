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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Sdk;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class NorthwindKeylessEntitiesQueryFbTest : NorthwindKeylessEntitiesQueryRelationalTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
{
	public NorthwindKeylessEntitiesQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
		: base(fixture)
	{ }

	protected override bool CanExecuteQueryString => false;

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task KeylessEntity_by_database_view(bool async)
	{
		return base.KeylessEntity_by_database_view(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Entity_mapped_to_view_on_right_side_of_join(bool async)
	{
		return base.Entity_mapped_to_view_on_right_side_of_join(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override async Task KeylessEntity_with_nav_defining_query(bool async)
	{
		Assert.Equal(
			"0",
			(await Assert.ThrowsAsync<EqualException>(
				() => base.KeylessEntity_with_nav_defining_query(async))).Actual);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Count_over_keyless_entity_with_pushdown_empty_projection(bool async)
	{
		return base.Count_over_keyless_entity_with_pushdown_empty_projection(async);
	}
}
