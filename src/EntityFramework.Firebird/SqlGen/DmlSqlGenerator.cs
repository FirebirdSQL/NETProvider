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

using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using FirebirdSql.Data.FirebirdClient;

namespace EntityFramework.Firebird.SqlGen;

internal static class DmlSqlGenerator
{
	#region Static Fields

	private const int CommandTextBuilderInitialCapacity = 256;

	#endregion

	#region Static Methods

	internal static string GenerateUpdateSql(DbUpdateCommandTree tree, out List<DbParameter> parameters, bool generateParameters = true)
	{
		var commandText = new StringBuilder(CommandTextBuilderInitialCapacity);
		var translator = new ExpressionTranslator(commandText, tree, null != tree.Returning, generateParameters);
		var first = true;

		commandText.Append("UPDATE ");
		tree.Target.Expression.Accept(translator);
		commandText.AppendLine();

		// set c1 = ..., c2 = ..., ...
		commandText.Append("SET ");

		foreach (DbSetClause setClause in tree.SetClauses)
		{
			if (first)
			{
				first = false;
			}
			else
			{
				commandText.Append(", ");
			}

			setClause.Property.Accept(translator);
			commandText.Append(" = ");
			setClause.Value.Accept(translator);

			translator.RegisterMemberValue(setClause.Property, setClause.Value);
		}

		if (first)
		{
			// If first is still true, it indicates there were no set
			// clauses. Introduce a fake set clause so that:
			// - we acquire the appropriate locks
			// - server-gen columns (e.g. timestamp) get recomputed

			var table = ((DbScanExpression)tree.Target.Expression).Target;
			// hope this column isn't indexed to not waste power
			var someColumn = table.ElementType.Members.Last(x => !MetadataHelpers.IsStoreGenerated(x));
			commandText.AppendFormat("{0} = {0}", GenerateMemberSql(someColumn));
		}
		commandText.AppendLine();

		// where c1 = ..., c2 = ...
		commandText.Append("WHERE ");
		tree.Predicate.Accept(translator);
		commandText.AppendLine();

		// generate returning sql
		GenerateReturningSql(commandText, tree, translator, tree.Returning);

		parameters = translator.Parameters;
		return commandText.ToString();
	}

	internal static string GenerateDeleteSql(DbDeleteCommandTree tree, out List<DbParameter> parameters, bool generateParameters = true)
	{
		var commandText = new StringBuilder(CommandTextBuilderInitialCapacity);
		var translator = new ExpressionTranslator(commandText, tree, false, generateParameters);

		commandText.Append("DELETE FROM ");
		tree.Target.Expression.Accept(translator);
		commandText.AppendLine();

		// where c1 = ... AND c2 = ...
		commandText.Append("WHERE ");
		tree.Predicate.Accept(translator);

		parameters = translator.Parameters;
		return commandText.ToString();
	}

	internal static string GenerateInsertSql(DbInsertCommandTree tree, out List<DbParameter> parameters, bool generateParameters = true)
	{
		var commandText = new StringBuilder(CommandTextBuilderInitialCapacity);
		var translator = new ExpressionTranslator(commandText, tree, null != tree.Returning, generateParameters);
		var first = true;

		commandText.Append("INSERT INTO ");
		tree.Target.Expression.Accept(translator);

		if (tree.SetClauses.Any())
		{
			// (c1, c2, c3, ...)
			commandText.Append("(");

			foreach (DbSetClause setClause in tree.SetClauses)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					commandText.Append(", ");
				}
				setClause.Property.Accept(translator);
			}
			commandText.AppendLine(")");

			// values c1, c2, ...
			first = true;
			commandText.Append("VALUES (");
			foreach (DbSetClause setClause in tree.SetClauses)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					commandText.Append(", ");
				}

				setClause.Value.Accept(translator);

				translator.RegisterMemberValue(setClause.Property, setClause.Value);
			}
			commandText.AppendLine(")");
		}
		else
		{
			commandText.AppendLine("DEFAULT VALUES");
		}
		// generate returning sql
		GenerateReturningSql(commandText, tree, translator, tree.Returning);

		parameters = translator.Parameters;
		return commandText.ToString();
	}

	// Generates SQL describing a member
	// Requires: member must belong to an entity type (a safe requirement for DML
	// SQL gen, where we only access table columns)
	internal static string GenerateMemberSql(EdmMember member)
	{
		return SqlGenerator.QuoteIdentifier(member.Name);
	}

	private static void GenerateReturningSql(
		StringBuilder commandText,
		DbModificationCommandTree tree,
		ExpressionTranslator translator,
		DbExpression returning)
	{
		// Nothing to do if there is no Returning expression
		if (returning == null)
		{
			return;
		}

		var table = ((DbScanExpression)tree.Target.Expression).Target;
		var columnsToFetch =
			table.ElementType.Members
				.Where(m => MetadataHelpers.IsStoreGenerated(m))
				.Except((!(tree is DbInsertCommandTree) ? table.ElementType.KeyMembers : Enumerable.Empty<EdmMember>()));

		var startBlock = new StringBuilder();
		var separator = string.Empty;

		startBlock.Append("EXECUTE BLOCK ");
		if (translator.Parameters.Any())
		{
			startBlock.AppendLine("(");
			separator = string.Empty;
			foreach (FbParameter param in translator.Parameters)
			{
				startBlock.Append(separator);
				startBlock.Append(param.ParameterName.Replace("@", string.Empty));
				startBlock.Append(" ");
				var member = translator.MemberValues.First(m => m.Value.Contains(param)).Key;
				startBlock.Append(SqlGenerator.GetSqlPrimitiveType(member.TypeUsage));
				if (param.FbDbType == FbDbType.VarChar || param.FbDbType == FbDbType.Char)
					startBlock.Append(" CHARACTER SET UTF8");
				startBlock.Append(" = ");
				startBlock.Append(param.ParameterName);

				separator = ", ";
			}
			startBlock.AppendLine();
			startBlock.Append(") ");
		}

		startBlock.AppendLine("RETURNS (");
		separator = string.Empty;
		foreach (var m in columnsToFetch)
		{
			startBlock.Append(separator);
			startBlock.Append(GenerateMemberSql(m));
			startBlock.Append(" ");
			startBlock.Append(SqlGenerator.GetSqlPrimitiveType(m.TypeUsage));

			separator = ", ";
		}
		startBlock.AppendLine(")");
		startBlock.AppendLine("AS BEGIN");

		var newCommand = ChangeParamsToPSQLParams(commandText.ToString(), translator.Parameters.Select(p => p.ParameterName).ToArray());
		commandText.Remove(0, commandText.Length);
		commandText.Insert(0, newCommand);
		commandText.Insert(0, startBlock.ToString());

		commandText.Append("RETURNING ");
		separator = string.Empty;
		foreach (var m in columnsToFetch)
		{
			commandText.Append(separator);
			commandText.Append(GenerateMemberSql(m));

			separator = ", ";
		}
		commandText.Append(" INTO ");
		separator = string.Empty;
		foreach (var m in columnsToFetch)
		{
			commandText.Append(separator);
			commandText.Append(":" + GenerateMemberSql(m));

			separator = ", ";
		}

		commandText.AppendLine(";");
		commandText.AppendLine("IF (ROW_COUNT > 0) THEN");
		commandText.AppendLine("  SUSPEND;");
		commandText.AppendLine("END");
	}

	private static string ChangeParamsToPSQLParams(string commandText, string[] parametersUsed)
	{
		var command = new StringBuilder(commandText);
		foreach (var param in parametersUsed)
		{
			command.Replace(param, ":" + param.Remove(0, 1));
		}
		return command.ToString();
	}

	#endregion
}
