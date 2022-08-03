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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services;

public sealed class FbTrace : FbService
{
	FbTraceVersion _version;

	public FbDatabaseTraceConfigurationCollection DatabasesConfigurations { get; }
	public FbServiceTraceConfiguration ServiceConfiguration { get; set; }

	public FbTrace(FbTraceVersion version = FbTraceVersion.Detect, string connectionString = null)
		: base(connectionString)
	{
		_version = version;
		DatabasesConfigurations = new FbDatabaseTraceConfigurationCollection();
	}

	public void Start(string sessionName)
	{
		var version = _version;
		if (version == FbTraceVersion.Detect)
		{
			version = DetectVersion();
		}
		try
		{
			try
			{
				var config = string.Join(Environment.NewLine, DatabasesConfigurations.BuildConfiguration(version), ServiceConfiguration?.BuildConfiguration(version) ?? string.Empty);

				Open();
				var startSpb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
				startSpb.Append(IscCodes.isc_action_svc_trace_start);
				if (!string.IsNullOrEmpty(sessionName))
					startSpb.Append2(IscCodes.isc_spb_trc_name, sessionName);
				startSpb.Append2(IscCodes.isc_spb_trc_cfg, config);
				StartTask(startSpb);
				ProcessServiceOutput(new ServiceParameterBuffer2(Service.ParameterBufferEncoding));
			}
			finally
			{
				Close();
			}
		}
		catch (Exception ex)
		{
			throw FbException.Create(ex);
		}
	}
	public async Task StartAsync(string sessionName, CancellationToken cancellationToken = default)
	{
		var version = _version;
		if (version == FbTraceVersion.Detect)
		{
			version = await DetectVersionAsync(cancellationToken).ConfigureAwait(false);
		}
		try
		{
			try
			{
				var config = string.Join(Environment.NewLine, DatabasesConfigurations.BuildConfiguration(version), ServiceConfiguration?.BuildConfiguration(version) ?? string.Empty);

				await OpenAsync(cancellationToken).ConfigureAwait(false);
				var startSpb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
				startSpb.Append(IscCodes.isc_action_svc_trace_start);
				if (!string.IsNullOrEmpty(sessionName))
					startSpb.Append2(IscCodes.isc_spb_trc_name, sessionName);
				startSpb.Append2(IscCodes.isc_spb_trc_cfg, config);
				await StartTaskAsync(startSpb, cancellationToken).ConfigureAwait(false);
				await ProcessServiceOutputAsync(new ServiceParameterBuffer2(Service.ParameterBufferEncoding), cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				await CloseAsync(cancellationToken).ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			throw FbException.Create(ex);
		}
	}

	public void Stop(int sessionID)
	{
		DoSimpleAction(IscCodes.isc_action_svc_trace_stop, sessionID);
	}
	public Task StopAsync(int sessionID, CancellationToken cancellationToken = default)
	{
		return DoSimpleActionAsync(IscCodes.isc_action_svc_trace_stop, sessionID, cancellationToken);
	}

	public void Suspend(int sessionID)
	{
		DoSimpleAction(IscCodes.isc_action_svc_trace_suspend, sessionID);
	}
	public Task SuspendAsync(int sessionID, CancellationToken cancellationToken = default)
	{
		return DoSimpleActionAsync(IscCodes.isc_action_svc_trace_suspend, sessionID, cancellationToken);
	}

	public void Resume(int sessionID)
	{
		DoSimpleAction(IscCodes.isc_action_svc_trace_resume, sessionID);
	}
	public Task ResumeAsync(int sessionID, CancellationToken cancellationToken = default)
	{
		return DoSimpleActionAsync(IscCodes.isc_action_svc_trace_resume, sessionID, cancellationToken);
	}

	public void List()
	{
		DoSimpleAction(IscCodes.isc_action_svc_trace_list, null);
	}
	public Task ListAsync(CancellationToken cancellationToken = default)
	{
		return DoSimpleActionAsync(IscCodes.isc_action_svc_trace_list, null, cancellationToken);
	}

	void DoSimpleAction(int action, int? sessionID)
	{
		try
		{
			try
			{
				Open();
				var startSpb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
				startSpb.Append(action);
				if (sessionID.HasValue)
					startSpb.Append(IscCodes.isc_spb_trc_id, (int)sessionID);
				StartTask(startSpb);
				ProcessServiceOutput(new ServiceParameterBuffer2(Service.ParameterBufferEncoding));
			}
			finally
			{
				Close();
			}
		}
		catch (Exception ex)
		{
			throw FbException.Create(ex);
		}
	}
	async Task DoSimpleActionAsync(int action, int? sessionID, CancellationToken cancellationToken = default)
	{
		try
		{
			try
			{
				await OpenAsync(cancellationToken).ConfigureAwait(false);
				var startSpb = new ServiceParameterBuffer2(Service.ParameterBufferEncoding);
				startSpb.Append(action);
				if (sessionID.HasValue)
					startSpb.Append(IscCodes.isc_spb_trc_id, (int)sessionID);
				await StartTaskAsync(startSpb, cancellationToken).ConfigureAwait(false);
				await ProcessServiceOutputAsync(new ServiceParameterBuffer2(Service.ParameterBufferEncoding), cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				await CloseAsync(cancellationToken).ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			throw FbException.Create(ex);
		}
	}

	FbTraceVersion DetectVersion()
	{
		var serverProperties = new FbServerProperties(ConnectionString);
		var serverVersion = FbServerProperties.ParseServerVersion(serverProperties.GetServerVersion());
		if (serverVersion < new Version(3, 0, 0, 0))
			return FbTraceVersion.Version1;
		else
			return FbTraceVersion.Version2;
	}
	async Task<FbTraceVersion> DetectVersionAsync(CancellationToken cancellationToken = default)
	{
		var serverProperties = new FbServerProperties(ConnectionString);
		var serverVersion = FbServerProperties.ParseServerVersion(await serverProperties.GetServerVersionAsync(cancellationToken).ConfigureAwait(false));
		if (serverVersion < new Version(3, 0, 0, 0))
			return FbTraceVersion.Version1;
		else
			return FbTraceVersion.Version2;
	}
}
