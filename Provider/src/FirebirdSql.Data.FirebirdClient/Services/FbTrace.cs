/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
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

namespace FirebirdSql.Data.Services
{
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

		public void Start(string sessionName) => StartImpl(sessionName, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task StartAsync(string sessionName, CancellationToken cancellationToken = default) => StartImpl(sessionName, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task StartImpl(string sessionName, AsyncWrappingCommonArgs async)
		{
			var version = _version;
			if (version == FbTraceVersion.Detect)
			{
				version = DetectVersion();
			}
			try
			{
				var config = string.Join(Environment.NewLine, DatabasesConfigurations.BuildConfiguration(version), ServiceConfiguration?.BuildConfiguration(version) ?? string.Empty);

				await Open(async).ConfigureAwait(false);
				var startSpb = new ServiceParameterBuffer();
				startSpb.Append(IscCodes.isc_action_svc_trace_start);
				if (!string.IsNullOrEmpty(sessionName))
					startSpb.Append(IscCodes.isc_spb_trc_name, sessionName);
				startSpb.Append(IscCodes.isc_spb_trc_cfg, config);
				await StartTask(startSpb, async).ConfigureAwait(false);
				await ProcessServiceOutput(EmptySpb, async).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				await Close(async).ConfigureAwait(false);
			}
		}

		public void Stop(int sessionID) => StopImpl(sessionID, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task StopAsync(int sessionID, CancellationToken cancellationToken = default) => StopImpl(sessionID, new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task StopImpl(int sessionID, AsyncWrappingCommonArgs async)
		{
			return DoSimpleAction(IscCodes.isc_action_svc_trace_stop, sessionID, async);
		}

		public void Suspend(int sessionID) => SuspendImpl(sessionID, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task SuspendAsync(int sessionID, CancellationToken cancellationToken = default) => SuspendImpl(sessionID, new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task SuspendImpl(int sessionID, AsyncWrappingCommonArgs async)
		{
			return DoSimpleAction(IscCodes.isc_action_svc_trace_suspend, sessionID, async);
		}

		public void Resume(int sessionID) => ResumeImpl(sessionID, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task ResumeAsync(int sessionID, CancellationToken cancellationToken = default) => ResumeImpl(sessionID, new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task ResumeImpl(int sessionID, AsyncWrappingCommonArgs async)
		{
			return DoSimpleAction(IscCodes.isc_action_svc_trace_resume, sessionID, async);
		}

		public void List() => ListImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task ListAsync(CancellationToken cancellationToken = default) => ListImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task ListImpl(AsyncWrappingCommonArgs async)
		{
			return DoSimpleAction(IscCodes.isc_action_svc_trace_list, null, async);
		}

		async Task DoSimpleAction(int action, int? sessionID, AsyncWrappingCommonArgs async)
		{
			try
			{
				await Open(async).ConfigureAwait(false);
				var startSpb = new ServiceParameterBuffer();
				startSpb.Append(action);
				if (sessionID.HasValue)
					startSpb.Append(IscCodes.isc_spb_trc_id, (int)sessionID);
				await StartTask(startSpb, async).ConfigureAwait(false);
				await ProcessServiceOutput(EmptySpb, async).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				await Close(async).ConfigureAwait(false);
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
	}
}
