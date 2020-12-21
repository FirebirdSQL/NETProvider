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
	public class AsyncSimpleQueryFbTest : AsyncSimpleQueryTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
    {
		public AsyncSimpleQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
			: base(fixture)
		{ }

		[Fact(Skip = "Missing view (and tables) for it.")]
		public override Task Query_backed_by_database_view()
		{
			return base.Query_backed_by_database_view();
		}

		[NotSupportedOnFirebirdFact]
		public override Task Intersect_non_entity()
		{
			return base.Intersect_non_entity();
		}

		[NotSupportedOnFirebirdFact]
		public override Task Except_non_entity()
		{
			return base.Except_non_entity();
		}

		[Fact(Skip = "Cancellation leaves the connection in undefined state for later.")]
		// See #23925 on EntityFrameworkCore.
		public override Task ToListAsync_can_be_canceled()
		{
			return base.ToListAsync_can_be_canceled();
		}
	}
}
