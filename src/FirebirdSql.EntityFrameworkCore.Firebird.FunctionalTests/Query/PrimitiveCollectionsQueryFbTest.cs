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
		await using var context = CreateContext();

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

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(bool async)
	{
		return base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_of_strings_contains_null(bool async)
	{
		return base.Column_collection_of_strings_contains_null(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_Count_with_one_value(bool async)
	{
		return base.Inline_collection_Count_with_one_value(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_Count_with_two_values(bool async)
    {
        return base.Inline_collection_Count_with_two_values(async);
    }

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_Count_with_three_values(bool async)
    {
        return base.Inline_collection_Count_with_three_values(async);
    }

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_Count(bool async)
	{
		return base.Parameter_collection_Count(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_of_ints_Contains_int(bool async)
	{
		return base.Parameter_collection_of_ints_Contains_int(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_of_nullable_ints_Contains_int(bool async)
	{
		return base.Parameter_collection_of_nullable_ints_Contains_int(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_of_nullable_ints_Contains_nullable_int(bool async)
	{
		return base.Parameter_collection_of_nullable_ints_Contains_nullable_int(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_of_strings_Contains_string(bool async)
	{
		return base.Parameter_collection_of_strings_Contains_string(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_of_strings_Contains_nullable_string(bool async)
	{
		return base.Parameter_collection_of_strings_Contains_nullable_string(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_of_DateTimes_Contains(bool async)
	{
		return base.Parameter_collection_of_DateTimes_Contains(async);
	}

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Parameter_collection_of_bools_Contains(bool async)
    {
        return base.Parameter_collection_of_bools_Contains(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Parameter_collection_of_enums_Contains(bool async)
    {
        return base.Parameter_collection_of_enums_Contains(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Parameter_collection_null_Contains(bool async)
    {
        return base.Parameter_collection_null_Contains(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_of_ints_Contains(bool async)
    {
        return base.Column_collection_of_ints_Contains(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_of_nullable_ints_Contains(bool async)
    {
        return base.Column_collection_of_nullable_ints_Contains(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_of_nullable_ints_Contains_null(bool async)
    {
        return base.Column_collection_of_nullable_ints_Contains_null(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_of_nullable_strings_contains_null(bool async)
    {
        return base.Column_collection_of_nullable_strings_contains_null(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_of_bools_Contains(bool async)
    {
        return base.Column_collection_of_bools_Contains(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_Count_method(bool async)
    {
        return base.Column_collection_Count_method(async);
    }

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_Count_with_predicate(bool async)
	{
		return base.Column_collection_Count_with_predicate(async);
	}

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_Length(bool async)
    {
        return base.Column_collection_Length(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_index_int(bool async)
    {
        return base.Column_collection_index_int(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_index_string(bool async)
    {
        return base.Column_collection_index_string(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_index_datetime(bool async)
    {
        return base.Column_collection_index_datetime(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_index_beyond_end(bool async)
    {
        return base.Column_collection_index_beyond_end(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Nullable_reference_column_collection_index_equals_nullable_column(bool async)
    {
        return base.Nullable_reference_column_collection_index_equals_nullable_column(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Non_nullable_reference_column_collection_index_equals_nullable_column(bool async)
    {
        return base.Non_nullable_reference_column_collection_index_equals_nullable_column(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Inline_collection_index_Column(bool async)
    {
        return base.Inline_collection_index_Column(async);
    }

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_index_Column_equal_Column(bool async)
	{
		return base.Parameter_collection_index_Column_equal_Column(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_index_Column_equal_constant(bool async)
	{
		return base.Parameter_collection_index_Column_equal_constant(async);
	}

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_ElementAt(bool async)
    {
        return base.Column_collection_ElementAt(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_Skip(bool async)
    {
        return base.Column_collection_Skip(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_Take(bool async)
    {
        return base.Column_collection_Take(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_Skip_Take(bool async)
    {
        return base.Column_collection_Skip_Take(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_OrderByDescending_ElementAt(bool async)
    {
        return base.Column_collection_OrderByDescending_ElementAt(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_Any(bool async)
    {
        return base.Column_collection_Any(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_Distinct(bool async)
    {
        return base.Column_collection_Distinct(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_Join_parameter_collection(bool async)
    {
        return base.Column_collection_Join_parameter_collection(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Inline_collection_Join_ordered_column_collection(bool async)
    {
        return base.Inline_collection_Join_ordered_column_collection(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Parameter_collection_Concat_column_collection(bool async)
    {
        return base.Parameter_collection_Concat_column_collection(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_Union_parameter_collection(bool async)
    {
        return base.Column_collection_Union_parameter_collection(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_Intersect_inline_collection(bool async)
    {
        return base.Column_collection_Intersect_inline_collection(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Inline_collection_Except_column_collection(bool async)
    {
        return base.Inline_collection_Except_column_collection(async);
    }

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(bool async)
	{
		return base.Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(async);
	}

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Parameter_collection_in_subquery_Union_column_collection(bool async)
    {
        return base.Parameter_collection_in_subquery_Union_column_collection(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Parameter_collection_in_subquery_Union_column_collection_nested(bool async)
    {
        return base.Parameter_collection_in_subquery_Union_column_collection_nested(async);
    }

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_in_subquery_Count_as_compiled_query(bool async)
	{
		return base.Parameter_collection_in_subquery_Count_as_compiled_query(async);
	}

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Column_collection_in_subquery_Union_parameter_collection(bool async)
    {
        return base.Column_collection_in_subquery_Union_parameter_collection(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Project_collection_of_ints_ordered(bool async)
    {
        return base.Project_collection_of_ints_ordered(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Project_collection_of_datetimes_filtered(bool async)
    {
        return base.Project_collection_of_datetimes_filtered(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Project_collection_of_nullable_ints_with_paging(bool async)
    {
        return base.Project_collection_of_nullable_ints_with_paging(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Project_collection_of_nullable_ints_with_paging2(bool async)
    {
        return base.Project_collection_of_nullable_ints_with_paging2(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Project_collection_of_nullable_ints_with_paging3(bool async)
    {
        return base.Project_collection_of_nullable_ints_with_paging3(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Project_collection_of_ints_with_distinct(bool async)
    {
        return base.Project_collection_of_ints_with_distinct(async);
    }

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_empty_collection_of_nullables_and_collection_only_containing_nulls(bool async)
	{
		return base.Project_empty_collection_of_nullables_and_collection_only_containing_nulls(async);
	}

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Project_multiple_collections(bool async)
    {
        return base.Project_multiple_collections(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Project_primitive_collections_element(bool async)
    {
        return base.Project_primitive_collections_element(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Nested_contains_with_Lists_and_no_inferred_type_mapping(bool async)
    {
        return base.Nested_contains_with_Lists_and_no_inferred_type_mapping(async);
    }

	[NotSupportedOnFirebirdTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Nested_contains_with_arrays_and_no_inferred_type_mapping(bool async)
    {
        return base.Nested_contains_with_arrays_and_no_inferred_type_mapping(async);
    }

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_Contains_over_subquery(bool async)
	{
		return base.Column_collection_Contains_over_subquery(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_First(bool async)
	{
		return base.Column_collection_First(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_FirstOrDefault(bool async)
	{
		return base.Column_collection_FirstOrDefault(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_SelectMany(bool async)
	{
		return base.Column_collection_SelectMany(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_SelectMany_with_filter(bool async)
	{
		return base.Column_collection_SelectMany_with_filter(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_SelectMany_with_Select_to_anonymous_type(bool async)
	{
		return base.Column_collection_SelectMany_with_Select_to_anonymous_type(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_Single(bool async)
	{
		return base.Column_collection_Single(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_SingleOrDefault(bool async)
	{
		return base.Column_collection_SingleOrDefault(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_Where_Count(bool async)
	{
		return base.Column_collection_Where_Count(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_Where_ElementAt(bool async)
	{
		return base.Column_collection_Where_ElementAt(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_Where_Skip(bool async)
	{
		return base.Column_collection_Where_Skip(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_Where_Skip_Take(bool async)
	{
		return base.Column_collection_Where_Skip_Take(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_Where_Take(bool async)
	{
		return base.Column_collection_Where_Take(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Column_collection_Where_Union(bool async)
	{
		return base.Column_collection_Where_Union(async);
	}
	
	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_Count_with_column_predicate_with_EF_Parameter(bool async)
	{
		return base.Inline_collection_Count_with_column_predicate_with_EF_Parameter(async);
	}
	
	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_List_Max_with_three_values(bool async)
	{
		return base.Inline_collection_List_Max_with_three_values(async);
	}
	
	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_List_Max_with_two_values(bool async)
	{
		return base.Inline_collection_List_Max_with_two_values(async);
	}
	
	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_List_Min_with_two_values(bool async)
	{
		return base.Inline_collection_List_Min_with_two_values(async);
	}
	
	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_List_value_index_Column(bool async)
	{
		return base.Inline_collection_List_value_index_Column(async);
	}
	
	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_Max_with_three_values(bool async)
	{
		return base.Inline_collection_Max_with_three_values(async);
	}
	
	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_Max_with_two_values(bool async)
	{
		return base.Inline_collection_Max_with_two_values(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_Min_with_three_values(bool async)
	{
		return base.Inline_collection_Min_with_three_values(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_Min_with_two_values(bool async)
	{
		return base.Inline_collection_Min_with_two_values(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_of_nullable_value_type_Max(bool async)
	{
		return base.Inline_collection_of_nullable_value_type_Max(async);
	}
	
	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_of_nullable_value_type_Min(bool async)
	{
		return base.Inline_collection_of_nullable_value_type_Min(async);
	}
	
	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_of_nullable_value_type_with_null_Max(bool async)
	{
		return base.Inline_collection_of_nullable_value_type_with_null_Max(async);
	}
	
	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_of_nullable_value_type_with_null_Min(bool async)
	{
		return base.Inline_collection_of_nullable_value_type_with_null_Min(async);
	}
	
	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_value_index_Column(bool async)
	{
		return base.Inline_collection_value_index_Column(async);
	}
	
	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Inline_collection_with_single_parameter_element_Count(bool async)
	{
		return base.Inline_collection_with_single_parameter_element_Count(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_Count_with_column_predicate_with_EF_Constant(bool async)
	{
		return base.Parameter_collection_Count_with_column_predicate_with_EF_Constant(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_Where_with_EF_Constant_Where_Any(bool async)
	{
		return base.Parameter_collection_Where_with_EF_Constant_Where_Any(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Project_inline_collection_with_Union(bool async)
	{
		return base.Project_inline_collection_with_Union(async);
	}

	[NotSupportedOnFirebirdTheory]
	[MemberData(nameof(IsAsyncData))]
	public override Task Parameter_collection_ImmutableArray_of_ints_Contains_int(bool async)
	{
		return base.Parameter_collection_ImmutableArray_of_ints_Contains_int(async);
	}

	PrimitiveCollectionsContext CreateContext()
	{
		return Fixture.CreateContext();
	}

	public class PrimitiveCollectionsQueryFbFixture : PrimitiveCollectionsQueryFixtureBase, ITestSqlLoggerFactory
	{
		protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

		public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
	}
}
