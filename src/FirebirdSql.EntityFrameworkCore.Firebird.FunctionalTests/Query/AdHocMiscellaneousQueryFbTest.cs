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
	protected override ITestStoreFactory NonSharedTestStoreFactory => FbTestStoreFactory.Instance;

	protected override DbContextOptionsBuilder SetParameterizedCollectionMode(DbContextOptionsBuilder optionsBuilder,
		ParameterTranslationMode parameterizedCollectionMode)
	{
		new FbDbContextOptionsBuilder(optionsBuilder).UseParameterizedCollectionMode(parameterizedCollectionMode);
		return optionsBuilder;
	}

	[Theory(Skip = SkipReasons.DoesNotHaveTheData)]
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

	[Theory(Skip = SkipReasons.HasDataInTheSameTransactionAsDDL)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Comparing_byte_column_to_enum_in_vb_creating_double_cast(bool async)
	{
		return base.Comparing_byte_column_to_enum_in_vb_creating_double_cast(async);
	}

	[Theory(Skip = SkipReasons.HasDataInTheSameTransactionAsDDL)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Comparing_enum_casted_to_byte_with_int_constant(bool async)
	{
		return base.Comparing_enum_casted_to_byte_with_int_constant(async);
	}

	[Theory(Skip = SkipReasons.HasDataInTheSameTransactionAsDDL)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Comparing_enum_casted_to_byte_with_int_parameter(bool async)
	{
		return base.Comparing_enum_casted_to_byte_with_int_parameter(async);
	}

	[Theory(Skip = SkipReasons.HasDataInTheSameTransactionAsDDL)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Null_check_removal_in_ternary_maintain_appropriate_cast(bool async)
	{
		return base.Null_check_removal_in_ternary_maintain_appropriate_cast(async);
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Operators_combine_nullability_of_entity_shapers()
	{
		return base.Operators_combine_nullability_of_entity_shapers();
	}

	[Theory(Skip = SkipReasons.NotSupportedByProvider)]
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

	// Firebird supports LATERAL, hence the query is translated (base test expects it to throw).
	public override async Task Correlated_SelectMany_DefaultIfEmpty_whole_object()
	{
		var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
		// LATERAL is Firebird 4+
		var fbTestStore = (FbTestStore)NonSharedTestStore;
		if (fbTestStore.ServerLessThan4())
			return;
		using var context = contextFactory.CreateDbContext();

		var query = from s in context.Statuses
					from countInfo in context.Requests
						.Where(r => r.PickupStatusId == s.PickupStatusId)
						.GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() })
						.DefaultIfEmpty()
					orderby s.PickupStatusId
					select new { s.PickupStatusId, countInfo };

		var result = await query.ToListAsync();

		Assert.Equal(3, result.Count);

		Assert.Equal(1, result[0].PickupStatusId);
		Assert.NotNull(result[0].countInfo);
		Assert.Equal(1, result[0].countInfo.pickupStatusId);
		Assert.Equal(2, result[0].countInfo.Count);

		Assert.Equal(2, result[1].PickupStatusId);
		Assert.Null(result[1].countInfo);

		Assert.Equal(3, result[2].PickupStatusId);
		Assert.NotNull(result[2].countInfo);
		Assert.Equal(3, result[2].countInfo.pickupStatusId);
		Assert.Equal(1, result[2].countInfo.Count);
	}

	protected override async Task Seed30915(Context30915 context)
	{
		context.Statuses.AddRange(
			new Context30915.PickupStatus30915 { PickupStatusId = 1, Name = "Active" },
			new Context30915.PickupStatus30915 { PickupStatusId = 2, Name = "NoRequests" },
			new Context30915.PickupStatus30915 { PickupStatusId = 3, Name = "Busy" });

		context.Requests.AddRange(
			new Context30915.PickupRequest30915 { PickupStatusId = 1, Priority = 5 },
			new Context30915.PickupRequest30915 { PickupStatusId = 1, Priority = null },
			new Context30915.PickupRequest30915 { PickupStatusId = 3, Priority = 7 });

		await context.SaveChangesAsync();
	}
}
