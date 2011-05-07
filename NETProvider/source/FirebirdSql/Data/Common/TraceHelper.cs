using System;
using System.Diagnostics;

namespace FirebirdSql.Common
{
	static class TraceHelper
	{
		public const string CategoryName = "FirebirdClient";
		public const string ConditionalSymbol = "TRACE";

		class IndentHolder : IDisposable
		{
			public void Dispose()
			{
				Trace.Unindent();
			}
		}

		public static void WriteLine(string format, params object[] args)
		{
#if (TRACE)
			Trace.WriteLine(string.Format(format, args), CategoryName);
#endif
		}

		public static IDisposable Indent()
		{
#if (TRACE)
			Trace.Indent();
			return new IndentHolder();
#endif
		}
	}
}
