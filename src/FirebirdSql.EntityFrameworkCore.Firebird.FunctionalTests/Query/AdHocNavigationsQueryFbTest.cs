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
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class AdHocNavigationsQueryFbTest(NonSharedFixture fixture) : AdHocNavigationsQueryRelationalTestBase(fixture)
{
	protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

	[NotSupportedOnFirebirdFact]
	public override Task Let_multiple_references_with_reference_to_outer()
	{
		return base.Let_multiple_references_with_reference_to_outer();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Projection_with_multiple_includes_and_subquery_with_set_operation()
	{
		return base.Projection_with_multiple_includes_and_subquery_with_set_operation();
	}

	[NotSupportedOnFirebirdFact]
	public override Task SelectMany_and_collection_in_projection_in_FirstOrDefault()
	{
		return base.SelectMany_and_collection_in_projection_in_FirstOrDefault();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Correlated_collection_correctly_associates_entities_with_byte_array_keys()
	{
		return base.Correlated_collection_correctly_associates_entities_with_byte_array_keys();
	}
}
