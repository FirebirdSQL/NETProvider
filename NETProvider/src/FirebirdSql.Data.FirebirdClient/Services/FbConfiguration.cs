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
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	Copyright (c) 2014 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Services
{
	public sealed class FbConfiguration : FbService
	{
		#region Constructors

		public FbConfiguration(string connectionString = null)
			: base(connectionString)
		{ }

		#endregion

		#region Methods

		public void SetSqlDialect(int sqlDialect)
		{
			this.StartSpb = new ServiceParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);
			this.StartSpb.Append(IscCodes.isc_spb_prp_set_sql_dialect, sqlDialect);

			this.Open();

			this.StartTask();

			this.Close();
		}

		public void SetSweepInterval(int sweepInterval)
		{
			this.StartSpb = new ServiceParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);
			this.StartSpb.Append(IscCodes.isc_spb_prp_sweep_interval, sweepInterval);

			this.Open();

			this.StartTask();

			this.Close();
		}

		public void SetPageBuffers(int pageBuffers)
		{
			this.StartSpb = new ServiceParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);
			this.StartSpb.Append(IscCodes.isc_spb_prp_page_buffers, pageBuffers);

			this.Open();

			this.StartTask();

			this.Close();
		}

		public void DatabaseShutdown(FbShutdownMode mode, int seconds)
		{
			this.StartSpb = new ServiceParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

			switch (mode)
			{
				case FbShutdownMode.Forced:
					this.StartSpb.Append(IscCodes.isc_spb_prp_shutdown_db, seconds);
					break;

				case FbShutdownMode.DenyTransaction:
					this.StartSpb.Append(IscCodes.isc_spb_prp_deny_new_transactions, seconds);
					break;

				case FbShutdownMode.DenyConnection:
					this.StartSpb.Append(IscCodes.isc_spb_prp_deny_new_attachments, seconds);
					break;
			}

			this.Open();

			this.StartTask();

			this.Close();
		}

		public void DatabaseShutdown2(FbShutdownOnlineMode mode, FbShutdownType type, int seconds)
		{
			this.StartSpb = new ServiceParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

			this.StartSpb.Append(IscCodes.isc_spb_prp_shutdown_mode, FbShutdownOnlineModeToIscCode(mode));

			switch (type)
			{
				case FbShutdownType.ForceShutdown:
					this.StartSpb.Append(IscCodes.isc_spb_prp_force_shutdown, seconds);
					break;

				case FbShutdownType.AttachmentsShutdown:
					this.StartSpb.Append(IscCodes.isc_spb_prp_attachments_shutdown, seconds);
					break;

				case FbShutdownType.TransactionsShutdown:
					this.StartSpb.Append(IscCodes.isc_spb_prp_transactions_shutdown, seconds);
					break;
			}

			this.Open();

			this.StartTask();

			this.Close();
		}

		public void DatabaseOnline()
		{
			this.StartSpb = new ServiceParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);
			this.StartSpb.Append(IscCodes.isc_spb_options, IscCodes.isc_spb_prp_db_online);

			this.Open();

			this.StartTask();

			this.Close();
		}

		public void DatabaseOnline2(FbShutdownOnlineMode mode)
		{
			this.StartSpb = new ServiceParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

			this.StartSpb.Append(IscCodes.isc_spb_prp_online_mode, FbShutdownOnlineModeToIscCode(mode));

			this.Open();

			this.StartTask();

			this.Close();
		}

		public void ActivateShadows()
		{
			this.StartSpb = new ServiceParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);
			this.StartSpb.Append(IscCodes.isc_spb_options, IscCodes.isc_spb_prp_activate);

			this.Open();

			this.StartTask();

			this.Close();
		}

		public void SetForcedWrites(bool forcedWrites)
		{
			this.StartSpb = new ServiceParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

			if (forcedWrites)
			{
				this.StartSpb.Append(IscCodes.isc_spb_prp_write_mode, (byte)IscCodes.isc_spb_prp_wm_sync);
			}
			else
			{
				this.StartSpb.Append(IscCodes.isc_spb_prp_write_mode, (byte)IscCodes.isc_spb_prp_wm_async);
			}

			this.Open();

			this.StartTask();

			this.Close();
		}

		public void SetReserveSpace(bool reserveSpace)
		{
			this.StartSpb = new ServiceParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

			if (reserveSpace)
			{
				this.StartSpb.Append(IscCodes.isc_spb_prp_reserve_space, (byte)IscCodes.isc_spb_prp_res);
			}
			else
			{
				this.StartSpb.Append(IscCodes.isc_spb_prp_reserve_space, (byte)IscCodes.isc_spb_prp_res_use_full);
			}

			this.Open();

			this.StartTask();

			this.Close();
		}

		public void SetAccessMode(bool readOnly)
		{
			this.StartSpb = new ServiceParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);
			this.StartSpb.Append(IscCodes.isc_spb_prp_access_mode, (byte)(readOnly ? IscCodes.isc_spb_prp_am_readonly : IscCodes.isc_spb_prp_am_readwrite));

			this.Open();

			this.StartTask();

			this.Close();
		}

		#endregion

		#region Private Methods

		byte FbShutdownOnlineModeToIscCode(FbShutdownOnlineMode mode)
		{
			switch (mode)
			{
				case FbShutdownOnlineMode.Normal:
					return IscCodes.isc_spb_prp_sm_normal;
				case FbShutdownOnlineMode.Multi:
					return IscCodes.isc_spb_prp_sm_multi;
				case FbShutdownOnlineMode.Single:
					return IscCodes.isc_spb_prp_sm_single;
				case FbShutdownOnlineMode.Full:
					return IscCodes.isc_spb_prp_sm_full;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion
	}
}
