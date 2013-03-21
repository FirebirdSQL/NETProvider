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
 *	Copyright (c) 2010-2012 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Text;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Services
{
	public class FbDatabaseTraceConfiguration : FbTraceConfiguration
	{
		public FbDatabaseTraceConfiguration()
		{
			this.Enabled = false;
			this.ConnectionID = 0;
			this.TimeThreshold = TimeSpan.FromMilliseconds(100);
			this.MaxSQLLength = 300;
			this.MaxBLRLength = 500;
			this.MaxDYNLength = 500;
			this.MaxArgumentLength = 80;
			this.MaxArgumentsCount = 30;
		}

		public string DatabaseName { get; set; }

		public bool Enabled { get; set; }

		public FbDatabaseTraceEvents Events { get; set; }

		public int ConnectionID { get; set; }

		public TimeSpan TimeThreshold { get; set; }
		public int MaxSQLLength { get; set; }
		public int MaxBLRLength { get; set; }
		public int MaxDYNLength { get; set; }
		public int MaxArgumentLength { get; set; }
		public int MaxArgumentsCount { get; set; }

		public string IncludeFilter { get; set; }
		public string ExcludeFilter { get; set; }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<database");
			sb.Append((!string.IsNullOrEmpty(this.DatabaseName) ? " " + WriteString(this.DatabaseName) : string.Empty));
			sb.AppendLine(">");
			sb.AppendFormat("enabled {0}", WriteBoolValue(this.Enabled));
			sb.AppendLine();
			if (!string.IsNullOrEmpty(this.IncludeFilter))
			{
				sb.AppendFormat("include_filter {0}", WriteRegEx(this.IncludeFilter));
				sb.AppendLine();
			}
			if (!string.IsNullOrEmpty(this.ExcludeFilter))
			{
				sb.AppendFormat("exclude_filter {0}", WriteRegEx(this.ExcludeFilter));
				sb.AppendLine();
			}
			sb.AppendFormat("log_connections {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.Connections)));
			sb.AppendLine();
			sb.AppendFormat("connection_id {0}", WriteNumber(this.ConnectionID));
			sb.AppendLine();
			sb.AppendFormat("log_transactions {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.Transactions)));
			sb.AppendLine();
			sb.AppendFormat("log_statement_prepare {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.StatementPrepare)));
			sb.AppendLine();
			sb.AppendFormat("log_statement_free {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.StatementFree)));
			sb.AppendLine();
			sb.AppendFormat("log_statement_start {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.StatementStart)));
			sb.AppendLine();
			sb.AppendFormat("log_statement_finish {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.StatementFinish)));
			sb.AppendLine();
			sb.AppendFormat("log_procedure_start {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.ProcedureStart)));
			sb.AppendLine();
			sb.AppendFormat("log_procedure_finish {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.ProcedureFinish)));
			sb.AppendLine();
			sb.AppendFormat("log_trigger_start {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.TriggerStart)));
			sb.AppendLine();
			sb.AppendFormat("log_trigger_finish {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.TriggerFinish)));
			sb.AppendLine();
			sb.AppendFormat("log_context {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.Context)));
			sb.AppendLine();
			sb.AppendFormat("log_errors {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.Errors)));
			sb.AppendLine();
			sb.AppendFormat("log_sweep {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.Sweep)));
			sb.AppendLine();
			sb.AppendFormat("print_plan {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.PrintPlan)));
			sb.AppendLine(); 
			sb.AppendFormat("print_perf {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.PrintPerf)));
			sb.AppendLine();
			sb.AppendFormat("log_blr_requests {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.BLRRequests)));
			sb.AppendLine();
			sb.AppendFormat("print_blr {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.PrintBLR)));
			sb.AppendLine();
			sb.AppendFormat("log_dyn_requests {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.DYNRequests)));
			sb.AppendLine();
			sb.AppendFormat("print_dyn {0}", WriteBoolValue(this.Events.HasFlag(FbDatabaseTraceEvents.PrintDYN)));
			sb.AppendLine();
			sb.AppendFormat("time_threshold {0}", WriteNumber((int)this.TimeThreshold.TotalMilliseconds));
			sb.AppendLine();
			sb.AppendFormat("max_sql_length {0}", WriteNumber(this.MaxSQLLength));
			sb.AppendLine();
			sb.AppendFormat("max_blr_length {0}", WriteNumber(this.MaxBLRLength));
			sb.AppendLine();
			sb.AppendFormat("max_dyn_length {0}", WriteNumber(this.MaxDYNLength));
			sb.AppendLine();
			sb.AppendFormat("max_arg_length {0}", WriteNumber(this.MaxArgumentLength));
			sb.AppendLine();
			sb.AppendFormat("max_arg_count {0}", WriteNumber(this.MaxArgumentsCount));
			sb.AppendLine();
			sb.AppendLine("</database>");
			return sb.ToString();
		}
	}
}
