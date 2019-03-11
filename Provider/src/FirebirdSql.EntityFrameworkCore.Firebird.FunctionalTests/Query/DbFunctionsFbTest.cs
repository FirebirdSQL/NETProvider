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

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class DbFunctionsFbTest : DbFunctionsTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
	{
		public DbFunctionsFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
			: base(fixture)
		{ }

		public override void Like_literal()
		{
			// yeah, becase MS SQL and it's default collate
			// and nobody in EF Core team cares
			using (var context = CreateContext())
			{
				var count = context.Customers.Count(c => EF.Functions.Like(c.ContactName, "%M%"));
				Assert.Equal(19, count);
			}
		}
	}
}
