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

//$Authors = Niek Schoemaker (@niekschoemaker)

using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;
using Xunit;
using Xunit.Abstractions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query.Associations.OwnedNavigations;

public class OwnedNavigationsCollectionFbTest : OwnedNavigationsCollectionRelationalTestBase<OwnedNavigationsFbFixture>
{
	public OwnedNavigationsCollectionFbTest(OwnedNavigationsFbFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture, testOutputHelper)
	{
		Fixture.TestSqlLoggerFactory.Clear();
		Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
	}

	[Theory]
	[MemberData(nameof(TrackingData))]
	public override Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Distinct_projected(queryTrackingBehavior);
	}
}
