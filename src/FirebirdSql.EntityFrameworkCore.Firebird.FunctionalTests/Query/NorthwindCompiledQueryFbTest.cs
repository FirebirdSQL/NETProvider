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

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class NorthwindCompiledQueryFbTest : NorthwindCompiledQueryTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
{
	public NorthwindCompiledQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
		: base(fixture)
	{ }

	[Fact]
	public override void MakeBinary_does_not_throw_for_unsupported_operator()
	{
		Assert.Equal(
			   CoreStrings.TranslationFailed("DbSet<Customer>()    .Where(c => c.CustomerID == (string)(__parameters[0]))"),
			   Assert.Throws<InvalidOperationException>(
				   () => base.MakeBinary_does_not_throw_for_unsupported_operator()).Message.Replace("\r", "").Replace("\n", ""));
	}

	[Fact]
	public override void Query_with_array_parameter()
	{
		var query = EF.CompileQuery(
			(NorthwindContext context, string[] args)
				=> context.Customers.Where(c => c.CustomerID == args[0]));

		using (var context = CreateContext())
		{
			Assert.Equal(
				CoreStrings.TranslationFailed("DbSet<Customer>()    .Where(c => c.CustomerID == __args[0])"),
				Assert.Throws<InvalidOperationException>(
					() => query(context, new[] { "ALFKI" }).First().CustomerID).Message.Replace("\r", "").Replace("\n", ""));
		}

		using (var context = CreateContext())
		{
			Assert.Equal(
				CoreStrings.TranslationFailed("DbSet<Customer>()    .Where(c => c.CustomerID == __args[0])"),
				Assert.Throws<InvalidOperationException>(
					() => query(context, new[] { "ANATR" }).First().CustomerID).Message.Replace("\r", "").Replace("\n", ""));
		}
	}

	[Fact]
	public override async Task Query_with_array_parameter_async()
	{
		var query = EF.CompileAsyncQuery(
						(NorthwindContext context, string[] args)
							=> context.Customers.Where(c => c.CustomerID == args[0]));

		using (var context = CreateContext())
		{
			Assert.Equal(
				CoreStrings.TranslationFailed("DbSet<Customer>()    .Where(c => c.CustomerID == __args[0])"),
				(await Assert.ThrowsAsync<InvalidOperationException>(
					() => Enumerate(query(context, new[] { "ALFKI" })))).Message.Replace("\r", "").Replace("\n", ""));
		}

		using (var context = CreateContext())
		{
			Assert.Equal(
				CoreStrings.TranslationFailed("DbSet<Customer>()    .Where(c => c.CustomerID == __args[0])"),
				(await Assert.ThrowsAsync<InvalidOperationException>(
					() => Enumerate(query(context, new[] { "ANATR" })))).Message.Replace("\r", "").Replace("\n", ""));
		}
	}

}
