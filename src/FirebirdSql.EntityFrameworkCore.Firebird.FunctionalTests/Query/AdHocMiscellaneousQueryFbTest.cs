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
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class AdHocMiscellaneousQueryFbTest(NonSharedFixture fixture) : AdHocMiscellaneousQueryRelationalTestBase(fixture)
{
	protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

	protected override DbContextOptionsBuilder SetParameterizedCollectionMode(DbContextOptionsBuilder optionsBuilder,
		ParameterTranslationMode parameterizedCollectionMode)
	{
		new FbDbContextOptionsBuilder(optionsBuilder).UseParameterizedCollectionMode(parameterizedCollectionMode);
		return optionsBuilder;
	}

	[DoesNotHaveTheDataTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task StoreType_for_UDF_used(bool async)
	{
		return base.StoreType_for_UDF_used(async);
	}

	[Theory(Skip = "Not interesting for Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multiple_different_entity_type_from_different_namespaces(bool async)
	{
		return base.Multiple_different_entity_type_from_different_namespaces(async);
	}

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Comparing_byte_column_to_enum_in_vb_creating_double_cast(bool async)
	{
		return base.Comparing_byte_column_to_enum_in_vb_creating_double_cast(async);
	}

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Comparing_enum_casted_to_byte_with_int_constant(bool async)
	{
		return base.Comparing_enum_casted_to_byte_with_int_constant(async);
	}

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Comparing_enum_casted_to_byte_with_int_parameter(bool async)
	{
		return base.Comparing_enum_casted_to_byte_with_int_parameter(async);
	}

	[HasDataInTheSameTransactionAsDDLTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Null_check_removal_in_ternary_maintain_appropriate_cast(bool async)
	{
		return base.Null_check_removal_in_ternary_maintain_appropriate_cast(async);
	}

	[NotSupportedOnFirebirdFact]
	public override Task Operators_combine_nullability_of_entity_shapers()
	{
		return base.Operators_combine_nullability_of_entity_shapers();
	}

	[NotSupportedByProviderTheory]
	[MemberData(nameof(InlinedRedactingData))]
	public override Task Check_inlined_constants_redacting(bool async, bool enableSensitiveDataLogging)
	{
		return base.Check_inlined_constants_redacting(async, enableSensitiveDataLogging);
	}

	protected override async Task Seed2951(Context2951 context)
	{
		await context.Database.ExecuteSqlRawAsync("""CREATE TABLE "ZeroKey" ("Id" INT)""");
		await context.Database.ExecuteSqlRawAsync("""INSERT INTO "ZeroKey" VALUES (NULL)""");
	}
}
