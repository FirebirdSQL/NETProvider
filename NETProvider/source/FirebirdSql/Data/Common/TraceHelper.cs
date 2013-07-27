using System;
using System.Diagnostics;

namespace FirebirdSql.Data.Common
{
	static class TraceHelper
	{
		public const string Name = "FirebirdSql.Data.FirebirdClient";
		public const string ConditionalSymbol = "TRACE";

		static TraceSource _instance;

		static TraceHelper()
		{
			_instance = new TraceSource(Name, SourceLevels.All);
		}

		public static void Trace(TraceEventType eventType, string message)
		{
			_instance.TraceEvent(eventType, default(int), message);
			_instance.Flush();
		}

		public static void Trace(TraceEventType eventType, string format, params object[] args)
		{
			_instance.TraceEvent(eventType, default(int), format, args);
			_instance.Flush();
		}
	}
}
