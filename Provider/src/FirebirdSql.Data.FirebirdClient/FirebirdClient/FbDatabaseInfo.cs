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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	public sealed class FbDatabaseInfo
	{
		#region Properties

		public FbConnection Connection { get; set; }

		#endregion

		#region Methods

		public string GetIscVersion() => GetIscVersionImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetIscVersionAsync(CancellationToken cancellationToken) => GetIscVersionImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<string> GetIscVersionImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<string>(IscCodes.isc_info_isc_version, async);
		}

		public string GetServerVersion() => GetServerVersionImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetServerVersionAsync(CancellationToken cancellationToken) => GetServerVersionImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<string> GetServerVersionImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<string>(IscCodes.isc_info_firebird_version, async);
		}

		public string GetServerClass() => GetServerClassImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetServerClassAsync(CancellationToken cancellationToken) => GetServerClassImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<string> GetServerClassImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<string>(IscCodes.isc_info_db_class, async);
		}

		public int GetPageSize() => GetPageSizeImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetPageSizeAsync(CancellationToken cancellationToken) => GetPageSizeImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetPageSizeImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_page_size, async);
		}

		public int GetAllocationPages() => GetAllocationPagesImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetAllocationPagesAsync(CancellationToken cancellationToken) => GetAllocationPagesImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetAllocationPagesImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_allocation, async);
		}

		public string GetBaseLevel() => GetBaseLevelImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetBaseLevelAsync(CancellationToken cancellationToken) => GetBaseLevelImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<string> GetBaseLevelImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<string>(IscCodes.isc_info_base_level, async);
		}

		public string GetDbId() => GetDbIdImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetDbIdAsync(CancellationToken cancellationToken) => GetDbIdImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<string> GetDbIdImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<string>(IscCodes.isc_info_db_id, async);
		}

		public string GetImplementation() => GetImplementationImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetImplementationAsync(CancellationToken cancellationToken) => GetImplementationImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<string> GetImplementationImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<string>(IscCodes.isc_info_implementation, async);
		}

		public bool GetNoReserve() => GetNoReserveImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<bool> GetNoReserveAsync(CancellationToken cancellationToken) => GetNoReserveImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<bool> GetNoReserveImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<bool>(IscCodes.isc_info_no_reserve, async);
		}

		public int GetOdsVersion() => GetOdsVersionImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetOdsVersionAsync(CancellationToken cancellationToken) => GetOdsVersionImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetOdsVersionImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_ods_version, async);
		}

		public int GetOdsMinorVersion() => GetOdsMinorVersionImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetOdsMinorVersionAsync(CancellationToken cancellationToken) => GetOdsMinorVersionImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetOdsMinorVersionImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_ods_minor_version, async);
		}

		public int GetMaxMemory() => GetMaxMemoryImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetMaxMemoryAsync(CancellationToken cancellationToken) => GetMaxMemoryImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetMaxMemoryImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_max_memory, async);
		}

		public int GetCurrentMemory() => GetCurrentMemoryImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetCurrentMemoryAsync(CancellationToken cancellationToken) => GetCurrentMemoryImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetCurrentMemoryImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_current_memory, async);
		}

		public bool GetForcedWrites() => GetForcedWritesImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<bool> GetForcedWritesAsync(CancellationToken cancellationToken) => GetForcedWritesImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<bool> GetForcedWritesImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<bool>(IscCodes.isc_info_forced_writes, async);
		}

		public int GetNumBuffers() => GetNumBuffersImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetNumBuffersAsync(CancellationToken cancellationToken) => GetNumBuffersImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetNumBuffersImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_num_buffers, async);
		}

		public int GetSweepInterval() => GetSweepIntervalImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetSweepIntervalAsync(CancellationToken cancellationToken) => GetSweepIntervalImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetSweepIntervalImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_sweep_interval, async);
		}

		public bool GetReadOnly() => GetReadOnlyImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<bool> GetReadOnlyAsync(CancellationToken cancellationToken) => GetReadOnlyImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<bool> GetReadOnlyImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<bool>(IscCodes.isc_info_db_read_only, async);
		}

		public int GetFetches() => GetFetchesImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetFetchesAsync(CancellationToken cancellationToken) => GetFetchesImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetFetchesImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_fetches, async);
		}

		public int GetMarks() => GetMarksImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetMarksAsync(CancellationToken cancellationToken) => GetMarksImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetMarksImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_marks, async);
		}

		public int GetReads() => GetReadsImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetReadsAsync(CancellationToken cancellationToken) => GetReadsImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetReadsImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_reads, async);
		}

		public int GetWrites() => GetWritesImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetWritesAsync(CancellationToken cancellationToken) => GetWritesImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetWritesImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_writes, async);
		}

		public int GetBackoutCount() => GetBackoutCountImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetBackoutCountAsync(CancellationToken cancellationToken) => GetBackoutCountImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetBackoutCountImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_backout_count, async);
		}

		public int GetDeleteCount() => GetDeleteCountImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetDeleteCountAsync(CancellationToken cancellationToken) => GetDeleteCountImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetDeleteCountImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_delete_count, async);
		}

		public int GetExpungeCount() => GetExpungeCountImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetExpungeCountAsync(CancellationToken cancellationToken) => GetExpungeCountImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetExpungeCountImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_expunge_count, async);
		}

		public int GetInsertCount() => GetInsertCountImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetInsertCountAsync(CancellationToken cancellationToken) => GetInsertCountImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetInsertCountImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_insert_count, async);
		}

		public int GetPurgeCount() => GetPurgeCountImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetPurgeCountAsync(CancellationToken cancellationToken) => GetPurgeCountImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetPurgeCountImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_purge_count, async);
		}

		public long GetReadIdxCount() => GetReadIdxCountImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<long> GetReadIdxCountAsync(CancellationToken cancellationToken) => GetReadIdxCountImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<long> GetReadIdxCountImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<long>(IscCodes.isc_info_read_idx_count, async);
		}

		public long GetReadSeqCount() => GetReadSeqCountImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<long> GetReadSeqCountAsync(CancellationToken cancellationToken) => GetReadSeqCountImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<long> GetReadSeqCountImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<long>(IscCodes.isc_info_read_seq_count, async);
		}

		public long GetUpdateCount() => GetUpdateCountImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<long> GetUpdateCountAsync(CancellationToken cancellationToken) => GetUpdateCountImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<long> GetUpdateCountImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<long>(IscCodes.isc_info_update_count, async);
		}

		public int GetDatabaseSizeInPages() => GetDatabaseSizeInPagesImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetDatabaseSizeInPagesAsync(CancellationToken cancellationToken) => GetDatabaseSizeInPagesImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetDatabaseSizeInPagesImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_db_size_in_pages, async);
		}

		public long GetOldestTransaction() => GetOldestTransactionImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<long> GetOldestTransactionAsync(CancellationToken cancellationToken) => GetOldestTransactionImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<long> GetOldestTransactionImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<long>(IscCodes.isc_info_oldest_transaction, async);
		}

		public long GetOldestActiveTransaction() => GetOldestActiveTransactionImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<long> GetOldestActiveTransactionAsync(CancellationToken cancellationToken) => GetOldestActiveTransactionImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<long> GetOldestActiveTransactionImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<long>(IscCodes.isc_info_oldest_active, async);
		}

		public long GetOldestActiveSnapshot() => GetOldestActiveSnapshotImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<long> GetOldestActiveSnapshotAsync(CancellationToken cancellationToken) => GetOldestActiveSnapshotImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<long> GetOldestActiveSnapshotImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<long>(IscCodes.isc_info_oldest_snapshot, async);
		}

		public long GetNextTransaction() => GetNextTransactionImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<long> GetNextTransactionAsync(CancellationToken cancellationToken) => GetNextTransactionImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<long> GetNextTransactionImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<long>(IscCodes.isc_info_next_transaction, async);
		}

		public int GetActiveTransactions() => GetActiveTransactionsImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<int> GetActiveTransactionsAsync(CancellationToken cancellationToken) => GetActiveTransactionsImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<int> GetActiveTransactionsImpl(AsyncWrappingCommonArgs async)
		{
			return GetValue<int>(IscCodes.isc_info_active_transactions, async);
		}

		public List<string> GetActiveUsers() => GetActiveUsersImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<List<string>> GetActiveUsersAsync(CancellationToken cancellationToken) => GetActiveUsersImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private Task<List<string>> GetActiveUsersImpl(AsyncWrappingCommonArgs async)
		{
			return GetList<string>(IscCodes.isc_info_user_names, async);
		}

		#endregion

		#region Constructors

		public FbDatabaseInfo()
		{ }

		public FbDatabaseInfo(FbConnection connection)
		{
			Connection = connection;
		}

		#endregion

		#region Private Methods

		private async Task<T> GetValue<T>(byte item, AsyncWrappingCommonArgs async)
		{
			FbConnection.EnsureOpen(Connection);

			var items = new byte[]
			{
				item,
				IscCodes.isc_info_end
			};
			var info = await Connection.InnerConnection.Database.GetDatabaseInfo(items, async).ConfigureAwait(false);
			return info.Any() ? (T)Convert.ChangeType(info[0], typeof(T)) : default;
		}

		private async Task<List<T>> GetList<T>(byte item, AsyncWrappingCommonArgs async)
		{
			FbConnection.EnsureOpen(Connection);

			var db = Connection.InnerConnection.Database;
			var items = new byte[]
				{
					item,
					IscCodes.isc_info_end
				};

			return (await db.GetDatabaseInfo(items, async).ConfigureAwait(false)).Cast<T>().ToList();
		}

		#endregion
	}
}
