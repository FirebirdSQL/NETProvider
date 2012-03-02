using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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

		public static Version ParseServerVersion(
#if (!NET_20)
			this 
#endif
			string s)
		{
			Match m = Regex.Match(s, @"\w{2}-\w(\d+\.\d+\.\d+\.\d+) .*");
			if (!m.Success)
				return null;
			return new Version(m.Groups[1].Value);
		}
	}
}
