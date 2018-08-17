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
		#region Fields

		private FbConnection _connection;

		#endregion

		#region Properties

		public FbConnection Connection
		{
			get { return _connection; }
			set { _connection = value; }
		}

		public string IscVersion
		{
			get { return GetString(IscCodes.isc_info_isc_version); }
		}

		public string ServerVersion
		{
			get { return GetString(IscCodes.isc_info_firebird_version); }
		}

		public string ServerClass
		{
			get { return GetString(IscCodes.isc_info_db_class); }
		}

		public int PageSize
		{
			get { return GetInt32(IscCodes.isc_info_page_size); }
		}

		public int AllocationPages
		{
			get { return GetInt32(IscCodes.isc_info_allocation); }
		}

		public string BaseLevel
		{
			get { return GetString(IscCodes.isc_info_base_level); }
		}

		public string DbId
		{
			get { return GetString(IscCodes.isc_info_db_id); }
		}

		public string Implementation
		{
			get { return GetString(IscCodes.isc_info_implementation); }
		}

		public bool NoReserve
		{
			get { return GetBoolean(IscCodes.isc_info_no_reserve); }
		}

		public int OdsVersion
		{
			get { return GetInt32(IscCodes.isc_info_ods_version); }
		}

		public int OdsMinorVersion
		{
			get { return GetInt32(IscCodes.isc_info_ods_minor_version); }
		}

		public int MaxMemory
		{
			get { return GetInt32(IscCodes.isc_info_max_memory); }
		}

		public int CurrentMemory
		{
			get { return GetInt32(IscCodes.isc_info_current_memory); }
		}

		public bool ForcedWrites
		{
			get { return GetBoolean(IscCodes.isc_info_forced_writes); }
		}

		public int NumBuffers
		{
			get { return GetInt32(IscCodes.isc_info_num_buffers); }
		}

		public int SweepInterval
		{
			get { return GetInt32(IscCodes.isc_info_sweep_interval); }
		}

		public bool ReadOnly
		{
			get { return GetBoolean(IscCodes.isc_info_db_read_only); }
		}

		public int Fetches
		{
			get { return GetInt32(IscCodes.isc_info_fetches); }
		}

		public int Marks
		{
			get { return GetInt32(IscCodes.isc_info_marks); }
		}

		public int Reads
		{
			get { return GetInt32(IscCodes.isc_info_reads); }
		}

		public int Writes
		{
			get { return GetInt32(IscCodes.isc_info_writes); }
		}

		public int BackoutCount
		{
			get { return GetInt32(IscCodes.isc_info_backout_count); }
		}

		public int DeleteCount
		{
			get { return GetInt32(IscCodes.isc_info_delete_count); }
		}

		public int ExpungeCount
		{
			get { return GetInt32(IscCodes.isc_info_expunge_count); }
		}

		public int InsertCount
		{
			get { return GetInt32(IscCodes.isc_info_insert_count); }
		}

		public int PurgeCount
		{
			get { return GetInt32(IscCodes.isc_info_purge_count); }
		}

		public int ReadIdxCount
		{
			get { return GetInt32(IscCodes.isc_info_read_idx_count); }
		}

		public int ReadSeqCount
		{
			get { return GetInt32(IscCodes.isc_info_read_seq_count); }
		}

		public int UpdateCount
		{
			get { return GetInt32(IscCodes.isc_info_update_count); }
		}

		public int DatabaseSizeInPages
		{
			get { return GetInt32(IscCodes.isc_info_db_size_in_pages); }
		}

		public int OldestTransaction
		{
			get { return GetInt32(IscCodes.isc_info_oldest_transaction); }
		}

		public int OldestActiveTransaction
		{
			get { return GetInt32(IscCodes.isc_info_oldest_active); }
		}

		public int OldestActiveSnapshot
		{
			get { return GetInt32(IscCodes.isc_info_oldest_snapshot); }
		}

		public int NextTransaction
		{
			get { return GetInt32(IscCodes.isc_info_next_transaction); }
		}

		public int ActiveTransactions
		{
			get { return GetInt32(IscCodes.isc_info_active_transactions); }
		}

		public List<string> ActiveUsers
		{
			get { return GetList<string>(IscCodes.isc_info_user_names); }
		}

		#endregion

		#region Constructors

		public FbDatabaseInfo()
		{
		}

		public FbDatabaseInfo(FbConnection connection)
		{
			_connection = connection;
		}

		#endregion

		#region Private Methods

		private string GetString(byte item)
		{
			FbConnection.EnsureOpen(_connection);

			var db = Connection.InnerConnection.Database;
			var items = new byte[]
				{
					item,
					IscCodes.isc_info_end
				};

			return (string)db.GetDatabaseInfo(items)[0];
		}

		private int GetInt32(byte item)
		{
			FbConnection.EnsureOpen(_connection);

			var db = Connection.InnerConnection.Database;
			var items = new byte[]
				{
					item,
					IscCodes.isc_info_end
				};

			var info = db.GetDatabaseInfo(items);

			return (info.Count > 0 ? (int)info[0] : 0);
		}

		private bool GetBoolean(byte item)
		{
			FbConnection.EnsureOpen(_connection);

			var db = Connection.InnerConnection.Database;
			var items = new byte[]
				{
					item,
					IscCodes.isc_info_end
				};

			var info = db.GetDatabaseInfo(items);

			return (info.Count > 0 ? (bool)info[0] : false);
		}

		private List<T> GetList<T>(byte item)
		{
			FbConnection.EnsureOpen(_connection);

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
