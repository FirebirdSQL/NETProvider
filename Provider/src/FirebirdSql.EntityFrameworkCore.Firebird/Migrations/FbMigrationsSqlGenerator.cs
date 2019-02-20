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
using System.Linq;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Migrations
{
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

		protected override void Generate(CreateTableOperation operation, IModel model, MigrationCommandListBuilder builder)
		{
			base.Generate(operation, model, builder);

			var columns = operation.Columns.Where(p => !p.IsNullable && string.IsNullOrWhiteSpace(p.DefaultValueSql) && p.DefaultValue == null);
			foreach (var column in columns)
			{
				var valueGenerationStrategy = column[FbAnnotationNames.ValueGenerationStrategy] as FbValueGenerationStrategy?;
				if (valueGenerationStrategy == FbValueGenerationStrategy.SequenceTrigger)
				{
					_behavior.CreateSequenceTriggerForColumn(column.Name, column.Table, column.Schema, builder);
				}
			}
		}

		protected override void Generate(RenameTableOperation operation, IModel model, MigrationCommandListBuilder builder)
			=> throw new NotSupportedException("Renaming table is not supported by Firebird.");

		protected override void Generate(DropTableOperation operation, IModel model, MigrationCommandListBuilder builder)
			=> base.Generate(operation, model, builder);

		protected override void Generate(AlterTableOperation operation, IModel model, MigrationCommandListBuilder builder)
			=> base.Generate(operation, model, builder);


		protected override void Generate(AlterColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
		{
			var valueGenerationStrategy = operation[FbAnnotationNames.ValueGenerationStrategy] as FbValueGenerationStrategy?;
			var oldValueGenerationStrategy = operation.OldColumn[FbAnnotationNames.ValueGenerationStrategy] as FbValueGenerationStrategy?;
			if (oldValueGenerationStrategy == FbValueGenerationStrategy.IdentityColumn && valueGenerationStrategy != FbValueGenerationStrategy.IdentityColumn)
			{
				throw new InvalidOperationException("Cannot remove identity on < FB4.");

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
				_behavior.DropSequenceTriggerForColumn(operation.Name, operation.Table, operation.Schema, builder);
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
				var type = GetColumnType(operation.Schema, operation.Table, operation.Name, operation.ClrType, operation.IsUnicode, operation.MaxLength, operation.IsFixedLength, operation.IsRowVersion, model);
				builder.Append(type);
			}
			if (valueGenerationStrategy == FbValueGenerationStrategy.IdentityColumn)
			{
				builder.Append(" GENERATED BY DEFAULT AS IDENTITY");
			}
			builder.Append(operation.IsNullable ? string.Empty : " NOT NULL");
			TerminateStatement(builder);

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
				DefaultValue(operation.DefaultValue, operation.DefaultValueSql, builder);
				TerminateStatement(builder);
			}

			if (valueGenerationStrategy == FbValueGenerationStrategy.SequenceTrigger)
			{
				_behavior.CreateSequenceTriggerForColumn(operation.Name, operation.Table, operation.Schema, builder);
			}
		}

		protected override void Generate(AddColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
			=> base.Generate(operation, model, builder);

		protected override void Generate(DropColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
			=> base.Generate(operation, model, builder);

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


		protected override void Generate(CreateIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
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
			TerminateStatement(builder);
		}

		protected override void Generate(DropIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
			=> base.Generate(operation, model, builder);

		protected override void Generate(RenameIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
			=> throw new NotSupportedException("Renaming index is not supported by Firebird.");


		protected override void Generate(CreateSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
		{
			builder.Append("CREATE SEQUENCE ");
			builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));
			builder.Append(" START WITH ");
			builder.Append(operation.StartValue);
			builder.Append(" INCREMENT BY ");
			builder.Append(operation.IncrementBy);
			TerminateStatement(builder);
		}

		protected override void Generate(AlterSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
		{
			builder.Append("ALTER SEQUENCE ");
			builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));
			builder.Append(" RESTART INCREMENT BY ");
			builder.Append(operation.IncrementBy);
			TerminateStatement(builder);
		}

		protected override void Generate(RestartSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
		{
			builder.Append("ALTER SEQUENCE ");
			builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));
			builder.Append(" START WITH ");
			builder.Append(operation.StartValue);
			TerminateStatement(builder);
		}

		protected override void Generate(RenameSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
			=> throw new NotSupportedException("Renaming sequence is not supported by Firebird.");


		protected override void Generate(AddPrimaryKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
			=> base.Generate(operation, model, builder);

		protected override void Generate(DropPrimaryKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
			=> base.Generate(operation, model, builder);


		protected override void Generate(AddForeignKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
			=> base.Generate(operation, model, builder);

		protected override void Generate(DropForeignKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
		{
			base.Generate(operation, model, builder);
		}


		protected override void ColumnDefinition(string schema, string table, string name, Type clrType, string type, bool? unicode, int? maxLength, bool? fixedLength, bool rowVersion, bool nullable, object defaultValue, string defaultValueSql, string computedColumnSql, IAnnotatable annotatable, IModel model, MigrationCommandListBuilder builder)
		{
			builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
				   .Append(" ")
				   .Append(type ?? GetColumnType(schema, table, name, clrType, unicode, maxLength, fixedLength, rowVersion, model));

			var valueGenerationStrategy = annotatable[FbAnnotationNames.ValueGenerationStrategy] as FbValueGenerationStrategy?;
			if (valueGenerationStrategy == FbValueGenerationStrategy.IdentityColumn)
			{
				builder.Append(" GENERATED BY DEFAULT AS IDENTITY");
			}

			if (!nullable)
			{
				builder.Append(" NOT NULL");
			}

			DefaultValue(defaultValue, defaultValueSql, builder);
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
}
