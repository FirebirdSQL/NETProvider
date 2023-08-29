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
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class Ef6GroupByFbTest : Ef6GroupByTestBase<Ef6GroupByFbTest.Ef6GroupByFbFixture>
{
	public Ef6GroupByFbTest(Ef6GroupByFbFixture fixture)
		: base(fixture)
	{ }

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Group_Join_from_LINQ_101(bool async)
	{
		return base.Group_Join_from_LINQ_101(async);
	}

	[Theory(Skip = "Different math on Firebird.")]
	[MemberData(nameof(IsAsyncData))]
	public override Task Average_Grouped_from_LINQ_101(bool async)
	{
		return base.Average_Grouped_from_LINQ_101(async);
	}

	public class Ef6GroupByFbFixture : Ef6GroupByFixtureBase
	{
		protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

		protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
		{
			base.OnModelCreating(modelBuilder, context);
			ModelHelpers.SetStringLengths(modelBuilder);
		}
	}
}
