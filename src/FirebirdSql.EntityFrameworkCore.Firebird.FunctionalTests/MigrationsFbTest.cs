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
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using FirebirdSql.EntityFrameworkCore.Firebird.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests;

public class MigrationsFbTest : MigrationsTestBase<MigrationsFbTest.MigrationsFbFixture>
{
	const string SkipReason = "Assumptions in model differ too much between base tests and Firebird.";

	public MigrationsFbTest(MigrationsFbFixture fixture)
		: base(fixture)
	{ }

	protected override string NonDefaultCollation => "WIN1250";

	[Fact(Skip = SkipReason)]
	public override Task Create_table() => base.Create_table();

	[Fact(Skip = SkipReason)]
	public override Task Create_table_all_settings() => base.Create_table_all_settings();

	[Fact(Skip = SkipReason)]
	public override Task Create_table_no_key() => base.Create_table_no_key();

	[Fact(Skip = SkipReason)]
	public override Task Create_table_with_comments() => base.Create_table_with_comments();

	[Fact(Skip = SkipReason)]
	public override Task Create_table_with_multiline_comments() => base.Create_table_with_multiline_comments();

	[Theory(Skip = SkipReason)]
	[InlineData(true)]
	[InlineData(false)]
	[InlineData(null)]
	public override Task Create_table_with_computed_column(bool? stored) => base.Create_table_with_computed_column(stored);

	[Fact(Skip = SkipReason)]
	public override Task Alter_table_add_comment() => base.Alter_table_add_comment();

	[Fact(Skip = SkipReason)]
	public override Task Alter_table_add_comment_non_default_schema() => base.Alter_table_add_comment_non_default_schema();

	[Fact(Skip = SkipReason)]
	public override Task Alter_table_change_comment() => base.Alter_table_change_comment();

	[Fact(Skip = SkipReason)]
	public override Task Alter_table_remove_comment() => base.Alter_table_remove_comment();

	[Fact(Skip = SkipReason)]
	public override Task Drop_table() => base.Drop_table();

	[Fact(Skip = SkipReason)]
	public override Task Rename_table() => base.Rename_table();

	[Fact(Skip = SkipReason)]
	public override Task Rename_table_with_primary_key() => base.Rename_table_with_primary_key();

	[Fact(Skip = SkipReason)]
	public override Task Move_table() => base.Move_table();

	[Fact(Skip = SkipReason)]
	public override Task Create_schema() => base.Create_schema();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_defaultValue_string() => base.Add_column_with_defaultValue_string();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_defaultValue_datetime() => base.Add_column_with_defaultValue_datetime();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_defaultValueSql() => base.Add_column_with_defaultValueSql();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_defaultValueSql_unspecified() => base.Add_column_with_defaultValueSql_unspecified();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_defaultValue_unspecified() => base.Add_column_with_defaultValue_unspecified();

	[Theory(Skip = SkipReason)]
	[InlineData(true)]
	[InlineData(false)]
	[InlineData(null)]
	public override Task Add_column_with_computedSql(bool? stored) => base.Add_column_with_computedSql(stored);

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_computedSql_unspecified() => base.Add_column_with_computedSql_unspecified();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_required() => base.Add_column_with_required();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_ansi() => base.Add_column_with_ansi();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_max_length() => base.Add_column_with_max_length();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_max_length_on_derived() => base.Add_column_with_max_length_on_derived();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_fixed_length() => base.Add_column_with_fixed_length();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_comment() => base.Add_column_with_comment();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_collation() => base.Add_column_with_collation();

	[Theory(Skip = SkipReason)]
	[InlineData(true)]
	[InlineData(false)]
	public override Task Add_column_computed_with_collation(bool stored) => base.Add_column_computed_with_collation(stored);

	[Fact(Skip = SkipReason)]
	public override Task Add_column_shared() => base.Add_column_shared();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_check_constraint() => base.Add_column_with_check_constraint();

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_change_type() => base.Alter_column_change_type();

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_make_required() => base.Alter_column_make_required();

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_make_required_with_index() => base.Alter_column_make_required_with_index();

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_make_required_with_composite_index() => base.Alter_column_make_required_with_composite_index();

	[Theory(Skip = SkipReason)]
	[InlineData(true)]
	[InlineData(false)]
	[InlineData(null)]
	public override Task Alter_column_make_computed(bool? stored) => base.Alter_column_make_computed(stored);

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_change_computed() => base.Alter_column_change_computed();

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_change_computed_type() => base.Alter_column_change_computed_type();

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_add_comment() => base.Alter_column_add_comment();

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_change_comment() => base.Alter_column_change_comment();

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_remove_comment() => base.Alter_column_remove_comment();

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_set_collation() => base.Alter_column_set_collation();

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_reset_collation() => base.Alter_column_reset_collation();

	[Fact(Skip = SkipReason)]
	public override Task Drop_column() => base.Drop_column();

	[Fact(Skip = SkipReason)]
	public override Task Drop_column_primary_key() => base.Drop_column_primary_key();

	[Fact(Skip = SkipReason)]
	public override Task Rename_column() => base.Rename_column();

	[Fact(Skip = SkipReason)]
	public override Task Create_index() => base.Create_index();

	[Fact(Skip = SkipReason)]
	public override Task Create_index_unique() => base.Create_index_unique();

	[Fact(Skip = SkipReason)]
	public override Task Create_index_with_filter() => base.Create_index_with_filter();

	[Fact(Skip = SkipReason)]
	public override Task Create_unique_index_with_filter() => base.Create_unique_index_with_filter();

	[Fact(Skip = SkipReason)]
	public override Task Drop_index() => base.Drop_index();

	[Fact(Skip = SkipReason)]
	public override Task Rename_index() => base.Rename_index();

	[Fact(Skip = SkipReason)]
	public override Task Add_primary_key_with_name() => base.Add_primary_key_with_name();

	[Fact(Skip = SkipReason)]
	public override Task Add_primary_key_composite_with_name() => base.Add_primary_key_composite_with_name();

	[Fact(Skip = SkipReason)]
	public override Task Add_foreign_key() => base.Add_foreign_key();

	[Fact(Skip = SkipReason)]
	public override Task Add_foreign_key_with_name() => base.Add_foreign_key_with_name();

	[Fact(Skip = SkipReason)]
	public override Task Drop_foreign_key() => base.Drop_foreign_key();

	[Fact(Skip = SkipReason)]
	public override Task Add_unique_constraint() => base.Add_unique_constraint();

	[Fact(Skip = SkipReason)]
	public override Task Add_unique_constraint_composite_with_name() => base.Add_unique_constraint_composite_with_name();

	[Fact(Skip = SkipReason)]
	public override Task Drop_unique_constraint() => base.Drop_unique_constraint();

	[Fact(Skip = SkipReason)]
	public override Task Add_check_constraint_with_name() => base.Add_check_constraint_with_name();

	[Fact(Skip = SkipReason)]
	public override Task Alter_check_constraint() => base.Alter_check_constraint();

	[Fact(Skip = SkipReason)]
	public override Task Drop_check_constraint() => base.Drop_check_constraint();

	[Fact(Skip = SkipReason)]
	public override Task Create_sequence() => base.Create_sequence();

	[Fact(Skip = SkipReason)]
	public override Task Create_sequence_all_settings() => base.Create_sequence_all_settings();

	[Fact(Skip = SkipReason)]
	public override Task Alter_sequence_all_settings() => base.Alter_sequence_all_settings();

	[Fact(Skip = SkipReason)]
	public override Task Alter_sequence_increment_by() => base.Alter_sequence_increment_by();

	[Fact(Skip = SkipReason)]
	public override Task Drop_sequence() => base.Drop_sequence();

	[Fact(Skip = SkipReason)]
	public override Task Rename_sequence() => base.Rename_sequence();

	[Fact(Skip = SkipReason)]
	public override Task Move_sequence() => base.Move_sequence();

	[Fact(Skip = SkipReason)]
	public override Task InsertDataOperation() => base.InsertDataOperation();

	[Fact(Skip = SkipReason)]
	public override Task DeleteDataOperation_simple_key() => base.DeleteDataOperation_simple_key();

	[Fact(Skip = SkipReason)]
	public override Task DeleteDataOperation_composite_key() => base.DeleteDataOperation_composite_key();

	[Fact(Skip = SkipReason)]
	public override Task UpdateDataOperation_simple_key() => base.UpdateDataOperation_simple_key();

	[Fact(Skip = SkipReason)]
	public override Task UpdateDataOperation_composite_key() => base.UpdateDataOperation_composite_key();

	[Fact(Skip = SkipReason)]
	public override Task Create_table_with_complex_properties_mapped_to_json() => base.Create_table_with_complex_properties_mapped_to_json();

	[Fact(Skip = SkipReason)]
	public override Task Create_table_with_complex_properties_with_nested_collection_mapped_to_json() => base.Create_table_with_complex_properties_with_nested_collection_mapped_to_json();

	[Fact(Skip = SkipReason)]
	public override Task Create_table_with_optional_complex_type_with_required_properties() => base.Create_table_with_optional_complex_type_with_required_properties();

	[Fact(Skip = SkipReason)]
	public override Task Multiop_create_table_and_drop_it_in_one_migration() => base.Multiop_create_table_and_drop_it_in_one_migration();

	[Fact(Skip = SkipReason)]
	public override Task Multiop_drop_table_and_create_the_same_table_in_one_migration() => base.Multiop_drop_table_and_create_the_same_table_in_one_migration();

	[Fact(Skip = SkipReason)]
	public override Task Multiop_rename_table_and_create_new_table_with_the_old_name() => base.Multiop_rename_table_and_create_new_table_with_the_old_name();

	[Fact(Skip = SkipReason)]
	public override Task Multiop_rename_table_and_drop() => base.Multiop_rename_table_and_drop();

	[Fact(Skip = SkipReason)]
	public override Task UpdateDataOperation_multiple_columns() => base.UpdateDataOperation_multiple_columns();

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_make_non_computed() => base.Alter_column_make_non_computed();

	[Fact(Skip = SkipReason)]
	public override Task Alter_computed_column_add_comment() => base.Alter_computed_column_add_comment();

	[Fact(Skip = SkipReason)]
	public override Task Drop_column_computed_and_non_computed_with_dependency() => base.Drop_column_computed_and_non_computed_with_dependency();

	[Fact(Skip = SkipReason)]
	public override Task SqlOperation() => base.SqlOperation();

	[Fact(Skip = SkipReason)]
	public override Task Add_primary_key_int() => base.Add_primary_key_int();

	[Fact(Skip = SkipReason)]
	public override Task Add_primary_key_string() => base.Add_primary_key_string();

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_change_computed_recreates_indexes() => base.Alter_column_change_computed_recreates_indexes();

	[Fact(Skip = SkipReason)]
	public override Task Alter_column_make_required_with_null_data() => base.Alter_column_make_required_with_null_data();

	[Fact(Skip = SkipReason)]
	public override Task Alter_index_change_sort_order() => base.Alter_index_change_sort_order();

	[Fact(Skip = SkipReason)]
	public override Task Alter_index_make_unique() => base.Alter_index_make_unique();

	[Fact(Skip = SkipReason)]
	public override Task Create_index_descending() => base.Create_index_descending();

	[Fact(Skip = SkipReason)]
	public override Task Create_index_descending_mixed() => base.Create_index_descending_mixed();

	[Fact(Skip = SkipReason)]
	public override Task Create_sequence_long() => base.Create_sequence_long();

	[Fact(Skip = SkipReason)]
	public override Task Create_sequence_short() => base.Create_sequence_short();

	[Fact(Skip = SkipReason)]
	public override Task Drop_primary_key_int() => base.Drop_primary_key_int();

	[Fact(Skip = SkipReason)]
	public override Task Drop_primary_key_string() => base.Drop_primary_key_string();

	[Fact(Skip = SkipReason)]
	public override Task Alter_sequence_restart_with() => base.Alter_sequence_restart_with();

	[Fact(Skip = SkipReason)]
	public override Task Add_column_with_unbounded_max_length() => base.Add_column_with_unbounded_max_length();

	[Fact(Skip = SkipReason)]
	public override Task Add_optional_primitive_collection_to_existing_table() => base.Add_optional_primitive_collection_to_existing_table();

	[Fact(Skip = SkipReason)]
	public override Task Add_required_primitve_collection_to_existing_table() => base.Add_required_primitve_collection_to_existing_table();

	[Fact(Skip = SkipReason)]
	public override Task Add_required_primitve_collection_with_custom_converter_and_custom_default_value_to_existing_table() => base.Add_required_primitve_collection_with_custom_converter_and_custom_default_value_to_existing_table();

	[Fact(Skip = SkipReason)]
	public override Task Add_required_primitve_collection_with_custom_default_value_to_existing_table() => base.Add_required_primitve_collection_with_custom_default_value_to_existing_table();

	[Fact(Skip = SkipReason)]
	public override Task Create_table_with_complex_type_with_required_properties_on_derived_entity_in_TPH() => base.Create_table_with_complex_type_with_required_properties_on_derived_entity_in_TPH();

	[Fact(Skip = SkipReason)]
	public override Task Create_table_with_optional_primitive_collection() => base.Create_table_with_optional_primitive_collection();

	[Fact(Skip = SkipReason)]
	public override Task Create_table_with_required_primitive_collection() => base.Create_table_with_required_primitive_collection();

	[Fact(Skip = SkipReason)]
	public override Task Add_required_primitve_collection_with_custom_default_value_sql_to_existing_table() => Task.CompletedTask;

	[Fact(Skip = SkipReason)]
	public override Task Add_required_primitive_collection_with_custom_default_value_sql_to_existing_table() => Task.CompletedTask;

	[Fact(Skip = SkipReason)]
	public override Task Add_json_columns_to_existing_table() => base.Add_json_columns_to_existing_table();

	[Fact(Skip = SkipReason)]
	public override Task Add_required_primitive_collection_to_existing_table() => base.Add_required_primitive_collection_to_existing_table();

	[Fact(Skip = SkipReason)]
	public override Task Add_required_primitive_collection_with_custom_converter_and_custom_default_value_to_existing_table() => base.Add_required_primitive_collection_with_custom_converter_and_custom_default_value_to_existing_table();

	[Fact(Skip = SkipReason)]
	public override Task Add_required_primitive_collection_with_custom_converter_to_existing_table() => base.Add_required_primitive_collection_with_custom_converter_to_existing_table();

	[Fact(Skip = SkipReason)]
	public override Task Add_required_primitive_collection_with_custom_default_value_to_existing_table() => base.Add_required_primitive_collection_with_custom_default_value_to_existing_table();

	[Fact(Skip = SkipReason)]
	public override Task Add_required_primitve_collection_with_custom_converter_to_existing_table() => base.Add_required_primitve_collection_with_custom_converter_to_existing_table();

	[Fact(Skip = SkipReason)]
	public override Task Convert_json_entities_to_regular_owned() => base.Convert_json_entities_to_regular_owned();

	[Fact(Skip = SkipReason)]
	public override Task Convert_regular_owned_entities_to_json() => base.Convert_regular_owned_entities_to_json();

	[Fact(Skip = SkipReason)]
	public override Task Convert_string_column_to_a_json_column_containing_collection() => base.Convert_string_column_to_a_json_column_containing_collection();

	[Fact(Skip = SkipReason)]
	public override Task Convert_string_column_to_a_json_column_containing_reference() => base.Convert_string_column_to_a_json_column_containing_reference();

	[Fact(Skip = SkipReason)]
	public override Task Convert_string_column_to_a_json_column_containing_required_reference() => base.Convert_string_column_to_a_json_column_containing_required_reference();

	[Fact(Skip = SkipReason)]
	public override Task Create_table_with_json_column() => base.Create_table_with_json_column();

	[Fact(Skip = SkipReason)]
	public override Task Create_table_with_json_column_explicit_json_column_names() => base.Create_table_with_json_column_explicit_json_column_names();

	[Fact(Skip = SkipReason)]
	public override Task Drop_json_columns_from_existing_table() => base.Drop_json_columns_from_existing_table();

	[Fact(Skip = SkipReason)]
	public override Task Rename_json_column() => base.Rename_json_column();

	[Fact(Skip = SkipReason)]
	public override Task Rename_table_with_json_column() => base.Rename_table_with_json_column();

	public class MigrationsFbFixture : MigrationsFixtureBase
	{
		protected override string StoreName => nameof(MigrationsFbTest);

		protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

		public override RelationalTestHelpers TestHelpers => FbTestHelpers.Instance;

		protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
#pragma warning disable EF1001
				 => base.AddServices(serviceCollection)
				.AddScoped<IDatabaseModelFactory, FbDatabaseModelFactory>();
#pragma warning restore EF1001
	}
}
