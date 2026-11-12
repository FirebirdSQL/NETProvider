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
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure;
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

	protected override DbContextOptionsBuilder SetParameterizedCollectionMode(DbContextOptionsBuilder optionsBuilder,
		ParameterTranslationMode parameterizedCollectionMode)
	{
		new FbDbContextOptionsBuilder(optionsBuilder).UseParameterizedCollectionMode(parameterizedCollectionMode);
		return optionsBuilder;
	}

	[Fact]
	public virtual async Task Json_representation_of_bool_array()
	{
		await using var context = Fixture.CreateContext();

		Assert.Equal(
			"[true,false]",
			await context.Database.SqlQuery<string>($@"SELECT ""Bools"" AS ""Value"" FROM ""PrimitiveCollectionsEntity"" WHERE ""Id"" = 1")
				.SingleAsync());
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override void Parameter_collection_in_subquery_and_Convert_as_compiled_query()
	{
		base.Parameter_collection_in_subquery_and_Convert_as_compiled_query();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query()
	{
		return base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Any()
	{
		return base.Column_collection_Any();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Contains_over_subquery()
	{
		return base.Column_collection_Contains_over_subquery();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Count_method()
	{
		return base.Column_collection_Count_method();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Count_with_predicate()
	{
		return base.Column_collection_Count_with_predicate();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Distinct()
	{
		return base.Column_collection_Distinct();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_ElementAt()
	{
		return base.Column_collection_ElementAt();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_First()
	{
		return base.Column_collection_First();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_FirstOrDefault()
	{
		return base.Column_collection_FirstOrDefault();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_in_subquery_Union_parameter_collection()
	{
		return base.Column_collection_in_subquery_Union_parameter_collection();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_index_beyond_end()
	{
		return base.Column_collection_index_beyond_end();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_index_datetime()
	{
		return base.Column_collection_index_datetime();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_index_int()
	{
		return base.Column_collection_index_int();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_index_string()
	{
		return base.Column_collection_index_string();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Intersect_inline_collection()
	{
		return base.Column_collection_Intersect_inline_collection();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Join_parameter_collection()
	{
		return base.Column_collection_Join_parameter_collection();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Length()
	{
		return base.Column_collection_Length();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_of_bools_Contains()
	{
		return base.Column_collection_of_bools_Contains();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_of_ints_Contains()
	{
		return base.Column_collection_of_ints_Contains();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_of_nullable_ints_Contains()
	{
		return base.Column_collection_of_nullable_ints_Contains();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_of_nullable_ints_Contains_null()
	{
		return base.Column_collection_of_nullable_ints_Contains_null();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_of_nullable_strings_contains_null()
	{
		return base.Column_collection_of_nullable_strings_contains_null();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_OrderByDescending_ElementAt()
	{
		return base.Column_collection_OrderByDescending_ElementAt();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_SelectMany()
	{
		return base.Column_collection_SelectMany();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_SelectMany_with_filter()
	{
		return base.Column_collection_SelectMany_with_filter();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_SelectMany_with_Select_to_anonymous_type()
	{
		return base.Column_collection_SelectMany_with_Select_to_anonymous_type();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Single()
	{
		return base.Column_collection_Single();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_SingleOrDefault()
	{
		return base.Column_collection_SingleOrDefault();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Skip()
	{
		return base.Column_collection_Skip();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Skip_Take()
	{
		return base.Column_collection_Skip_Take();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Take()
	{
		return base.Column_collection_Take();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Union_parameter_collection()
	{
		return base.Column_collection_Union_parameter_collection();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Where_Count()
	{
		return base.Column_collection_Where_Count();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Where_ElementAt()
	{
		return base.Column_collection_Where_ElementAt();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Where_Skip()
	{
		return base.Column_collection_Where_Skip();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Where_Skip_Take()
	{
		return base.Column_collection_Where_Skip_Take();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Where_Take()
	{
		return base.Column_collection_Where_Take();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_Where_Union()
	{
		return base.Column_collection_Where_Union();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_Count_with_column_predicate_with_EF_Parameter()
	{
		return base.Inline_collection_Count_with_column_predicate_with_EF_Parameter();
	}


	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_Count_with_one_value()
	{
		return base.Inline_collection_Count_with_one_value();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_Count_with_two_values()
	{
		return base.Inline_collection_Count_with_two_values();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_Count_with_three_values()
	{
		return base.Inline_collection_Count_with_three_values();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_Except_column_collection()
	{
		return base.Inline_collection_Except_column_collection();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_index_Column()
	{
		return base.Inline_collection_index_Column();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_index_Column_with_EF_Constant()
	{
		return base.Inline_collection_index_Column_with_EF_Constant();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_Join_ordered_column_collection()
	{
		return base.Inline_collection_Join_ordered_column_collection();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_List_value_index_Column()
	{
		return base.Inline_collection_List_value_index_Column();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_of_nullable_value_type_Max()
	{
		return base.Inline_collection_of_nullable_value_type_Max();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_of_nullable_value_type_Min()
	{
		return base.Inline_collection_of_nullable_value_type_Min();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_of_nullable_value_type_with_null_Max()
	{
		return base.Inline_collection_of_nullable_value_type_with_null_Max();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_of_nullable_value_type_with_null_Min()
	{
		return base.Inline_collection_of_nullable_value_type_with_null_Min();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_value_index_Column()
	{
		return base.Inline_collection_value_index_Column();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Inline_collection_with_single_parameter_element_Count()
	{
		return base.Inline_collection_with_single_parameter_element_Count();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Non_nullable_reference_column_collection_index_equals_nullable_column()
	{
		return base.Non_nullable_reference_column_collection_index_equals_nullable_column();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Nullable_reference_column_collection_index_equals_nullable_column()
	{
		return base.Nullable_reference_column_collection_index_equals_nullable_column();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Parameter_collection_Concat_column_collection()
	{
		return base.Parameter_collection_Concat_column_collection();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Parameter_collection_Count()
	{
		return base.Parameter_collection_Count();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Parameter_collection_Count_with_column_predicate_with_EF_Constant()
	{
		return base.Parameter_collection_Count_with_column_predicate_with_EF_Constant();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Parameter_collection_ImmutableArray_of_ints_Contains_int()
	{
		return base.Parameter_collection_ImmutableArray_of_ints_Contains_int();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Parameter_collection_in_subquery_Count_as_compiled_query()
	{
		return base.Parameter_collection_in_subquery_Count_as_compiled_query();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Parameter_collection_in_subquery_Union_column_collection()
	{
		return base.Parameter_collection_in_subquery_Union_column_collection();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query()
	{
		return base.Parameter_collection_in_subquery_Union_column_collection_as_compiled_query();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Parameter_collection_in_subquery_Union_column_collection_nested()
	{
		return base.Parameter_collection_in_subquery_Union_column_collection_nested();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Parameter_collection_index_Column_equal_Column()
	{
		return base.Parameter_collection_index_Column_equal_Column();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Parameter_collection_index_Column_equal_constant()
	{
		return base.Parameter_collection_index_Column_equal_constant();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Parameter_collection_Where_with_EF_Constant_Where_Any()
	{
		return base.Parameter_collection_Where_with_EF_Constant_Where_Any();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Project_collection_of_datetimes_filtered()
	{
		return base.Project_collection_of_datetimes_filtered();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Project_collection_of_ints_ordered()
	{
		return base.Project_collection_of_ints_ordered();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Project_collection_of_nullable_ints_with_paging2()
	{
		return base.Project_collection_of_nullable_ints_with_paging2();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Project_empty_collection_of_nullables_and_collection_only_containing_nulls()
	{
		return base.Project_empty_collection_of_nullables_and_collection_only_containing_nulls();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Project_inline_collection_with_Union()
	{
		return base.Project_inline_collection_with_Union();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Project_multiple_collections()
	{
		return base.Project_multiple_collections();
	}

	[Fact(Skip = SkipReasons.NotSupportedByProvider)]
	public override Task Values_of_enum_casted_to_underlying_value()
	{
		return base.Values_of_enum_casted_to_underlying_value();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Multidimensional_array_is_not_supported()
	{
		return base.Multidimensional_array_is_not_supported();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_with_custom_converter()
	{
		return base.Column_with_custom_converter();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Constant_with_inferred_value_converter()
	{
		return base.Constant_with_inferred_value_converter();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Inline_collection_in_query_filter()
	{
		return base.Inline_collection_in_query_filter();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_inside_json_owned_entity()
	{
		return base.Column_collection_inside_json_owned_entity();
	}

	[Theory(Skip = SkipReasons.NotSupportedByProvider)]
#pragma warning disable xUnit1016
	[MemberData(nameof(ParameterTranslationModeValues))]
#pragma warning restore xUnit1016
	public override Task Parameter_collection_Count_with_column_predicate_with_default_mode(ParameterTranslationMode mode)
	{
		return base.Parameter_collection_Count_with_column_predicate_with_default_mode(mode);
	}

	[Theory(Skip = SkipReasons.NotSupportedByProvider)]
#pragma warning disable xUnit1016
	[MemberData(nameof(ParameterTranslationModeValues))]
#pragma warning restore xUnit1016
	public override Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Constant(ParameterTranslationMode mode)
	{
		return base.Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Constant(mode);
	}

	[Theory(Skip = SkipReasons.NotSupportedByProvider)]
#pragma warning disable xUnit1016
	[MemberData(nameof(ParameterTranslationModeValues))]
#pragma warning restore xUnit1016
	public override Task Parameter_collection_Contains_with_default_mode_EF_Constant(ParameterTranslationMode mode)
	{
		return base.Parameter_collection_Contains_with_default_mode_EF_Constant(mode);
	}

	[Theory(Skip = SkipReasons.NotSupportedByProvider)]
#pragma warning disable xUnit1016
	[MemberData(nameof(ParameterTranslationModeValues))]
#pragma warning restore xUnit1016
	public override Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Parameter(ParameterTranslationMode mode)
	{
		return base.Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Parameter(mode);
	}

	[Theory(Skip = SkipReasons.NotSupportedByProvider)]
#pragma warning disable xUnit1016
	[MemberData(nameof(ParameterTranslationModeValues))]
#pragma warning restore xUnit1016
	public override Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_MultipleParameters(ParameterTranslationMode mode)
	{
		return base.Parameter_collection_Count_with_column_predicate_with_default_mode_EF_MultipleParameters(mode);
	}

	public class PrimitiveCollectionsQueryFbFixture : PrimitiveCollectionsQueryFixtureBase, ITestSqlLoggerFactory
	{
		protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

		public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_of_strings_Contains_null()
	{
		return base.Column_collection_of_strings_Contains_null();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Column_collection_of_strings_Contains()
	{
		return base.Column_collection_of_strings_Contains();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Compiled_query_with_uncorrelated_parameter_collection_expression()
	{
		return base.Compiled_query_with_uncorrelated_parameter_collection_expression();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Inline_collection_SelectMany_with_unreferenced_collection_value()
	{
		return base.Inline_collection_SelectMany_with_unreferenced_collection_value();
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Parameter_collection_empty_Join()
	{
		return base.Parameter_collection_empty_Join();
	}

	[Theory(Skip = SkipReasons.NotSupportedOnFirebird)]
#pragma warning disable xUnit1016
	[MemberData(nameof(ParameterTranslationModeValues))]
#pragma warning restore xUnit1016
	public override Task Parameter_collection_of_enum_Cast_from_different_enum_type(ParameterTranslationMode mode)
	{
		return base.Parameter_collection_of_enum_Cast_from_different_enum_type(mode);
	}

	[Fact(Skip = SkipReasons.NotSupportedOnFirebird)]
	public override Task Subquery_over_primitive_collection_on_inheritance_derived_type()
	{
		return base.Subquery_over_primitive_collection_on_inheritance_derived_type();
	}
}
