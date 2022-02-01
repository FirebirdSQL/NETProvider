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

public class FbServiceTraceConfiguration : FbTraceConfiguration
{
	public FbServiceTraceConfiguration()
	{
		Enabled = false;
	}

	public bool Enabled { get; set; }

	public FbServiceTraceEvents Events { get; set; }

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
		sb.AppendLine("<services>");
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
		sb.AppendFormat("log_services {0}", WriteBoolValue(Events.HasFlag(FbServiceTraceEvents.Services)));
		sb.AppendLine();
		sb.AppendFormat("log_service_query {0}", WriteBoolValue(Events.HasFlag(FbServiceTraceEvents.ServiceQuery)));
		sb.AppendLine();
		sb.AppendFormat("log_errors {0}", WriteBoolValue(Events.HasFlag(FbServiceTraceEvents.Errors)));
		sb.AppendLine();
		sb.AppendFormat("log_warnings {0}", WriteBoolValue(Events.HasFlag(FbServiceTraceEvents.Warnings)));
		sb.AppendLine();
		sb.AppendFormat("log_initfini {0}", WriteBoolValue(Events.HasFlag(FbServiceTraceEvents.InitFini)));
		sb.AppendLine();
		sb.AppendLine("</services>");
		return sb.ToString();
	}
	string BuildConfiguration2()
	{
		var sb = new StringBuilder();
		sb.AppendLine("services");
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
		sb.AppendFormat("log_services = {0}", WriteBoolValue(Events.HasFlag(FbServiceTraceEvents.Services)));
		sb.AppendLine();
		sb.AppendFormat("log_service_query = {0}", WriteBoolValue(Events.HasFlag(FbServiceTraceEvents.ServiceQuery)));
		sb.AppendLine();
		sb.AppendFormat("log_errors = {0}", WriteBoolValue(Events.HasFlag(FbServiceTraceEvents.Errors)));
		sb.AppendLine();
		sb.AppendFormat("log_warnings = {0}", WriteBoolValue(Events.HasFlag(FbServiceTraceEvents.Warnings)));
		sb.AppendLine();
		sb.AppendFormat("log_initfini = {0}", WriteBoolValue(Events.HasFlag(FbServiceTraceEvents.InitFini)));
		sb.AppendLine();
		sb.AppendLine("}");
		return sb.ToString();
	}
}
