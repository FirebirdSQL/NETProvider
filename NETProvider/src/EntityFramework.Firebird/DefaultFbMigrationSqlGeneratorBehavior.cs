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

		public IEnumerable<string> GenerateIdentityForColumn(string columnName, string tableName)
		{
			var triggerName = string.Format("ID_{0}_{1}", tableName, columnName);

			using (var writer = FbMigrationSqlGenerator.SqlWriter())
			{
				writer.WriteLine("EXECUTE BLOCK");
				writer.WriteLine("AS");
				writer.WriteLine("BEGIN");
				writer.Indent++;
				writer.Write("if not exists(select 1 from rdb$generators where rdb$generator_name = '");
				writer.Write(IdentitySequenceName);
				writer.Write("') then");
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
				writer.Write(FbMigrationSqlGenerator.Quote(triggerName));
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
	}
}
