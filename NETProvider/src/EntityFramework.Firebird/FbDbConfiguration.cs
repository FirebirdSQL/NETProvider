using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebirdSql.Data.EntityFramework6
{
	public abstract class FbDbConfiguration : DbConfiguration
	{
		public FbDbConfiguration()
		{
			AddInterceptor(new MigrationsTransactionsInterceptor());
		}
	}
}
