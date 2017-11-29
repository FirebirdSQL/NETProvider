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
			AppendInsertCommandHeader(commandStringBuilder, name, null, writeOperations);
			AppendValuesHeader(commandStringBuilder, writeOperations);
			AppendValues(commandStringBuilder, writeOperations);
			if (readOperations.Any())
			{
				commandStringBuilder.AppendLine();
				commandStringBuilder.Append("RETURNING ");
				commandStringBuilder.Append(string.Join(", ", readOperations.Select(x => SqlGenerationHelper.DelimitIdentifier(x.ColumnName))));
				result = ResultSetMapping.LastInResultSet;
			}
			commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();
			return result;
		}

		public override ResultSetMapping AppendUpdateOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
		{
			var result = ResultSetMapping.NoResultSet;
			var name = command.TableName;
			var operations = command.ColumnModifications;
			var writeOperations = operations.Where(o => o.IsWrite).ToList();
			var readOperations = operations.Where(o => o.IsRead).ToList();
			var conditionOperations = operations.Where(o => o.IsCondition).ToList();
			AppendUpdateCommandHeader(commandStringBuilder, name, null, writeOperations);
			AppendWhereClause(commandStringBuilder, conditionOperations);
			if (readOperations.Any())
			{
				commandStringBuilder.AppendLine();
				commandStringBuilder.Append("RETURNING ");
				commandStringBuilder.Append(string.Join(", ", readOperations.Select(x => SqlGenerationHelper.DelimitIdentifier(x.ColumnName))));
				result = ResultSetMapping.LastInResultSet;
			}
			commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();
			return result;
		}

		public override ResultSetMapping AppendDeleteOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
		{
			var sqlGenerationHelper = (IFbSqlGenerationHelper)SqlGenerationHelper;
			var name = command.TableName;
			var operations = command.ColumnModifications;
			var conditionOperations = operations.Where(o => o.IsCondition).ToList();
			commandStringBuilder.Append("EXECUTE BLOCK (");
			var separator = string.Empty;
			foreach (var item in conditionOperations)
			{
				commandStringBuilder.Append(separator);

				var type = GetColumnType(item);
				var parameterName = item.UseOriginalValueParameter
					? item.OriginalParameterName
					: item.ParameterName;
				commandStringBuilder.Append(parameterName);
				commandStringBuilder.Append(" ");
				commandStringBuilder.Append(type);
				commandStringBuilder.Append(" = ?");

				separator = ", ";
			}
			commandStringBuilder.AppendLine(")");
			commandStringBuilder.AppendLine("RETURNS (ROWS_AFFECTED INT)");
			commandStringBuilder.AppendLine("AS");
			commandStringBuilder.AppendLine("BEGIN");
			AppendDeleteCommandHeader(commandStringBuilder, name, null);
			var oldParameterNameMarker = sqlGenerationHelper.ParameterNameMarker;
			sqlGenerationHelper.ParameterNameMarker = ":";
			try
			{
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
	}
}
