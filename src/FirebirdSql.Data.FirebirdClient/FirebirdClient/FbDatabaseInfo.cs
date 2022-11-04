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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.FirebirdClient;

public sealed class FbDatabaseInfo
{
	#region Properties

	public FbConnection Connection { get; set; }

	#endregion

	#region Methods

	public string GetIscVersion()
	{
		return GetValue<string>(IscCodes.isc_info_isc_version);
	}
	public Task<string> GetIscVersionAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<string>(IscCodes.isc_info_isc_version, cancellationToken);
	}

	public string GetServerVersion()
	{
		return GetValue<string>(IscCodes.isc_info_firebird_version);
	}
	public Task<string> GetServerVersionAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<string>(IscCodes.isc_info_firebird_version, cancellationToken);
	}

	public string GetServerClass()
	{
		return GetValue<string>(IscCodes.isc_info_db_class);
	}
	public Task<string> GetServerClassAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<string>(IscCodes.isc_info_db_class, cancellationToken);
	}

	public int GetPageSize()
	{
		return GetValue<int>(IscCodes.isc_info_page_size);
	}
	public Task<int> GetPageSizeAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_page_size, cancellationToken);
	}

	public int GetAllocationPages()
	{
		return GetValue<int>(IscCodes.isc_info_allocation);
	}
	public Task<int> GetAllocationPagesAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_allocation, cancellationToken);
	}

	public string GetBaseLevel()
	{
		return GetValue<string>(IscCodes.isc_info_base_level);
	}
	public Task<string> GetBaseLevelAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<string>(IscCodes.isc_info_base_level, cancellationToken);
	}

	public string GetDbId()
	{
		return GetValue<string>(IscCodes.isc_info_db_id);
	}
	public Task<string> GetDbIdAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<string>(IscCodes.isc_info_db_id, cancellationToken);
	}

	public string GetImplementation()
	{
		return GetValue<string>(IscCodes.isc_info_implementation);
	}
	public Task<string> GetImplementationAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<string>(IscCodes.isc_info_implementation, cancellationToken);
	}

	public bool GetNoReserve()
	{
		return GetValue<bool>(IscCodes.isc_info_no_reserve);
	}
	public Task<bool> GetNoReserveAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<bool>(IscCodes.isc_info_no_reserve, cancellationToken);
	}

	public int GetOdsVersion()
	{
		return GetValue<int>(IscCodes.isc_info_ods_version);
	}
	public Task<int> GetOdsVersionAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_ods_version, cancellationToken);
	}

	public int GetOdsMinorVersion()
	{
		return GetValue<int>(IscCodes.isc_info_ods_minor_version);
	}
	public Task<int> GetOdsMinorVersionAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_ods_minor_version, cancellationToken);
	}

	public int GetMaxMemory()
	{
		return GetValue<int>(IscCodes.isc_info_max_memory);
	}
	public Task<int> GetMaxMemoryAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_max_memory, cancellationToken);
	}

	public int GetCurrentMemory()
	{
		return GetValue<int>(IscCodes.isc_info_current_memory);
	}
	public Task<int> GetCurrentMemoryAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_current_memory, cancellationToken);
	}

	public bool GetForcedWrites()
	{
		return GetValue<bool>(IscCodes.isc_info_forced_writes);
	}
	public Task<bool> GetForcedWritesAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<bool>(IscCodes.isc_info_forced_writes, cancellationToken);
	}

	public int GetNumBuffers()
	{
		return GetValue<int>(IscCodes.isc_info_num_buffers);
	}
	public Task<int> GetNumBuffersAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_num_buffers, cancellationToken);
	}

	public int GetSweepInterval()
	{
		return GetValue<int>(IscCodes.isc_info_sweep_interval);
	}
	public Task<int> GetSweepIntervalAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_sweep_interval, cancellationToken);
	}

	public bool GetReadOnly()
	{
		return GetValue<bool>(IscCodes.isc_info_db_read_only);
	}
	public Task<bool> GetReadOnlyAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<bool>(IscCodes.isc_info_db_read_only, cancellationToken);
	}

	public int GetFetches()
	{
		return GetValue<int>(IscCodes.isc_info_fetches);
	}
	public Task<int> GetFetchesAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_fetches, cancellationToken);
	}

	public int GetMarks()
	{
		return GetValue<int>(IscCodes.isc_info_marks);
	}
	public Task<int> GetMarksAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_marks, cancellationToken);
	}

	public int GetReads()
	{
		return GetValue<int>(IscCodes.isc_info_reads);
	}
	public Task<int> GetReadsAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_reads, cancellationToken);
	}

	public int GetWrites()
	{
		return GetValue<int>(IscCodes.isc_info_writes);
	}
	public Task<int> GetWritesAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_writes, cancellationToken);
	}

	public int GetBackoutCount()
	{
		return GetValue<int>(IscCodes.isc_info_backout_count);
	}
	public Task<int> GetBackoutCountAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_backout_count, cancellationToken);
	}

	public int GetDeleteCount()
	{
		return GetValue<int>(IscCodes.isc_info_delete_count);
	}
	public Task<int> GetDeleteCountAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_delete_count, cancellationToken);
	}

	public int GetExpungeCount()
	{
		return GetValue<int>(IscCodes.isc_info_expunge_count);
	}
	public Task<int> GetExpungeCountAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_expunge_count, cancellationToken);
	}

	public int GetInsertCount()
	{
		return GetValue<int>(IscCodes.isc_info_insert_count);
	}
	public Task<int> GetInsertCountAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_insert_count, cancellationToken);
	}

	public int GetPurgeCount()
	{
		return GetValue<int>(IscCodes.isc_info_purge_count);
	}
	public Task<int> GetPurgeCountAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_purge_count, cancellationToken);
	}

	public long GetReadIdxCount()
	{
		return GetValue<long>(IscCodes.isc_info_read_idx_count);
	}
	public Task<long> GetReadIdxCountAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<long>(IscCodes.isc_info_read_idx_count, cancellationToken);
	}

	public long GetReadSeqCount()
	{
		return GetValue<long>(IscCodes.isc_info_read_seq_count);
	}
	public Task<long> GetReadSeqCountAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<long>(IscCodes.isc_info_read_seq_count, cancellationToken);
	}

	public long GetUpdateCount()
	{
		return GetValue<long>(IscCodes.isc_info_update_count);
	}
	public Task<long> GetUpdateCountAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<long>(IscCodes.isc_info_update_count, cancellationToken);
	}

	public int GetDatabaseSizeInPages()
	{
		return GetValue<int>(IscCodes.isc_info_db_size_in_pages);
	}
	public Task<int> GetDatabaseSizeInPagesAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_db_size_in_pages, cancellationToken);
	}

	public long GetOldestTransaction()
	{
		return GetValue<long>(IscCodes.isc_info_oldest_transaction);
	}
	public Task<long> GetOldestTransactionAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<long>(IscCodes.isc_info_oldest_transaction, cancellationToken);
	}

	public long GetOldestActiveTransaction()
	{
		return GetValue<long>(IscCodes.isc_info_oldest_active);
	}
	public Task<long> GetOldestActiveTransactionAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<long>(IscCodes.isc_info_oldest_active, cancellationToken);
	}

	public long GetOldestActiveSnapshot()
	{
		return GetValue<long>(IscCodes.isc_info_oldest_snapshot);
	}
	public Task<long> GetOldestActiveSnapshotAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<long>(IscCodes.isc_info_oldest_snapshot, cancellationToken);
	}

	public long GetNextTransaction()
	{
		return GetValue<long>(IscCodes.isc_info_next_transaction);
	}
	public Task<long> GetNextTransactionAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<long>(IscCodes.isc_info_next_transaction, cancellationToken);
	}

	public List<long> GetActiveTransactions()
	{
		return GetList<long>(IscCodes.isc_info_active_transactions);
	}
	public Task<List<long>> GetActiveTransactionsAsync(CancellationToken cancellationToken = default)
	{
		return GetListAsync<long>(IscCodes.isc_info_active_transactions, cancellationToken);
	}

	public int GetActiveTransactionsCount()
	{
		return GetValue<int>(IscCodes.isc_info_active_tran_count);
	}
	public Task<int> GetActiveTransactionsCountAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.isc_info_active_tran_count, cancellationToken);
	}

	public List<string> GetActiveUsers()
	{
		return GetList<string>(IscCodes.isc_info_user_names);
	}
	public Task<List<string>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
	{
		return GetListAsync<string>(IscCodes.isc_info_user_names, cancellationToken);
	}

	public string GetWireCrypt()
	{
		return GetValue<string>(IscCodes.fb_info_wire_crypt);
	}
	public Task<string> GetWireCryptAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<string>(IscCodes.fb_info_wire_crypt, cancellationToken);
	}

	public string GetCryptPlugin()
	{
		return GetValue<string>(IscCodes.fb_info_crypt_plugin);
	}
	public Task<string> GetCryptPluginAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<string>(IscCodes.fb_info_crypt_plugin, cancellationToken);
	}

	public DateTime GetCreationDate()
	{
		return GetValue<DateTime>(IscCodes.isc_info_creation_date);
	}
	public Task<DateTime> GetCreationDateAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<DateTime>(IscCodes.isc_info_creation_date, cancellationToken);
	}

	public long GetNextAttachment()
	{
		return GetValue<long>(IscCodes.fb_info_next_attachment);
	}
	public Task<long> GetNextAttachmentAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<long>(IscCodes.fb_info_next_attachment, cancellationToken);
	}

	public long GetNextStatement()
	{
		return GetValue<long>(IscCodes.fb_info_next_statement);
	}
	public Task<long> GetNextStatementAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<long>(IscCodes.fb_info_next_statement, cancellationToken);
	}

	public string GetReplicaMode()
	{
		return GetValue<string>(IscCodes.fb_info_replica_mode);
	}
	public Task<string> GetReplicaModeAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<string>(IscCodes.fb_info_replica_mode, cancellationToken);
	}

	public string GetDbFileId()
	{
		return GetValue<string>(IscCodes.fb_info_db_file_id);
	}
	public Task<string> GetDbFileIdAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<string>(IscCodes.fb_info_db_file_id, cancellationToken);
	}

	public Guid GetDbGuid()
	{
		return GetValue<Guid>(IscCodes.fb_info_db_guid);
	}
	public Task<Guid> GetDbGuidAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<Guid>(IscCodes.fb_info_db_guid, cancellationToken);
	}

	public FbZonedDateTime GetCreationTimestamp()
	{
		return GetValue<FbZonedDateTime>(IscCodes.fb_info_creation_timestamp_tz);
	}
	public Task<FbZonedDateTime> GetCreationTimestampAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<FbZonedDateTime>(IscCodes.fb_info_creation_timestamp_tz, cancellationToken);
	}

	public int GetProtocolVersion()
	{
		return GetValue<int>(IscCodes.fb_info_protocol_version);
	}
	public Task<int> GetProtocolVersionAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.fb_info_protocol_version, cancellationToken);
	}

	public int GetStatementTimeoutDatabase()
	{
		return GetValue<int>(IscCodes.fb_info_statement_timeout_db);
	}
	public Task<int> GetStatementTimeoutDatabaseAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.fb_info_statement_timeout_db, cancellationToken);
	}

	public int GetStatementTimeoutAttachment()
	{
		return GetValue<int>(IscCodes.fb_info_statement_timeout_att);
	}
	public Task<int> GetStatementTimeoutAttachmentAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<int>(IscCodes.fb_info_statement_timeout_att, cancellationToken);
	}

	#endregion

	#region Constructors

	public FbDatabaseInfo(FbConnection connection = null)
	{
		Connection = connection;
	}

	#endregion

	#region Private Methods

	private T GetValue<T>(byte item)
	{
		FbConnection.EnsureOpen(Connection);

		var items = new byte[]
		{
			item,
			IscCodes.isc_info_end
		};
		var info = Connection.InnerConnection.Database.GetDatabaseInfo(items);
		return info.Any() ? InfoValuesHelper.ConvertValue<T>(info[0]) : default;
	}
	private async Task<T> GetValueAsync<T>(byte item, CancellationToken cancellationToken = default)
	{
		FbConnection.EnsureOpen(Connection);

		var items = new byte[]
		{
			item,
			IscCodes.isc_info_end
		};
		var info = await Connection.InnerConnection.Database.GetDatabaseInfoAsync(items, cancellationToken).ConfigureAwait(false);
		return info.Any() ? InfoValuesHelper.ConvertValue<T>(info[0]) : default;
	}

	private List<T> GetList<T>(byte item)
	{
		FbConnection.EnsureOpen(Connection);

		var items = new byte[]
		{
			item,
			IscCodes.isc_info_end
		};

		return (Connection.InnerConnection.Database.GetDatabaseInfo(items)).Select(InfoValuesHelper.ConvertValue<T>).ToList();
	}
	private async Task<List<T>> GetListAsync<T>(byte item, CancellationToken cancellationToken = default)
	{
		FbConnection.EnsureOpen(Connection);

		var items = new byte[]
		{
			item,
			IscCodes.isc_info_end
		};

		return (await Connection.InnerConnection.Database.GetDatabaseInfoAsync(items, cancellationToken).ConfigureAwait(false)).Select(InfoValuesHelper.ConvertValue<T>).ToList();
	}

	#endregion
}
