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
using System.Data;
using System.Linq;
using System.Text;
using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Update.Internal;

public class FbUpdateSqlGenerator : UpdateSqlGenerator, IFbUpdateSqlGenerator
{
	public FbUpdateSqlGenerator(UpdateSqlGeneratorDependencies dependencies)
		: base(dependencies)
	{ }

	public override ResultSetMapping AppendInsertOperation(StringBuilder commandStringBuilder, IReadOnlyModificationCommand command, int commandPosition, out bool requiresTransaction)
	{
		var result = default(ResultSetMapping);
		var name = command.TableName;
		var operations = command.ColumnModifications;
		var writeOperations = operations.Where(o => o.IsWrite).ToList();
		var readOperations = operations.Where(o => o.IsRead).ToList();
		var anyRead = readOperations.Any();
		AppendInsertCommandHeader(commandStringBuilder, name, null, writeOperations);
		AppendValuesHeader(commandStringBuilder, writeOperations);
		AppendValues(commandStringBuilder, name, null, writeOperations);
		if (anyRead)
		{
			commandStringBuilder.AppendLine();
			commandStringBuilder.Append("RETURNING ");
			commandStringBuilder.AppendJoin(readOperations, (b, e) =>
			{
				b.Append(SqlGenerationHelper.DelimitIdentifier(e.ColumnName));
			}, ", ");
			result = ResultSetMapping.HasResultRow;
		}
		else
		{
			result = ResultSetMapping.NoResults;
		}
		commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();

		requiresTransaction = true;
		return result;
	}

	public override ResultSetMapping AppendUpdateOperation(StringBuilder commandStringBuilder, IReadOnlyModificationCommand command, int commandPosition, out bool requiresTransaction)
	{
		var result = default(ResultSetMapping);
		var name = command.TableName;
		var operations = command.ColumnModifications;
		var writeOperations = operations.Where(o => o.IsWrite).ToList();
		var readOperations = operations.Where(o => o.IsRead).ToList();
		var conditionOperations = operations.Where(o => o.IsCondition).ToList();
		var inputOperations = GenerateParameters(operations.Where(o => o.IsWrite || o.IsCondition)).ToList();
		var anyRead = readOperations.Any();
		commandStringBuilder.Append("EXECUTE BLOCK (");
		commandStringBuilder.AppendJoin(inputOperations, (b, p) =>
		{
			b.Append(p.name);
			b.Append(" ");
			b.Append(p.type);
			b.Append(" = ?");
		}, ", ");
		commandStringBuilder.AppendLine(")");
		commandStringBuilder.Append("RETURNS (");
		if (anyRead)
		{
			commandStringBuilder.AppendJoin(readOperations, (b, e) =>
			{
				var type = GetColumnType(e);
				b.Append(SqlGenerationHelper.DelimitIdentifier(e.ColumnName));
				b.Append(" ");
				b.Append(type);
			}, ", ");
		}
		else
		{
			commandStringBuilder.Append("ROWS_AFFECTED INT");
		}
		commandStringBuilder.AppendLine(")");
		commandStringBuilder.AppendLine("AS");
		commandStringBuilder.AppendLine("BEGIN");
		AppendUpdateCommandHeader(commandStringBuilder, name, null, writeOperations);
		AppendWhereClause(commandStringBuilder, conditionOperations);
		if (anyRead)
		{
			commandStringBuilder.AppendLine();
			commandStringBuilder.Append("RETURNING ");
			commandStringBuilder.AppendJoin(readOperations, (b, e) =>
			{
				b.Append(SqlGenerationHelper.DelimitIdentifier(e.ColumnName));
			}, ", ");
			commandStringBuilder.Append(" INTO ");
			commandStringBuilder.AppendJoin(readOperations, (b, e) =>
			{
				b.Append(":");
				b.Append(SqlGenerationHelper.DelimitIdentifier(e.ColumnName));
			}, ", ");
		}
		commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();
		if (!anyRead)
		{
			commandStringBuilder.AppendLine("ROWS_AFFECTED = ROW_COUNT;");
			commandStringBuilder.AppendLine("SUSPEND;");
			result = ResultSetMapping.ResultSetWithRowsAffectedOnly;
		}
		else
		{
			commandStringBuilder.AppendLine("IF (ROW_COUNT > 0) THEN");
			commandStringBuilder.AppendLine("SUSPEND;");
			result = ResultSetMapping.HasResultRow;
		}
		commandStringBuilder.Append("END");
		commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();

		requiresTransaction = true;
		return result;
	}

	public override ResultSetMapping AppendDeleteOperation(StringBuilder commandStringBuilder, IReadOnlyModificationCommand command, int commandPosition, out bool requiresTransaction)
	{
		var name = command.TableName;
		var operations = command.ColumnModifications;
		var conditionOperations = operations.Where(o => o.IsCondition).ToList();
		var inputOperations = GenerateParameters(conditionOperations);
		commandStringBuilder.Append("EXECUTE BLOCK (");
		commandStringBuilder.AppendJoin(inputOperations, (b, p) =>
		{
			b.Append(p.name);
			b.Append(" ");
			b.Append(p.type);
			b.Append(" = ?");
		}, ", ");
		commandStringBuilder.AppendLine(")");
		commandStringBuilder.AppendLine("RETURNS (ROWS_AFFECTED INT)");
		commandStringBuilder.AppendLine("AS");
		commandStringBuilder.AppendLine("BEGIN");
		AppendDeleteCommandHeader(commandStringBuilder, name, null);
		AppendWhereClause(commandStringBuilder, conditionOperations);
		commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();
		commandStringBuilder.AppendLine();
		commandStringBuilder.AppendLine("ROWS_AFFECTED = ROW_COUNT;");
		commandStringBuilder.AppendLine("SUSPEND;");
		commandStringBuilder.Append("END");
		commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();

		requiresTransaction = true;
		return ResultSetMapping.ResultSetWithRowsAffectedOnly;
	}

	// workaround for GenerateBlockParameterName
	protected override void AppendUpdateCommandHeader(StringBuilder commandStringBuilder, string name, string schema, IReadOnlyList<IColumnModification> operations)
	{
		commandStringBuilder.Append("UPDATE ");
		SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);
		commandStringBuilder.Append(" SET ")
			.AppendJoin(
				operations,
				SqlGenerationHelper,
				(sb, o, helper) =>
				{
					helper.DelimitIdentifier(sb, o.ColumnName);
					sb.Append(" = ");
					if (!o.UseCurrentValueParameter)
					{
						AppendSqlLiteral(sb, o.Value, o.Property);
					}
					else
					{
						((IFbSqlGenerationHelper)helper).GenerateBlockParameterName(sb, o.ParameterName);
					}
				});
	}

	// workaround for GenerateBlockParameterName
	protected override void AppendWhereCondition(StringBuilder commandStringBuilder, IColumnModification columnModification, bool useOriginalValue)
	{
		SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);
		if ((useOriginalValue ? columnModification.OriginalValue : columnModification.Value) == null)
		{
			commandStringBuilder.Append(" IS NULL");
			return;
		}

		commandStringBuilder.Append(" = ");
		if (!columnModification.UseCurrentValueParameter && !columnModification.UseOriginalValueParameter)
		{
			AppendSqlLiteral(commandStringBuilder, columnModification.Value, columnModification.Property);
		}
		else
		{
			((IFbSqlGenerationHelper)SqlGenerationHelper).GenerateBlockParameterName(commandStringBuilder, useOriginalValue ? columnModification.OriginalParameterName : columnModification.ParameterName);
		}
	}

	string GetColumnType(IColumnModification column)
	{
		return Dependencies.TypeMappingSource.GetMapping(column.Property).StoreType;
	}

	IEnumerable<(string name, string type)> GenerateParameters(IEnumerable<IColumnModification> columns)
	{
		foreach (var item in columns)
		{
			var type = GetColumnType(item);
			if (item.UseCurrentValueParameter)
			{
				yield return (item.ParameterName, type);
			}
			if (item.UseOriginalValueParameter)
			{
				yield return (item.OriginalParameterName, type);
			}
		}
	}

	public override string GenerateNextSequenceValueOperation(string name, string schema)
	{
		var builder = new StringBuilder();
		builder.Append("SELECT NEXT VALUE FOR ");
		builder.Append(SqlGenerationHelper.DelimitIdentifier(name));
		builder.Append(" FROM RDB$DATABASE");
		return builder.ToString();
	}

	/*override*/
	void AppendSqlLiteral(StringBuilder commandStringBuilder, object value, IProperty property)
	{
		var mapping = property != null
			? Dependencies.TypeMappingSource.FindMapping(property)
			: null;
		mapping ??= Dependencies.TypeMappingSource.GetMappingForValue(value);
		commandStringBuilder.Append(mapping.GenerateProviderValueSqlLiteral(value));
	}

	public override ResultSetMapping AppendStoredProcedureCall(
		StringBuilder commandStringBuilder,
		IReadOnlyModificationCommand command,
		int commandPosition,
		out bool requiresTransaction)
	{
		var storedProcedure = command.StoreStoredProcedure;
		var resultSetMapping = ResultSetMapping.NoResults;

		foreach (var resultColumn in storedProcedure.ResultColumns)
		{
			resultSetMapping = ResultSetMapping.LastInResultSet;
			if (resultColumn == command.RowsAffectedColumn)
			{
				resultSetMapping |= ResultSetMapping.ResultSetWithRowsAffectedOnly;
			}
			else
			{
				resultSetMapping = ResultSetMapping.LastInResultSet;
				break;
			}
		}

		if (resultSetMapping == ResultSetMapping.NoResults)
		{
			commandStringBuilder.Append("EXECUTE PROCEDURE ");
		}
		else
		{
			commandStringBuilder.Append("SELECT ");

			var first = true;

			foreach (var resultColumn in storedProcedure.ResultColumns)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					commandStringBuilder.Append(", ");
				}

				if (resultColumn == command.RowsAffectedColumn || resultColumn.Name == "RowsAffected")
				{
					SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, "ROWCOUNT");
				}
				else
				{
					SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, resultColumn.Name);
				}
			}
			commandStringBuilder.Append(" FROM ");
		}

		SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, storedProcedure.Name);

		if (storedProcedure.Parameters.Any())
		{
			commandStringBuilder.Append("(");

			var first = true;

			for (var i = 0; i < command.ColumnModifications.Count; i++)
			{
				var columnModification = command.ColumnModifications[i];
				if (columnModification.Column is not IStoreStoredProcedureParameter parameter)
				{
					continue;
				}

				if (parameter.Direction.HasFlag(ParameterDirection.Output))
				{
					throw new InvalidOperationException("Output parameters are not supported in stored procedures");
				}

				if (first)
				{
					first = false;
				}
				else
				{
					commandStringBuilder.Append(", ");
				}
				SqlGenerationHelper.GenerateParameterNamePlaceholder(
					commandStringBuilder, columnModification.UseOriginalValueParameter
						? columnModification.OriginalParameterName!
						: columnModification.ParameterName!);
			}

			commandStringBuilder.Append(")");
		}

		commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
		requiresTransaction = true;
		return resultSetMapping;
	}
}
