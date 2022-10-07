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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Migrations;

public class FbMigrationsSqlGenerator : MigrationsSqlGenerator
{
	readonly IFbMigrationSqlGeneratorBehavior _behavior;
	readonly IFbOptions _options;

	public FbMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IFbMigrationSqlGeneratorBehavior behavior, IFbOptions options)
		: base(dependencies)
	{
		_behavior = behavior;
		_options = options;
	}

	protected override void Generate(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
	{
		switch (operation)
		{
			default:
				base.Generate(operation, model, builder);
				break;
		}
	}

	protected override void Generate(CreateTableOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
	{
		base.Generate(operation, model, builder, true);

		var columns = operation.Columns.Where(p => !p.IsNullable && string.IsNullOrWhiteSpace(p.DefaultValueSql) && p.DefaultValue == null);
		foreach (var column in columns)
		{
			var valueGenerationStrategy = column[FbAnnotationNames.ValueGenerationStrategy] as FbValueGenerationStrategy?;
			if (valueGenerationStrategy == FbValueGenerationStrategy.SequenceTrigger)
			{
				_behavior.CreateSequenceTriggerForColumn(column.Name, column.Table, column.Schema, Options, builder);
			}
		}
	}

	protected override void Generate(RenameTableOperation operation, IModel model, MigrationCommandListBuilder builder)
		=> throw new NotSupportedException("Renaming table is not supported by Firebird.");

	protected override void Generate(DropTableOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
		=> base.Generate(operation, model, builder, terminate);

	protected override void Generate(AlterTableOperation operation, IModel model, MigrationCommandListBuilder builder)
		=> base.Generate(operation, model, builder);


	protected override void Generate(AlterColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
	{
		var valueGenerationStrategy = operation[FbAnnotationNames.ValueGenerationStrategy] as FbValueGenerationStrategy?;
		var oldValueGenerationStrategy = operation.OldColumn[FbAnnotationNames.ValueGenerationStrategy] as FbValueGenerationStrategy?;
		if (oldValueGenerationStrategy == FbValueGenerationStrategy.IdentityColumn && valueGenerationStrategy != FbValueGenerationStrategy.IdentityColumn)
		{
			throw new InvalidOperationException("Cannot remove identity from column.");

			// will be recreated, if needed, by next statement
			// supported only on FB4
			//builder.Append("ALTER TABLE ");
			//builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));
			//builder.Append(" ALTER COLUMN ");
			//builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
			//builder.Append(" DROP IDENTITY");
			//TerminateStatement(builder);
		}
		if (oldValueGenerationStrategy == FbValueGenerationStrategy.SequenceTrigger && valueGenerationStrategy != FbValueGenerationStrategy.SequenceTrigger)
		{
			_behavior.DropSequenceTriggerForColumn(operation.Name, operation.Table, operation.Schema, Options, builder);
		}

		// will be recreated, if needed, by next statement
		builder.Append("ALTER TABLE ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));
		builder.Append(" ALTER COLUMN ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
		builder.Append(" DROP NOT NULL");
		TerminateStatement(builder);

		builder.Append("ALTER TABLE ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));
		builder.Append(" ALTER COLUMN ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
		builder.Append(" TYPE ");
		if (operation.ColumnType != null)
		{
			builder.Append(operation.ColumnType);
		}
		else
		{
			var type = GetColumnType(operation.Schema, operation.Table, operation.Name, operation, model);
			builder.Append(type);
		}
		if (operation.Collation != null)
		{
			builder.Append(" COLLATE ");
			builder.Append(operation.Collation);
		}
		if (valueGenerationStrategy == FbValueGenerationStrategy.IdentityColumn)
		{
			builder.Append(" GENERATED BY DEFAULT AS IDENTITY");
		}
		TerminateStatement(builder);

		if (!operation.IsNullable)
		{
			builder.Append("ALTER TABLE ");
			builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));
			builder.Append(" ALTER COLUMN ");
			builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
			builder.Append(" SET NOT NULL");
			TerminateStatement(builder);
		}

		if (operation.DefaultValue != null || !string.IsNullOrWhiteSpace(operation.DefaultValueSql))
		{
			builder.Append("ALTER TABLE ");
			builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));
			builder.Append(" ALTER COLUMN ");
			builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
			builder.Append(" DROP DEFAULT");
			TerminateStatement(builder);

			builder.Append("ALTER TABLE ");
			builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));
			builder.Append(" ALTER COLUMN ");
			builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
			builder.Append(" SET");
			DefaultValue(operation.DefaultValue, operation.DefaultValueSql, operation.ColumnType, builder);
			TerminateStatement(builder);
		}

		if (valueGenerationStrategy == FbValueGenerationStrategy.SequenceTrigger)
		{
			_behavior.CreateSequenceTriggerForColumn(operation.Name, operation.Table, operation.Schema, Options, builder);
		}
	}

	protected override void Generate(AddColumnOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
		=> base.Generate(operation, model, builder, terminate);

	protected override void Generate(DropColumnOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
	{
		builder.Append("ALTER TABLE ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));
		builder.Append(" DROP ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
		if (terminate)
			TerminateStatement(builder);
	}

	protected override void Generate(RenameColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
	{
		builder.Append("ALTER TABLE ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));
		builder.Append(" ALTER COLUMN ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
		builder.Append(" TO ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName));
		TerminateStatement(builder);
	}


	protected override void Generate(CreateIndexOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
	{
		builder.Append("CREATE ");
		if (operation.IsUnique)
		{
			builder.Append("UNIQUE ");
		}
		IndexTraits(operation, model, builder);
		builder.Append("INDEX ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
		builder.Append(" ON ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));
		if (!string.IsNullOrEmpty(operation.Filter))
		{
			builder.Append(" COMPUTED BY (");
			builder.Append(operation.Filter);
			builder.Append(")");
		}
		else
		{
			builder.Append(" (");
			builder.Append(ColumnList(operation.Columns));
			builder.Append(")");
		}
		if (terminate)
			TerminateStatement(builder);
	}

	protected override void Generate(DropIndexOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
	{
		builder.Append("DROP ");
		IndexTraits(operation, model, builder);
		builder.Append("INDEX ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
		if (terminate)
			TerminateStatement(builder);
	}
	protected override void Generate(RenameIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
		=> throw new NotSupportedException("Renaming index is not supported by Firebird.");


	protected override void Generate(CreateSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
	{
		builder.Append("CREATE SEQUENCE ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));
		builder.Append(" START WITH ");
		builder.Append(operation.StartValue.ToString(CultureInfo.InvariantCulture));
		builder.Append(" INCREMENT BY ");
		builder.Append(operation.IncrementBy.ToString(CultureInfo.InvariantCulture));
		TerminateStatement(builder);
	}

	protected override void Generate(AlterSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
	{
		builder.Append("ALTER SEQUENCE ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));
		builder.Append(" RESTART INCREMENT BY ");
		builder.Append(operation.IncrementBy.ToString(CultureInfo.InvariantCulture));
		TerminateStatement(builder);
	}

	protected override void Generate(RestartSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
	{
		builder.Append("ALTER SEQUENCE ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));
		builder.Append(" START WITH ");
		builder.Append(operation.StartValue.ToString(CultureInfo.InvariantCulture));
		TerminateStatement(builder);
	}

	protected override void Generate(RenameSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
		=> throw new NotSupportedException("Renaming sequence is not supported by Firebird.");

	protected override void Generate(DropSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
		=> base.Generate(operation, model, builder);

	protected override void Generate(AddPrimaryKeyOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
		=> base.Generate(operation, model, builder, terminate);

	protected override void Generate(DropPrimaryKeyOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
		=> base.Generate(operation, model, builder, terminate);


	protected override void Generate(AddForeignKeyOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
		=> base.Generate(operation, model, builder, terminate);

	protected override void Generate(DropForeignKeyOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
		=> base.Generate(operation, model, builder, terminate);


	protected override void Generate(AddUniqueConstraintOperation operation, IModel model, MigrationCommandListBuilder builder)
		=> base.Generate(operation, model, builder);

	protected override void Generate(DropUniqueConstraintOperation operation, IModel model, MigrationCommandListBuilder builder)
		=> base.Generate(operation, model, builder);


	protected override void Generate(AlterDatabaseOperation operation, IModel model, MigrationCommandListBuilder builder)
		=> base.Generate(operation, model, builder);


	protected override void Generate(SqlOperation operation, IModel model, MigrationCommandListBuilder builder)
		=> base.Generate(operation, model, builder);


	protected override void Generate(DropSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
		=> throw new NotSupportedException("Schemas are not supported by Firebird.");

	protected override void Generate(EnsureSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
		=> throw new NotSupportedException("Schemas are not supported by Firebird.");


	protected override void ColumnDefinition(string schema, string table, string name, ColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
	{
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name));
		builder.Append(" ");
		builder.Append(operation.ColumnType ?? GetColumnType(schema, table, name, operation, model));

		if (operation.Collation != null)
		{
			builder.Append(" COLLATE ");
			builder.Append(operation.Collation);
		}

		var valueGenerationStrategy = operation[FbAnnotationNames.ValueGenerationStrategy] as FbValueGenerationStrategy?;
		if (valueGenerationStrategy == FbValueGenerationStrategy.IdentityColumn)
		{
			builder.Append(" GENERATED BY DEFAULT AS IDENTITY");
		}

		DefaultValue(operation.DefaultValue, operation.DefaultValueSql, operation.ColumnType, builder);

		if (!operation.IsNullable)
		{
			builder.Append(" NOT NULL");
		}
	}

	protected override void DefaultValue(object defaultValue, string defaultValueSql, string columnType, MigrationCommandListBuilder builder)
	{
		if (defaultValueSql != null)
		{
			builder.Append(" DEFAULT ");
			builder.Append(defaultValueSql);
		}
		else if (defaultValue != null)
		{
			builder.Append(" DEFAULT ");
			var typeMapping = Dependencies.TypeMappingSource.GetMappingForValue(defaultValue);
			builder.Append(typeMapping.GenerateSqlLiteral(defaultValue));
		}
	}

	protected override void ForeignKeyConstraint(AddForeignKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
	{
		if (operation.Name != null)
		{
			builder.Append("CONSTRAINT ");
			builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
			builder.Append(" ");
		}
		builder.Append("FOREIGN KEY (");
		builder.Append(ColumnList(operation.Columns));
		builder.Append(") REFERENCES ");
		builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.PrincipalTable, operation.PrincipalSchema));
		if (operation.PrincipalColumns != null)
		{
			builder.Append(" (");
			builder.Append(ColumnList(operation.PrincipalColumns));
			builder.Append(")");
		}
		if (operation.OnUpdate != ReferentialAction.Restrict)
		{
			builder.Append(" ON UPDATE ");
			ForeignKeyAction(operation.OnUpdate, builder);
		}
		if (operation.OnDelete != ReferentialAction.Restrict)
		{
			builder.Append(" ON DELETE ");
			ForeignKeyAction(operation.OnDelete, builder);
		}
	}

	protected override void ForeignKeyAction(ReferentialAction referentialAction, MigrationCommandListBuilder builder)
	{
		switch (referentialAction)
		{
			case ReferentialAction.NoAction:
				builder.Append("NO ACTION");
				break;
			default:
				base.ForeignKeyAction(referentialAction, builder);
				break;
		}
	}

	protected virtual void TerminateStatement(MigrationCommandListBuilder builder)
	{
		builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
		EndStatement(builder);
	}
}
