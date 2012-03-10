using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace FirebirdSql.Data.Common
{
	static class Extensions
	{
		public static bool HasFlag(this Enum e, Enum flag)
		{
#if (!NET_40)
			return ((Convert.ToInt32(e) & Convert.ToInt32(flag)) != 0);
#else
			return e.HasFlag(flag);
#endif
		}

		public static Version ParseServerVersion(this string s)
		{
			Match m = Regex.Match(s, @"\w{2}-\w(\d+\.\d+\.\d+\.\d+) .*");
			if (!m.Success)
				return null;
			return new Version(m.Groups[1].Value);
		}

		public static bool CultureAwareEquals(this string string1, string string2)
		{
			return CultureInfo.CurrentCulture.CompareInfo.Compare(
				string1,
				string2,
				CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth |
				CompareOptions.IgnoreCase) == 0 ? true : false;
		}
	}
}

#if (NET_20)
namespace System.Runtime.CompilerServices
{
	[AttributeUsageAttribute(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
	public class ExtensionAttribute : Attribute
	{
	}
}
#endif
