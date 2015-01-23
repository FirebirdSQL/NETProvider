/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Text;
using System.Data;
using System.Collections;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	public sealed class FbDatabaseInfo
	{
		#region Fields

		private FbConnection connection;

		#endregion

		#region Properties

		public FbConnection Connection
		{
			get { return this.connection; }
			set { this.connection = value; }
		}

		public string IscVersion
		{
			get { return this.GetString(IscCodes.isc_info_isc_version); }
		}

		public string ServerVersion
		{
			get { return this.GetString(IscCodes.isc_info_firebird_version); }
		}

		public string ServerClass
		{
			get { return this.GetString(IscCodes.isc_info_db_class); }
		}

		public int PageSize
		{
			get { return this.GetInt32(IscCodes.isc_info_page_size); }
		}

		public int AllocationPages
		{
			get { return this.GetInt32(IscCodes.isc_info_allocation); }
		}

		public string BaseLevel
		{
			get { return this.GetString(IscCodes.isc_info_base_level); }
		}

		public string DbId
		{
			get { return this.GetString(IscCodes.isc_info_db_id); }
		}

		public string Implementation
		{
			get { return this.GetString(IscCodes.isc_info_implementation); }
		}

		public bool NoReserve
		{
			get { return this.GetBoolean(IscCodes.isc_info_no_reserve); }
		}

		public int OdsVersion
		{
			get { return this.GetInt32(IscCodes.isc_info_ods_version); }
		}

		public int OdsMinorVersion
		{
			get { return this.GetInt32(IscCodes.isc_info_ods_minor_version); }
		}

		public int MaxMemory
		{
			get { return this.GetInt32(IscCodes.isc_info_max_memory); }
		}

		public int CurrentMemory
		{
			get { return this.GetInt32(IscCodes.isc_info_current_memory); }
		}

		public bool ForcedWrites
		{
			get { return this.GetBoolean(IscCodes.isc_info_forced_writes); }
		}

		public int NumBuffers
		{
			get { return this.GetInt32(IscCodes.isc_info_num_buffers); }
		}

		public int SweepInterval
		{
			get { return this.GetInt32(IscCodes.isc_info_sweep_interval); }
		}

		public bool ReadOnly
		{
			get { return this.GetBoolean(IscCodes.isc_info_db_read_only); }
		}

		public int Fetches
		{
			get { return this.GetInt32(IscCodes.isc_info_fetches); }
		}

		public int Marks
		{
			get { return this.GetInt32(IscCodes.isc_info_marks); }
		}

		public int Reads
		{
			get { return this.GetInt32(IscCodes.isc_info_reads); }
		}

		public int Writes
		{
			get { return this.GetInt32(IscCodes.isc_info_writes); }
		}

		public int BackoutCount
		{
			get { return this.GetInt32(IscCodes.isc_info_backout_count); }
		}

		public int DeleteCount
		{
			get { return this.GetInt32(IscCodes.isc_info_delete_count); }
		}

		public int ExpungeCount
		{
			get { return this.GetInt32(IscCodes.isc_info_expunge_count); }
		}

		public int InsertCount
		{
			get { return this.GetInt32(IscCodes.isc_info_insert_count); }
		}

		public int PurgeCount
		{
			get { return this.GetInt32(IscCodes.isc_info_purge_count); }
		}

		public int ReadIdxCount
		{
			get { return this.GetInt32(IscCodes.isc_info_read_idx_count); }
		}

		public int ReadSeqCount
		{
			get { return this.GetInt32(IscCodes.isc_info_read_seq_count); }
		}

		public int UpdateCount
		{
			get { return this.GetInt32(IscCodes.isc_info_update_count); }
		}

		public int DatabaseSizeInPages
		{
			get { return this.GetInt32(IscCodes.isc_info_db_size_in_pages); }
		}

		public int OldestTransaction
		{
			get { return this.GetInt32(IscCodes.isc_info_oldest_transaction); }
		}

		public int OldestActiveTransaction
		{
			get { return this.GetInt32(IscCodes.isc_info_oldest_active); }
		}

		public int OldestActiveSnapshot
		{
			get { return this.GetInt32(IscCodes.isc_info_oldest_snapshot); }
		}

		public int NextTransaction
		{
			get { return this.GetInt32(IscCodes.isc_info_next_transaction); }
		}

		public int ActiveTransactions
		{
			get { return this.GetInt32(IscCodes.isc_info_active_transactions); }
		}

		public ArrayList ActiveUsers
		{
			get { return this.GetArrayList(IscCodes.isc_info_user_names); }
		}

		#endregion

		#region Constructors

		public FbDatabaseInfo()
		{
		}

		public FbDatabaseInfo(FbConnection connection)
		{
			this.connection = connection;
		}

		#endregion

		#region Private Methods

		private string GetString(byte item)
		{
			this.CheckConnection();

			IDatabase db = this.Connection.InnerConnection.Database;
			byte[] items = new byte[]
				{
					item,
					IscCodes.isc_info_end
				};

			return (string)db.GetDatabaseInfo(items)[0];
		}

		private int GetInt32(byte item)
		{
			this.CheckConnection();

			IDatabase db = this.Connection.InnerConnection.Database;
			byte[] items = new byte[]
				{
					item,
					IscCodes.isc_info_end
				};

			ArrayList info = db.GetDatabaseInfo(items);

			return (info.Count > 0 ? (int)info[0] : 0);
		}

		private bool GetBoolean(byte item)
		{
			this.CheckConnection();

			IDatabase db = this.Connection.InnerConnection.Database;
			byte[] items = new byte[]
				{
					item,
					IscCodes.isc_info_end
				};

			ArrayList info = db.GetDatabaseInfo(items);

			return (info.Count > 0 ? (bool)info[0] : false);
		}

		private ArrayList GetArrayList(byte item)
		{
			this.CheckConnection();

			IDatabase db = this.Connection.InnerConnection.Database;
			byte[] items = new byte[]
				{
					item,
					IscCodes.isc_info_end
				};

			return db.GetDatabaseInfo(items);
		}

		private void CheckConnection()
		{
			if (this.connection == null ||
				this.connection.State == ConnectionState.Closed)
			{
				throw new InvalidOperationException("Connection must be valid and open");
			}
		}

		#endregion
	}
}
