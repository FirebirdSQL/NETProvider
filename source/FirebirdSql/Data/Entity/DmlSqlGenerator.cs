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
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2008 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 *  
 *  Based on the Microsoft Entity Framework Provider Sample SP1 Beta 1
 */

#if (NET_35 && ENTITY_FRAMEWORK)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;
using System.Data.Common.Utils;
using System.Data.Mapping.Update.Internal;
using System.Linq;

using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Entity
{
    /// <summary>
    /// Class generating SQL for a DML command tree.
    /// </summary>
    internal static class DmlSqlGenerator
    {
        #region � Static Fields �

        private static readonly int commandTextBuilderInitialCapacity = 256;

        #endregion

        #region � Static Methods �

        internal static string GenerateUpdateSql(DbUpdateCommandTree tree, out List<DbParameter> parameters)
        {
            StringBuilder commandText = new StringBuilder(commandTextBuilderInitialCapacity);
            ExpressionTranslator translator = new ExpressionTranslator(commandText, tree, null != tree.Returning);
            bool first = true;

            // update [schemaName].[tableName]
            commandText.Append("update ");
            tree.Target.Expression.Accept(translator);
            commandText.AppendLine();

            // set c1 = ..., c2 = ..., ...            
            commandText.Append("set ");

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
                //
                // We use the following pattern:
                //
                //  update Foo
                //  set @i = 0
                //  where ...
#warning How to do on FB
                //DbParameter parameter = translator.CreateParameter(default(Int32), DbType.Int32);
                //commandText.Append(parameter.ParameterName);
                //commandText.Append(" = 0");
            }
            commandText.AppendLine();

            // where c1 = ..., c2 = ...
            commandText.Append("where ");
            tree.Predicate.Accept(translator);
            commandText.AppendLine();

            // generate returning sql
            GenerateReturningSql(commandText, tree, translator, tree.Returning);

            parameters = translator.Parameters;
            return commandText.ToString();
        }

        internal static string GenerateDeleteSql(DbDeleteCommandTree tree, out List<DbParameter> parameters)
        {
            StringBuilder commandText = new StringBuilder(commandTextBuilderInitialCapacity);
            ExpressionTranslator translator = new ExpressionTranslator(commandText, tree, false);

            // delete [schemaName].[tableName]
            commandText.Append("delete from ");
            tree.Target.Expression.Accept(translator);
            commandText.AppendLine();

            // where c1 = ... AND c2 = ...
            commandText.Append("where ");
            tree.Predicate.Accept(translator);

            parameters = translator.Parameters;
            return commandText.ToString();
        }

        internal static string GenerateInsertSql(DbInsertCommandTree tree, out List<DbParameter> parameters)
        {
            StringBuilder commandText = new StringBuilder(commandTextBuilderInitialCapacity);
            ExpressionTranslator translator = new ExpressionTranslator(commandText, tree, null != tree.Returning);
            bool first = true;

            // insert [schemaName].[tableName]
            commandText.Append("insert into ");
            tree.Target.Expression.Accept(translator);

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
            commandText.Append("values (");
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

        /// <summary>
        /// Generates SQL fragment returning server-generated values.
        /// Requires: translator knows about member values so that we can figure out
        /// how to construct the key predicate.
        /// <code>
        /// Sample SQL:
        ///     
        ///     select IdentityValue
        ///     from dbo.MyTable
        ///     where @@ROWCOUNT > 0 and IdentityValue = scope_identity()
        /// 
        /// or
        /// 
        ///     select TimestamptValue
        ///     from dbo.MyTable
        ///     where @@ROWCOUNT > 0 and Id = 1
        /// 
        /// Note that we filter on rowcount to ensure no rows are returned if no rows were modified.
        /// </code>
        /// </summary>
        /// <param name="commandText">Builder containing command text</param>
        /// <param name="tree">Modification command tree</param>
        /// <param name="translator">Translator used to produce DML SQL statement
        /// for the tree</param>
        /// <param name="returning">Returning expression. If null, the method returns
        /// immediately without producing a SELECT statement.</param>
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

            /// for the V1 only one row is changed per command

            EntitySetBase table = ((DbScanExpression)tree.Target.Expression).Target;
            List<EdmMember> columnsToFetch = new List<EdmMember>();

            foreach (EdmMember tableColumn in table.ElementType.Members)
            {
                const string StoreGeneratedPatternFacetName = "StoreGeneratedPattern";

                Facet item = null;
                if (tableColumn.TypeUsage.Facets.TryGetValue(StoreGeneratedPatternFacetName, false, out item) &&
                    (((StoreGeneratedPattern)item.Value) == StoreGeneratedPattern.Computed || ((StoreGeneratedPattern)item.Value) == StoreGeneratedPattern.Identity))
                {
                    columnsToFetch.Add(tableColumn);
                }
                //else if (table.ElementType.KeyMembers.Contains(tableColumn))
                //{
                //    columnsToFetch.Add(tableColumn);
                //}
            }

            StringBuilder startBlock = new StringBuilder();
            string separator = string.Empty;

            startBlock.AppendLine("execute block (");
            separator = string.Empty;
            foreach (KeyValuePair<EdmMember, DbParameter> item in translator.MemberValues)
            {
                startBlock.Append(separator);
                startBlock.Append(item.Value.ParameterName.Replace("@", ""));
                startBlock.Append(" ");
                startBlock.Append(SqlGenerator.GetSqlPrimitiveType(item.Key.TypeUsage));
                startBlock.Append(" = ");
                startBlock.Append(item.Value.ParameterName);

                separator = ", ";
            }
            startBlock.AppendLine(") ");

            startBlock.AppendLine("returns (");
            separator = string.Empty;
            foreach (EdmMember m in columnsToFetch)
            {
                startBlock.Append(separator);
                startBlock.Append(GenerateMemberSql(m));
                startBlock.Append(" ");
                startBlock.Append(SqlGenerator.GetSqlPrimitiveType(m.TypeUsage));

                separator = ", ";
            }
            startBlock.AppendLine(")");
            startBlock.AppendLine("as begin");

            string newCommand = ChangeParamsToPSQLParams(commandText.ToString(), translator.Parameters.Select(p => p.ParameterName).ToArray());
            commandText.Remove(0, commandText.Length);
            commandText.Insert(0, newCommand);
            commandText.Insert(0, startBlock.ToString());

            commandText.Append("returning ");
            separator = string.Empty;
            foreach (EdmMember m in columnsToFetch)
            {
                commandText.Append(separator);
                commandText.Append(GenerateMemberSql(m));
                commandText.Append(" into ");
                commandText.Append(":" + GenerateMemberSql(m));

                separator = ", ";
            }
            commandText.AppendLine(";");
            commandText.AppendLine("suspend;");
            commandText.AppendLine("end");

            Debug.WriteLine(commandText.ToString());
        }

        private static string ChangeParamsToPSQLParams(string commandText, string[] parametersUsed)
        {
            const string valuesPattern = "values (";
            int limitPosition = commandText.LastIndexOf(valuesPattern) + valuesPattern.Length;
            StringBuilder command = new StringBuilder(commandText);
            foreach (string param in parametersUsed)
            {
                command.Replace(param, ":" + param.Substring(1), limitPosition, command.Length - limitPosition);
            }
            return command.ToString();
        }

        #endregion
    }
}
#endif
