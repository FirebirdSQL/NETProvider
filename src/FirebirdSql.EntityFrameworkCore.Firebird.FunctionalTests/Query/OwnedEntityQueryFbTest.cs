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
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class OwnedEntityQueryFbTest(NonSharedFixture fixture) : OwnedEntityQueryRelationalTestBase(fixture)
{
	protected override ITestStoreFactory NonSharedTestStoreFactory => FbTestStoreFactory.Instance;

	[Theory(Skip = SkipReasons.HasDataInTheSameTransactionAsDDL)]
	[MemberData(nameof(IsAsyncData))]
	public override Task Multiple_owned_reference_mapped_to_own_table_containing_owned_collection_in_split_query(bool async)
	{
		return base.Multiple_owned_reference_mapped_to_own_table_containing_owned_collection_in_split_query(async);
	}

	// Same as base, but with quoted identifiers in raw SQL.
	public override async Task Inconsistent_owned_entity_data_logs_warning_and_does_not_cause_identity_conflict()
	{
		var contextFactory = await InitializeNonSharedTest<Context38223>(
			shouldLogCategory: c => c == DbLoggerCategory.Query.Name,
			onConfiguring: b => b.ConfigureWarnings(c => c.Log(CoreEventId.InconsistentOwnedDataWarning)),
			seed: async c =>
			{
				var rootEntity = new Context38223.RootEntity
				{
					Id = Guid.NewGuid(),
					Outer = new Context38223.Outer { RequiredProperty = 1, Inner = new Context38223.Inner { InnerProperty = 42 } }
				};
				c.Add(rootEntity);
				await c.SaveChangesAsync();

				await c.Database.ExecuteSqlRawAsync(
					"""UPDATE "RootEntity" SET "Outer_RequiredProperty" = NULL WHERE "Id" = {0}""", rootEntity.Id);
			});

		using var context = contextFactory.CreateDbContext();

		ListLoggerFactory.Clear();

		var root = await context.Set<Context38223.RootEntity>().SingleAsync();

		Assert.NotNull(root);
		Assert.Null(root.Outer);

		Assert.Contains(
			ListLoggerFactory.Log,
			l => l.Id == CoreEventId.InconsistentOwnedDataWarning && l.Level == LogLevel.Warning);

		root.Outer = new Context38223.Outer { RequiredProperty = 1, Inner = new Context38223.Inner { InnerProperty = 2 } };

		await context.SaveChangesAsync();
	}
}
