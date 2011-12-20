/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2010 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Text;

using FirebirdSql.Common;

namespace FirebirdSql.Data.Services
{
	public class FbServiceTraceConfiguration
	{
		public FbServiceTraceConfiguration()
		{
			this.Enabled = false;
		}

		public bool Enabled { get; set; }

		public FbServiceTraceEvents Events { get; set; }

		public string IncludeFilter { get; set; }
		public string ExcludeFilter { get; set; }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<services>");
			sb.AppendFormat("enabled {0}", (this.Enabled ? "true" : "false"));
			sb.AppendLine();
			if (!string.IsNullOrEmpty(this.IncludeFilter))
			{
				sb.AppendFormat("include_filter {0}", this.IncludeFilter);
				sb.AppendLine();
			}
			if (!string.IsNullOrEmpty(this.ExcludeFilter))
			{
				sb.AppendFormat("exclude_filter {0}", this.ExcludeFilter);
				sb.AppendLine();
			}
			sb.AppendFormat("log_services {0}", (Extensions.HasFlag(this.Events, FbServiceTraceEvents.Services) ? "true" : "false"));
			sb.AppendLine();
			sb.AppendFormat("log_service_query {0}", (Extensions.HasFlag(this.Events, FbServiceTraceEvents.ServiceQuery) ? "true" : "false"));
			sb.AppendLine("</services>");
			return sb.ToString();
		}
	}
}
