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

using System.Reflection;
using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class AdHocQuerySplittingQueryFbTest(NonSharedFixture fixture) : AdHocQuerySplittingQueryTestBase(fixture)
{
	protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

	static readonly FieldInfo _querySplittingBehaviorFieldInfo = typeof(RelationalOptionsExtension).GetField("_querySplittingBehavior", BindingFlags.NonPublic | BindingFlags.Instance);

	protected override DbContextOptionsBuilder SetQuerySplittingBehavior(DbContextOptionsBuilder optionsBuilder, QuerySplittingBehavior splittingBehavior)
	{
		new FbDbContextOptionsBuilder(optionsBuilder).UseQuerySplittingBehavior(splittingBehavior);
		return optionsBuilder;
	}

	protected override DbContextOptionsBuilder ClearQuerySplittingBehavior(DbContextOptionsBuilder optionsBuilder)
	{
#pragma warning disable EF1001
		var extension = optionsBuilder.Options.FindExtension<FbOptionsExtension>();
		if (extension == null)
		{
			extension = new FbOptionsExtension();
		}
		else
		{
			_querySplittingBehaviorFieldInfo.SetValue(extension, null);
		}
		((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
		return optionsBuilder;
#pragma warning restore EF1001
	}

	[Fact(Skip = "Not supported because of current test initialization setup.")]
	public override Task Can_query_with_nav_collection_in_projection_with_split_query_in_parallel_async()
	{
		return base.Can_query_with_nav_collection_in_projection_with_split_query_in_parallel_async();
	}

	[Fact(Skip = "Not supported because of current test initialization setup.")]
	public override Task Can_query_with_nav_collection_in_projection_with_split_query_in_parallel_sync()
	{
		return base.Can_query_with_nav_collection_in_projection_with_split_query_in_parallel_sync();
	}
}

