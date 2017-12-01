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
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Parsing.Structure;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests.Query
{
	// https://github.com/aspnet/EntityFrameworkCore/issues/6482
	public static class ToSqlExtension
	{
		static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();
		static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");
		static readonly PropertyInfo NodeTypeProviderField = QueryCompilerTypeInfo.DeclaredProperties.Single(x => x.Name == "NodeTypeProvider");
		static readonly MethodInfo CreateQueryParserMethod = QueryCompilerTypeInfo.DeclaredMethods.First(x => x.Name == "CreateQueryParser");
		static readonly FieldInfo DatabaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");
		static readonly PropertyInfo DatabaseDependenciesField = typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");

		public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
		{
			if (!(query is EntityQueryable<TEntity>) && !(query is InternalDbSet<TEntity>))
			{
				throw new ArgumentException("Invalid query");
			}

			var queryCompiler = (IQueryCompiler)QueryCompilerField.GetValue(query.Provider);
			var nodeTypeProvider = (INodeTypeProvider)NodeTypeProviderField.GetValue(queryCompiler);
			var parser = (IQueryParser)CreateQueryParserMethod.Invoke(queryCompiler, new object[] { nodeTypeProvider });
			var queryModel = parser.GetParsedQuery(query.Expression);
			var database = DatabaseField.GetValue(queryCompiler);
			var queryCompilationContextFactory = ((DatabaseDependencies)DatabaseDependenciesField.GetValue(database)).QueryCompilationContextFactory;
			var queryCompilationContext = queryCompilationContextFactory.Create(false);
			var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
			modelVisitor.CreateQueryExecutor<TEntity>(queryModel);
			var sql = modelVisitor.Queries.First().ToString();

			return sql;
		}
	}
}
