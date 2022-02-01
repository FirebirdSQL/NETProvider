/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Text;

namespace FirebirdSql.Data.Services;

public class FbDatabaseTraceConfiguration : FbTraceConfiguration
{
	public FbDatabaseTraceConfiguration()
	{
		Enabled = false;
		ConnectionID = 0;
		TimeThreshold = TimeSpan.FromMilliseconds(100);
		MaxSQLLength = 300;
		MaxBLRLength = 500;
		MaxDYNLength = 500;
		MaxArgumentLength = 80;
		MaxArgumentsCount = 30;
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

	public string IncludeGdsCodes { get; set; }
	public string ExcludeGdsCodes { get; set; }

	public string BuildConfiguration(FbTraceVersion version)
	{
		switch (version)
		{
			case FbTraceVersion.Version1:
				return BuildConfiguration1();
			case FbTraceVersion.Version2:
				return BuildConfiguration2();
			default:
				throw new ArgumentOutOfRangeException(nameof(version));
		}
	}
	string BuildConfiguration1()
	{
		var sb = new StringBuilder();
		sb.Append("<database");
		sb.Append((!string.IsNullOrEmpty(DatabaseName) ? $" {WriteRegEx(DatabaseName)}" : string.Empty));
		sb.AppendLine(">");
		sb.AppendFormat("enabled {0}", WriteBoolValue(Enabled));
		sb.AppendLine();
		if (!string.IsNullOrEmpty(IncludeFilter))
		{
			sb.AppendFormat("include_filter {0}", WriteRegEx(IncludeFilter));
			sb.AppendLine();
		}
		if (!string.IsNullOrEmpty(ExcludeFilter))
		{
			sb.AppendFormat("exclude_filter {0}", WriteRegEx(ExcludeFilter));
			sb.AppendLine();
		}
		sb.AppendFormat("log_connections {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.Connections)));
		sb.AppendLine();
		sb.AppendFormat("connection_id {0}", WriteNumber(ConnectionID));
		sb.AppendLine();
		sb.AppendFormat("log_transactions {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.Transactions)));
		sb.AppendLine();
		sb.AppendFormat("log_statement_prepare {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.StatementPrepare)));
		sb.AppendLine();
		sb.AppendFormat("log_statement_free {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.StatementFree)));
		sb.AppendLine();
		sb.AppendFormat("log_statement_start {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.StatementStart)));
		sb.AppendLine();
		sb.AppendFormat("log_statement_finish {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.StatementFinish)));
		sb.AppendLine();
		sb.AppendFormat("log_procedure_start {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.ProcedureStart)));
		sb.AppendLine();
		sb.AppendFormat("log_procedure_finish {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.ProcedureFinish)));
		sb.AppendLine();
		sb.AppendFormat("log_trigger_start {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.TriggerStart)));
		sb.AppendLine();
		sb.AppendFormat("log_trigger_finish {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.TriggerFinish)));
		sb.AppendLine();
		sb.AppendFormat("log_context {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.Context)));
		sb.AppendLine();
		sb.AppendFormat("log_errors {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.Errors)));
		sb.AppendLine();
		sb.AppendFormat("log_warnings {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.Warnings)));
		sb.AppendLine();
		sb.AppendFormat("log_initfini {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.InitFini)));
		sb.AppendLine();
		sb.AppendFormat("log_sweep {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.Sweep)));
		sb.AppendLine();
		sb.AppendFormat("print_plan {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.PrintPlan)));
		sb.AppendLine();
		sb.AppendFormat("print_perf {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.PrintPerf)));
		sb.AppendLine();
		sb.AppendFormat("log_blr_requests {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.BLRRequests)));
		sb.AppendLine();
		sb.AppendFormat("print_blr {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.PrintBLR)));
		sb.AppendLine();
		sb.AppendFormat("log_dyn_requests {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.DYNRequests)));
		sb.AppendLine();
		sb.AppendFormat("print_dyn {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.PrintDYN)));
		sb.AppendLine();
		sb.AppendFormat("time_threshold {0}", WriteNumber((int)TimeThreshold.TotalMilliseconds));
		sb.AppendLine();
		sb.AppendFormat("max_sql_length {0}", WriteNumber(MaxSQLLength));
		sb.AppendLine();
		sb.AppendFormat("max_blr_length {0}", WriteNumber(MaxBLRLength));
		sb.AppendLine();
		sb.AppendFormat("max_dyn_length {0}", WriteNumber(MaxDYNLength));
		sb.AppendLine();
		sb.AppendFormat("max_arg_length {0}", WriteNumber(MaxArgumentLength));
		sb.AppendLine();
		sb.AppendFormat("max_arg_count {0}", WriteNumber(MaxArgumentsCount));
		sb.AppendLine();
		sb.AppendLine("</database>");
		return sb.ToString();
	}
	string BuildConfiguration2()
	{
		var sb = new StringBuilder();
		sb.Append("database");
		sb.Append((!string.IsNullOrEmpty(DatabaseName) ? $" = {WriteRegEx(DatabaseName)}" : string.Empty));
		sb.AppendLine("{");
		sb.AppendFormat("enabled = {0}", WriteBoolValue(Enabled));
		sb.AppendLine();
		if (!string.IsNullOrEmpty(IncludeFilter))
		{
			sb.AppendFormat("include_filter = {0}", WriteRegEx(IncludeFilter));
			sb.AppendLine();
		}
		if (!string.IsNullOrEmpty(ExcludeFilter))
		{
			sb.AppendFormat("exclude_filter = {0}", WriteRegEx(ExcludeFilter));
			sb.AppendLine();
		}
		if (!string.IsNullOrEmpty(IncludeGdsCodes))
		{
			sb.AppendFormat("include_gds_codes = {0}", WriteString(IncludeGdsCodes));
			sb.AppendLine();
		}
		if (!string.IsNullOrEmpty(ExcludeGdsCodes))
		{
			sb.AppendFormat("exclude_gds_codes = {0}", WriteString(ExcludeGdsCodes));
			sb.AppendLine();
		}
		sb.AppendFormat("log_connections = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.Connections)));
		sb.AppendLine();
		sb.AppendFormat("connection_id = {0}", WriteNumber(ConnectionID));
		sb.AppendLine();
		sb.AppendFormat("log_transactions = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.Transactions)));
		sb.AppendLine();
		sb.AppendFormat("log_statement_prepare = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.StatementPrepare)));
		sb.AppendLine();
		sb.AppendFormat("log_statement_free = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.StatementFree)));
		sb.AppendLine();
		sb.AppendFormat("log_statement_start = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.StatementStart)));
		sb.AppendLine();
		sb.AppendFormat("log_statement_finish = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.StatementFinish)));
		sb.AppendLine();
		sb.AppendFormat("log_procedure_start = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.ProcedureStart)));
		sb.AppendLine();
		sb.AppendFormat("log_procedure_finish = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.ProcedureFinish)));
		sb.AppendLine();
		sb.AppendFormat("log_function_start = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.FunctionStart)));
		sb.AppendLine();
		sb.AppendFormat("log_function_finish = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.FunctionFinish)));
		sb.AppendLine();
		sb.AppendFormat("log_trigger_start = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.TriggerStart)));
		sb.AppendLine();
		sb.AppendFormat("log_trigger_finish = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.TriggerFinish)));
		sb.AppendLine();
		sb.AppendFormat("log_context = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.Context)));
		sb.AppendLine();
		sb.AppendFormat("log_errors = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.Errors)));
		sb.AppendLine();
		sb.AppendFormat("log_warnings = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.Warnings)));
		sb.AppendLine();
		sb.AppendFormat("log_initfini = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.InitFini)));
		sb.AppendLine();
		sb.AppendFormat("log_sweep = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.Sweep)));
		sb.AppendLine();
		sb.AppendFormat("print_plan = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.PrintPlan)));
		sb.AppendLine();
		sb.AppendFormat("explain_plan = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.ExplainPlan)));
		sb.AppendLine();
		sb.AppendFormat("print_perf = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.PrintPerf)));
		sb.AppendLine();
		sb.AppendFormat("log_blr_requests = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.BLRRequests)));
		sb.AppendLine();
		sb.AppendFormat("print_blr = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.PrintBLR)));
		sb.AppendLine();
		sb.AppendFormat("log_dyn_requests = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.DYNRequests)));
		sb.AppendLine();
		sb.AppendFormat("print_dyn = {0}", WriteBoolValue(Events.HasFlag(FbDatabaseTraceEvents.PrintDYN)));
		sb.AppendLine();
		sb.AppendFormat("time_threshold = {0}", WriteNumber((int)TimeThreshold.TotalMilliseconds));
		sb.AppendLine();
		sb.AppendFormat("max_sql_length = {0}", WriteNumber(MaxSQLLength));
		sb.AppendLine();
		sb.AppendFormat("max_blr_length = {0}", WriteNumber(MaxBLRLength));
		sb.AppendLine();
		sb.AppendFormat("max_dyn_length = {0}", WriteNumber(MaxDYNLength));
		sb.AppendLine();
		sb.AppendFormat("max_arg_length = {0}", WriteNumber(MaxArgumentLength));
		sb.AppendLine();
		sb.AppendFormat("max_arg_count = {0}", WriteNumber(MaxArgumentsCount));
		sb.AppendLine();
		sb.AppendLine("}");
		return sb.ToString();
	}
}
