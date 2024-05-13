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
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class OptionalDependentQueryFbTest : OptionalDependentQueryTestBase<OptionalDependentQueryFbFixture>
{
	public OptionalDependentQueryFbTest(OptionalDependentQueryFbFixture fixture)
		: base(fixture)
	{ }

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Basic_projection_entity_with_all_optional(bool async)
	{
		return base.Basic_projection_entity_with_all_optional(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Basic_projection_entity_with_some_required(bool async)
	{
		return base.Basic_projection_entity_with_some_required(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Filter_nested_optional_dependent_with_all_optional_compared_to_not_null(bool async)
	{
		return base.Filter_nested_optional_dependent_with_all_optional_compared_to_not_null(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Filter_nested_optional_dependent_with_all_optional_compared_to_null(bool async)
	{
		return base.Filter_nested_optional_dependent_with_all_optional_compared_to_null(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Filter_nested_optional_dependent_with_some_required_compared_to_not_null(bool async)
	{
		return base.Filter_nested_optional_dependent_with_some_required_compared_to_not_null(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Filter_nested_optional_dependent_with_some_required_compared_to_null(bool async)
	{
		return base.Filter_nested_optional_dependent_with_some_required_compared_to_null(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Filter_optional_dependent_with_all_optional_compared_to_not_null(bool async)
	{
		return base.Filter_optional_dependent_with_all_optional_compared_to_not_null(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Filter_optional_dependent_with_all_optional_compared_to_null(bool async)
	{
		return base.Filter_optional_dependent_with_all_optional_compared_to_null(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Filter_optional_dependent_with_some_required_compared_to_not_null(bool async)
	{
		return base.Filter_optional_dependent_with_some_required_compared_to_not_null(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Filter_optional_dependent_with_some_required_compared_to_null(bool async)
	{
		return base.Filter_optional_dependent_with_some_required_compared_to_null(async);
	}
}
