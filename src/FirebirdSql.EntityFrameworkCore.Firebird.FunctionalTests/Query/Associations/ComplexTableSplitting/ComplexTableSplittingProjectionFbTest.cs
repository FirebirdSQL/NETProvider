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
using Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query.Associations.ComplexTableSplitting;

public class ComplexTableSplittingProjectionFbTest : ComplexTableSplittingProjectionRelationalTestBase<ComplexTableSplittingFbFixture>
{
	public ComplexTableSplittingProjectionFbTest(ComplexTableSplittingFbFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture, testOutputHelper)
	{
		Fixture.TestSqlLoggerFactory.Clear();
		Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
	}

	// Users OUTER join
	[Theory(Skip = SkipReasons.NotSupportedByProvider)]
	[MemberData(nameof(TrackingData))]
	public override Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
	{
		return base.Select_subquery_optional_related_FirstOrDefault(queryTrackingBehavior);
	}

	// Users OUTER join
	[Theory(Skip = SkipReasons.NotSupportedByProvider)]
	[MemberData(nameof(TrackingData))]
	public override Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
	{
		return base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior);
	}

	[Theory]
	[MemberData(nameof(TrackingData))]
	public override Task Select_subquery_FirstOrDefault_complex_collection(QueryTrackingBehavior queryTrackingBehavior)
	{
		var fbTestStore = (FbTestStore)Fixture.TestStore;
		if (fbTestStore.ServerLessThan4())
			return Task.CompletedTask;
		return base.Select_subquery_FirstOrDefault_complex_collection(queryTrackingBehavior);
	}
}
