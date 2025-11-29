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
using Microsoft.EntityFrameworkCore.Query;
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
