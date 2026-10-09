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

using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Helpers;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class PrimitiveCollectionsQueryFbTest : PrimitiveCollectionsQueryRelationalTestBase<PrimitiveCollectionsQueryFbTest.PrimitiveCollectionsQueryFbFixture>
{
	public PrimitiveCollectionsQueryFbTest(PrimitiveCollectionsQueryFbFixture fixture)
		: base(fixture)
	{ }

	[ConditionalFact]
	public virtual async Task Json_representation_of_bool_array()
	{
		await using var context = Fixture.CreateContext();

		Assert.Equal(
			"[true,false]",
			await context.Database.SqlQuery<string>($@"SELECT ""Bools"" AS ""Value"" FROM ""PrimitiveCollectionsEntity"" WHERE ""Id"" = 1")
				.SingleAsync());
	}

	[NotSupportedOnFirebirdFact]
	public override void Parameter_collection_in_subquery_and_Convert_as_compiled_query()
	{
		base.Parameter_collection_in_subquery_and_Convert_as_compiled_query();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query()
	{
		return base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Any()
	{
		return base.Column_collection_Any();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Contains_over_subquery()
	{
		return base.Column_collection_Contains_over_subquery();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Count_method()
	{
		return base.Column_collection_Count_method();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Count_with_predicate()
	{
		return base.Column_collection_Count_with_predicate();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Distinct()
	{
		return base.Column_collection_Distinct();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_ElementAt()
	{
		return base.Column_collection_ElementAt();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_First()
	{
		return base.Column_collection_First();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_FirstOrDefault()
	{
		return base.Column_collection_FirstOrDefault();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_in_subquery_Union_parameter_collection()
	{
		return base.Column_collection_in_subquery_Union_parameter_collection();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_index_beyond_end()
	{
		return base.Column_collection_index_beyond_end();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_index_datetime()
	{
		return base.Column_collection_index_datetime();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_index_int()
	{
		return base.Column_collection_index_int();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_index_string()
	{
		return base.Column_collection_index_string();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Intersect_inline_collection()
	{
		return base.Column_collection_Intersect_inline_collection();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Join_parameter_collection()
	{
		return base.Column_collection_Join_parameter_collection();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Length()
	{
		return base.Column_collection_Length();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_of_bools_Contains()
	{
		return base.Column_collection_of_bools_Contains();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_of_ints_Contains()
	{
		return base.Column_collection_of_ints_Contains();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_of_nullable_ints_Contains()
	{
		return base.Column_collection_of_nullable_ints_Contains();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_of_nullable_ints_Contains_null()
	{
		return base.Column_collection_of_nullable_ints_Contains_null();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_of_nullable_strings_contains_null()
	{
		return base.Column_collection_of_nullable_strings_contains_null();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_of_strings_contains_null()
	{
		return base.Column_collection_of_strings_contains_null();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_OrderByDescending_ElementAt()
	{
		return base.Column_collection_OrderByDescending_ElementAt();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_SelectMany()
	{
		return base.Column_collection_SelectMany();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_SelectMany_with_filter()
	{
		return base.Column_collection_SelectMany_with_filter();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_SelectMany_with_Select_to_anonymous_type()
	{
		return base.Column_collection_SelectMany_with_Select_to_anonymous_type();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Single()
	{
		return base.Column_collection_Single();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_SingleOrDefault()
	{
		return base.Column_collection_SingleOrDefault();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Skip()
	{
		return base.Column_collection_Skip();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Skip_Take()
	{
		return base.Column_collection_Skip_Take();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Take()
	{
		return base.Column_collection_Take();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Union_parameter_collection()
	{
		return base.Column_collection_Union_parameter_collection();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Where_Count()
	{
		return base.Column_collection_Where_Count();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Where_ElementAt()
	{
		return base.Column_collection_Where_ElementAt();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Where_Skip()
	{
		return base.Column_collection_Where_Skip();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Where_Skip_Take()
	{
		return base.Column_collection_Where_Skip_Take();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Where_Take()
	{
		return base.Column_collection_Where_Take();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Column_collection_Where_Union()
	{
		return base.Column_collection_Where_Union();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_Count_with_column_predicate_with_EF_Parameter()
	{
		return base.Inline_collection_Count_with_column_predicate_with_EF_Parameter();
	}


	[NotSupportedByProviderFact]
	public override Task Inline_collection_Count_with_one_value()
	{
		return base.Inline_collection_Count_with_one_value();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_Count_with_two_values()
	{
		return base.Inline_collection_Count_with_two_values();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_Count_with_three_values()
	{
		return base.Inline_collection_Count_with_three_values();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_Except_column_collection()
	{
		return base.Inline_collection_Except_column_collection();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_index_Column()
	{
		return base.Inline_collection_index_Column();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_index_Column_with_EF_Constant()
	{
		return base.Inline_collection_index_Column_with_EF_Constant();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_Join_ordered_column_collection()
	{
		return base.Inline_collection_Join_ordered_column_collection();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_List_value_index_Column()
	{
		return base.Inline_collection_List_value_index_Column();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_of_nullable_value_type_Max()
	{
		return base.Inline_collection_of_nullable_value_type_Max();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_of_nullable_value_type_Min()
	{
		return base.Inline_collection_of_nullable_value_type_Min();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_of_nullable_value_type_with_null_Max()
	{
		return base.Inline_collection_of_nullable_value_type_with_null_Max();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_of_nullable_value_type_with_null_Min()
	{
		return base.Inline_collection_of_nullable_value_type_with_null_Min();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_value_index_Column()
	{
		return base.Inline_collection_value_index_Column();
	}

	[NotSupportedByProviderFact]
	public override Task Inline_collection_with_single_parameter_element_Count()
	{
		return base.Inline_collection_with_single_parameter_element_Count();
	}

	[NotSupportedByProviderFact]
	public override Task Non_nullable_reference_column_collection_index_equals_nullable_column()
	{
		return base.Non_nullable_reference_column_collection_index_equals_nullable_column();
	}

	[NotSupportedByProviderFact]
	public override Task Nullable_reference_column_collection_index_equals_nullable_column()
	{
		return base.Nullable_reference_column_collection_index_equals_nullable_column();
	}

	[NotSupportedByProviderFact]
	public override Task Parameter_collection_Concat_column_collection()
	{
		return base.Parameter_collection_Concat_column_collection();
	}

	[NotSupportedByProviderFact]
	public override Task Parameter_collection_Count()
	{
		return base.Parameter_collection_Count();
	}

	[NotSupportedByProviderFact]
	public override Task Parameter_collection_Count_with_column_predicate_with_EF_Constant()
	{
		return base.Parameter_collection_Count_with_column_predicate_with_EF_Constant();
	}

	[NotSupportedByProviderFact]
	public override Task Parameter_collection_ImmutableArray_of_ints_Contains_int()
	{
		return base.Parameter_collection_ImmutableArray_of_ints_Contains_int();
	}

	[NotSupportedByProviderFact]
	public override Task Parameter_collection_in_subquery_Count_as_compiled_query()
	{
		return base.Parameter_collection_in_subquery_Count_as_compiled_query();
	}

	[NotSupportedByProviderFact]
	public override Task Parameter_collection_in_subquery_Union_column_collection()
	{
		return base.Parameter_collection_in_subquery_Union_column_collection();
	}

	[NotSupportedByProviderFact]
	public override Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query()
	{
		return base.Parameter_collection_in_subquery_Union_column_collection_as_compiled_query();
	}

	[NotSupportedByProviderFact]
	public override Task Parameter_collection_in_subquery_Union_column_collection_nested()
	{
		return base.Parameter_collection_in_subquery_Union_column_collection_nested();
	}

	[NotSupportedByProviderFact]
	public override Task Parameter_collection_index_Column_equal_Column()
	{
		return base.Parameter_collection_index_Column_equal_Column();
	}

	[NotSupportedByProviderFact]
	public override Task Parameter_collection_index_Column_equal_constant()
	{
		return base.Parameter_collection_index_Column_equal_constant();
	}

	[NotSupportedByProviderFact]
	public override Task Parameter_collection_Where_with_EF_Constant_Where_Any()
	{
		return base.Parameter_collection_Where_with_EF_Constant_Where_Any();
	}

	[NotSupportedByProviderFact]
	public override Task Project_collection_of_datetimes_filtered()
	{
		return base.Project_collection_of_datetimes_filtered();
	}

	[NotSupportedByProviderFact]
	public override Task Project_collection_of_ints_ordered()
	{
		return base.Project_collection_of_ints_ordered();
	}

	[NotSupportedByProviderFact]
	public override Task Project_collection_of_nullable_ints_with_paging2()
	{
		return base.Project_collection_of_nullable_ints_with_paging2();
	}

	[NotSupportedByProviderFact]
	public override Task Project_empty_collection_of_nullables_and_collection_only_containing_nulls()
	{
		return base.Project_empty_collection_of_nullables_and_collection_only_containing_nulls();
	}

	[NotSupportedByProviderFact]
	public override Task Project_inline_collection_with_Union()
	{
		return base.Project_inline_collection_with_Union();
	}

	[NotSupportedByProviderFact]
	public override Task Project_multiple_collections()
	{
		return base.Project_multiple_collections();
	}

	[NotSupportedByProviderFact]
	public override Task Values_of_enum_casted_to_underlying_value()
	{
		return base.Values_of_enum_casted_to_underlying_value();
	}

	public class PrimitiveCollectionsQueryFbFixture : PrimitiveCollectionsQueryFixtureBase, ITestSqlLoggerFactory
	{
		protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

		public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
	}
}
