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

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class AsyncGearsOfWarQueryFbTest : AsyncGearsOfWarQueryTestBase<GearsOfWarQueryFbFixture>
	{
		public AsyncGearsOfWarQueryFbTest(GearsOfWarQueryFbFixture fixture)
			: base(fixture)
		{ }

		// the original implementation has wrong assert
		public override async Task GroupBy_Select_sum()
		{
			using var ctx = CreateContext();
			var result = await ctx.Missions.GroupBy(m => m.CodeName).Select(g => g.Sum(m => m.Rating)).ToListAsync();
			Assert.Equal(6.3, result.Sum() ?? double.NaN, precision: 1);
		}
	}
}
