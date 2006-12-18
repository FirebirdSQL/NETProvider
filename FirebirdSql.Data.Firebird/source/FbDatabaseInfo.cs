/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Text;
using System.Data;
using System.Collections;

using FirebirdSql.Data.Firebird.Gds;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/overview/*'/>
	public sealed class FbDatabaseInfo
	{
		#region FIELDS

		private FbConnection connection;

		#endregion

		#region PROPERTIES

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="Connection"]/*'/>
		public FbConnection Connection
		{
			get { return connection; }
			set { connection = value;}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="IscVersion"]/*'/>
		public string IscVersion
		{
			get 
			{ 
				return (string)getItemInfo(GdsCodes.isc_info_isc_version);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ServerVersion"]/*'/>
		public string ServerVersion
		{
			get 
			{ 
				return (string)getItemInfo(GdsCodes.isc_info_firebird_version);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ServerClass"]/*'/>
		public string ServerClass
		{
			get 
			{
				return (string)getItemInfo(GdsCodes.isc_info_db_class);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="PageSize"]/*'/>
		public int PageSize
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_page_size);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="AllocationPages"]/*'/>
		public int AllocationPages
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_allocation);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="BaseLevel"]/*'/>
		public string BaseLevel
		{
			get
			{
				return (string)getItemInfo(GdsCodes.isc_info_base_level);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="DbId"]/*'/>
		public string DbId
		{
			get
			{
				return (string)getItemInfo(GdsCodes.isc_info_db_id);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="Implementation"]/*'/>
		public string Implementation
		{
			get
			{
				return (string)getItemInfo(GdsCodes.isc_info_implementation);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="NoReserve"]/*'/>
		public bool NoReserve
		{
			get
			{
				return (bool)getItemInfo(GdsCodes.isc_info_no_reserve);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="OdsVersion"]/*'/>
		public int OdsVersion
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_ods_version);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="OdsMinorVersion"]/*'/>
		public int OdsMinorVersion
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_ods_minor_version);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="MaxMemory"]/*'/>
		public int MaxMemory
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_max_memory);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="CurrentMemory"]/*'/>
		public int CurrentMemory
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_current_memory);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ForcedWrites"]/*'/>
		public bool ForcedWrites
		{
			get
			{
				return (bool)getItemInfo(GdsCodes.isc_info_forced_writes);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="NumBuffers"]/*'/>
		public int NumBuffers
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_num_buffers);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="SweepInterval"]/*'/>
		public int SweepInterval
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_sweep_interval);
			}			
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ReadOnly"]/*'/>
		public bool ReadOnly
		{
			get
			{				
				return (bool)getItemInfo(GdsCodes.isc_info_db_read_only);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="Fetches"]/*'/>
		public int Fetches
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_fetches);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="Marks"]/*'/>
		public int Marks
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_marks);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="Reads"]/*'/>
		public int Reads
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_reads);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="Writes"]/*'/>
		public int Writes
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_writes);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="BackoutCount"]/*'/>
		public int BackoutCount
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_backout_count);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="DeleteCount"]/*'/>
		public int DeleteCount
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_delete_count);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ExpungeCount"]/*'/>
		public int ExpungeCount
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_expunge_count);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="InsertCount"]/*'/>
		public int InsertCount
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_insert_count);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="PurgeCount"]/*'/>
		public int PurgeCount
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_purge_count);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ReadIdxCount"]/*'/>
		public int ReadIdxCount
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_read_idx_count);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ReadSeqCount"]/*'/>
		public int ReadSeqCount
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_read_seq_count);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="UpdateCount"]/*'/>
		public int UpdateCount
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_update_count);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="DatabaseSizeInPages"]/*'/>
		public int DatabaseSizeInPages
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_db_size_in_pages);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="OldestTransaction"]/*'/>
		public int OldestTransaction
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_oldest_transaction);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="OldestActiveTransaction"]/*'/>
		public int OldestActiveTransaction
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_oldest_active);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="OldestActiveSnapshot"]/*'/>
		public int OldestActiveSnapshot
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_oldest_snapshot);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="NextTransaction"]/*'/>
		public int NextTransaction
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_next_transaction);
			}
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ActiveTransactions"]/*'/>
		public int ActiveTransactions
		{
			get
			{
				return (int)getItemInfo(GdsCodes.isc_info_active_transactions);
			}
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/constructor[@name="ctor"]/*'/>
		public FbDatabaseInfo()
		{
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/constructor[@name="ctor(FbConnection)"]/*'/>
		public FbDatabaseInfo(FbConnection connection)
		{
			this.connection	= connection;
		}

		#endregion

		#region METHODS
		
		private object getItemInfo(byte itemNumber)
		{
			// Response buffer
			byte[] buffer = new byte[1024];
	
			// Database info
			byte[] databaseInfo  = new byte[]
			{
				itemNumber,
				GdsCodes.isc_info_end
			};
			
			checkConnection();			

			GdsDbAttachment db = connection.DbConnection.DB;

			db.GetDatabaseInfo(databaseInfo, buffer.Length, buffer);

			int pos 	= 0;
			int length	= 0;
			int type	= 0;
			while ((type = buffer[pos++]) != GdsCodes.isc_info_end)
			{
				length 	= db.VaxInteger(buffer, pos, 2);
				pos 	+= 2;
				switch (type) 
				{
					//
					// Database characteristics
					//

					case GdsCodes.isc_info_allocation:
						// Number of database pages allocated
						return db.VaxInteger(buffer, pos, length);

					case GdsCodes.isc_info_base_level:
						/* Database version (level) number:
						 *  		1 byte containing the number 1
						 *  		1 byte containing the version number
						 */
						return buffer[pos].ToString() + "." + buffer[pos + 1].ToString();

					case GdsCodes.isc_info_db_id:
					{
						/* Database file name and site name:
						 *  	• 1 byte containing the number 2
						 *  	• 1 byte containing the length, d, of the database file name in bytes
						 *  	• A string of d bytes, containing the database file name
						 *  	• 1 byte containing the length, l, of the site name in bytes
						 *  	• A string of l bytes, containing the site name
						 */
						string dbFile 	= Encoding.Default.GetString(buffer, pos + 2, buffer[pos + 1]);
						int sitePos 	= pos + 2 + buffer[pos + 1];
						int siteLength	= buffer[sitePos];
						string siteName = Encoding.Default.GetString(buffer, sitePos + 1, siteLength);
						sitePos 		+= siteLength + 1;
						siteLength		= buffer[sitePos];						
						siteName 		+= "." + Encoding.Default.GetString(buffer, sitePos + 1, siteLength);
						
						return siteName + ":" + dbFile;
					}

					case GdsCodes.isc_info_implementation:
						/* Database implementation number:
						 * 		• 1 byte containing a 1
						 *	 	• 1 byte containing the implementation number
						 *  	• 1 byte containing a “class” number, either 1 or 12
						 */
						return 	buffer[pos].ToString() 		+ "." + 
								buffer[pos + 1].ToString() 	+ "." + 
								buffer[pos + 2].ToString();

					case GdsCodes.isc_info_no_reserve:
						/* 0 or 1
						 * 		• 0 indicates space is reserved on each database page for holding
						 * 			backup versions of modified records [Default]
						 * 		• 1 indicates no space is reserved for such records
						 */
						return buffer[pos] == 1 ? true : false;

					case GdsCodes.isc_info_ods_version:
						/* ODS major version number
						 * 		• Databases with different major version numbers have different
						 * 			physical layouts; a database engine can only access databases
						 * 			with a particular ODS major version number
						 * 		• Trying to attach to a database with a different ODS number
						 * 			results in an error
						 */
						 return db.VaxInteger(buffer, pos, length);

					case GdsCodes.isc_info_ods_minor_version:
						/* On-disk structure (ODS) minor version number; an increase in a
						 * minor version number indicates a non-structural change, one that
						 * still allows the database to be accessed by database engines with
						 * the same major version number but possibly different minor
						 * version numbers
						 */
						return db.VaxInteger(buffer, pos, length);

					case GdsCodes.isc_info_page_size:
						/* Number of bytes per page of the attached database; use with
						 * isc_info_allocation to determine the size of the database
						 */
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_isc_version:
					{
						/* Version identification string of the database implementation:
						 * 		• 1 byte containing the number 1
						 * 		• 1 byte specifying the length, n, of the following string
						 * 		• n bytes containing the version identification string
						 */						
						int len = buffer[pos + 1];
						return Encoding.Default.GetString(buffer, pos + 2, len);
					}

					//
					// Environmental characteristics
					//

					case GdsCodes.isc_info_current_memory:
						// Amount of server memory (in bytes) currently in use
						return db.VaxInteger(buffer, pos, length);

					case GdsCodes.isc_info_forced_writes:
						/* Number specifying the mode in which database writes are performed
						 * (0 for asynchronous, 1 for synchronous)
						 */
						 return buffer[pos] == 1 ? true : false;
						 
					case GdsCodes.isc_info_max_memory:
						/* Maximum amount of memory (in bytes) used at one time since the first
						 * process attached to the database
						 */
						return db.VaxInteger(buffer, pos, length);

					case GdsCodes.isc_info_num_buffers:
						// Number of memory buffers currently allocated
						return db.VaxInteger(buffer, pos, length);

					case GdsCodes.isc_info_sweep_interval:
						/* Number of transactions that are committed between “sweeps” to
						 * remove database record versions that are no longer needed
						 */
						return db.VaxInteger(buffer, pos, length);

					//
					// Performance statistics
					//

					case GdsCodes.isc_info_fetches:
						// Number of reads from the memory buffer cache
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_marks:
						// Number of writes to the memory buffer cache
						return db.VaxInteger(buffer, pos, length);
													
					case GdsCodes.isc_info_reads:
						// Number of page reads
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_writes:
						// Number of page writes
						return db.VaxInteger(buffer, pos, length);

					//
					// Database operation counts
					//

					case GdsCodes.isc_info_backout_count:
						// Number of removals of a version of a record
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_delete_count:
						// Number of database deletes since the database was last attached
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_expunge_count:
						/* Number of removals of a record and all of its ancestors, for records
						 * whose deletions have been committed
						 */
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_insert_count:
						// Number of inserts into the database since the database was last attached 
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_purge_count:
						// Number of removals of old versions of fully mature records
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_read_idx_count:
						// Number of reads done via an index since the database was last attached
						return db.VaxInteger(buffer, pos, length);

					case GdsCodes.isc_info_read_seq_count:
						/* Number of sequential sequential table scans (row reads) done on each 
						 * table since the database was last attached
						 */
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_update_count:
						// Number of database updates since the database was last attached
						return db.VaxInteger(buffer, pos, length);
					
					//
					// Misc
					//

					case GdsCodes.isc_info_firebird_version:
					{						
						int len = buffer[pos + 1];
						return Encoding.Default.GetString(buffer, pos + 2, len);
					}

					case GdsCodes.isc_info_db_class:
					{
						int serverClass = db.VaxInteger(buffer, pos, length);
						if (serverClass == GdsCodes.isc_info_db_class_classic_access)
						{
							return "CLASSIC SERVER";
						}
						else
						{
							return "SUPER SERVER";
						}
						
					}
											
					case GdsCodes.isc_info_db_read_only:
						return buffer[pos] == 1 ? true : false;

					case GdsCodes.isc_info_db_size_in_pages:
						// Database size in pages.
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_oldest_transaction:
						// Number of oldest transaction
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_oldest_active:
						// Number of oldest active transaction
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_oldest_snapshot:
						// Number of oldest snapshot transaction
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_next_transaction:
						// Number of next transaction
						return db.VaxInteger(buffer, pos, length);
					
					case GdsCodes.isc_info_active_transactions:
						// Number of active transactions
						return db.VaxInteger(buffer, pos, length);
				}
				
				pos += length;
			}
			
			return 0;
		}

		private void checkConnection()
		{
			if (connection == null || connection.State == ConnectionState.Closed)
			{
				throw new InvalidOperationException("Connection must valid and open");
			}			
		}

		#endregion
	}
}
