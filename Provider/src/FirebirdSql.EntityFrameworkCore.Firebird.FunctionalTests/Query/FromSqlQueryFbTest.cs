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

using System.Data.Common;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query
{
	public class FromSqlQueryFbTest : FromSqlQueryTestBase<NorthwindQueryFbFixture<NoopModelCustomizer>>
	{
		public FromSqlQueryFbTest(NorthwindQueryFbFixture<NoopModelCustomizer> fixture)
			: base(fixture)
		{ }

		[NotSupportedOnFirebirdFact]
		public override void Bad_data_error_handling_invalid_cast_key()
		{
			base.Bad_data_error_handling_invalid_cast_key();
		}

		[NotSupportedOnFirebirdFact]
		public override void Bad_data_error_handling_invalid_cast_no_tracking()
		{
			base.Bad_data_error_handling_invalid_cast_no_tracking();
		}

		[NotSupportedOnFirebirdFact]
		public override void Bad_data_error_handling_invalid_cast_projection()
		{
			base.Bad_data_error_handling_invalid_cast_projection();
		}

		[NotSupportedOnFirebirdFact]
		public override void Bad_data_error_handling_invalid_cast()
		{
			base.Bad_data_error_handling_invalid_cast();
		}

		[DoesNotHaveTheDataFact]
		public override void FromSqlRaw_queryable_simple_projection_composed()
		{
			base.FromSqlRaw_queryable_simple_projection_composed();
		}

		protected override DbParameter CreateDbParameter(string name, object value)
			=> new FbParameter
			{
				ParameterName = name,
				Value = value
			};
	}
}
