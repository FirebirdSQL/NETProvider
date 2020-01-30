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

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class GroupByQueryFbTest : GroupByQueryTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
	{
		public GroupByQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
			: base(fixture)
		{ }

		[Theory]
		[MemberData(nameof(IsAsyncData))]
		public override Task GroupBy_Property_Select_Count_with_predicate(bool async)
		{
			return Assert.ThrowsAsync<InvalidOperationException>(
				() => base.GroupBy_Property_Select_Count_with_predicate(async));
		}

		[Theory]
		[MemberData(nameof(IsAsyncData))]
		public override Task GroupBy_Property_Select_LongCount_with_predicate(bool async)
		{
			return Assert.ThrowsAsync<InvalidOperationException>(
				() => base.GroupBy_Property_Select_LongCount_with_predicate(async));
		}
	}
}
