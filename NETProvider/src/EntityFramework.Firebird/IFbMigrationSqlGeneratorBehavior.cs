using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebirdSql.Data.EntityFramework6
{
	public interface IFbMigrationSqlGeneratorBehavior
	{
		IEnumerable<string> GenerateIdentityForColumn(string columnName, string tableName);
	}
}
