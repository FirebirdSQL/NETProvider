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
 *	Copyright (c) 2010-2017 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;

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

		public void Start(string sessionName)
		{
			var version = _version;
			if (version == FbTraceVersion.Detect)
			{
				version = DetectVersion();
			}
			try
			{
				var config = string.Join(Environment.NewLine, DatabasesConfigurations.BuildConfiguration(version), ServiceConfiguration?.BuildConfiguration(version) ?? string.Empty);

				StartSpb = new ServiceParameterBuffer();
				StartSpb.Append(IscCodes.isc_action_svc_trace_start);
				if (!string.IsNullOrEmpty(sessionName))
					StartSpb.Append(IscCodes.isc_spb_trc_name, sessionName);
				StartSpb.Append(IscCodes.isc_spb_trc_cfg, config);

				Open();
				StartTask();
				ProcessServiceOutput();
			}
			catch (Exception ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				Close();
			}
		}

		public void Stop(int sessionID)
		{
			DoSimpleAction(IscCodes.isc_action_svc_trace_stop, sessionID);
		}

		public void Suspend(int sessionID)
		{
			DoSimpleAction(IscCodes.isc_action_svc_trace_suspend, sessionID);
		}

		public void Resume(int sessionID)
		{
			DoSimpleAction(IscCodes.isc_action_svc_trace_resume, sessionID);
		}

		public void List()
		{
			DoSimpleAction(IscCodes.isc_action_svc_trace_list);
		}

		void DoSimpleAction(int action, int? sessionID = null)
		{
			try
			{
				StartSpb = new ServiceParameterBuffer();
				StartSpb.Append(action);
				if (sessionID.HasValue)
					StartSpb.Append(IscCodes.isc_spb_trc_id, (int)sessionID);

				Open();
				StartTask();
				ProcessServiceOutput();
			}
			catch (Exception ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				Close();
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
