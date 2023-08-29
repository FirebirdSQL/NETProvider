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

using System.Linq;
using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class NorthwindWhereQueryFbTest : NorthwindWhereQueryRelationalTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
{
	public NorthwindWhereQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
		: base(fixture)
	{ }

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	// See #24234 on efcore.
	public override async Task Like_with_non_string_column_using_ToString(bool async)
	{
		using var context = CreateContext();

		var query = context.Set<Order>().Where(o => EF.Functions.Like(o.OrderID.ToString(), "%20%"));
		var result = async ? await query.ToListAsync() : query.ToList();

		Assert.Equal(new[] { 10320, 10420, 10520, 10620, 10720, 10820, 10920, 11020 }, result.Select(e => e.OrderID).OrderBy(x => x));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	// See #24234 on efcore.
	public override async Task Like_with_non_string_column_using_double_cast(bool async)
	{
		using var context = CreateContext();

		var query = context.Set<Order>().Where(o => EF.Functions.Like((string)(object)o.OrderID, "%20%"));
		var result = async ? await query.ToListAsync() : query.ToList();

		Assert.Equal(new[] { 10320, 10420, 10520, 10620, 10720, 10820, 10920, 11020 }, result.Select(e => e.OrderID).OrderBy(x => x));
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_datetime_utcnow(bool async)
	{
		return base.Where_datetime_utcnow(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_datetimeoffset_now_component(bool async)
	{
		return base.Where_datetimeoffset_now_component(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_datetimeoffset_utcnow_component(bool async)
	{
		return base.Where_datetimeoffset_utcnow_component(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_datetimeoffset_utcnow(bool async)
	{
		return base.Where_datetimeoffset_utcnow(async);
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_bitwise_xor(bool async)
	{
		return AssertTranslationFailed(() => base.Where_bitwise_xor(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_compare_constructed_equal(bool async)
	{
		return AssertTranslationFailed(() => base.Where_compare_constructed_equal(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_compare_constructed_multi_value_equal(bool async)
	{
		return AssertTranslationFailed(() => base.Where_compare_constructed_multi_value_equal(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_compare_constructed_multi_value_not_equal(bool async)
	{
		return AssertTranslationFailed(() => base.Where_compare_constructed_multi_value_not_equal(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_compare_tuple_constructed_equal(bool async)
	{
		return AssertTranslationFailed(() => base.Where_compare_tuple_constructed_equal(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_compare_tuple_constructed_multi_value_equal(bool async)
	{
		return AssertTranslationFailed(() => base.Where_compare_tuple_constructed_multi_value_equal(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_compare_tuple_constructed_multi_value_not_equal(bool async)
	{
		return AssertTranslationFailed(() => base.Where_compare_tuple_constructed_multi_value_not_equal(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_compare_tuple_create_constructed_equal(bool async)
	{
		return AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_equal(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_compare_tuple_create_constructed_multi_value_equal(bool async)
	{
		return AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_multi_value_equal(async));
	}

	[Theory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Where_compare_tuple_create_constructed_multi_value_not_equal(bool async)
	{
		return AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_multi_value_not_equal(async));
	}
}
