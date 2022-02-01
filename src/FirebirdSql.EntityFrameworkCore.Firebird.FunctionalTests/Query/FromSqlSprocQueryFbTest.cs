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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class FromSqlSprocQueryFbTest : FromSqlSprocQueryTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
{
	public FromSqlSprocQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
		: base(fixture)
	{ }

	protected override string TenMostExpensiveProductsSproc => throw new NotSupportedException();
	protected override string CustomerOrderHistorySproc => throw new NotSupportedException();

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_select_and_stored_procedure(bool async)
	{
		return base.From_sql_queryable_select_and_stored_procedure(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_select_and_stored_procedure_on_client(bool async)
	{
		return base.From_sql_queryable_select_and_stored_procedure_on_client(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure(bool async)
	{
		return base.From_sql_queryable_stored_procedure(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_and_select(bool async)
	{
		return base.From_sql_queryable_stored_procedure_and_select(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_and_select_on_client(bool async)
	{
		return base.From_sql_queryable_stored_procedure_and_select_on_client(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_composed(bool async)
	{
		return base.From_sql_queryable_stored_procedure_composed(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_composed_on_client(bool async)
	{
		return base.From_sql_queryable_stored_procedure_composed_on_client(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_min(bool async)
	{
		return base.From_sql_queryable_stored_procedure_min(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_min_on_client(bool async)
	{
		return base.From_sql_queryable_stored_procedure_min_on_client(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_projection(bool async)
	{
		return base.From_sql_queryable_stored_procedure_projection(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_re_projection(bool async)
	{
		return base.From_sql_queryable_stored_procedure_re_projection(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_re_projection_on_client(bool async)
	{
		return base.From_sql_queryable_stored_procedure_re_projection_on_client(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_take(bool async)
	{
		return base.From_sql_queryable_stored_procedure_take(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_take_on_client(bool async)
	{
		return base.From_sql_queryable_stored_procedure_take_on_client(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_with_include_throws(bool async)
	{
		return base.From_sql_queryable_stored_procedure_with_include_throws(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_with_parameter(bool async)
	{
		return base.From_sql_queryable_stored_procedure_with_parameter(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_with_parameter_composed(bool async)
	{
		return base.From_sql_queryable_stored_procedure_with_parameter_composed(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_with_parameter_composed_on_client(bool async)
	{
		return base.From_sql_queryable_stored_procedure_with_parameter_composed_on_client(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_with_tag(bool async)
	{
		return base.From_sql_queryable_stored_procedure_with_tag(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_with_multiple_stored_procedures(bool async)
	{
		return base.From_sql_queryable_with_multiple_stored_procedures(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_with_multiple_stored_procedures_on_client(bool async)
	{
		return base.From_sql_queryable_with_multiple_stored_procedures_on_client(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_with_caller_info_tag(bool async)
	{
		return base.From_sql_queryable_stored_procedure_with_caller_info_tag(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_with_caller_info_tag_and_other_tags(bool async)
	{
		return base.From_sql_queryable_stored_procedure_with_caller_info_tag_and_other_tags(async);
	}

	[DoesNotHaveTheDataTheory]
	[InlineData(false)]
	[InlineData(true)]
	public override Task From_sql_queryable_stored_procedure_with_tags(bool async)
	{
		return base.From_sql_queryable_stored_procedure_with_tags(async);
	}
}
