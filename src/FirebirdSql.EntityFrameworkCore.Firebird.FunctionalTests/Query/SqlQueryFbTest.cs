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

using System.Data.Common;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class SqlQueryFbTest : SqlQueryTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
{
	public SqlQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
		: base(fixture)
	{ }

	// Uses INTERSECT
	[NotSupportedByProviderTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multiple_occurrences_of_SqlQuery_with_db_parameter_adds_two_parameters(bool async)
	{
		return base.Multiple_occurrences_of_SqlQuery_with_db_parameter_adds_two_parameters(async);
	}

	[Theory(Skip = "Provider does the casting.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Bad_data_error_handling_invalid_cast(bool async)
	{
		return base.Bad_data_error_handling_invalid_cast(async);
	}

	[Theory(Skip = "Provider does the casting.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Bad_data_error_handling_invalid_cast_key(bool async)
	{
		return base.Bad_data_error_handling_invalid_cast_key(async);
	}

	[Theory(Skip = "Provider does the casting.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Bad_data_error_handling_invalid_cast_no_tracking(bool async)
	{
		return base.Bad_data_error_handling_invalid_cast_no_tracking(async);
	}

	[Theory(Skip = "Provider does the casting.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Bad_data_error_handling_invalid_cast_projection(bool async)
	{
		return base.Bad_data_error_handling_invalid_cast_projection(async);
	}

	[Theory(Skip = "Provider does the casting.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task SqlQueryRaw_queryable_simple_projection_composed(bool async)
	{
		return base.SqlQueryRaw_queryable_simple_projection_composed(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SqlQueryRaw_with_dbParameter_mixed_in_subquery(bool async)
	{
		return base.SqlQueryRaw_with_dbParameter_mixed_in_subquery(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SqlQueryRaw_queryable_multiple_composed_with_parameters_and_closure_parameters(bool async)
	{
		return base.SqlQueryRaw_queryable_multiple_composed_with_parameters_and_closure_parameters(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SqlQueryRaw_queryable_multiple_composed_with_closure_parameters(bool async)
	{
		return base.SqlQueryRaw_queryable_multiple_composed_with_closure_parameters(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SqlQueryRaw_queryable_multiple_composed(bool async)
	{
		return base.SqlQueryRaw_queryable_multiple_composed(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SqlQueryRaw_in_subquery_with_positional_dbParameter_without_name(bool async)
	{
		return base.SqlQueryRaw_in_subquery_with_positional_dbParameter_without_name(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SqlQueryRaw_in_subquery_with_positional_dbParameter_with_name(bool async)
	{
		return base.SqlQueryRaw_in_subquery_with_positional_dbParameter_with_name(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SqlQueryRaw_in_subquery_with_dbParameter(bool async)
	{
		return base.SqlQueryRaw_in_subquery_with_dbParameter(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SqlQueryRaw_does_not_parameterize_interpolated_string(bool async)
	{
		return base.SqlQueryRaw_does_not_parameterize_interpolated_string(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SqlQueryRaw_queryable_with_null_parameter(bool async)
	{
		return base.SqlQueryRaw_queryable_with_null_parameter(async);
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task SqlQuery_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated(bool async)
	{
		return base.SqlQuery_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated(async);
	}

	[Theory(Skip = "Firebird matches the casing exactly. Frankly the test is weird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task SqlQueryRaw_queryable_simple_different_cased_columns_and_not_enough_columns_throws(bool async)
	{
		return base.SqlQueryRaw_queryable_simple_different_cased_columns_and_not_enough_columns_throws(async);
	}

	protected override DbParameter CreateDbParameter(string name, object value)
		=> new FbParameter { ParameterName = name, Value = value };
}
