using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Common
{
	static class PageSizeHelper
	{
		public static bool IsValidPageSize(int value)
		{
			return value == 1024
				|| value == 2048
				|| value == 4096
				|| value == 8192
				|| value == 16384;
		}
	}
}
