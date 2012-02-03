using System;
using System.Collections.Generic;
using System.Text;

namespace FirebirdSql.Data.Common
{
	static class Extensions
	{
		public static bool HasFlag(
#if (!NET_20)
			this 
#endif
			Enum e, Enum flag)
		{
#if (!NET_40)
			return ((Convert.ToInt32(e) & Convert.ToInt32(flag)) != 0);
#else
			return e.HasFlag(flag);
#endif
		}
	}
}
