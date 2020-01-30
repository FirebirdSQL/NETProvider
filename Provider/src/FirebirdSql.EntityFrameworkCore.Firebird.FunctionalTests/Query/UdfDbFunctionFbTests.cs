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
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class UdfDbFunctionFbTests : UdfDbFunctionTestBase<UdfDbFunctionFbTests.Fb>
	{
		public UdfDbFunctionFbTests(Fb fixture)
			: base(fixture)
		{ }

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
					.HasTranslation(args => SqlFunctionExpression.Create((string)null, "IsDate", args, isDateMethodInfo.ReturnType, null));
				var isDateMethodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(IsDateInstance));
				modelBuilder.HasDbFunction(isDateMethodInfo2)
					.HasTranslation(args => SqlFunctionExpression.Create((string)null, "IsDate", args, isDateMethodInfo2.ReturnType, null));

				var methodInfo = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthStatic));
				modelBuilder.HasDbFunction(methodInfo)
					.HasTranslation(args => SqlFunctionExpression.Create("char_length", args, methodInfo.ReturnType, null));
				var methodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthInstance));
				modelBuilder.HasDbFunction(methodInfo2)
					.HasTranslation(args => SqlFunctionExpression.Create("char_length", args, methodInfo2.ReturnType, null));

				modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerWithMostOrdersAfterDateStatic)))
					.HasName("GetCustWithMostOrdersAfterDate");
				modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerWithMostOrdersAfterDateInstance)))
					.HasName("GetCustWithMostOrdersAfterDate");

				modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IdentityString)))
					.HasSchema(null);
			}
		}


		public class Fb : UdfFixtureBase
		{
			protected override string StoreName { get; } = nameof(UdfDbFunctionFbTests);
			protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;
			protected override Type ContextType { get; } = typeof(FbUDFSqlContext);

			protected override void Seed(DbContext context)
			{
				base.Seed(context);

				context.Database.ExecuteSqlRaw(
					@"create function ""CustomerOrderCount"" (customerId int)
                                                    returns int
                                                    as
                                                    begin
                                                        return (select count(""Id"") from ""Orders"" where ""CustomerId"" = :customerId);
                                                    end");

				context.Database.ExecuteSqlRaw(
					@"create function ""StarValue"" (starCount int, val varchar(1000))
                                                    returns varchar(1000)
                                                    as
                                                    begin
                                                        return rpad('', :starCount, '*') || :val;
                                                    end");

				context.Database.ExecuteSqlRaw(
					@"create function ""DollarValue"" (starCount int, val varchar(1000))
                                                    returns varchar(1000)
                                                    as
                                                    begin
                                                        return rpad('', :starCount, '$') || :val;
                                                    end");

				context.Database.ExecuteSqlRaw(
					@"create function ""GetReportingPeriodStartDate"" (period int)
                                                    returns timestamp
                                                    as
                                                    begin
                                                        return cast('1998-01-01' as timestamp);
                                                    end");

				context.Database.ExecuteSqlRaw(
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

				context.Database.ExecuteSqlRaw(
					@"create function ""IsTopCustomer"" (customerId int)
                                                    returns boolean
                                                    as
                                                    begin
                                                        if (:customerId = 1) then
                                                            return true;

                                                        return false;
                                                    end");

				context.Database.ExecuteSqlRaw(
					@"create function ""IdentityString"" (customerName varchar(1000))
                                                    returns varchar(1000)
                                                    as
                                                    begin
                                                        return :customerName;
                                                    end");

				context.Database.ExecuteSqlRaw(
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

				context.SaveChanges();
			}
		}
	}
}
