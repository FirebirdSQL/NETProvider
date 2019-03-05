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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public partial class SimpleQueryFbTest : SimpleQueryTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
	{
		public SimpleQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
			: base(fixture)
		{ }

		[Theory(Skip = "Different DECIMAL handling on Firebird.")]
		[MemberData(nameof(IsAsyncData))]
		public override Task Sum_with_division_on_decimal(bool isAsync)
		{
			return base.Sum_with_division_on_decimal(isAsync);
		}

		[Fact(Skip = "Missing view (and tables) for it.")]
		public override void Query_backed_by_database_view()
		{
			base.Query_backed_by_database_view();
		}

		[Theory(Skip = "Wrong ordering (see #14874 on EntityFrameworkCore).")]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_subquery_correlated_client_eval(bool isAsync)
		{
			return base.Where_subquery_correlated_client_eval(isAsync);
		}
	}
}
