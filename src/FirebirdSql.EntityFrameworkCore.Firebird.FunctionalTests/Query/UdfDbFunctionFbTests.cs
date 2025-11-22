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
using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class UdfDbFunctionFbTests : UdfDbFunctionTestBase<UdfDbFunctionFbTests.Fb>
{
	public UdfDbFunctionFbTests(Fb fixture)
		: base(fixture)
	{ }

	[NotSupportedOnFirebirdFact]
	public override void QF_CrossApply_Correlated_Select_Anonymous()
	{
		base.QF_CrossApply_Correlated_Select_Anonymous();
	}

	[NotSupportedOnFirebirdFact]
	public override void QF_OuterApply_Correlated_Select_QF()
	{
		base.QF_OuterApply_Correlated_Select_QF();
	}

	[NotSupportedOnFirebirdFact]
	public override void Udf_with_argument_being_comparison_of_nullable_columns()
	{
		base.Udf_with_argument_being_comparison_of_nullable_columns();
	}

	[Fact]
	public override void QF_Select_Correlated_Subquery_In_Anonymous_MultipleCollections()
	{
		base.QF_Select_Correlated_Subquery_In_Anonymous_MultipleCollections();
	}

	[NotSupportedOnFirebirdFact]
	public override void QF_CrossApply_Correlated_Select_Result()
	{
		base.QF_CrossApply_Correlated_Select_Result();
	}

	[NotSupportedOnFirebirdFact]
	public override void QF_Select_Correlated_Subquery_In_Anonymous()
	{
		base.QF_Select_Correlated_Subquery_In_Anonymous();
	}

	[NotSupportedOnFirebirdFact]
	public override void QF_Correlated_Func_Call_With_Navigation()
	{
		base.QF_Correlated_Func_Call_With_Navigation();
	}

	[NotSupportedOnFirebirdFact]
	public override void QF_Select_Correlated_Direct_With_Function_Query_Parameter_Correlated_In_Anonymous()
	{
		base.QF_Select_Correlated_Direct_With_Function_Query_Parameter_Correlated_In_Anonymous();
	}

	[NotSupportedOnFirebirdFact]
	public override void QF_OuterApply_Correlated_Select_Entity()
	{
		base.QF_OuterApply_Correlated_Select_Entity();
	}

	[NotSupportedOnFirebirdFact]
	public override void QF_Correlated_Nested_Func_Call()
	{
		base.QF_Correlated_Nested_Func_Call();
	}

	[NotSupportedOnFirebirdFact]
	public override void QF_OuterApply_Correlated_Select_Anonymous()
	{
		base.QF_OuterApply_Correlated_Select_Anonymous();
	}

	[NotSupportedOnFirebirdFact]
	public override void QF_Select_Correlated_Subquery_In_Anonymous_Nested_With_QF()
	{
		base.QF_Select_Correlated_Subquery_In_Anonymous_Nested_With_QF();
	}

	[NotSupportedOnFirebirdFact]
	public override void QF_Correlated_Select_In_Anonymous()
	{
		base.QF_Correlated_Select_In_Anonymous();
	}

	[NotSupportedOnFirebirdFact]
	public override void QF_CrossApply_Correlated_Select_QF_Type()
	{
		base.QF_CrossApply_Correlated_Select_QF_Type();
	}

	[NotSupportedOnFirebirdFact]
	public override void Udf_with_argument_being_comparison_to_null_parameter()
	{
		base.Udf_with_argument_being_comparison_to_null_parameter();
	}

	[DoesNotHaveTheDataFact]
	public override void QF_CrossJoin_Not_Correlated()
	{
		base.QF_CrossJoin_Not_Correlated();
	}

	[DoesNotHaveTheDataFact]
	public override void DbSet_mapped_to_function()
	{
		base.DbSet_mapped_to_function();
	}

	[DoesNotHaveTheDataFact]
	public override void QF_LeftJoin_Select_Result()
	{
		base.QF_LeftJoin_Select_Result();
	}

	[DoesNotHaveTheDataFact]
	public override void QF_Join()
	{
		base.QF_Join();
	}

	[DoesNotHaveTheDataFact]
	public override void QF_LeftJoin_Select_Anonymous()
	{
		base.QF_LeftJoin_Select_Anonymous();
	}

	[DoesNotHaveTheDataFact]
	public override void QF_Stand_Alone_Parameter()
	{
		base.QF_Stand_Alone_Parameter();
	}

	[DoesNotHaveTheDataFact]
	public override void QF_Stand_Alone()
	{
		base.QF_Stand_Alone();
	}

	[DoesNotHaveTheDataFact]
	public override void QF_Nested()
	{
		base.QF_Nested();
	}

	[DoesNotHaveTheDataFact]
	public override void QF_CrossJoin_Parameter()
	{
		base.QF_CrossJoin_Parameter();
	}

	protected class FbUDFSqlContext : UDFSqlContext
	{
		public FbUDFSqlContext(DbContextOptions options)
			: base(options)
		{ }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			var isDateMethodInfo = typeof(UDFSqlContext).GetMethod(nameof(IsDateStatic));
			modelBuilder.HasDbFunction(isDateMethodInfo)
				.HasTranslation(args => new SqlFunctionExpression(null, "IsDate", args, true, new[] { true }, isDateMethodInfo.ReturnType, null));
			var isDateMethodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(IsDateInstance));
			modelBuilder.HasDbFunction(isDateMethodInfo2)
				.HasTranslation(args => new SqlFunctionExpression(null, "IsDate", args, true, new[] { true }, isDateMethodInfo2.ReturnType, null));

			var methodInfo = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthStatic));
			modelBuilder.HasDbFunction(methodInfo)
				.HasTranslation(args => new SqlFunctionExpression("char_length", args, true, new[] { true }, methodInfo.ReturnType, null));
			var methodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthInstance));
			modelBuilder.HasDbFunction(methodInfo2)
				.HasTranslation(args => new SqlFunctionExpression("char_length", args, true, new[] { true }, methodInfo2.ReturnType, null));
			var methodInfo3 = typeof(UDFSqlContext).GetMethod(nameof(StringLength));
			modelBuilder.HasDbFunction(methodInfo3)
				.HasTranslation(args => new SqlFunctionExpression("char_length", args, true, new[] { true }, methodInfo3.ReturnType, null));

			modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerWithMostOrdersAfterDateStatic)))
				.HasName("GetCustWithMostOrdersAfterDate");
			modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerWithMostOrdersAfterDateInstance)))
				.HasName("GetCustWithMostOrdersAfterDate");

			modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IdentityString)))
				.HasSchema(null);

			ModelHelpers.SetPrimaryKeyGeneration(modelBuilder);
		}
	}


	public class Fb : UdfFixtureBase
	{
		protected override string StoreName { get; } = nameof(UdfDbFunctionFbTests);
		protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;
		protected override Type ContextType { get; } = typeof(FbUDFSqlContext);

		protected override async Task SeedAsync(DbContext context)
		{
			await base.SeedAsync(context);

			await context.Database.ExecuteSqlRawAsync(
				@"create function ""CustomerOrderCount"" (customerId int)
                                                    returns int
                                                    as
                                                    begin
                                                        return (select count(""Id"") from ""Orders"" where ""CustomerId"" = :customerId);
                                                    end");

			await context.Database.ExecuteSqlRawAsync(
				@"create function ""StarValue"" (starCount int, val varchar(1000))
                                                    returns varchar(1000)
                                                    as
                                                    begin
                                                        return rpad('', :starCount, '*') || :val;
                                                    end");

			await context.Database.ExecuteSqlRawAsync(
				@"create function ""DollarValue"" (starCount int, val varchar(1000))
                                                    returns varchar(1000)
                                                    as
                                                    begin
                                                        return rpad('', :starCount, '$') || :val;
                                                    end");

			await context.Database.ExecuteSqlRawAsync(
				@"create function ""GetReportingPeriodStartDate"" (period int)
                                                    returns timestamp
                                                    as
                                                    begin
                                                        return cast('1998-01-01' as timestamp);
                                                    end");

			await context.Database.ExecuteSqlRawAsync(
				@"create function ""GetCustWithMostOrdersAfterDate"" (searchDate Date)
                                                    returns int
                                                    as
                                                    begin
                                                        return (select first 1 ""CustomerId""
                                                                from ""Orders""
                                                                where ""OrderDate"" > :searchDate
                                                                group by ""CustomerId""
                                                                order by count(""Id"") desc);
                                                    end");

			await context.Database.ExecuteSqlRawAsync(
				@"create function ""IsTopCustomer"" (customerId int)
                                                    returns boolean
                                                    as
                                                    begin
                                                        if (:customerId = 1) then
                                                            return true;

                                                        return false;
                                                    end");

			await context.Database.ExecuteSqlRawAsync(
				@"create function ""IdentityString"" (customerName varchar(1000))
                                                    returns varchar(1000)
                                                    as
                                                    begin
                                                        return :customerName;
                                                    end");

			await context.Database.ExecuteSqlRawAsync(
				@"create function ""IsDate"" (val varchar(1000))
                                                    returns boolean
                                                    as
                                                    declare dummy date;
                                                    begin
                                                        begin
                                                            begin
                                                                dummy = cast(val as date);
                                                            end
                                                            when any do
                                                            begin
                                                                return false;
                                                            end
                                                        end
                                                        return true;
                                                    end");

			await context.Database.ExecuteSqlRawAsync(
				@"create procedure ""GetTopTwoSellingProducts""
                                                    returns
                                                    (
                                                        ""ProductId"" int not null,
                                                        ""AmountSold"" int
                                                    )
                                                    as
                                                    begin
                                                        for select first 2 ""ProductId"", sum(""Quantity"") as ""TotalSold""
                                                        from ""LineItem""
                                                        group by ""ProductId""
                                                        order by ""TotalSold"" desc
                                                        into :""ProductId"", :""AmountSold"" do
                                                        begin
                                                            suspend;
                                                        end
                                                    end");

			await context.Database.ExecuteSqlRawAsync(
				@"create procedure ""GetOrdersWithMultipleProducts""(customerId int)
                                                    returns
                                                    (
                                                        ""OrderId"" int not null,
                                                        ""CustomerId"" int not null,
                                                        ""OrderDate"" timestamp
                                                    )
                                                    as
                                                    begin
                                                        for select o.""Id"", :customerId, ""OrderDate""
                                                        from ""Orders"" o
                                                        join ""LineItem"" li on o.""Id"" = li.""OrderId""
                                                        where o.""CustomerId"" = :customerId
                                                        group by o.""Id"", ""OrderDate""
                                                        having count(""ProductId"") > 1
                                                        into :""OrderId"", :""CustomerId"", :""OrderDate"" do
                                                        begin
                                                            suspend;
                                                        end
                                                    end");

			await context.SaveChangesAsync();
		}
	}
}
