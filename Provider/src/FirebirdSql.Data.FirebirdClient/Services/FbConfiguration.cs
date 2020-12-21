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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Services
{
	public sealed class FbConfiguration : FbService
	{
		public FbConfiguration(string connectionString = null)
			: base(connectionString)
		{ }

		public void SetSqlDialect(int sqlDialect) => SetSqlDialectImpl(sqlDialect, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task SetSqlDialectAsync(int sqlDialect, CancellationToken cancellationToken = default) => SetSqlDialectImpl(sqlDialect, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task SetSqlDialectImpl(int sqlDialect, AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_properties);
			startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
			startSpb.Append(IscCodes.isc_spb_prp_set_sql_dialect, sqlDialect);
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public void SetSweepInterval(int sweepInterval) => SetSweepIntervalImpl(sweepInterval, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task SetSweepIntervalAsync(int sweepInterval, CancellationToken cancellationToken = default) => SetSweepIntervalImpl(sweepInterval, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task SetSweepIntervalImpl(int sweepInterval, AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_properties);
			startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
			startSpb.Append(IscCodes.isc_spb_prp_sweep_interval, sweepInterval);
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public void SetPageBuffers(int pageBuffers) => SetPageBuffersImpl(pageBuffers, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task SetPageBuffersAsync(int pageBuffers, CancellationToken cancellationToken = default) => SetPageBuffersImpl(pageBuffers, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task SetPageBuffersImpl(int pageBuffers, AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_properties);
			startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
			startSpb.Append(IscCodes.isc_spb_prp_page_buffers, pageBuffers);
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public void DatabaseShutdown(FbShutdownMode mode, int seconds) => DatabaseShutdownImpl(mode, seconds, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task DatabaseShutdownAsync(FbShutdownMode mode, int seconds, CancellationToken cancellationToken = default) => DatabaseShutdownImpl(mode, seconds, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task DatabaseShutdownImpl(FbShutdownMode mode, int seconds, AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_properties);
			startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
			switch (mode)
			{
				case FbShutdownMode.Forced:
					startSpb.Append(IscCodes.isc_spb_prp_shutdown_db, seconds);
					break;
				case FbShutdownMode.DenyTransaction:
					startSpb.Append(IscCodes.isc_spb_prp_deny_new_transactions, seconds);
					break;
				case FbShutdownMode.DenyConnection:
					startSpb.Append(IscCodes.isc_spb_prp_deny_new_attachments, seconds);
					break;
			}
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public void DatabaseShutdown2(FbShutdownOnlineMode mode, FbShutdownType type, int seconds) => DatabaseShutdown2Impl(mode, type, seconds, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task DatabaseShutdown2Async(FbShutdownOnlineMode mode, FbShutdownType type, int seconds, CancellationToken cancellationToken = default) => DatabaseShutdown2Impl(mode, type, seconds, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task DatabaseShutdown2Impl(FbShutdownOnlineMode mode, FbShutdownType type, int seconds, AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_properties);
			startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
			startSpb.Append(IscCodes.isc_spb_prp_shutdown_mode, FbShutdownOnlineModeToIscCode(mode));
			switch (type)
			{
				case FbShutdownType.ForceShutdown:
					startSpb.Append(IscCodes.isc_spb_prp_force_shutdown, seconds);
					break;
				case FbShutdownType.AttachmentsShutdown:
					startSpb.Append(IscCodes.isc_spb_prp_attachments_shutdown, seconds);
					break;
				case FbShutdownType.TransactionsShutdown:
					startSpb.Append(IscCodes.isc_spb_prp_transactions_shutdown, seconds);
					break;
			}
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public void DatabaseOnline() => DatabaseOnlineImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task DatabaseOnlineAsync(CancellationToken cancellationToken = default) => DatabaseOnlineImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task DatabaseOnlineImpl(AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_properties);
			startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
			startSpb.Append(IscCodes.isc_spb_options, IscCodes.isc_spb_prp_db_online);
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public void DatabaseOnline2(FbShutdownOnlineMode mode) => DatabaseOnline2Impl(mode, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task DatabaseOnline2Async(FbShutdownOnlineMode mode, CancellationToken cancellationToken = default) => DatabaseOnline2Impl(mode, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task DatabaseOnline2Impl(FbShutdownOnlineMode mode, AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_properties);
			startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
			startSpb.Append(IscCodes.isc_spb_prp_online_mode, FbShutdownOnlineModeToIscCode(mode));
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public void ActivateShadows() => ActivateShadowsImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task ActivateShadowsAsync(CancellationToken cancellationToken = default) => ActivateShadowsImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task ActivateShadowsImpl(AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_properties);
			startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
			startSpb.Append(IscCodes.isc_spb_options, IscCodes.isc_spb_prp_activate);
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public void SetForcedWrites(bool forcedWrites) => SetForcedWritesImpl(forcedWrites, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task SetForcedWritesAsync(bool forcedWrites, CancellationToken cancellationToken = default) => SetForcedWritesImpl(forcedWrites, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task SetForcedWritesImpl(bool forcedWrites, AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_properties);
			startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
			if (forcedWrites)
			{
				startSpb.Append(IscCodes.isc_spb_prp_write_mode, (byte)IscCodes.isc_spb_prp_wm_sync);
			}
			else
			{
				startSpb.Append(IscCodes.isc_spb_prp_write_mode, (byte)IscCodes.isc_spb_prp_wm_async);
			}
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public void SetReserveSpace(bool reserveSpace) => SetReserveSpaceImpl(reserveSpace, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task SetReserveSpaceAsync(bool reserveSpace, CancellationToken cancellationToken = default) => SetReserveSpaceImpl(reserveSpace, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task SetReserveSpaceImpl(bool reserveSpace, AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_properties);
			startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
			if (reserveSpace)
			{
				startSpb.Append(IscCodes.isc_spb_prp_reserve_space, (byte)IscCodes.isc_spb_prp_res);
			}
			else
			{
				startSpb.Append(IscCodes.isc_spb_prp_reserve_space, (byte)IscCodes.isc_spb_prp_res_use_full);
			}
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public void SetAccessMode(bool readOnly) => SetAccessModeImpl(readOnly, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task SetAccessModeAsync(bool readOnly, CancellationToken cancellationToken = default) => SetAccessModeImpl(readOnly, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task SetAccessModeImpl(bool readOnly, AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_properties);
			startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
			startSpb.Append(IscCodes.isc_spb_prp_access_mode, (byte)(readOnly ? IscCodes.isc_spb_prp_am_readonly : IscCodes.isc_spb_prp_am_readwrite));
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public void NoLinger() => NoLingerImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task NoLingerAsync(CancellationToken cancellationToken = default) => NoLingerImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task NoLingerImpl(AsyncWrappingCommonArgs async)
		{
			EnsureDatabase();

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_properties);
			startSpb.Append(IscCodes.isc_spb_dbname, Database, SpbFilenameEncoding);
			startSpb.Append(IscCodes.isc_spb_options, IscCodes.isc_spb_prp_nolinger);
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		private static byte FbShutdownOnlineModeToIscCode(FbShutdownOnlineMode mode)
		{
			return mode switch
			{
				FbShutdownOnlineMode.Normal => IscCodes.isc_spb_prp_sm_normal,
				FbShutdownOnlineMode.Multi => IscCodes.isc_spb_prp_sm_multi,
				FbShutdownOnlineMode.Single => IscCodes.isc_spb_prp_sm_single,
				FbShutdownOnlineMode.Full => IscCodes.isc_spb_prp_sm_full,
				_ => throw new ArgumentOutOfRangeException(nameof(mode)),
			};
		}
	}
}
