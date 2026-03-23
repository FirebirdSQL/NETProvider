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

namespace FirebirdSql.Data.FirebirdClient;

/// <summary>
/// Provides the names of the ActivitySource and Meter used by the Firebird ADO.NET provider
/// for OpenTelemetry integration.
/// </summary>
/// <remarks>
/// To subscribe to Firebird traces, use:
/// <code>builder.AddSource(FbTelemetry.ActivitySourceName)</code>
/// To subscribe to Firebird metrics, use:
/// <code>builder.AddMeter(FbTelemetry.MeterName)</code>
/// </remarks>
public static class FbTelemetry
{
	/// <summary>
	/// The name of the <see cref="System.Diagnostics.ActivitySource"/> used for distributed tracing.
	/// Use with <c>TracerProviderBuilder.AddSource()</c>.
	/// </summary>
	public const string ActivitySourceName = "FirebirdSql.Data";

	/// <summary>
	/// The name of the <see cref="System.Diagnostics.Metrics.Meter"/> used for metrics.
	/// Use with <c>MeterProviderBuilder.AddMeter()</c>.
	/// </summary>
	public const string MeterName = "FirebirdSql.Data";
}
