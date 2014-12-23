/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2014 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 *	
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.EntityFramework6.SqlGen;

namespace FirebirdSql.Data.EntityFramework6
{
	public class DefaultFbMigrationSqlGeneratorBehavior : IFbMigrationSqlGeneratorBehavior
	{
		const string IdentitySequenceName = "GEN_IDENTITY";

		public IEnumerable<string> CreateIdentityForColumn(string columnName, string tableName)
		{
			using (var writer = FbMigrationSqlGenerator.SqlWriter())
			{
				writer.WriteLine("EXECUTE BLOCK");
				writer.WriteLine("AS");
				writer.WriteLine("BEGIN");
				writer.Indent++;
				writer.Write("if (not exists(select 1 from rdb$generators where rdb$generator_name = '");
				writer.Write(IdentitySequenceName);
				writer.Write("')) then");
				writer.WriteLine();
				writer.WriteLine("begin");
				writer.Indent++;
				writer.Write("execute statement 'create sequence ");
				writer.Write(IdentitySequenceName);
				writer.Write("';");
				writer.WriteLine();
				writer.Indent--;
				writer.WriteLine("end");
				writer.Indent--;
				writer.Write("END");
				yield return writer.ToString();
			}

			using (var writer = FbMigrationSqlGenerator.SqlWriter())
			{

				writer.Write("CREATE OR ALTER TRIGGER ");
				writer.Write(FbMigrationSqlGenerator.Quote(CreateTriggerName(columnName, tableName)));
				writer.Write(" ACTIVE BEFORE INSERT ON ");
				writer.Write(FbMigrationSqlGenerator.Quote(tableName));
				writer.WriteLine();
				writer.WriteLine("AS");
				writer.WriteLine("BEGIN");
				writer.Indent++;
				writer.Write("if (new.");
				writer.Write(FbMigrationSqlGenerator.Quote(columnName));
				writer.Write(" is null) then");
				writer.WriteLine();
				writer.WriteLine("begin");
				writer.Indent++;
				writer.Write("new.");
				writer.Write(FbMigrationSqlGenerator.Quote(columnName));
				writer.Write(" = next value for ");
				writer.Write(IdentitySequenceName);
				writer.Write(";");
				writer.WriteLine();
				writer.Indent--;
				writer.WriteLine("end");
				writer.Indent--;
				writer.Write("END");
				yield return writer.ToString();
			}
		}

		public IEnumerable<string> DropIdentityForColumn(string columnName, string tableName)
		{
			var triggerName = CreateTriggerName(columnName, tableName);
			using (var writer = FbMigrationSqlGenerator.SqlWriter())
			{
				writer.WriteLine("EXECUTE BLOCK");
				writer.WriteLine("AS");
				writer.WriteLine("BEGIN");
				writer.Indent++;
				writer.Write("if (exists(select 1 from rdb$triggers where rdb$trigger_name = '");
				writer.Write(triggerName);
				writer.Write("')) then");
				writer.WriteLine();
				writer.WriteLine("begin");
				writer.Indent++;
				writer.Write("execute statement 'drop trigger ");
				writer.Write(FbMigrationSqlGenerator.Quote(triggerName));
				writer.Write("';");
				writer.WriteLine();
				writer.Indent--;
				writer.WriteLine("end");
				writer.Indent--;
				writer.Write("END");
				yield return writer.ToString();
			}
		}

		static string CreateTriggerName(string columnName, string tableName)
		{
			return string.Format("ID_{0}_{1}", tableName, columnName);
		}
	}
}
