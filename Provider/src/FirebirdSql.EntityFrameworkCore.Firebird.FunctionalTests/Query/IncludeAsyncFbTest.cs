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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class IncludeAsyncFbTest : IncludeAsyncTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
	{
		public IncludeAsyncFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
			: base(fixture)
		{ }

		[Fact]
		// See #19363 on EntityFrameworkCore.
		public override async Task Include_duplicate_collection_result_operator()
		{
			using var context = CreateContext();
			var customers
				= await (from c1 in context.Set<Customer>()
							 .Include(c => c.Orders)
							 .OrderBy(c => c.CustomerID)
							 .Take(2)
						 from c2 in context.Set<Customer>()
							 .Include(c => c.Orders)
							 .OrderBy(c => c.CustomerID)
							 .Skip(2)
							 .Take(2)
						 select new { c1, c2 })
					.OrderBy(x => x.c1.CustomerID).ThenBy(x => x.c2.CustomerID)
					.Take(1)
					.ToListAsync();

			Assert.Single(customers);
			Assert.Equal(6, customers.SelectMany(c => c.c1.Orders).Count());
			Assert.True(customers.SelectMany(c => c.c1.Orders).All(o => o.Customer != null));
			Assert.Equal(7, customers.SelectMany(c => c.c2.Orders).Count());
			Assert.True(customers.SelectMany(c => c.c2.Orders).All(o => o.Customer != null));
			Assert.Equal(15, context.ChangeTracker.Entries().Count());
		}
	}
}
