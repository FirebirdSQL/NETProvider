using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	//[TestFixture(FbServerType.Default)]
	public class DebugTests
	{
		[Test, Category("foobar")]
		public void Test01()
		{
			for (int i = 0; i < 10; i++)
			{
				var cs = TestsBase.BuildConnectionString(FbServerType.Default);
				using (var conn = new FbConnection(cs))
				{
					conn.Open();
				}
				//FbConnection.CreateDatabase(cs, true);
				Thread.Sleep(500);
			}
		}
	}
}
