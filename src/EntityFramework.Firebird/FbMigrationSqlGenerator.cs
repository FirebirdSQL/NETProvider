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
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EntityFramework.Firebird.SqlGen;
using FirebirdSql.Data.Common;

namespace EntityFramework.Firebird;

public class FbMigrationSqlGenerator : MigrationSqlGenerator
{
	readonly IFbMigrationSqlGeneratorBehavior _behavior;

	string _migrationsHistoryTableName;

	public FbMigrationSqlGenerator(IFbMigrationSqlGeneratorBehavior behavior = null)
	{
		_behavior = behavior ?? new DefaultFbMigrationSqlGeneratorBehavior();
	}

	public override IEnumerable<MigrationStatement> Generate(IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
	{
		InitializeProviderServices(providerManifestToken);

		var lastOperation = migrationOperations.Last();
		var historyOperation = lastOperation is UpdateDatabaseOperation updateDatabaseOperation
			? updateDatabaseOperation.Migrations.First().Operations.OfType<HistoryOperation>().First()
			: lastOperation as HistoryOperation;

		if (historyOperation != null)
		{
			var modify = historyOperation.CommandTrees.First();
			_migrationsHistoryTableName = ((DbScanExpression)modify.Target.Expression).Target.Table;
		}
		//This happens only and only if downgrading database to initial point (ie. reverting also Initial migration)
		else
		{
			var dropTableOperation = (DropTableOperation)lastOperation; //DropTableOperation for MigrationHistory-table
			_migrationsHistoryTableName = Regex.Replace(dropTableOperation.Name, @".+\.(.+)", "$1");
		}

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
		var tableName = CheckName(ExtractName(operation.Table));
		var column = operation.Column;
		if (column.IsNullable != null
				&& !column.IsNullable.Value
				&& column.DefaultValue == null
				&& string.IsNullOrWhiteSpace(column.DefaultValueSql)
				&& !column.IsIdentity
				&& !column.IsTimestamp)
		{
			column.DefaultValue = column.ClrDefaultValue;
		}
		var columnData = Generate(column, tableName);
		using (var writer = SqlWriter())
		{
			writer.Write("ALTER TABLE ");
			writer.Write(Quote(tableName));
			writer.Write(" ADD ");
			writer.Write(columnData.Item1);
			yield return Statement(writer);
		}
		foreach (var item in columnData.Item2.Select(x => Statement(x)))
			yield return item;
	}

	protected virtual IEnumerable<MigrationStatement> Generate(AddForeignKeyOperation operation)
	{
		using (var writer = SqlWriter())
		{
			writer.Write("ALTER TABLE ");
			writer.Write(Quote(CheckName(ExtractName(operation.DependentTable))));
			writer.Write(" ADD CONSTRAINT ");
			writer.Write(Quote(CheckName(CreateItemName(operation.Name))));
			writer.Write(" FOREIGN KEY (");
			WriteColumns(writer, operation.DependentColumns.Select(Quote));
			writer.Write(") REFERENCES ");
			writer.Write(Quote(CheckName(ExtractName(operation.PrincipalTable))));
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
			writer.Write(Quote(CheckName(ExtractName(operation.Table))));
			writer.Write(" ADD CONSTRAINT ");
			writer.Write(Quote(CheckName(CreateItemName(operation.Name))));
			writer.Write(" PRIMARY KEY (");
			WriteColumns(writer, operation.Columns.Select(Quote));
			writer.Write(")");
			yield return Statement(writer);
		}
	}

	protected virtual IEnumerable<MigrationStatement> Generate(AlterColumnOperation operation)
	{
		var column = operation.Column;
		var tableName = CheckName(ExtractName(operation.Table));
		var columnName = CheckName(column.Name);
		// drop NOT NULL first, either it will be recreated or it was to drop
		using (var writer = SqlWriter())
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
			writer.Write(tableName);
			writer.Write("' and cc.rdb$trigger_name  = '");
			writer.Write(columnName);
			writer.Write("'");
			writer.WriteLine();
			writer.WriteLine("into :constraint_name;");
			writer.WriteLine("if (constraint_name is not null) then");
			writer.WriteLine("begin");
			writer.Indent++;
			writer.Write("execute statement 'alter table ");
			writer.Write(Quote(tableName));
			writer.Write(" drop constraint ' || :constraint_name;");
			writer.WriteLine();
			writer.Indent--;
			writer.WriteLine("end");
			writer.Indent--;
			writer.Write("END");
			yield return Statement(writer);
		}
		// drop identity trigger first, either it will be recreated or it was to drop
		foreach (var item in _behavior.DropIdentityForColumn(columnName, tableName))
			yield return Statement(item);

		using (var writer = SqlWriter())
		{
			writer.Write("ALTER TABLE ");
			writer.Write(Quote(tableName));
			writer.Write(" ALTER COLUMN ");
			writer.Write(Quote(columnName));
			writer.Write(" TYPE ");
			writer.Write(BuildPropertyType(column));
			// possible NOT NULL drop was dropped with statement above
			if (column.IsNullable != null && !column.IsNullable.Value)
			{
				writer.Write(" NOT NULL");
			}
			if (column.Type == PrimitiveTypeKind.Boolean)
			{
				writer.Write(" CHECK(");
				writer.Write(Quote(columnName));
				writer.Write(" IN (0,1))");
			}
			yield return Statement(writer);
		}

		if (column.DefaultValue != null || !string.IsNullOrWhiteSpace(column.DefaultValueSql))
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Quote(tableName));
				writer.Write(" ALTER COLUMN ");
				writer.Write(Quote(columnName));
				writer.Write(" DROP DEFAULT");
				yield return Statement(writer);
			}

			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Quote(tableName));
				writer.Write(" ALTER COLUMN ");
				writer.Write(Quote(columnName));
				writer.Write(" SET DEFAULT ");
				writer.Write(column.DefaultValue != null ? WriteValue((dynamic)column.DefaultValue) : column.DefaultValueSql);
				yield return Statement(writer);
			}
		}

		if (column.IsIdentity)
		{
			// possible identity drop was dropped with statement above
			foreach (var item in _behavior.CreateIdentityForColumn(columnName, tableName))
				yield return Statement(item);
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
			writer.Write(Quote(CheckName(CreateItemName(BuildIndexName(operation)))));
			writer.Write(" ON ");
			writer.Write(Quote(CheckName(ExtractName(operation.Table))));
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
		var tableName = CheckName(ExtractName(operation.Name));
		var isMigrationsHistoryTable = tableName.Equals(_migrationsHistoryTableName, StringComparison.InvariantCulture);
		var columnsData = operation.Columns.Select(x => Generate(x, tableName)).ToArray();
		using (var writer = SqlWriter())
		{
			if (isMigrationsHistoryTable)
			{
				writer.WriteLine("EXECUTE BLOCK");
				writer.WriteLine("AS");
				writer.WriteLine("BEGIN");
				writer.Indent++;
				writer.WriteLine("EXECUTE STATEMENT");
				writer.Indent++;
				writer.Write("'");
			}
			writer.Write("CREATE TABLE ");
			writer.Write(Quote(tableName));
			writer.Write(" (");
			writer.WriteLine();
			writer.Indent++;
			WriteColumns(writer, columnsData.Select(x => x.Item1), true);
			writer.Indent--;
			writer.WriteLine();
			writer.Write(")");
			if (isMigrationsHistoryTable)
			{
				writer.WriteLine("'");
				writer.Indent--;
				writer.WriteLine("WITH AUTONOMOUS TRANSACTION;");
				writer.Indent--;
				writer.Write("END");
			}
			yield return Statement(writer);
		}
		if (operation.PrimaryKey != null)
		{
			foreach (var item in Generate(operation.PrimaryKey))
				yield return item;
		}
		foreach (var item in columnsData.SelectMany(x => x.Item2).Select(x => Statement(x)))
			yield return item;
	}

	protected virtual IEnumerable<MigrationStatement> Generate(DropColumnOperation operation)
	{
		using (var writer = SqlWriter())
		{
			writer.Write("ALTER TABLE ");
			writer.Write(Quote(CheckName(ExtractName(operation.Table))));
			writer.Write(" DROP ");
			writer.Write(Quote(CheckName(operation.Name)));
			yield return Statement(writer);
		}
	}

	protected virtual IEnumerable<MigrationStatement> Generate(DropForeignKeyOperation operation)
	{
		using (var writer = SqlWriter())
		{
			writer.Write("ALTER TABLE ");
			writer.Write(Quote(CheckName(ExtractName(operation.DependentTable))));
			writer.Write(" DROP CONSTRAINT ");
			writer.Write(Quote(CheckName(CreateItemName(operation.Name))));
			yield return Statement(writer);
		}
	}

	protected virtual IEnumerable<MigrationStatement> Generate(DropIndexOperation operation)
	{
		using (var writer = SqlWriter())
		{
			writer.Write("DROP INDEX ");
			writer.Write(Quote(CheckName(CreateItemName(BuildIndexName(operation)))));
			yield return Statement(writer);
		}
	}

	protected virtual IEnumerable<MigrationStatement> Generate(DropPrimaryKeyOperation operation)
	{
		using (var writer = SqlWriter())
		{
			writer.Write("ALTER TABLE ");
			writer.Write(Quote(CheckName(ExtractName(operation.Table))));
			writer.Write(" DROP CONSTRAINT ");
			writer.Write(Quote(CheckName(CreateItemName(operation.Name))));
			yield return Statement(writer);
		}
	}

	protected virtual IEnumerable<MigrationStatement> Generate(DropProcedureOperation operation)
	{
		using (var writer = SqlWriter())
		{
			writer.Write("DROP PROCEDURE ");
			writer.Write(Quote(CheckName(ExtractName(operation.Name))));
			yield return Statement(writer);
		}
	}

	protected virtual IEnumerable<MigrationStatement> Generate(DropTableOperation operation)
	{
		using (var writer = SqlWriter())
		{
			writer.Write("DROP TABLE ");
			writer.Write(Quote(CheckName(ExtractName(operation.Name))));
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
			writer.Write(Quote(CheckName(ExtractName(operation.Table))));
			writer.Write(" ALTER COLUMN ");
			writer.Write(Quote(CheckName(operation.Name)));
			writer.Write(" TO ");
			writer.Write(Quote(CheckName(operation.NewName)));
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
					const int MigrationIdColumn = 0;
					const int ContextKeyColumn = 1;
					const int ModelColumn = 2;
					const int VersionColumn = 3;
					const int MaxChunkLength = 32000;

					var dbInsert = (DbInsertCommandTree)commandTree;
					var modelData = ((dbInsert.SetClauses[ModelColumn] as DbSetClause).Value as DbConstantExpression).Value as byte[];

					// If model length is less than max value, stick to original version
					if (modelData.Length < MaxChunkLength)
					{
						using (var writer = SqlWriter())
						{
							writer.Write(DmlSqlGenerator.GenerateInsertSql(dbInsert, out _, generateParameters: false));
							yield return Statement(writer);
						}
					}
					else
					{
						// If it's bigger - we split it into chunks, as big as possible
						var dataChunks = modelData.Split(MaxChunkLength);

						// We can't change CommandTree, but we can create new one, only difference being data length
						using (var writer = SqlWriter())
						{
							var setClauses = new ReadOnlyCollection<DbModificationClause>(
									new List<DbModificationClause>
									{
											dbInsert.SetClauses[MigrationIdColumn],
											dbInsert.SetClauses[ContextKeyColumn],
											DbExpressionBuilder.SetClause(
												((DbSetClause)dbInsert.SetClauses[ModelColumn]).Property,
												dataChunks.ElementAt(0).ToArray()
											),
											dbInsert.SetClauses[VersionColumn],
									});


							var newCommandTree = new DbInsertCommandTree(dbInsert.MetadataWorkspace, commandTree.DataSpace, dbInsert.Target, setClauses, dbInsert.Returning);

							writer.Write(DmlSqlGenerator.GenerateInsertSql(newCommandTree, out _, generateParameters: false));
							yield return Statement(writer);
						}

						// Now we have first Insert, let's update it with chunks of remaing data
						foreach (var dataChunk in dataChunks.Skip(1))
						{
							using (var writer = SqlWriter())
							{
								var modelProperty = (dbInsert.SetClauses[ModelColumn] as DbSetClause).Property as DbPropertyExpression;

								var modificationClauses = new List<DbModificationClause>
									{
										// Updating existing chunk of data with subsequent part
										DbExpressionBuilder.SetClause(
											modelProperty,
											// TODO: Better solution required
											// Best if we could use DbExpression.Concat, but it returns DbFunctionExpression, which is not supported
											// Here we'll get SET Model = 'data', which we can update as text later
											dataChunk.ToArray()
										)
									}.AsReadOnly();

								var updateCommandTree = new DbUpdateCommandTree(dbInsert.MetadataWorkspace,
									dbInsert.DataSpace,
									dbInsert.Target,
									// Predicate is MigrationId value
									DbExpressionBuilder.Equal(
										((DbSetClause)dbInsert.SetClauses[MigrationIdColumn]).Property,
										((DbSetClause)dbInsert.SetClauses[MigrationIdColumn]).Value),
									modificationClauses,
									dbInsert.Returning);

								writer.Write(DmlSqlGenerator.GenerateUpdateSql(updateCommandTree, out _, generateParameters: false));

								// Since we couldn't concat before, replacing query as string
								// Replacing SET Model = 'data'
								//		with SET Model = Model || 'data'
								// Model being first is important, since these are parts of single value
								var statement = writer.ToString();
								var newStatement = statement.Replace($"SET \"{modelProperty.Property.Name}\" = ", $"SET \"{modelProperty.Property.Name}\" = \"{modelProperty.Property.Name}\" || ");

								yield return Statement(newStatement);
							}
						}
					}
					break;
				case DbCommandTreeKind.Delete:
					using (var writer = SqlWriter())
					{
						writer.Write(DmlSqlGenerator.GenerateDeleteSql((DbDeleteCommandTree)commandTree, out _, generateParameters: false));
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
			writer.Write(Quote(CheckName(ExtractName(operation.Name))));
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

	protected (string, IEnumerable<string>) Generate(ColumnModel column, string tableName)
	{
		var builder = new StringBuilder();
		var additionalCommands = new List<string>();

		var columnName = CheckName(column.Name);

		var columnType = BuildPropertyType(column);
		builder.Append(Quote(columnName));
		builder.Append(" ");
		builder.Append(columnType);

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

		if ((column.IsNullable != null)
			&& !column.IsNullable.Value)
		{
			builder.Append(" NOT NULL");
		}

		if (column.Type == PrimitiveTypeKind.Boolean)
		{
			builder.Append(" CHECK(");
			builder.Append(Quote(columnName));
			builder.Append(" IN (0,1))");
		}

		if (column.IsIdentity)
		{
			var identity = _behavior.CreateIdentityForColumn(columnName, tableName);
			additionalCommands.AddRange(identity.Where(x => !string.IsNullOrWhiteSpace(x)));
		}

		return (builder.ToString(), additionalCommands);
	}

	protected string Generate(ParameterModel parameter)
	{
		var builder = new StringBuilder();
		builder.Append(Quote(CheckName(parameter.Name)));
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
		return SqlGenerator.FormatString(value, true);
	}

	protected static string WriteValue(TimeSpan value)
	{
		return SqlGenerator.FormatTime(value);
	}

	protected internal static string Quote(string name)
	{
		return SqlGenerator.QuoteIdentifier(name);
	}

	internal static SqlWriter SqlWriter()
	{
		var result = new SqlWriter(new StringBuilder());
		result.Indent++;
		return result;
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

	static string BuildIndexName(IndexOperation indexOperation)
	{
		return !indexOperation.HasDefaultName
			? indexOperation.Name
			: IndexOperation.BuildDefaultName(new[] { ExtractName(indexOperation.Table) }.Concat(indexOperation.Columns));
	}

	static string ExtractName(string name)
	{
		return name.Substring(name.LastIndexOf('.') + 1);
	}

	static string CreateItemName(string name)
	{
		while (true)
		{
			var match = Regex.Match(name, @"^(?<prefix>.+_)[^.]+\.(?<suffix>.+)$");
			if (!match.Success)
				break;
			name = match.Result("${prefix}${suffix}");
		}
		return name;
	}

	static string CheckName(string name)
	{
		const int LengthLimit = 31;
		if (name.Length > LengthLimit)
			throw new ArgumentOutOfRangeException($"The name '{name}' is longer than Firebird's {LengthLimit} characters limit for object names.");
		return name;
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
