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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;
using Remotion.Linq.Parsing.Structure;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests
{
	public class SelectTests : EntityFrameworkCoreTestsBase
	{
		[Test]
		public void SimpleSelect()
		{
			using (var db = GetDbContext<SelectContext>())
			{
				var data = db.Set<MonAttachment>().ToList();
				Assert.IsNotEmpty(data);
			}
		}

		[Test]
		public void SelectWithWhere()
		{
			using (var db = GetDbContext<SelectContext>())
			{
				var query = db.Set<MonAttachment>()
					.Where(x => x.AttachmentName.Trim() != string.Empty && x.Timestamp.Second > -1);
				Assert.DoesNotThrow(() => query.Load());
				var sql = query.ToSql();
				StringAssert.Contains("TRIM(", sql);
				StringAssert.Contains("EXTRACT(SECOND FROM", sql);
			}
		}
	}

	class SelectContext : FbTestDbContext
	{
		public SelectContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			var monAttachmentConf = modelBuilder.Entity<MonAttachment>();
			monAttachmentConf.HasKey(x => x.AttachmentId);
			monAttachmentConf.Property(x => x.AttachmentId).HasColumnName("MON$ATTACHMENT_ID");
			monAttachmentConf.Property(x => x.AttachmentName).HasColumnName("MON$ATTACHMENT_NAME");
			monAttachmentConf.Property(x => x.Timestamp).HasColumnName("MON$TIMESTAMP");
			monAttachmentConf.ToTable("MON$ATTACHMENTS");
		}
	}

	class MonAttachment
	{
		public int AttachmentId { get; set; }
		public string AttachmentName { get; set; }
		public DateTime Timestamp { get; set; }
	}

	// https://github.com/aspnet/EntityFrameworkCore/issues/6482
	public static class ToSqlExtension
	{
		static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();
		static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");
		static readonly PropertyInfo NodeTypeProviderField = QueryCompilerTypeInfo.DeclaredProperties.Single(x => x.Name == "NodeTypeProvider");
		static readonly MethodInfo CreateQueryParserMethod = QueryCompilerTypeInfo.DeclaredMethods.First(x => x.Name == "CreateQueryParser");
		static readonly FieldInfo DataBaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");
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
			var database = DataBaseField.GetValue(queryCompiler);
			var queryCompilationContextFactory = ((DatabaseDependencies)DatabaseDependenciesField.GetValue(database)).QueryCompilationContextFactory;
			var queryCompilationContext = queryCompilationContextFactory.Create(false);
			var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
			modelVisitor.CreateQueryExecutor<TEntity>(queryModel);
			var sql = modelVisitor.Queries.First().ToString();

			return sql;
		}
	}
}
