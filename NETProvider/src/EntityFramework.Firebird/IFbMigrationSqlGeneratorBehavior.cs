using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebirdSql.Data.EntityFramework6
{
	public interface IFbMigrationSqlGeneratorBehavior
	{
		IEnumerable<string> CreateIdentityForColumn(string columnName, string tableName);
		IEnumerable<string> DropIdentityForColumn(string columnName, string tableName);
	}
}
