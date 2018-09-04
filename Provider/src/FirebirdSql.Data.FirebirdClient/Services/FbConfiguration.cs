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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Services
{
	public sealed class FbConfiguration : FbService
	{
		public FbConfiguration(string connectionString = null)
			: base(connectionString)
		{ }

		public void SetSqlDialect(int sqlDialect)
		{
			StartSpb = new ServiceParameterBuffer();
			StartSpb.Append(IscCodes.isc_action_svc_properties);
			StartSpb.Append(IscCodes.isc_spb_dbname, Database);
			StartSpb.Append(IscCodes.isc_spb_prp_set_sql_dialect, sqlDialect);

			Open();
			StartTask();
			Close();
		}

		public void SetSweepInterval(int sweepInterval)
		{
			StartSpb = new ServiceParameterBuffer();
			StartSpb.Append(IscCodes.isc_action_svc_properties);
			StartSpb.Append(IscCodes.isc_spb_dbname, Database);
			StartSpb.Append(IscCodes.isc_spb_prp_sweep_interval, sweepInterval);

			Open();
			StartTask();
			Close();
		}

		public void SetPageBuffers(int pageBuffers)
		{
			StartSpb = new ServiceParameterBuffer();
			StartSpb.Append(IscCodes.isc_action_svc_properties);
			StartSpb.Append(IscCodes.isc_spb_dbname, Database);
			StartSpb.Append(IscCodes.isc_spb_prp_page_buffers, pageBuffers);

			Open();
			StartTask();
			Close();
		}

		public void DatabaseShutdown(FbShutdownMode mode, int seconds)
		{
			StartSpb = new ServiceParameterBuffer();
			StartSpb.Append(IscCodes.isc_action_svc_properties);
			StartSpb.Append(IscCodes.isc_spb_dbname, Database);
			switch (mode)
			{
				case FbShutdownMode.Forced:
					StartSpb.Append(IscCodes.isc_spb_prp_shutdown_db, seconds);
					break;
				case FbShutdownMode.DenyTransaction:
					StartSpb.Append(IscCodes.isc_spb_prp_deny_new_transactions, seconds);
					break;
				case FbShutdownMode.DenyConnection:
					StartSpb.Append(IscCodes.isc_spb_prp_deny_new_attachments, seconds);
					break;
			}

			Open();
			StartTask();
			Close();
		}

		public void DatabaseShutdown2(FbShutdownOnlineMode mode, FbShutdownType type, int seconds)
		{
			StartSpb = new ServiceParameterBuffer();
			StartSpb.Append(IscCodes.isc_action_svc_properties);
			StartSpb.Append(IscCodes.isc_spb_dbname, Database);
			StartSpb.Append(IscCodes.isc_spb_prp_shutdown_mode, FbShutdownOnlineModeToIscCode(mode));
			switch (type)
			{
				case FbShutdownType.ForceShutdown:
					StartSpb.Append(IscCodes.isc_spb_prp_force_shutdown, seconds);
					break;
				case FbShutdownType.AttachmentsShutdown:
					StartSpb.Append(IscCodes.isc_spb_prp_attachments_shutdown, seconds);
					break;
				case FbShutdownType.TransactionsShutdown:
					StartSpb.Append(IscCodes.isc_spb_prp_transactions_shutdown, seconds);
					break;
			}

			Open();
			StartTask();
			Close();
		}

		public void DatabaseOnline()
		{
			StartSpb = new ServiceParameterBuffer();
			StartSpb.Append(IscCodes.isc_action_svc_properties);
			StartSpb.Append(IscCodes.isc_spb_dbname, Database);
			StartSpb.Append(IscCodes.isc_spb_options, IscCodes.isc_spb_prp_db_online);

			Open();
			StartTask();
			Close();
		}

		public void DatabaseOnline2(FbShutdownOnlineMode mode)
		{
			StartSpb = new ServiceParameterBuffer();
			StartSpb.Append(IscCodes.isc_action_svc_properties);
			StartSpb.Append(IscCodes.isc_spb_dbname, Database);
			StartSpb.Append(IscCodes.isc_spb_prp_online_mode, FbShutdownOnlineModeToIscCode(mode));

			Open();
			StartTask();
			Close();
		}

		public void ActivateShadows()
		{
			StartSpb = new ServiceParameterBuffer();
			StartSpb.Append(IscCodes.isc_action_svc_properties);
			StartSpb.Append(IscCodes.isc_spb_dbname, Database);
			StartSpb.Append(IscCodes.isc_spb_options, IscCodes.isc_spb_prp_activate);

			Open();
			StartTask();
			Close();
		}

		public void SetForcedWrites(bool forcedWrites)
		{
			StartSpb = new ServiceParameterBuffer();
			StartSpb.Append(IscCodes.isc_action_svc_properties);
			StartSpb.Append(IscCodes.isc_spb_dbname, Database);
			if (forcedWrites)
			{
				StartSpb.Append(IscCodes.isc_spb_prp_write_mode, (byte)IscCodes.isc_spb_prp_wm_sync);
			}
			else
			{
				StartSpb.Append(IscCodes.isc_spb_prp_write_mode, (byte)IscCodes.isc_spb_prp_wm_async);
			}

			Open();
			StartTask();
			Close();
		}

		public void SetReserveSpace(bool reserveSpace)
		{
			StartSpb = new ServiceParameterBuffer();
			StartSpb.Append(IscCodes.isc_action_svc_properties);
			StartSpb.Append(IscCodes.isc_spb_dbname, Database);
			if (reserveSpace)
			{
				StartSpb.Append(IscCodes.isc_spb_prp_reserve_space, (byte)IscCodes.isc_spb_prp_res);
			}
			else
			{
				StartSpb.Append(IscCodes.isc_spb_prp_reserve_space, (byte)IscCodes.isc_spb_prp_res_use_full);
			}

			Open();
			StartTask();
			Close();
		}

		public void SetAccessMode(bool readOnly)
		{
			StartSpb = new ServiceParameterBuffer();
			StartSpb.Append(IscCodes.isc_action_svc_properties);
			StartSpb.Append(IscCodes.isc_spb_dbname, Database);
			StartSpb.Append(IscCodes.isc_spb_prp_access_mode, (byte)(readOnly ? IscCodes.isc_spb_prp_am_readonly : IscCodes.isc_spb_prp_am_readwrite));

			Open();
			StartTask();
			Close();
		}

		public void NoLinger()
		{
			StartSpb = new ServiceParameterBuffer();
			StartSpb.Append(IscCodes.isc_action_svc_properties);
			StartSpb.Append(IscCodes.isc_spb_dbname, Database);
			StartSpb.Append(IscCodes.isc_spb_options, IscCodes.isc_spb_prp_nolinger);

			Open();
			StartTask();
			Close();
		}

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
					throw new ArgumentOutOfRangeException(nameof(mode));
			}
		}
	}
}
