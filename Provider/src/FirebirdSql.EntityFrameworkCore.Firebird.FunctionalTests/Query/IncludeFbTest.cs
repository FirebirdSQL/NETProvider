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
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class IncludeFbTest : IncludeTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
	{
		public IncludeFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
			: base(fixture)
		{ }

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(true, true)]
		// See #19222 on EntityFrameworkCore.
		public override async Task Include_is_not_ignored_when_projection_contains_client_method_and_complex_expression(
			bool useString, bool async)
		{
			using (var context = CreateContext())
			{
				var query = from e in (useString
								? context.Employees.Include(nameof(Employee.Manager))
								: context.Employees.Include(e => e.Manager))
							where e.EmployeeID == 1 || e.EmployeeID == 2
							orderby e.EmployeeID
							select e.Manager != null ? "Employee " + ClientMethod(e) : "";

				var result = async
					? await query.ToListAsync()
					: query.ToList();

				Assert.Collection(
					result,
					e => Assert.Equal("Employee Nancy reports to Andrew", e),
					e2 => Assert.Equal("", e2));
			}
		}

		private static string ClientMethod(Employee e)
			=> e.FirstName + " reports to " + e.Manager.FirstName + e.Manager.LastName;

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		// See #19363 on EntityFrameworkCore.
		public override void Include_duplicate_collection_result_operator(bool useString)
		{
			using var context = CreateContext();
			var customers
				= useString
					? (from c1 in context.Set<Customer>()
						   .Include("Orders")
						   .OrderBy(c => c.CustomerID)
						   .Take(2)
					   from c2 in context.Set<Customer>()
						   .Include("Orders")
						   .OrderBy(c => c.CustomerID)
						   .Skip(2)
						   .Take(2)
					   select new { c1, c2 })
					.OrderBy(x => x.c1.CustomerID).ThenBy(x => x.c2.CustomerID)
					.Take(1)
					.ToList()
					: (from c1 in context.Set<Customer>()
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
					.ToList();

			Assert.Single(customers);
			Assert.Equal(6, customers.SelectMany(c => c.c1.Orders).Count());
			Assert.True(customers.SelectMany(c => c.c1.Orders).All(o => o.Customer != null));
			Assert.Equal(7, customers.SelectMany(c => c.c2.Orders).Count());
			Assert.True(customers.SelectMany(c => c.c2.Orders).All(o => o.Customer != null));
			Assert.Equal(15, context.ChangeTracker.Entries().Count());

			foreach (var customer in customers.Select(e => e.c1))
			{
				CheckIsLoaded(
					context,
					customer,
					ordersLoaded: true,
					orderDetailsLoaded: false,
					productLoaded: false);
			}

			foreach (var customer in customers.Select(e => e.c2))
			{
				CheckIsLoaded(
					context,
					customer,
					ordersLoaded: true,
					orderDetailsLoaded: false,
					productLoaded: false);
			}
		}

		private static void CheckIsLoaded(
			NorthwindContext context,
			Customer customer,
			bool ordersLoaded,
			bool orderDetailsLoaded,
			bool productLoaded)
		{
			context.ChangeTracker.AutoDetectChangesEnabled = false;

			Assert.Equal(ordersLoaded, context.Entry(customer).Collection(e => e.Orders).IsLoaded);
			if (customer.Orders != null)
			{
				foreach (var order in customer.Orders)
				{
					Assert.Equal(ordersLoaded, context.Entry(order).Reference(e => e.Customer).IsLoaded);

					Assert.Equal(orderDetailsLoaded, context.Entry(order).Collection(e => e.OrderDetails).IsLoaded);
					if (order.OrderDetails != null)
					{
						foreach (var orderDetail in order.OrderDetails)
						{
							Assert.Equal(orderDetailsLoaded, context.Entry(orderDetail).Reference(e => e.Order).IsLoaded);

							Assert.Equal(productLoaded, context.Entry(orderDetail).Reference(e => e.Product).IsLoaded);
							if (orderDetail.Product != null)
							{
								Assert.False(context.Entry(orderDetail.Product).Collection(e => e.OrderDetails).IsLoaded);
							}
						}
					}
				}
			}
		}
	}
}
