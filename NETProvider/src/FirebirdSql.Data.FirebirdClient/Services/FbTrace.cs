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
using System.Collections.Generic;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services
{
	public sealed class FbTrace : FbService
	{
		#region Constructors
		public FbTrace(string connectionString = null)
			: base(connectionString)
		{
			this.DatabasesConfigurations = new FbDatabaseTraceConfigurationCollection();
		}
		#endregion

		#region Properties
		public FbDatabaseTraceConfigurationCollection DatabasesConfigurations { get; private set; }
		public FbServiceTraceConfiguration ServiceConfiguration { get; set; }
		#endregion

		#region Methods
		public void Start(string sessionName)
		{
			try
			{
				string config = this.DatabasesConfigurations.ToString() + (this.ServiceConfiguration != null ? this.ServiceConfiguration.ToString() : string.Empty);

				this.StartSpb = new ServiceParameterBuffer();

				this.StartSpb.Append(IscCodes.isc_action_svc_trace_start);
				if (!string.IsNullOrEmpty(sessionName))
					this.StartSpb.Append(IscCodes.isc_spb_trc_name, sessionName);
				this.StartSpb.Append(IscCodes.isc_spb_trc_cfg, config);

				this.Open();
				this.StartTask();
				this.ProcessServiceOutput();
			}
			catch (Exception ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				this.Close();
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
		#endregion

		void DoSimpleAction(int action, int? sessionID = null)
		{
			try
			{
				this.StartSpb = new ServiceParameterBuffer();

				this.StartSpb.Append(action);
				if (sessionID.HasValue)
					this.StartSpb.Append(IscCodes.isc_spb_trc_id, (int)sessionID);

				this.Open();
				this.StartTask();
				this.ProcessServiceOutput();
			}
			catch (Exception ex)
			{
				throw new FbException(ex.Message, ex);
			}
			finally
			{
				this.Close();
			}
		}
	}
}
