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
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class GearsOfWarQueryFbTest : GearsOfWarQueryTestBase<GearsOfWarQueryFbFixture>
	{
		public GearsOfWarQueryFbTest(GearsOfWarQueryFbFixture fixture)
			: base(fixture)
		{ }

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task DateTimeOffset_DateAdd_AddDays(bool isAsync)
		{
			return base.DateTimeOffset_DateAdd_AddDays(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task DateTimeOffset_DateAdd_AddHours(bool isAsync)
		{
			return base.DateTimeOffset_DateAdd_AddHours(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task DateTimeOffset_DateAdd_AddMilliseconds(bool isAsync)
		{
			return base.DateTimeOffset_DateAdd_AddMilliseconds(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task DateTimeOffset_DateAdd_AddMinutes(bool isAsync)
		{
			return base.DateTimeOffset_DateAdd_AddMinutes(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task DateTimeOffset_DateAdd_AddMonths(bool isAsync)
		{
			return base.DateTimeOffset_DateAdd_AddMonths(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task DateTimeOffset_DateAdd_AddSeconds(bool isAsync)
		{
			return base.DateTimeOffset_DateAdd_AddSeconds(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task DateTimeOffset_DateAdd_AddYears(bool isAsync)
		{
			return base.DateTimeOffset_DateAdd_AddYears(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_date_component(bool isAsync)
		{
			return base.Where_datetimeoffset_date_component(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_dayofyear_component(bool isAsync)
		{
			return base.Where_datetimeoffset_dayofyear_component(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_day_component(bool isAsync)
		{
			return base.Where_datetimeoffset_day_component(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_hour_component(bool isAsync)
		{
			return base.Where_datetimeoffset_hour_component(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_millisecond_component(bool isAsync)
		{
			return base.Where_datetimeoffset_millisecond_component(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_minute_component(bool isAsync)
		{
			return base.Where_datetimeoffset_minute_component(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_month_component(bool isAsync)
		{
			return base.Where_datetimeoffset_month_component(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_now(bool isAsync)
		{
			return base.Where_datetimeoffset_now(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_second_component(bool isAsync)
		{
			return base.Where_datetimeoffset_second_component(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_utcnow(bool isAsync)
		{
			return base.Where_datetimeoffset_utcnow(isAsync);
		}

		[NotSupportedOnFirebirdTheory]
		[MemberData(nameof(IsAsyncData))]
		public override Task Where_datetimeoffset_year_component(bool isAsync)
		{
			return base.Where_datetimeoffset_year_component(isAsync);
		}

		[GeneratedNameTooLong]
		public override void Project_collection_navigation_with_inheritance1()
		{
			base.Project_collection_navigation_with_inheritance1();
		}

		[Fact(Skip = "See #15164 on EntityFrameworkCore.")]
		public override void Correlated_collection_with_top_level_FirstOrDefault()
		{
			base.Correlated_collection_with_top_level_FirstOrDefault();
		}
	}
}
