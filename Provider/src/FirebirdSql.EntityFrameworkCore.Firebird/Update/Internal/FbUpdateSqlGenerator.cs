/*
 * The contents of this file are subject to the Initial
 * Developer's Public License Version 1.0 (the "License");
 * you may not use this file except in compliance with the
 * License. You may obtain a copy of the License at
 * https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 * Software distributed under the License is distributed on
 * an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 * express or implied. See the License for the specific
 * language governing rights and limitations under the License.
 *
 * All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Linq;
using System.Text;
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
#warning Finish
			throw new NotImplementedException();
		}

		protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
		{
#warning Finish
			throw new NotImplementedException();
		}

		public override ResultSetMapping AppendInsertOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
		{
			commandStringBuilder.Clear();
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
	}
}
