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
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
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
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AddForeignKeyOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AddPrimaryKeyOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AlterColumnOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AlterProcedureOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(AlterTableOperation operation)
		{
			// Nothing to do since there is no inherent semantics associated with annotations
			yield break;
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateIndexOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateProcedureOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(CreateTableOperation operation)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropColumnOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("ALTER TABLE ");
				writer.Write(Quote(operation.Table));
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
				writer.Write(Quote(operation.DependentTable));
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
				writer.Write(Quote(operation.Table));
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
				writer.Write(Quote(operation.Name));
				yield return Statement(writer);
			}
		}

		protected virtual IEnumerable<MigrationStatement> Generate(DropTableOperation operation)
		{
			using (var writer = SqlWriter())
			{
				writer.Write("DROP TABLE ");
				writer.Write(Quote(operation.Name));
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
				writer.Write(Quote(operation.Table));
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
			throw new NotImplementedException();
		}

		protected virtual string Generate(ColumnModel column)
		{
			throw new NotImplementedException();
		}

		protected virtual string Generate(ParameterModel parameter)
		{
			throw new NotImplementedException();
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

		IEnumerable<MigrationStatement> GenerateStatements(IEnumerable<MigrationOperation> operations)
		{
			return operations.Select<dynamic, IEnumerable<MigrationStatement>>(x => Generate(x)).SelectMany(x => x);
		}

		static SqlWriter SqlWriter()
		{
			var result = new SqlWriter(new StringBuilder());
			result.Indent++;
			return result;
		}

		static DbConnection CreateConnection()
		{
			return DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(FbProviderServices.ProviderInvariantName).CreateConnection();
		}

		#endregion
	}
}
