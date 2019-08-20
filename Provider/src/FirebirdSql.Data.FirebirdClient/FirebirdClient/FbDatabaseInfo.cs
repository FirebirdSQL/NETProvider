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

//$Authors = Carlos Guzman Alvarez

using System;
using System.Text;
using System.Data;

using FirebirdSql.Data.Common;
using System.Collections.Generic;
using System.Linq;

namespace FirebirdSql.Data.FirebirdClient
{
	public sealed class FbDatabaseInfo
	{
		#region Properties

		public FbConnection Connection { get; set; }

		public string IscVersion => GetValue<string>(IscCodes.isc_info_isc_version);
		public string ServerVersion => GetValue<string>(IscCodes.isc_info_firebird_version);
		public string ServerClass => GetValue<string>(IscCodes.isc_info_db_class);
		public int PageSize => GetValue<int>(IscCodes.isc_info_page_size);
		public int AllocationPages => GetValue<int>(IscCodes.isc_info_allocation);
		public string BaseLevel => GetValue<string>(IscCodes.isc_info_base_level);
		public string DbId => GetValue<string>(IscCodes.isc_info_db_id);
		public string Implementation => GetValue<string>(IscCodes.isc_info_implementation);
		public bool NoReserve => GetValue<bool>(IscCodes.isc_info_no_reserve);
		public int OdsVersion => GetValue<int>(IscCodes.isc_info_ods_version);
		public int OdsMinorVersion => GetValue<int>(IscCodes.isc_info_ods_minor_version);
		public int MaxMemory => GetValue<int>(IscCodes.isc_info_max_memory);
		public int CurrentMemory => GetValue<int>(IscCodes.isc_info_current_memory);
		public bool ForcedWrites => GetValue<bool>(IscCodes.isc_info_forced_writes);
		public int NumBuffers => GetValue<int>(IscCodes.isc_info_num_buffers);
		public int SweepInterval => GetValue<int>(IscCodes.isc_info_sweep_interval);
		public bool ReadOnly => GetValue<bool>(IscCodes.isc_info_db_read_only);
		public int Fetches => GetValue<int>(IscCodes.isc_info_fetches);
		public int Marks => GetValue<int>(IscCodes.isc_info_marks);
		public int Reads => GetValue<int>(IscCodes.isc_info_reads);
		public int Writes => GetValue<int>(IscCodes.isc_info_writes);
		public int BackoutCount => GetValue<int>(IscCodes.isc_info_backout_count);
		public int DeleteCount => GetValue<int>(IscCodes.isc_info_delete_count);
		public int ExpungeCount => GetValue<int>(IscCodes.isc_info_expunge_count);
		public int InsertCount => GetValue<int>(IscCodes.isc_info_insert_count);
		public int PurgeCount => GetValue<int>(IscCodes.isc_info_purge_count);
		public long ReadIdxCount => GetValue<long>(IscCodes.isc_info_read_idx_count);
		public long ReadSeqCount => GetValue<long>(IscCodes.isc_info_read_seq_count);
		public long UpdateCount => GetValue<long>(IscCodes.isc_info_update_count);
		public int DatabaseSizeInPages => GetValue<int>(IscCodes.isc_info_db_size_in_pages);
		public long OldestTransaction => GetValue<long>(IscCodes.isc_info_oldest_transaction);
		public long OldestActiveTransaction => GetValue<long>(IscCodes.isc_info_oldest_active);
		public long OldestActiveSnapshot => GetValue<long>(IscCodes.isc_info_oldest_snapshot);
		public long NextTransaction => GetValue<long>(IscCodes.isc_info_next_transaction);
		public int ActiveTransactions => GetValue<int>(IscCodes.isc_info_active_transactions);
		public List<string> ActiveUsers => GetList<string>(IscCodes.isc_info_user_names);

		#endregion

		#region Constructors

		public FbDatabaseInfo()
		{
		}

		public FbDatabaseInfo(FbConnection connection)
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
			return info.Any() ? (T)Convert.ChangeType(info[0], typeof(T)) : default;
		}

		private List<T> GetList<T>(byte item)
		{
			FbConnection.EnsureOpen(Connection);

			var db = Connection.InnerConnection.Database;
			var items = new byte[]
				{
					item,
					IscCodes.isc_info_end
				};

			return db.GetDatabaseInfo(items).Cast<T>().ToList();
		}

		#endregion
	}
}
