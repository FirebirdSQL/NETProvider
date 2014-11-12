/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied. See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2014 Jiri Cincura (jiri@cincura.net)
 *		Based on my work on NuoDbMigrationSqlGenerator for NuoDB.
 *  All Rights Reserved.
 *  
 */

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.EntityFramework6.SqlGen;

namespace FirebirdSql.Data.EntityFramework6
{
	public class FbMigrationSqlGenerator : MigrationSqlGenerator
	{
		public override IEnumerable<MigrationStatement> Generate(IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
		{
			InitializeProviderServices(providerManifestToken);
			return GenerateStatements(migrationOperations).ToArray();
		}

		void InitializeProviderServices(string providerManifestToken)
		{
			using (var connection = CreateConnection())
			{
				ProviderManifest = DbProviderServices.GetProviderServices(connection).GetProviderManifest(providerManifestToken);
			}
		}

		#region Operations

		protected IEnumerable<MigrationStatement> Generate(MigrationOperation operation)
		{
			throw new NotSupportedException(string.Format("Unknown operation '{0}'.", operation.GetType().FullName));
		}

		protected virtual IEnumerable<MigrationStatement> Generate(UpdateDatabaseOperation operation)
		{
			return GenerateStatements(operation.Migrations.SelectMany(x => x.Operations));
		}

		protected virtual IEnumerable<MigrationStatement> Generate(SqlOperation operation)
		{
			yield return Statement(operation.Sql, operation.SuppressTransaction);
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AddColumnOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Quote(ExtractName(operation.Table)));
				writer.Write(" ADD ");
				var column = operation.Column;
				writer.Write(Generate(column));
				if (column.IsNullable != null
					&& !column.IsNullable.Value
					&& column.DefaultValue == null
					&& string.IsNullOrWhiteSpace(column.DefaultValueSql)
					&& !column.IsIdentity
					&& !column.IsTimestamp)
				{
					writer.Write(" DEFAULT ");
					writer.Write(WriteValue((dynamic)column.ClrDefaultValue));
				}
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AddForeignKeyOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Quote(ExtractName(operation.DependentTable)));
				writer.Write(" ADD FOREIGN KEY (");
				WriteColumns(writer, operation.DependentColumns.Select(Quote));
				writer.Write(") REFERENCES ");
				writer.Write(Quote(ExtractName(operation.PrincipalTable)));
				writer.Write(" (");
				WriteColumns(writer, operation.PrincipalColumns.Select(Quote));
				writer.Write(")");
				if (operation.CascadeDelete)
				{
					writer.Write(" ON DELETE CASCADE");
				}
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AddPrimaryKeyOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Quote(ExtractName(operation.Table)));
				writer.Write(" ADD CONSTRAINT ");
				writer.Write(Quote(operation.Name));
				writer.Write(" PRIMARY KEY ");
				writer.Write("(");
				WriteColumns(writer, operation.Columns.Select(Quote));
				writer.Write(")");
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AlterColumnOperation operation)
		{
			var column = operation.Column;
			// drop NOT NULL first, either it will be recreated or it was to drop
			using(var writer = SqlWriter())
			{
				writer.WriteLine("EXECUTE BLOCK");
				writer.WriteLine("AS");
				writer.WriteLine("declare constraint_name type of column rdb$relation_constraints.rdb$constraint_name;");
				writer.WriteLine("BEGIN");
				writer.Indent++;
				writer.WriteLine("select rc.rdb$constraint_name");
				writer.WriteLine("from rdb$relation_constraints rc");
				writer.WriteLine("join rdb$check_constraints cc on rc.rdb$constraint_name = cc.rdb$constraint_name");
				writer.Write("where rc.rdb$constraint_type = 'NOT NULL' and rc.rdb$relation_name = '");
				writer.Write(ExtractName(operation.Table));
				writer.Write("' and cc.rdb$trigger_name  = '");
				writer.Write(column.Name);
				writer.Write("'");
				writer.WriteLine();
				writer.WriteLine("into :constraint_name;");
				writer.WriteLine("if (constraint_name is not null) then");
				writer.WriteLine("begin");
				writer.Indent++;
				writer.Write("execute statement 'alter table ");
				writer.Write(Quote(ExtractName(operation.Table)));
				writer.Write(" drop constraint ' || :constraint_name;");
				writer.WriteLine();
				writer.Indent--;
				writer.WriteLine("end");
				writer.Indent--;
				writer.Write("END");
				yield return Statement(writer);
			}
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Quote(ExtractName(operation.Table)));
				writer.Write(" ALTER COLUMN ");
				writer.Write(Quote(column.Name));
				writer.Write(" TYPE ");
				writer.Write(BuildPropertyType(column));
				// possible NOT NULL drop was dropped with statement above
				if (column.IsNullable != null && !column.IsNullable.Value)
				{
					writer.Write(" NOT NULL");
				}
				yield return Statement(writer);
			}

			if (column.DefaultValue != null || !string.IsNullOrWhiteSpace(column.DefaultValueSql))
			{
				using (var writer = SqlWriter())
				{
					writer.Write("ALTER TABLE ");
					writer.Write(Quote(ExtractName(operation.Table)));
					writer.Write(" ALTER COLUMN ");
					writer.Write(Quote(column.Name));
					writer.Write(" DROP DEFAULT");
					yield return Statement(writer);
				}

				using (var writer = SqlWriter())
				{
					writer.Write("ALTER TABLE ");
					writer.Write(Quote(ExtractName(operation.Table)));
					writer.Write(" ALTER COLUMN ");
					writer.Write(Quote(column.Name));
					writer.Write(" SET DEFAULT ");
					writer.Write(column.DefaultValue != null ? WriteValue((dynamic)column.DefaultValue) : column.DefaultValueSql);
					yield return Statement(writer);
				}
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AlterProcedureOperation operation)
		{
			return Generate(operation, "ALTER");
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AlterTableOperation operation)
		{
			// Nothing to do since there is no inherent semantics associated with annotations
			yield break;
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateIndexOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("CREATE ");
				if (operation.IsUnique)
				{
					writer.Write("UNIQUE ");
				}
				writer.Write("INDEX ");
				writer.Write(Quote(operation.Name));
				writer.Write(" ON ");
				writer.Write(Quote(ExtractName(operation.Table)));
				writer.Write("(");
				WriteColumns(writer, operation.Columns.Select(Quote));
				writer.Write(")");
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateProcedureOperation operation)
		{
			return Generate(operation, "CREATE");
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateTableOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("CREATE TABLE ");
				writer.Write(Quote(ExtractName(operation.Name)));
				writer.Write(" (");
				writer.WriteLine();
				writer.Indent++;
				WriteColumns(writer, operation.Columns.Select(Generate), true);
				writer.Indent--;
				writer.WriteLine();
				writer.Write(")");
				yield return Statement(writer);
			}
			if (operation.PrimaryKey != null)
			{
				foreach (var item in Generate(operation.PrimaryKey))
					yield return item;
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropColumnOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Quote(ExtractName(operation.Table)));
				writer.Write(" DROP COLUMN ");
				writer.Write(Quote(operation.Name));
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropForeignKeyOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Quote(ExtractName(operation.DependentTable)));
				writer.Write(" DROP CONSTRAINT ");
				writer.Write(Quote(operation.Name));
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropIndexOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("DROP INDEX ");
				writer.Write(Quote(operation.Name));
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropPrimaryKeyOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Quote(ExtractName(operation.Table)));
				writer.Write(" DROP CONSTRAINT ");
				writer.Write(Quote(operation.Name));
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropProcedureOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("DROP PROCEDURE ");
				writer.Write(Quote(ExtractName(operation.Name)));
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropTableOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("DROP TABLE ");
				writer.Write(Quote(ExtractName(operation.Name)));
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(MoveProcedureOperation operation)
		{
			throw new NotSupportedException("Moving procedure is not supported by Firebird.");
		}

		protected virtual IEnumerable<MigrationStatement> Generate(MoveTableOperation operation)
		{
			throw new NotSupportedException("Moving table is not supported by Firebird.");
		}

		protected virtual IEnumerable<MigrationStatement> Generate(RenameColumnOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Quote(ExtractName(operation.Table)));
				writer.Write(" ALTER COLUMN ");
				writer.Write(Quote(operation.Name));
				writer.Write(" TO ");
				writer.Write(Quote(operation.NewName));
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(RenameIndexOperation operation)
		{
			throw new NotSupportedException("Renaming index is not supported by Firebird.");
		}

		protected virtual IEnumerable<MigrationStatement> Generate(RenameProcedureOperation operation)
		{
			throw new NotSupportedException("Renaming procedure is not supported by Firebird.");
		}

		protected virtual IEnumerable<MigrationStatement> Generate(RenameTableOperation operation)
		{
			throw new NotSupportedException("Renaming table is not supported by Firebird.");
		}

		protected virtual IEnumerable<MigrationStatement> Generate(HistoryOperation operation)
		{
			foreach (var commandTree in operation.CommandTrees)
			{
				List<DbParameter> _;

				switch (commandTree.CommandTreeKind)
				{
					case DbCommandTreeKind.Insert:
						using (var writer = SqlWriter())
						{
							writer.Write(DmlSqlGenerator.GenerateInsertSql((DbInsertCommandTree)commandTree, out _,
								generateParameters: false));
							yield return Statement(writer);
						}
						break;
					case DbCommandTreeKind.Delete:
						using (var writer = SqlWriter())
						{
							writer.Write(DmlSqlGenerator.GenerateDeleteSql((DbDeleteCommandTree)commandTree, out _,
								generateParameters: false));
							yield return Statement(writer);
						}
						break;
				}
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(ProcedureOperation operation, string action)
		{
			using (var writer = SqlWriter())
			{
				var inputParameters = operation.Parameters.Where(x => !x.IsOutParameter).ToArray();
				var outputParameters = operation.Parameters.Where(x => x.IsOutParameter).ToArray();

				writer.Write(action);
				writer.Write(" PROCEDURE ");
				writer.Write(Quote(ExtractName(operation.Name)));
				if (inputParameters.Any())
				{
					writer.Write(" (");
					writer.WriteLine();
					writer.Indent++;
					WriteColumns(writer, inputParameters.Select(Generate), true);
					writer.Indent--;
					writer.WriteLine();
					writer.Write(")");
				}
				if (outputParameters.Any())
				{
					writer.WriteLine();
					writer.Write("RETURNS (");
					writer.WriteLine();
					writer.Indent++;
					WriteColumns(writer, outputParameters.Select(Generate), true);
					writer.Indent--;
					writer.WriteLine();
					writer.Write(")");
				}
				writer.WriteLine();
				writer.WriteLine("AS");
				writer.WriteLine("BEGIN");
				writer.Write(operation.BodySql);
				writer.WriteLine();
				writer.Write("END");
				yield return Statement(writer);
			}
		}

		protected string Generate(ColumnModel column)
		{
			var builder = new StringBuilder();
			builder.Append(Quote(column.Name));
			builder.Append(" ");
			builder.Append(BuildPropertyType(column));

			if ((column.IsNullable != null)
				&& !column.IsNullable.Value)
			{
				builder.Append(" NOT NULL");
			}

			if (column.DefaultValue != null)
			{
				builder.Append(" DEFAULT ");
				builder.Append(WriteValue((dynamic)column.DefaultValue));
			}
			else if (!string.IsNullOrWhiteSpace(column.DefaultValueSql))
			{
				builder.Append(" DEFAULT ");
				builder.Append(column.DefaultValueSql);
			}
			else if (column.IsIdentity)
			{
#warning Not supported on FB. What to do reasonably?
			}

			return builder.ToString();
		}

		protected string Generate(ParameterModel parameter)
		{
			var builder = new StringBuilder();
			builder.Append(Quote(parameter.Name));
			builder.Append(" ");
			builder.Append(BuildPropertyType(parameter));
			return builder.ToString();
		}

		#endregion

		#region Helpers

		static MigrationStatement Statement(SqlWriter sqlWriter, bool suppressTransaction = false)
		{
			return Statement(sqlWriter.ToString(), suppressTransaction);
		}
		protected static MigrationStatement Statement(string sql, bool suppressTransaction = false)
		{
			return new MigrationStatement
			{
				Sql = sql,
				SuppressTransaction = suppressTransaction,
				BatchTerminator = ";",
			};
		}

		protected static string Quote(string name)
		{
			return SqlGenerator.QuoteIdentifier(name);
		}

		protected static string WriteValue(object value)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}", value);
		}

		protected static string WriteValue(DateTime value)
		{
			return SqlGenerator.FormatDateTime(value);
		}

		protected static string WriteValue(byte[] value)
		{
			return SqlGenerator.FormatBinary(value);
		}

		protected static string WriteValue(bool value)
		{
			return SqlGenerator.FormatBoolean(value);
		}

		protected static string WriteValue(Guid value)
		{
			return SqlGenerator.FormatGuid(value);
		}

		protected static string WriteValue(string value)
		{
			return SqlGenerator.FormatString(value, true, value.Length);
		}

		protected static string WriteValue(TimeSpan value)
		{
			return SqlGenerator.FormatTime(value);
		}

		string BuildPropertyType(PropertyModel propertyModel)
		{
			var storeTypeName = propertyModel.StoreType;
			var typeUsage = ProviderManifest.GetStoreType(propertyModel.TypeUsage);
			if (!string.IsNullOrWhiteSpace(storeTypeName))
			{
				typeUsage = BuildStoreTypeUsage(storeTypeName, propertyModel) ?? typeUsage;
			}
			return SqlGenerator.GetSqlPrimitiveType(typeUsage);
		}

		string ExtractName(string name)
		{
			return name.Substring(name.LastIndexOf('.') + 1);
		}

		static SqlWriter SqlWriter()
		{
			var result = new SqlWriter(new StringBuilder());
			result.Indent++;
			return result;
		}

		static void WriteColumns(SqlWriter writer, IEnumerable<string> columns, bool separateLines = false)
		{
			var separator = (string)null;
			foreach (var column in columns)
			{
				if (separator != null)
				{
					writer.Write(separator);
					if (separateLines)
						writer.WriteLine();
				}
				writer.Write(column);
				separator = ", ";
			}
		}

		static DbConnection CreateConnection()
		{
			return DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(FbProviderServices.ProviderInvariantName).CreateConnection();
		}

		IEnumerable<MigrationStatement> GenerateStatements(IEnumerable<MigrationOperation> operations)
		{
			return operations.Select<dynamic, IEnumerable<MigrationStatement>>(x => Generate(x)).SelectMany(x => x);
		}

		#endregion
	}
}
