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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Update.Internal
{
	public class FbUpdateSqlGenerator : UpdateSqlGenerator, IFbUpdateSqlGenerator
	{
		readonly IRelationalTypeMapper _typeMapper;

		public FbUpdateSqlGenerator(UpdateSqlGeneratorDependencies dependencies, IRelationalTypeMapper typeMapper)
			: base(dependencies)
		{
			_typeMapper = typeMapper;
		}

		protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
		{
			throw new InvalidOperationException();
		}

		protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
		{
			throw new InvalidOperationException();
		}

		public override ResultSetMapping AppendInsertOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
		{
			var result = ResultSetMapping.NoResultSet;
			var name = command.TableName;
			var operations = command.ColumnModifications;
			var writeOperations = operations.Where(o => o.IsWrite).ToList();
			var readOperations = operations.Where(o => o.IsRead).ToList();
			var anyRead = readOperations.Any();
			AppendInsertCommandHeader(commandStringBuilder, name, null, writeOperations);
			AppendValuesHeader(commandStringBuilder, writeOperations);
			AppendValues(commandStringBuilder, writeOperations);
			if (anyRead)
			{
				commandStringBuilder.AppendLine();
				commandStringBuilder.Append("RETURNING ");
				commandStringBuilder.AppendJoin(readOperations, (b, e) =>
				{
					b.Append(SqlGenerationHelper.DelimitIdentifier(e.ColumnName));
				}, ", ");
				result = ResultSetMapping.LastInResultSet;
			}
			commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();
			return result;
		}

		public override ResultSetMapping AppendUpdateOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
		{
			var sqlGenerationHelper = (IFbSqlGenerationHelper)SqlGenerationHelper;
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
			var oldParameterNameMarker = sqlGenerationHelper.ParameterNameMarker;
			sqlGenerationHelper.ParameterNameMarker = ":";
			try
			{
				AppendUpdateCommandHeader(commandStringBuilder, name, null, writeOperations);
				AppendWhereClause(commandStringBuilder, conditionOperations);
			}
			finally
			{
				sqlGenerationHelper.ParameterNameMarker = oldParameterNameMarker;
			}
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
			}
			else
			{
				commandStringBuilder.AppendLine("IF (ROW_COUNT > 0) THEN");
				commandStringBuilder.AppendLine("SUSPEND;");
			}
			commandStringBuilder.AppendLine("END");
			commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();
			return ResultSetMapping.LastInResultSet;
		}

		public override ResultSetMapping AppendDeleteOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
		{
			var sqlGenerationHelper = (IFbSqlGenerationHelper)SqlGenerationHelper;
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
			var oldParameterNameMarker = sqlGenerationHelper.ParameterNameMarker;
			sqlGenerationHelper.ParameterNameMarker = ":";
			try
			{
				AppendDeleteCommandHeader(commandStringBuilder, name, null);
				AppendWhereClause(commandStringBuilder, conditionOperations);
			}
			finally
			{
				sqlGenerationHelper.ParameterNameMarker = oldParameterNameMarker;
			}
			commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();
			commandStringBuilder.AppendLine();
			commandStringBuilder.AppendLine("ROWS_AFFECTED = ROW_COUNT;");
			commandStringBuilder.AppendLine("SUSPEND;");
			commandStringBuilder.AppendLine("END");
			commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();
			return ResultSetMapping.LastInResultSet;
		}

		string GetColumnType(ColumnModification column)
		{
			return _typeMapper.GetMapping(column.Property).StoreType;
		}

		IEnumerable<(string name, string type)> GenerateParameters(IEnumerable<ColumnModification> columns)
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
	}
}
