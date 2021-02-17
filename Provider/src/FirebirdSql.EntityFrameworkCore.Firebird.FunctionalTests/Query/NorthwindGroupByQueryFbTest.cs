/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class NorthwindGroupByQueryFbTest : NorthwindGroupByQueryTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
	{
		public NorthwindGroupByQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
			: base(fixture)
		{ }

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Select_uncorrelated_collection_with_groupby_works(bool async)
		{
			return base.Select_uncorrelated_collection_with_groupby_works(async);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Select_uncorrelated_collection_with_groupby_multiple_collections_work(bool async)
		{
			return base.Select_uncorrelated_collection_with_groupby_multiple_collections_work(async);
		}

		[Theory(Skip = "efcore#19027")]
		[MemberData(nameof(IsAsyncData))]
		public override Task GroupBy_scalar_subquery(bool async)
		{
			return base.GroupBy_scalar_subquery(async);
		}
	}
}
