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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;
using Xunit;
using Xunit.Abstractions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query.Associations.OwnedNavigations;

public class OwnedNavigationsSetOperationsFbTest : OwnedNavigationsSetOperationsRelationalTestBase<OwnedNavigationsFbFixture>
{
	public OwnedNavigationsSetOperationsFbTest(OwnedNavigationsFbFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture, testOutputHelper)
	{
		Fixture.TestSqlLoggerFactory.Clear();
		Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
	}

	[NotSupportedByProviderTheory]
	[MemberData(nameof(TrackingData))]
	public override Task Over_associate_collection_projected(QueryTrackingBehavior queryTrackingBehavior)
	{
		return base.Over_associate_collection_projected(queryTrackingBehavior);
	}
}
