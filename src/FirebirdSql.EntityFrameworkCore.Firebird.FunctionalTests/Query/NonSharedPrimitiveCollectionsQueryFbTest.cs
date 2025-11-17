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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class NonSharedPrimitiveCollectionsQueryFbTest(NonSharedFixture fixture) : NonSharedPrimitiveCollectionsQueryRelationalTestBase(fixture)
{
	protected override DbContextOptionsBuilder SetParameterizedCollectionMode(DbContextOptionsBuilder optionsBuilder,
		ParameterTranslationMode parameterizedCollectionMode)
	{
		new FbDbContextOptionsBuilder(optionsBuilder).UseParameterizedCollectionMode(parameterizedCollectionMode);
		return optionsBuilder;
	}

	[NotSupportedOnFirebirdFact]
	public override Task Array_of_string()
	{
		return base.Array_of_string();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Array_of_int()
	{
		return base.Array_of_int();
	}

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_long()
    {
        return base.Array_of_long();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_short()
    {
        return base.Array_of_short();
    }

	[NotSupportedOnFirebirdFact]
	public override Task Array_of_byte()
	{
		return base.Array_of_byte();
	}

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_double()
    {
        return base.Array_of_double();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_float()
    {
        return base.Array_of_float();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_decimal()
    {
        return base.Array_of_decimal();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_DateTime()
    {
        return base.Array_of_DateTime();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_DateTime_with_milliseconds()
    {
        return base.Array_of_DateTime_with_milliseconds();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_DateTime_with_microseconds()
    {
        return base.Array_of_DateTime_with_microseconds();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_DateOnly()
    {
        return base.Array_of_DateOnly();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_TimeOnly()
    {
        return base.Array_of_TimeOnly();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_TimeOnly_with_milliseconds()
    {
        return base.Array_of_TimeOnly_with_milliseconds();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_TimeOnly_with_microseconds()
    {
        return base.Array_of_TimeOnly_with_microseconds();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_DateTimeOffset()
    {
        return base.Array_of_DateTimeOffset();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_bool()
    {
        return base.Array_of_bool();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_Guid()
    {
        return base.Array_of_Guid();
    }

	[NotSupportedOnFirebirdFact]
    public override Task Array_of_byte_array()
    {
        return base.Array_of_byte_array();
    }

	[NotSupportedOnFirebirdFact]
	public override Task Array_of_enum()
	{
		return base.Array_of_enum();
	}

	[NotSupportedOnFirebirdFact]
    public override Task Multidimensional_array_is_not_supported()
    {
        return base.Multidimensional_array_is_not_supported();
    }

	[NotSupportedOnFirebirdFact]
	public override Task Column_with_custom_converter()
	{
		return base.Column_with_custom_converter();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Parameter_with_inferred_value_converter()
	{
		return base.Parameter_with_inferred_value_converter();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Constant_with_inferred_value_converter()
	{
		return base.Constant_with_inferred_value_converter();
	}

	[NotSupportedOnFirebirdFact]
	public override Task Inline_collection_in_query_filter()
	{
		return base.Inline_collection_in_query_filter();
	}

	[NotSupportedOnFirebirdFact]
    public override Task Column_collection_inside_json_owned_entity()
    {
        return base.Column_collection_inside_json_owned_entity();
    }

	[NotSupportedByProviderTheory]
#pragma warning disable xUnit1016
	[MemberData(nameof(ParameterTranslationModeValues))]
#pragma warning restore xUnit1016
	public override Task Parameter_collection_Count_with_column_predicate_with_default_mode(ParameterTranslationMode mode)
	{
		return base.Parameter_collection_Count_with_column_predicate_with_default_mode(mode);
	}

	[NotSupportedByProviderTheory]
#pragma warning disable xUnit1016
	[MemberData(nameof(ParameterTranslationModeValues))]
#pragma warning restore xUnit1016
	public override Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Constant(ParameterTranslationMode mode)
	{
		return base.Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Constant(mode);
	}

	[NotSupportedByProviderTheory]
#pragma warning disable xUnit1016
	[MemberData(nameof(ParameterTranslationModeValues))]
#pragma warning restore xUnit1016
	public override Task Parameter_collection_Contains_with_default_mode_EF_Constant(ParameterTranslationMode mode)
	{
		return base.Parameter_collection_Contains_with_default_mode_EF_Constant(mode);
	}

	[NotSupportedByProviderTheory]
#pragma warning disable xUnit1016
	[MemberData(nameof(ParameterTranslationModeValues))]
#pragma warning restore xUnit1016
	public override Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Parameter(ParameterTranslationMode mode)
	{
		return base.Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Parameter(mode);
	}

	[NotSupportedByProviderTheory]
#pragma warning disable xUnit1016
	[MemberData(nameof(ParameterTranslationModeValues))]
#pragma warning restore xUnit1016
	public override Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_MultipleParameters(ParameterTranslationMode mode)
	{
		return base.Parameter_collection_Count_with_column_predicate_with_default_mode_EF_MultipleParameters(mode);
	}

	protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;
}
