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
 * 
 *  This file was originally ported from JayBird <http://firebird.sourceforge.net/>
 */

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

using FirebirdSql.Logging;
using FirebirdSql.Data.INGDS;

namespace FirebirdSql.Data.NGDS
{
	/// <include file='xmldoc/gds.xml' path='doc/member[@name="T:gds"]/*'/>
	internal class GDS : IGDS
	{
		#region CONSTANTS

		/* The protocol is defined blocks, rather than messages, to
		 * separate the protocol from the transport layer.  
		 */
		public const int CONNECT_VERSION2	= 2;

		/* Protocol 4 is protocol 3 plus server management functions */
		public const int PROTOCOL_VERSION3	= 3;
		public const int PROTOCOL_VERSION4	= 4;

		/* Protocol 5 includes support for a d_float data type */
		public const int PROTOCOL_VERSION5	= 5;

		/* Protocol 6 includes support for cancel remote events, blob seek,
		 * and unknown message type 
		 */
		public const int PROTOCOL_VERSION6	= 6;

		/* Protocol 7 includes DSQL support */
		public const int PROTOCOL_VERSION7	= 7;

		/* Protocol 8 includes collapsing first receive into a send, drop database,
		 * DSQL execute 2, DSQL execute immediate 2, DSQL insert, services, and
		 * transact request.
		 */
		public const int PROTOCOL_VERSION8	= 8;

		/* Protocol 9 includes support for SPX32
		 * SPX32 uses WINSOCK instead of Novell SDK
		 * In order to differentiate between the old implementation
		 * of SPX and this one, different PROTOCOL VERSIONS are used 
		 */
		public const int PROTOCOL_VERSION9	= 9;

		/* Protocol 10 includes support for warnings and removes the requirement for
		 * encoding and decoding status codes.*/
		public const int PROTOCOL_VERSION10	= 10;

		// Operation (packet) types
		public const int op_void                = 0;    // Packet has been voided
		public const int op_connect             = 1;    // Connect to remote server
		public const int op_exit                = 2;    // Remote end has exitted
		public const int op_accept              = 3;    // Server accepts connection
		public const int op_reject              = 4;    // Server rejects connection
		public const int op_protocol            = 5;    // Protocol selection
		public const int op_disconnect          = 6;    // Connect is going away
		public const int op_credit              = 7;    // Grant (buffer) credits
		public const int op_continuation        = 8;    // Continuation packet
		public const int op_response            = 9;    // Generic response block

		// Page server operations

		public const int op_open_file           = 10;   // Open file for page service
		public const int op_create_file         = 11;   // Create file for page service
		public const int op_close_file          = 12;   // Close file for page service
		public const int op_read_page           = 13;   // optionally lock and read page
		public const int op_write_page          = 14;   // write page and optionally release lock
		public const int op_lock                = 15;   // sieze lock
		public const int op_convert_lock        = 16;   // convert existing lock
		public const int op_release_lock        = 17;   // release existing lock
		public const int op_blocking            = 18;   // blocking lock message

		// Full context server operations

		public const int op_attach              = 19;   // Attach database
		public const int op_create              = 20;   // Create database
		public const int op_detach              = 21;   // Detach database
		public const int op_compile             = 22;   // Request based operations
		public const int op_start               = 23;
		public const int op_start_and_send      = 24;
		public const int op_send                = 25;
		public const int op_receive             = 26;
		public const int op_unwind              = 27;
		public const int op_release             = 28;

		public const int op_transaction         = 29;   // Transaction operations
		public const int op_commit              = 30;
		public const int op_rollback            = 31;
		public const int op_prepare             = 32;
		public const int op_reconnect           = 33;

		public const int op_create_blob         = 34;   // Blob operations //
		public const int op_open_blob           = 35;
		public const int op_get_segment         = 36;
		public const int op_put_segment         = 37;
		public const int op_cancel_blob         = 38;
		public const int op_close_blob          = 39;

		public const int op_info_database       = 40;   // Information services
		public const int op_info_request        = 41;
		public const int op_info_transaction    = 42;
		public const int op_info_blob           = 43;

		public const int op_batch_segments      = 44;   // Put a bunch of blob segments

		public const int op_mgr_set_affinity    = 45;   // Establish server affinity
		public const int op_mgr_clear_affinity  = 46;   // Break server affinity
		public const int op_mgr_report          = 47;   // Report on server

		public const int op_que_events          = 48;   // Que event notification request
		public const int op_cancel_events       = 49;   // Cancel event notification request
		public const int op_commit_retaining    = 50;   // Commit retaining (what else)
		public const int op_prepare2            = 51;   // Message form of prepare
		public const int op_event               = 52;   // Completed event request (asynchronous)
		public const int op_connect_request     = 53;   // Request to establish connection
		public const int op_aux_connect         = 54;   // Establish auxiliary connection
		public const int op_ddl                 = 55;   // DDL call
		public const int op_open_blob2          = 56;
		public const int op_create_blob2        = 57;
		public const int op_get_slice           = 58;
		public const int op_put_slice           = 59;
		public const int op_slice               = 60;   // Successful response to public const int op_get_slice
		public const int op_seek_blob           = 61;   // Blob seek operation

		// DSQL operations //

		public const int op_allocate_statement  = 62;   // allocate a statment handle
		public const int op_execute             = 63;   // execute a prepared statement
		public const int op_exec_immediate      = 64;   // execute a statement
		public const int op_fetch               = 65;   // fetch a record
		public const int op_fetch_response      = 66;   // response for record fetch
		public const int op_free_statement      = 67;   // free a statement
		public const int op_prepare_statement   = 68;   // prepare a statement
		public const int op_set_cursor          = 69;   // set a cursor name
		public const int op_info_sql            = 70;

		public const int op_dummy               = 71;   // dummy packet to detect loss of client

		public const int op_response_piggyback  = 72;   // response block for piggybacked messages
		public const int op_start_and_receive   = 73;
		public const int op_start_send_and_receive  = 74;

		public const int op_exec_immediate2     = 75;   // execute an immediate statement with msgs
		public const int op_execute2            = 76;   // execute a statement with msgs
		public const int op_insert              = 77;
		public const int op_sql_response        = 78;   // response from execute; exec immed; insert

		public const int op_transact            = 79;
		public const int op_transact_response   = 80;
		public const int op_drop_database       = 81;

		public const int op_service_attach      = 82;
		public const int op_service_detach      = 83;
		public const int op_service_info        = 84;
		public const int op_service_start       = 85;

		public const int op_rollback_retaining  = 86;

		public const int MAX_BUFFER_SIZE	= 1024;
		public const int MAX_FETCH_ROWS		= 200;

		#endregion

		#region FIELDS

		// Log
		private Log4CSharp log = null;

		#endregion

		#region CONSTRUCTORS
	    
		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public GDS() 
		{			
			#if (_DEBUG)
				log = new Log4CSharp(GetType(), "fbprov.log", Mode.APPEND);
			#endif
		}

		#endregion

		#region DATABASE_METHODS

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_create_database(System.String,FirebirdSql.Data.INGDS.isc_db_handle,FirebirdSql.Data.INGDS.IClumplet)"]/*'/>
		public void isc_create_database(string file_name,
									isc_db_handle db_handle,
									IClumplet c)
		{
			isc_db_handle_impl db = (isc_db_handle_impl) db_handle;

			if (db == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_db_handle);
			}

			lock (db) 
			{
				DbAttachInfo dbai = new DbAttachInfo(file_name);
				connect(db, dbai);
				try 
				{
					if (log != null) log.Debug("op_create ");

					db.Output.WriteInt(op_create);
					db.Output.WriteInt(0);				// packet->p_atch->p_atch_database
					db.Output.WriteString(dbai.FileName);
					db.Output.WriteTyped(GdsCodes.isc_dpb_version1, (IXdrable)c);
					db.Output.Flush();

					if (log != null) log.Debug("sent");

					try 
					{
						Response r = receiveResponse(db);
						db.Rdb_id = r.resp_object;
					} 
					catch (GDSException g) 
					{
						disconnect(db);
						throw g;
					}
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_write_err);
				}
			}
		}

		public void isc_attach_database(string host,
										int port,
										string file_name,
										isc_db_handle db_handle,
										IClumplet dpb)
		{
			try
			{
				DbAttachInfo dbai = new DbAttachInfo(host, port, file_name);
				isc_attach_database(dbai, db_handle, dpb);
			}
			catch(GDSException ge)
			{
				throw ge;
			}
		}

		public void isc_attach_database(string connectstring,
									isc_db_handle db_handle,
									IClumplet dpb)
		{
			try
			{
				DbAttachInfo dbai = new DbAttachInfo(connectstring);
				isc_attach_database(dbai, db_handle, dpb);
			}
			catch(GDSException ge)
			{
				throw ge;
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_attach_database(DbAttachInfo,FirebirdSql.Data.INGDS.isc_db_handle,FirebirdSql.Data.INGDS.IClumplet)"]/*'/>
		public void isc_attach_database(DbAttachInfo dbai,
			isc_db_handle db_handle,
			IClumplet dpb)
		{
			isc_db_handle_impl db = (isc_db_handle_impl) db_handle;

			if (db == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_db_handle);
			}

			lock (db) 
			{
				if (log != null) log.Debug("op_attach ");
				connect(db, dbai);
				try
				{
					db.Output.WriteInt(op_attach);
					db.Output.WriteInt(0);                // packet->p_atch->p_atch_database
					db.Output.WriteString(dbai.FileName);
					db.Output.WriteTyped(GdsCodes.isc_dpb_version1, (IXdrable)dpb);
					db.Output.Flush();
					if (log != null) log.Debug("sent");
					
					try 
					{
						Response r = receiveResponse(db);
						db.Rdb_id = r.resp_object;						
					}
					catch (GDSException ge) 
					{
						disconnect(db);						
						throw ge;
					}
				} 
				catch (IOException)
				{
					throw new GDSException(GdsCodes.isc_net_write_err);
				}
			}			
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_database_info(FirebirdSql.Data.INGDS.isc_db_handle,System.Int32,System.Array,System.Int32,System.Array)"]/*'/>
		public void isc_database_info(isc_db_handle db_handle,
									int item_length,
									byte[] items,
									int buffer_length,
									byte[] buffer)
		{
			isc_db_handle_impl db = (isc_db_handle_impl) db_handle;
			if (db == null)
			{
				throw new GDSException(GdsCodes.isc_bad_db_handle);
			}
			
			lock (db) 
			{
				if (log != null) log.Debug("op_database_info ");	
				
				try 
				{					
					// see src/remote/protocol.h for packet definition (p_info struct)					
					db.Output.WriteInt(op_info_database);		//	operation
					db.Output.WriteInt(db.Rdb_id);				//	db_handle
					db.Output.WriteInt(0);						//	incarnation
					db.Output.WriteBuffer(items, item_length);  //	items
					db.Output.WriteInt(buffer_length);			//	result buffer length

					db.Output.Flush();

					if (log != null) log.Debug("sent");

					Response r = receiveResponse(db);

					System.Array.Copy(r.resp_data, 0, buffer, 0, buffer_length);
				}
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_network_error);
				}
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_detach_database(FirebirdSql.Data.INGDS.isc_db_handle)"]/*'/>
		public void isc_detach_database(isc_db_handle db_handle)
		{
			isc_db_handle_impl db = (isc_db_handle_impl) db_handle;
			if (db == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_db_handle);
			}

			lock (db) 
			{
				if (db_handle.HasTransactions()) 
				{
					throw new GDSException(GdsCodes.isc_open_trans, db.TransactionCount());
				}
	        
				try 
				{
					if (log != null) log.Debug("op_detach ");
					db.Output.WriteInt(op_detach);
					db.Output.WriteInt(db.Rdb_id);
					db.Output.Flush();            
					if (log != null) log.Debug("sent");
					receiveResponse(db);					
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_network_error);
				}
				finally
				{
					try 
					{
						disconnect(db);
					}
					catch (IOException) 
					{
						throw new GDSException(GdsCodes.isc_network_error);
					} 
				}
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_drop_database(FirebirdSql.Data.INGDS.isc_db_handle)"]/*'/>
		public void isc_drop_database(isc_db_handle db_handle)
		{
			isc_db_handle_impl db = (isc_db_handle_impl) db_handle;

			if (db == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_db_handle);
			}

			lock (db) 
			{				
				try 
				{
					if (log != null) log.Debug("op_drop_database ");
					db.Output.WriteInt(op_drop_database);
					db.Output.WriteInt(db.Rdb_id);
					db.Output.Flush();            
					if (log != null) log.Debug("sent");
					receiveResponse(db);
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_network_error);
				}
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_expand_dpb(System.Array,System.Int32,System.Int32,System.Array)"]/*'/>
		public byte[] isc_expand_dpb(byte[] dpb, int dpb_length,
			int param, object[] parameters)
		{
			return dpb;
		}

		#endregion

		#region TRANSACTION_METHODS

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_start_transaction(FirebirdSql.Data.INGDS.isc_tr_handle,FirebirdSql.Data.INGDS.isc_db_handle,System.Collections.ArrayList)"]/*'/>
		public void isc_start_transaction(isc_tr_handle tr_handle,
											isc_db_handle db_handle,
											ArrayList tpb)
		{                               

			isc_tr_handle_impl tr = (isc_tr_handle_impl) tr_handle;
			isc_db_handle_impl db = (isc_db_handle_impl) db_handle;

			if (tr_handle == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_trans_handle);
			}

			if (db_handle == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_db_handle);
			}
			
			lock (db) 
			{
				if (tr.State != TxnState.NOTRANSACTION) 
				{
					throw new GDSException(GdsCodes.isc_tra_state);
				}
				tr.State = TxnState.TRANSACTIONSTARTING;

				try {
					if (log != null) log.Debug("op_transaction ");
					db.Output.WriteInt(op_transaction);
					db.Output.WriteInt(db.Rdb_id);
					db.Output.WriteSet(GdsCodes.isc_tpb_version3, tpb);					
					db.Output.Flush();            
					if (log != null) log.Debug("sent");					
					Response r = receiveResponse(db);
					tr.TransactionId = r.resp_object;
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_network_error);
				}
				
				tr.DbHandle = db;
				tr.State    = TxnState.TRANSACTIONSTARTED;				
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_commit_transaction(FirebirdSql.Data.INGDS.isc_tr_handle)"]/*'/>
		public void isc_commit_transaction(isc_tr_handle tr_handle)
		{
			if (tr_handle == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_trans_handle);
			}

			isc_tr_handle_impl tr = (isc_tr_handle_impl) tr_handle;
			isc_db_handle_impl db = (isc_db_handle_impl)tr.DbHandle;

			lock (db) 
			{
				if (tr.State != TxnState.TRANSACTIONSTARTED && tr.State != TxnState.TRANSACTIONPREPARED)				
				{
					throw new GDSException(GdsCodes.isc_tra_state);
				}
				
				tr.State = TxnState.TRANSACTIONCOMMITTING;

				try 
				{					
					if (log != null) 
					{
						log.Debug("op_commit ");
						log.Debug("tr.rtr_id: " + tr.TransactionId);
					}
					db.Output.WriteInt(op_commit);
					db.Output.WriteInt(tr.TransactionId);
					db.Output.Flush();
					if (log != null) log.Debug("sent");
					receiveResponse(db);
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}

				tr.State = TxnState.NOTRANSACTION;
				tr.UnsetDbHandle();
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_commit_retaining(FirebirdSql.Data.INGDS.isc_tr_handle)"]/*'/>
		public void isc_commit_retaining(isc_tr_handle tr_handle)
		{
			isc_tr_handle_impl tr = (isc_tr_handle_impl) tr_handle;
			if (tr == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_trans_handle);
			}
			isc_db_handle_impl db = (isc_db_handle_impl)tr.DbHandle;

			lock (db) 
			{
				if (tr.State != TxnState.TRANSACTIONSTARTED && tr.State != TxnState.TRANSACTIONPREPARED)
				{
					throw new GDSException(GdsCodes.isc_tra_state);
				}
				tr.State = TxnState.TRANSACTIONCOMMITTING;

				try 
				{
					if (log != null) log.Debug("op_commit_retaining ");
					db.Output.WriteInt(op_commit_retaining);
					db.Output.WriteInt(tr.TransactionId);
					db.Output.Flush();
					if (log != null) log.Debug("sent");
					receiveResponse(db);
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
				tr.State = TxnState.TRANSACTIONSTARTED;
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_prepare_transaction(FirebirdSql.Data.INGDS.isc_tr_handle)"]/*'/>
		public void isc_prepare_transaction(isc_tr_handle tr_handle)
		{
			isc_tr_handle_impl tr = (isc_tr_handle_impl) tr_handle;
			if (tr == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_trans_handle);
			}
						
			isc_db_handle_impl db = (isc_db_handle_impl)tr.DbHandle;

			lock (db) 
			{
				if (tr.State != TxnState.TRANSACTIONSTARTED) 
				{
					throw new GDSException(GdsCodes.isc_tra_state);
				}
				tr.State = TxnState.TRANSACTIONPREPARING;
				try 
				{
					if (log != null) log.Debug("op_prepare ");
					db.Output.WriteInt(op_prepare);
					db.Output.WriteInt(tr.TransactionId);
					db.Output.Flush();
					if (log != null) log.Debug("sent");
					receiveResponse(db);
				} 
				catch (IOException)
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
				tr.State = TxnState.TRANSACTIONPREPARED;
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_prepare_transaction2(FirebirdSql.Data.INGDS.isc_tr_handle,System.Array)"]/*'/>
		public void isc_prepare_transaction2(isc_tr_handle tr_handle,
											byte[] bytes)
		{
			isc_tr_handle_impl tr = (isc_tr_handle_impl) tr_handle;
			if (tr == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_trans_handle);
			}
			isc_db_handle_impl db = (isc_db_handle_impl)tr.DbHandle;

			lock (db) 
			{
				if (tr.State != TxnState.TRANSACTIONSTARTED) 
				{
					throw new GDSException(GdsCodes.isc_tra_state);
				}
				tr.State = TxnState.TRANSACTIONPREPARING;
				try 
				{
					if (log != null) log.Debug("op_prepare2 ");
					db.Output.WriteInt(op_prepare2);
					db.Output.WriteInt(tr.TransactionId);
					db.Output.WriteBuffer(bytes, bytes.Length);
					db.Output.Flush();
					if (log != null) log.Debug("sent");
					receiveResponse(db);
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}

				tr.State = TxnState.TRANSACTIONPREPARED;
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_rollback_transaction(FirebirdSql.Data.INGDS.isc_tr_handle)"]/*'/>
		public void isc_rollback_transaction(isc_tr_handle tr_handle)
		{
			isc_tr_handle_impl tr = (isc_tr_handle_impl) tr_handle;
			if (tr == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_trans_handle);
			}

			isc_db_handle_impl db = (isc_db_handle_impl)tr.DbHandle;

			lock (db)
			{
				if (tr.State == TxnState.NOTRANSACTION)
				{
					throw new GDSException(GdsCodes.isc_tra_state);
				}

				tr.State = TxnState.TRANSACTIONROLLINGBACK;

				try 
				{
					if (log != null) log.Debug("op_rollback ");
					db.Output.WriteInt(op_rollback);
					db.Output.WriteInt(tr.TransactionId);
					db.Output.Flush();            
					if (log != null) log.Debug("sent");
					receiveResponse(db);
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
				finally
				{
					tr.State = TxnState.NOTRANSACTION;
					tr.UnsetDbHandle();
				}
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_rollback_retaining(FirebirdSql.Data.INGDS.isc_tr_handle)"]/*'/>
		public void isc_rollback_retaining(isc_tr_handle tr_handle)
		{
			isc_tr_handle_impl tr = (isc_tr_handle_impl) tr_handle;
			if (tr == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_trans_handle);
			}
			isc_db_handle_impl db = (isc_db_handle_impl)tr.DbHandle;

			lock (db) 
			{
				if (tr.State != TxnState.TRANSACTIONSTARTED && tr.State != TxnState.TRANSACTIONPREPARED)
				{
					throw new GDSException(GdsCodes.isc_tra_state);
				}
				tr.State = TxnState.TRANSACTIONROLLINGBACK;

				try 
				{
					if (log != null) log.Debug("op_rollback_retaining ");
					db.Output.WriteInt(op_rollback_retaining);
					db.Output.WriteInt(tr.TransactionId);
					db.Output.Flush();
					if (log != null) log.Debug("sent");
					receiveResponse(db);
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
				tr.State = TxnState.TRANSACTIONSTARTED;
			}
		}

		#endregion

		#region DYNAMIC_SQL_METHODS

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_dsql_allocate_statement(FirebirdSql.Data.INGDS.isc_db_handle,FirebirdSql.Data.INGDS.isc_stmt_handle)"]/*'/>
		public void isc_dsql_allocate_statement(isc_db_handle db_handle,
											isc_stmt_handle stmt_handle)
		{
			isc_db_handle_impl db = (isc_db_handle_impl) db_handle;
			isc_stmt_handle_impl stmt = (isc_stmt_handle_impl) stmt_handle;

			if (db_handle == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_db_handle);
			}

			if (stmt_handle == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_req_handle);
			}

			lock (db) 
			{
				try 
				{
					if (log != null) log.Debug("op_allocate_statement ");
					db.Output.WriteInt(op_allocate_statement);
					db.Output.WriteInt(db.Rdb_id);
					db.Output.Flush();
					if (log != null) log.Debug("sent");
					Response r  = receiveResponse(db);
					stmt.rsr_id = r.resp_object;
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}

				stmt.rsr_rdb = db;
				db.SqlRequest.Add(stmt);
				stmt.allRowsFetched = false;
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_dsql_allocate_statement2(FirebirdSql.Data.INGDS.isc_db_handle,FirebirdSql.Data.INGDS.isc_stmt_handle)"]/*'/>
		public void isc_dsql_alloc_statement2(isc_db_handle db_handle,
											isc_stmt_handle stmt_handle)
		{
			throw new GDSException(GdsCodes.isc_wish_list);
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_dsql_describe(FirebirdSql.Data.INGDS.isc_stmt_handle,System.Int32)"]/*'/>
		public XSQLDA isc_dsql_describe(isc_stmt_handle stmt_handle,
									int da_version)
		{
			byte[] describe_select_info = new byte[] { 
									GdsCodes.isc_info_sql_select,
									GdsCodes.isc_info_sql_describe_vars,
									GdsCodes.isc_info_sql_sqlda_seq,
									GdsCodes.isc_info_sql_type,
									GdsCodes.isc_info_sql_sub_type,
									GdsCodes.isc_info_sql_scale,
									GdsCodes.isc_info_sql_length,
									GdsCodes.isc_info_sql_field,
									GdsCodes.isc_info_sql_relation,
									GdsCodes.isc_info_sql_owner,
									GdsCodes.isc_info_sql_alias,
									GdsCodes.isc_info_sql_describe_end};

			try
			{
				byte[] buffer = isc_dsql_sql_info(stmt_handle, 
										describe_select_info.Length, 
										describe_select_info, MAX_BUFFER_SIZE);

				return parseSqlInfo(stmt_handle, buffer, describe_select_info);
			}
			catch(GDSException ge)
			{
				throw ge;
			}
		}
		
		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_dsql_describe_bind(FirebirdSql.Data.INGDS.isc_stmt_handle,System.Int32)"]/*'/>
		public XSQLDA isc_dsql_describe_bind(isc_stmt_handle stmt_handle,
											int da_version) 
		{
			byte[] describe_bind_info = new byte[] { 
									GdsCodes.isc_info_sql_bind,
									GdsCodes.isc_info_sql_describe_vars,
									GdsCodes.isc_info_sql_sqlda_seq,
									GdsCodes.isc_info_sql_type,
									GdsCodes.isc_info_sql_sub_type,
									GdsCodes.isc_info_sql_scale,
									GdsCodes.isc_info_sql_length,
									GdsCodes.isc_info_sql_field,
									GdsCodes.isc_info_sql_relation,
									GdsCodes.isc_info_sql_owner,
									GdsCodes.isc_info_sql_alias,
									GdsCodes.isc_info_sql_describe_end };

			isc_stmt_handle_impl stmt = (isc_stmt_handle_impl) stmt_handle;

			try
			{		        
				byte[] buffer = isc_dsql_sql_info(stmt_handle,
								describe_bind_info.Length, describe_bind_info,
								MAX_BUFFER_SIZE);
		        
				stmt.in_sqlda = parseSqlInfo(stmt_handle, buffer, describe_bind_info);
			}
			catch(GDSException ge)
			{
				throw ge;
			}

			return stmt.in_sqlda;
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_dsql_execute(FirebirdSql.Data.INGDS.isc_tr_handle,FirebirdSql.Data.INGDS.isc_stmt_handle,System.Int32,FirebirdSql.Data.INGDS.XSQLDA)"]/*'/>
		public void isc_dsql_execute(isc_tr_handle tr_handle,
									isc_stmt_handle stmt_handle,
									int da_version,
									XSQLDA xsqlda)
		{
			try
			{
				isc_dsql_execute2(tr_handle, stmt_handle, da_version,
					xsqlda, null);
			}
			catch(GDSException ge)
			{
				throw ge;
			}
		}
		
		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_dsql_execute2(FirebirdSql.Data.INGDS.isc_tr_handle,FirebirdSql.Data.INGDS.isc_stmt_handle,System.Int32,FirebirdSql.Data.INGDS.XSQLDA,FirebirdSql.Data.INGDS.XSQLDA)"]/*'/>
		public void isc_dsql_execute2(isc_tr_handle tr_handle,
									isc_stmt_handle stmt_handle,
									int da_version,									
									XSQLDA in_xsqlda,
									XSQLDA out_xsqlda)
		{
			isc_tr_handle_impl	 tr		= (isc_tr_handle_impl) tr_handle;
			isc_stmt_handle_impl stmt	= (isc_stmt_handle_impl) stmt_handle;
			isc_db_handle_impl	 db		= stmt.rsr_rdb;

			stmt.ClearRows();
	        
			// Test Handles needed here
			lock (db) 
			{
				try 
				{
					if (log != null) log.Debug((out_xsqlda == null) ? "op_execute " : "op_execute2 ");
					
					if (!isSQLDataOK(in_xsqlda)) 
					{
						throw new GDSException(GdsCodes.isc_dsql_sqlda_value_err);
					}
	                
					db.Output.WriteInt((out_xsqlda == null) ? op_execute : op_execute2);
					db.Output.WriteInt(stmt.rsr_id);
					db.Output.WriteInt(tr.TransactionId);

					writeBLR(db, in_xsqlda);
					db.Output.WriteInt(0);		//message number = in_message_type
					db.Output.WriteInt(((in_xsqlda == null) ? 0 : 1));  //stmt->rsr_bind_format

					if (in_xsqlda != null) 
					{
						writeSQLData(db, in_xsqlda);
					}

					if (out_xsqlda != null) 
					{
						writeBLR(db, out_xsqlda);
						db.Output.WriteInt(0); //out_message_number = out_message_type
					}
					db.Output.Flush();            
					if (log != null) log.Debug("sent");

					if (nextOperation(db) == op_sql_response) 
					{
						//this would be an Execute procedure
						stmt.rows.Add(receiveSqlResponse(db, out_xsqlda));
						stmt.allRowsFetched = true;
						stmt.isSingletonResult = true;
					}
					else 
					{
						stmt.isSingletonResult = false;
					}

					receiveResponse(db);
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_dsql_execute_immediate(FirebirdSql.Data.INGDS.isc_db_handle,FirebirdSql.Data.INGDS.isc_tr_handle,System.String,System.Int32,FirebirdSql.Data.INGDS.XSQLDA)"]/*'/>
		public void isc_dsql_execute_immediate(isc_db_handle db_handle,
											isc_tr_handle tr_handle,
											string statement,
											int dialect,
											XSQLDA xsqlda)
		{
			try
			{
				isc_dsql_exec_immed2(db_handle, tr_handle, statement, dialect, xsqlda, null);
			}
			catch(GDSException ge)
			{
				throw ge;
			}
		}
	    
		public void isc_dsql_execute_immediate(isc_db_handle db_handle,
											isc_tr_handle tr_handle,
											string statement,
											Encoding encoding,
											int dialect,
											XSQLDA xsqlda)
		{
			try
			{
				isc_dsql_exec_immed2(db_handle, tr_handle, statement, encoding, dialect, 
										xsqlda, null);
			}
			catch(GDSException ge)
			{
				throw ge;
			}
		}
		
		public void isc_dsql_exec_immed2(isc_db_handle db_handle,
										isc_tr_handle tr_handle,
										string statement,
										int dialect,
										XSQLDA in_xsqlda,
										XSQLDA out_xsqlda)
		{
			try
			{
				isc_dsql_exec_immed2(db_handle, tr_handle, statement, Encoding.Default, dialect, 
										in_xsqlda, out_xsqlda);
			}
			catch(GDSException ge)
			{
				throw ge;
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_dsql_exec_immed2(FirebirdSql.Data.INGDS.isc_db_handle,FirebirdSql.Data.INGDS.isc_tr_handle,System.String,System.Int32,FirebirdSql.Data.INGDS.XSQLDA,FirebirdSql.Data.INGDS.XSQLDA)"]/*'/>
		public void isc_dsql_exec_immed2(isc_db_handle db_handle,
										isc_tr_handle tr_handle,
										string statement,
										Encoding encoding,
										int dialect,
										XSQLDA in_xsqlda,
										XSQLDA out_xsqlda)
		{
			isc_tr_handle_impl tr = (isc_tr_handle_impl) tr_handle;
			isc_db_handle_impl db = (isc_db_handle_impl) db_handle;

			// Test Handles
			lock (db) 
			{
				try 
				{
					if (!isSQLDataOK(in_xsqlda)) 
					{
						throw new GDSException(GdsCodes.isc_dsql_sqlda_value_err);
					}
	                
					if (in_xsqlda == null && out_xsqlda == null) 
					{
						if (log != null) log.Debug("op_exec_immediate ");
						db.Output.WriteInt(op_exec_immediate);
					} 
					else 
					{
						if (log != null) log.Debug("op_exec_immediate2 ");
						db.Output.WriteInt(op_exec_immediate2);

						writeBLR(db, in_xsqlda);
						db.Output.WriteInt(0);
						db.Output.WriteInt(((in_xsqlda == null) ? 0 : 1));

						if (in_xsqlda != null) 
						{
							writeSQLData(db, in_xsqlda);
						}

						writeBLR(db, out_xsqlda);
						db.Output.WriteInt(0);
					}

					db.Output.WriteInt(tr.TransactionId);
					db.Output.WriteInt(0);
					db.Output.WriteInt(dialect);
					db.Output.WriteString(statement, encoding);
					db.Output.WriteString(String.Empty);
					db.Output.WriteInt(0);
					db.Output.Flush();            
					
					if (log != null) log.Debug("sent");

					if (nextOperation(db) == op_sql_response) 
					{
						receiveSqlResponse(db, out_xsqlda);
					}

					receiveResponse(db);
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_dsql_fetch(FirebirdSql.Data.INGDS.isc_stmt_handle,System.Int32,FirebirdSql.Data.INGDS.XSQLDA)"]/*'/>
		public object[] isc_dsql_fetch(isc_stmt_handle stmt_handle,
								int da_version,
								XSQLDA xsqlda)
		{

			isc_stmt_handle_impl stmt = (isc_stmt_handle_impl) stmt_handle;
			isc_db_handle_impl db = stmt.rsr_rdb;

			if (stmt_handle == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_req_handle);
			}

			if (xsqlda == null) 
			{
				throw new GDSException(GdsCodes.isc_dsql_sqlda_err);
			}

			if (!stmt.allRowsFetched && stmt.rows.Count == 0) 
			{
				//Fetch next batch of rows
				lock (db) 
				{
					try 
					{						
						if (log != null) log.Debug("op_fetch ");
						db.Output.WriteInt(op_fetch);
						db.Output.WriteInt(stmt.rsr_id);
						writeBLR(db, xsqlda);
						db.Output.WriteInt(0);              // p_sqldata_message_number
						db.Output.WriteInt(MAX_FETCH_ROWS); // p_sqldata_messages
						db.Output.Flush();
						if (log != null) log.Debug("sent");

						if (nextOperation(db) == op_fetch_response) 
						{
							int sqldata_status;
							int sqldata_messages;
							do 
							{
								int op = readOperation(db);
								sqldata_status = db.Input.ReadInt();
								sqldata_messages = db.Input.ReadInt();

								if (sqldata_messages > 0 && sqldata_status == 0) 
								{
									stmt.rows.Add(readSQLData(db, xsqlda));
								}

							} while (sqldata_messages > 0 && sqldata_status == 0);

							if (sqldata_status == 100) 
							{
								if (log != null) log.Debug("all rows successfully fetched");
								stmt.allRowsFetched = true;
							}

						}
						else 
						{
							receiveResponse(db);
						}
					} 
					catch (IOException) 
					{
						throw new GDSException(GdsCodes.isc_net_read_err);
					}
				}
			}

			if (stmt.rows.Count > 0) 
			{
				//Return next row from cache.				
				object[] row = (object[])stmt.rows[0];
				
				stmt.rows.RemoveAt(0);
				
				for (int i = 0; i < xsqlda.sqld; i++) 
				{
					xsqlda.sqlvar[i].sqldata = row[i];					
					xsqlda.sqlvar[i].sqlind = (row[i] == null ? -1 : 0);
				}
				
				for (int i = xsqlda.sqld; i < xsqlda.sqln; i++) 
				{
					//is this really necessary?
					xsqlda.sqlvar[i].sqldata = null;
				}

				return row;
			}
			else 
			{
				for (int i = 0; i < xsqlda.sqln; i++) 
				{
					xsqlda.sqlvar[i].sqldata = null;
				}
				
				return null; //no rows fetched
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_dsql_free_statement(FirebirdSql.Data.INGDS.isc_stmt_handle,System.Int32)"]/*'/>
		public void isc_dsql_free_statement(isc_stmt_handle stmt_handle,
			int option)
		{
			isc_stmt_handle_impl stmt = (isc_stmt_handle_impl) stmt_handle;
			isc_db_handle_impl db = stmt.rsr_rdb;

			if (stmt_handle == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_req_handle);
			}

			// Does not seem to be possible or necessary to close
			// an execute procedure statement.
			if (stmt.isSingletonResult && option == GdsCodes.DSQL_close) 
			{
				return;        
			}
	        
			lock (db) 
			{
				try 
				{
					if (!db.IsValid) 
					{
						// too late, socket has been closed
						return;
					}

					if (log != null) log.Debug("op_free_statement ");
					db.Output.WriteInt(op_free_statement);
					db.Output.WriteInt(stmt.rsr_id);
					db.Output.WriteInt(option);
					db.Output.Flush();
					if (log != null) log.Debug("sent");

					receiveResponse(db);
					if (option == GdsCodes.DSQL_drop) 
					{
						stmt.in_sqlda	= null;
						stmt.out_sqlda	= null;
					}
					stmt.ClearRows();
					db.SqlRequest.Remove(stmt);
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
			}
		}

		public XSQLDA isc_dsql_prepare(isc_tr_handle tr_handle,
									isc_stmt_handle stmt_handle,
									string statement,
									int dialect)
		{
			try
			{
				return isc_dsql_prepare(tr_handle, stmt_handle, statement, Encoding.Default, dialect);
			}
			catch(GDSException ge)
			{
				throw ge;
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_dsql_prepare(FirebirdSql.Data.INGDS.isc_tr_handle,FirebirdSql.Data.INGDS.isc_stmt_handle,System.String,System.String,System.Int32)"]/*'/>
		public XSQLDA isc_dsql_prepare(isc_tr_handle tr_handle,
									isc_stmt_handle stmt_handle,
									string statement,
									Encoding encoding,
									int dialect)
		{
			isc_tr_handle_impl tr	  = (isc_tr_handle_impl) tr_handle;
			isc_stmt_handle_impl stmt = (isc_stmt_handle_impl) stmt_handle;
			isc_db_handle_impl db	  = stmt.rsr_rdb;

			if (tr_handle == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_trans_handle);
			}

			if (stmt_handle == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_req_handle);
			}

			// reinitialize stmt SQLDA members.
			stmt.in_sqlda  = null;
			stmt.out_sqlda = null;

			byte[] sql_prepare_info = new byte[] { 
									GdsCodes.isc_info_sql_select,
									GdsCodes.isc_info_sql_describe_vars,
									GdsCodes.isc_info_sql_sqlda_seq,
									GdsCodes.isc_info_sql_type,
									GdsCodes.isc_info_sql_sub_type,
									GdsCodes.isc_info_sql_scale,
									GdsCodes.isc_info_sql_length,
									GdsCodes.isc_info_sql_field,
									GdsCodes.isc_info_sql_relation,
									GdsCodes.isc_info_sql_owner,
									GdsCodes.isc_info_sql_alias,
									GdsCodes.isc_info_sql_describe_end};

			lock (db) 
			{
				try 
				{
					if (log != null) log.Debug("op_prepare_statement {0}", statement);
					db.Output.WriteInt(op_prepare_statement);
					db.Output.WriteInt(tr.TransactionId);
					db.Output.WriteInt(stmt.rsr_id);
					db.Output.WriteInt(dialect);
					db.Output.WriteString(statement, encoding);
					db.Output.WriteBuffer(sql_prepare_info, sql_prepare_info.Length);
					db.Output.WriteInt(MAX_BUFFER_SIZE);
					db.Output.Flush();

					if (log != null) log.Debug("sent");

					Response r = receiveResponse(db);
					stmt.out_sqlda = parseSqlInfo(stmt_handle, r.resp_data, sql_prepare_info);
					return stmt.out_sqlda;
				} 
				catch (IOException)
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}				
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_dsql_set_cursor_name(FirebirdSql.Data.INGDS.isc_stmt_handle,System.String,System.Int32)"]/*'/>
		public void isc_dsql_set_cursor_name(isc_stmt_handle stmt_handle,
											string cursor_name,
											int type)
		{
			isc_stmt_handle_impl stmt = (isc_stmt_handle_impl) stmt_handle;
			isc_db_handle_impl db = stmt.rsr_rdb;

			if (stmt_handle == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_req_handle);
			}

			lock (db) 
			{
				try 
				{
					if (log != null) log.Debug("op_set_cursor ");
					db.Output.WriteInt(op_set_cursor);
					db.Output.WriteInt(stmt.rsr_id);

					byte[] buffer = new byte[cursor_name.Length + 1];
					System.Array.Copy(Encoding.Default.GetBytes(cursor_name), 0,
							buffer, 0, Encoding.Default.GetByteCount(cursor_name));
					buffer[cursor_name.Length] = (byte) 0;

					db.Output.WriteBuffer(buffer, buffer.Length);
					db.Output.WriteInt(0);
					db.Output.Flush();
					if (log != null) log.Debug("sent");

					receiveResponse(db);
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
			}
		}
		
		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_dsql_sql_info(FirebirdSql.Data.INGDS.isc_stmt_handle,System.Int32,System.Array,System.Int32)"]/*'/>
		public byte[] isc_dsql_sql_info(isc_stmt_handle stmt_handle,
									int item_length,
									byte[] items,
									int buffer_length)
		{
			isc_stmt_handle_impl stmt = (isc_stmt_handle_impl) stmt_handle;
			isc_db_handle_impl db = stmt.rsr_rdb;

			lock (db) 
			{
				try 
				{
					if (log != null) log.Debug("op_info_sql ");
					db.Output.WriteInt(op_info_sql);
					db.Output.WriteInt(stmt.rsr_id);
					db.Output.WriteInt(0);
					db.Output.WriteBuffer(items, item_length);
					db.Output.WriteInt(buffer_length);
					db.Output.Flush();
					if (log != null) log.Debug("sent");
					Response r = receiveResponse(db);
					return r.resp_data;
				} 
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_vax_integer(System.Array,system.Int32,system.Int32)"]/*'/>
		public int isc_vax_integer(byte[] buffer, int pos, int length) 
		{
			int value;
			int shift;

			value = shift = 0;

			int i = pos;
			while (--length >= 0) 
			{
				value += (buffer[i++] & 0xff) << shift;
				shift += 8;
			}
			
			return value;
		}

		#endregion

		#region BLOB_METHODS

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_create_blob2(FirebirdSql.Data.INGDS.isc_db_handle,FirebirdSql.Data.INGDS.isc_tr_handle,FirebirdSql.Data.INGDS.isc_blob_handle,FirebirdSql.Data.INGDS.IClumplet)"]/*'/>
		public void isc_create_blob2(isc_db_handle db_handle,
			isc_tr_handle tr_handle,
			isc_blob_handle blob_handle, //contains blob_id
			IClumplet bpb)
		{
			try
			{
				openOrCreateBlob(db_handle, tr_handle, blob_handle, bpb, (bpb == null)? op_create_blob: op_create_blob2);
				((isc_blob_handle_impl)blob_handle).RBLAddValue(GdsCodes.RBL_create);
			}
			catch(GDSException ge)
			{
				throw ge;
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_open_blob2(FirebirdSql.Data.INGDS.isc_db_handle,FirebirdSql.Data.INGDS.isc_tr_handle,FirebirdSql.Data.INGDS.isc_blob_handle,FirebirdSql.Data.INGDS.IClumplet)"]/*'/>
		public void isc_open_blob2(isc_db_handle db_handle,
			isc_tr_handle tr_handle,
			isc_blob_handle blob_handle, //contains blob_id
			IClumplet bpb)
		{
			try
			{
				openOrCreateBlob(db_handle, tr_handle, blob_handle, bpb, (bpb == null)? op_open_blob: op_open_blob2);
			}
			catch(GDSException ge)
			{
				throw ge;
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:openOrCreateBlob(FirebirdSql.Data.INGDS.isc_db_handle,FirebirdSql.Data.INGDS.isc_tr_handle,FirebirdSql.Data.INGDS.isc_blob_handle,FirebirdSql.Data.INGDS.IClumplet,System.Int32)"]/*'/>
		private void openOrCreateBlob(isc_db_handle db_handle,
							isc_tr_handle tr_handle,
							isc_blob_handle blob_handle,		//contains blob_id
							IClumplet bpb,
							int op)
		{
			isc_db_handle_impl db = (isc_db_handle_impl) db_handle;
			isc_tr_handle_impl tr = (isc_tr_handle_impl) tr_handle;
			isc_blob_handle_impl blob = (isc_blob_handle_impl) blob_handle;

			if (db == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_db_handle);
			}
			if (tr == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_trans_handle);
			}
			if (blob == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_segstr_handle);
			}

			lock (db) 
			{
				try 
				{
					if (log != null) 
					{
						log.Debug((bpb == null)? "op_open/create_blob ": "op_open/create_blob2 ");
						log.Debug("op: " + op);
					}
					db.Output.WriteInt(op);
					if (bpb != null) 
					{
						db.Output.WriteTyped(GdsCodes.isc_bpb_version1, (IXdrable)bpb);
					}
					db.Output.WriteInt(tr.TransactionId);	// ??really a short?
					if (log != null) log.Debug("sending blob_id: " + blob.blob_id);
					db.Output.WriteLong(blob.blob_id);
					db.Output.Flush();            

					if (log != null) log.Debug("sent");
					Response r = receiveResponse(db);
					blob.db = db;
					blob.tr = tr;
					blob.rbl_id = r.resp_object;
					blob.blob_id = r.resp_blob_id;
					tr.AddBlob(blob);
				}
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_get_segment(FirebirdSql.Data.INGDS.isc_blob_handle,System.Int32)"]/*'/>
		public byte[] isc_get_segment(isc_blob_handle blob_handle,
				int requested)
		{
			isc_blob_handle_impl blob = (isc_blob_handle_impl) blob_handle;
			isc_db_handle_impl db = blob.db;
			if (db == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_db_handle);
			}
			isc_tr_handle_impl tr = blob.tr;
			if (tr == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_trans_handle);
			}

			lock (db) 
			{
				try 
				{
					if (log != null) log.Debug("op_get_segment ");
					db.Output.WriteInt(op_get_segment);
					db.Output.WriteInt(blob.rbl_id); //short???
					if (log != null) log.Debug("trying to read bytes: " +((requested + 2 < short.MaxValue) ? requested+2: short.MaxValue));
					db.Output.WriteInt((requested + 2 < short.MaxValue) ? requested+2 : short.MaxValue);
					db.Output.WriteInt(0);//writeBuffer for put segment;
					db.Output.Flush();
					if (log != null) log.Debug("sent");
					Response resp = receiveResponse(db);
					blob.RBLRemoveValue(GdsCodes.RBL_segment);
					if (resp.resp_object == 1) 
					{
						blob.RBLAddValue(GdsCodes.RBL_segment);						
					}
					else if (resp.resp_object == 2) {
						blob.RBLAddValue(GdsCodes.RBL_eof_pending);
					}
					byte[] buffer = resp.resp_data;
					if (buffer.Length == 0) 
					{
						// previous segment was last, this has no data
						return buffer;
					}
					int len = 0;
					int srcpos = 0;
					int destpos = 0;
					while (srcpos < buffer.Length) 
					{
						len = isc_vax_integer(buffer, srcpos, 2);
						srcpos	+= 2;
						System.Array.Copy(buffer, srcpos, buffer, destpos, len);
						srcpos	+= len;
						destpos += len;
					}
					byte[] result = new byte[destpos];
					System.Array.Copy(buffer, 0, result, 0, destpos);

					return result;
				}
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_put_segment(FirebirdSql.Data.INGDS.isc_blob_handle,System.Array)"]/*'/>
		public void isc_put_segment(isc_blob_handle blob_handle, byte[] buffer)
		{
			isc_blob_handle_impl blob = (isc_blob_handle_impl) blob_handle;
			isc_db_handle_impl db = blob.db;
			if (db == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_db_handle);
			}
			isc_tr_handle_impl tr = blob.tr;
			if (tr == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_trans_handle);
			}

			lock (db) 
			{
				try 
				{
					if (log != null) log.Debug("op_batch_segments ");
					db.Output.WriteInt(op_batch_segments);
					if (log != null) log.Debug("blob.rbl_id:  " + blob.rbl_id);
					db.Output.WriteInt(blob.rbl_id); //short???
					if (log != null) log.Debug("buffer.Length " + buffer.Length);
					db.Output.WriteBlobBuffer(buffer);
					if (log != null) log.Debug("sent");
					db.Output.Flush();            
					Response resp = receiveResponse(db);
				}
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_close_blob(FirebirdSql.Data.INGDS.isc_blob_handle)"]/*'/>
		public void isc_close_blob(isc_blob_handle blob_handle)
		{
			isc_blob_handle_impl blob = (isc_blob_handle_impl) blob_handle;
			isc_db_handle_impl	 db   = blob.db;
			if (db == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_db_handle);
			}
			isc_tr_handle_impl tr = blob.tr;
			if (tr == null) 
			{
				throw new GDSException(GdsCodes.isc_bad_trans_handle);
			}
			releaseObject(db, op_close_blob, blob.rbl_id);
			tr.RemoveBlob(blob);
		}

		#endregion

		#region ERROR_INFO_METHODS

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:isc_sql_code(System.Int32)"]/*'/>
		public int isc_sql_code(int errcode)
		{
			ushort	code;
			/* SQL code -999 (GENERIC_SQLCODE) is generic, meaning "no other sql code
			 * known".  Now scan the status vector, seeing if there is ANY sqlcode
			 * reported.  Make note of the first error in the status vector who's 
			 * SQLCODE is NOT -999, that will be the return code if there is no specific
			 * sqlerr reported. */
			int	sqlCode = GdsCodes.GENERIC_SQLCODE;	/* error of last resort */
						 
			ushort fac	  = (ushort)SqlCode.GetFacility(errcode);
			ushort class_ = (ushort)SqlCode.GetClass(errcode);

			code = (ushort)SqlCode.GetCode(errcode);

			if ((code < SqlCode.SqlCodes.Length / 2) &&
				(SqlCode.SqlCodes[code] != GdsCodes.GENERIC_SQLCODE))
			{
				sqlCode = SqlCode.SqlCodes[code];				
			}

			return sqlCode;
		}

		#endregion

		#region HANDLE_DECLARATION_METHODS

		public isc_db_handle get_new_isc_db_handle() 
		{
			return new isc_db_handle_impl();
		}

		public isc_tr_handle get_new_isc_tr_handle() 
		{
			return new isc_tr_handle_impl();
		}

		public isc_stmt_handle get_new_isc_stmt_handle() 
		{
			return new isc_stmt_handle_impl();
		}

		public isc_blob_handle get_new_isc_blob_handle() 
		{
			return new isc_blob_handle_impl();
		}

		public static IClumplet NewClumplet(int type, string content) 
		{
			return new Clumplet(type, Encoding.Default.GetBytes(content));
		}

		public static IClumplet NewClumplet(int type)
		{
			return new Clumplet(type, new byte[] {});
		}

		public static IClumplet NewClumplet(int type, int c)
		{
			return new Clumplet(type, new byte[] {(byte)(c>>24), (byte)(c>>16), (byte)(c>>8), (byte)c});
		}

		public static IClumplet NewClumplet(int type, short c)
		{
			return new Clumplet(type, new byte[] {(byte)c, (byte)(c>>8)});
		}

		public static IClumplet NewClumplet(int type, byte[] content) 
		{
			return new Clumplet(type, content);
		}

		public static IClumplet CloneClumplet(IClumplet c) 
		{
			if (c == null) 
			{
				return null;
			}
			return new Clumplet((Clumplet)c);
		}

		#endregion

		#region MISC_METHODS

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:connect(FirebirdSql.Data.NGDS.isc_db_handle_impl,FirebirdSql.Data.NGDS.DbAttachInfo)"]/*'/>
		private void connect(isc_db_handle_impl db,
							DbAttachInfo dbai)
		{
			try 
			{
				try 
				{					
					IPAddress hostadd = Dns.Resolve(dbai.Server).AddressList[0];
					IPEndPoint EPhost = new IPEndPoint(hostadd, dbai.Port);
					db.DbSocket = new Socket(AddressFamily.InterNetwork,
											SocketType.Stream,
											ProtocolType.IP);

					if (log != null) log.Debug("Setting Soket buffers size to : {0}", db.PacketSize);

					// Set Receive Buffer size
					db.DbSocket.SetSocketOption(SocketOptionLevel.Socket,
						SocketOptionName.ReceiveBuffer,
						db.PacketSize);

					// Set Send Buffer size
					db.DbSocket.SetSocketOption(SocketOptionLevel.Socket,
						SocketOptionName.SendBuffer,
						db.PacketSize);

					// Make the socket to connect to the Server
					db.DbSocket.Connect(EPhost);					
					db.DbNetworkStream = new NetworkStream(db.DbSocket, true);					
					
					if (log != null) log.Debug("Got socket");
				} 
				catch (SocketException ex2) 
				{
					string message = "Cannot resolve host " + dbai.Server;
					if (log != null) log.Error(message, ex2);
					throw new GDSException(GdsCodes.isc_arg_gds, GdsCodes.isc_network_error, dbai.Server);
				}

				db.Output = new XdrOutputStream(new BufferedStream(db.DbNetworkStream));
				db.Input  = new XdrInputStream(new BufferedStream(db.DbNetworkStream));
				
				// Here we identify the user to the engine.  This may or may not be used 
				// as login info to a database.				
				string user = Environment.UserName;
				if (log != null) log.Debug("user.name: " + user);
				string host = System.Net.Dns.GetHostName();
				if (log != null) log.Debug("host: " + host);
				
				int n = 0;
				byte[] user_id = new byte[200];
				
				int userLength = Encoding.Default.GetByteCount(user);
				user_id[n++] = 1;		// CNCT_user
				user_id[n++] = (byte)userLength;
				System.Array.Copy(Encoding.Default.GetBytes(user), 0, user_id, n, userLength);
				n += userLength;
				
				int hostLength = Encoding.Default.GetByteCount(host);
				user_id[n++] = 4;		// CNCT_host
				user_id[n++] = (byte)host.Length;
				System.Array.Copy(Encoding.Default.GetBytes(host), 0, user_id, n, hostLength);
				n += hostLength;
	            
				user_id[n++] = 6;		// CNCT_user_verification
				user_id[n++] = 0;

				if (log != null) log.Debug("op_connect");
				db.Output.WriteInt(op_connect);
				db.Output.WriteInt(op_attach);
				db.Output.WriteInt(CONNECT_VERSION2);	// CONNECT_VERSION2
				db.Output.WriteInt(1);						// arch_generic

				db.Output.WriteString(dbai.FileName);		// p_cnct_file
				db.Output.WriteInt(1);						// p_cnct_count
				db.Output.WriteBuffer(user_id, n);			// p_cnct_user_id
				
				db.Output.WriteInt(PROTOCOL_VERSION10);
				db.Output.WriteInt(1);						// arch_generic
				db.Output.WriteInt(2);						// ptype_rpc
				db.Output.WriteInt(3);						// ptype_batch_send
				db.Output.WriteInt(2);
			
				db.Output.Flush();				
				
				if (log != null) 
				{
					log.Debug("sent");
					log.Debug("op_accept");
				}

				if (readOperation(db) == op_accept) 
				{
					db.ProtocolVersion		= db.Input.ReadInt();
					db.ProtocolArchitecture = db.Input.ReadInt();
					db.Input.ReadInt();
					if (log != null) log.Debug("received");
				} 
				else 
				{
					disconnect(db);
					if (log != null) log.Debug("not received");
					throw new GDSException(GdsCodes.isc_connect_reject);
				}
			} 
			catch (IOException ex) 
			{
				if (log != null) log.InfoEx("IOException while trying to connect to db:", ex);
				throw new GDSException(GdsCodes.isc_arg_gds, GdsCodes.isc_network_error, dbai.Server);
			}
		}

		/// <include file='xmldoc/gds.xml' path='doc/member[@name="M:disconnect(FirebirdSql.Data.NGDS.isc_db_handle_impl)"]/*'/>
		private void disconnect(isc_db_handle_impl db)
		{
			try
			{
				if (log != null) log.Info("About to invalidate db handle");
				db.Invalidate();
				if (log != null) log.Info("successfully invalidated db handle");
			}
			catch(IOException ioe)
			{
				throw new GDSException(GdsCodes.isc_arg_gds, GdsCodes.isc_net_read_err, ioe.Message);
			}
		}

		private object[] receiveSqlResponse(isc_db_handle_impl db,
										XSQLDA xsqlda)
		{
			try 
			{
				if (log != null) log.Debug("op_sql_response ");
				if (readOperation(db) == op_sql_response) 
				{
					int messages = db.Input.ReadInt();
					if (log != null) log.Debug("received");
					if (messages > 0) 
					{
						return readSQLData(db, xsqlda);
					}
					else 
					{
						return null;
					}
				} 
				else 
				{
					if (log != null) log.Debug("not received");
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
			} 
			catch (IOException ex) 
			{
				if (log != null) log.WarnEx("IOException in receiveSQLResponse", ex);
				// ex.getMessage() makes little sense here, it will not be displayed
				// because error message for isc_net_read_err does not accept params
				throw new GDSException(GdsCodes.isc_arg_gds, GdsCodes.isc_net_read_err, ex.Message);
			}
		}

		private Response receiveResponse(isc_db_handle_impl db)
		{
			try 
			{
				int op = readOperation(db);
				if (op == op_response) 
				{				
					Response r = new Response();

					r.resp_object = db.Input.ReadInt();
					if (log != null) log.Debug("op_response resp_object: " + r.resp_object);
					r.resp_blob_id = db.Input.ReadLong();
					if (log != null) log.Debug("op_response resp_blob_id: " + r.resp_blob_id);
					r.resp_data = db.Input.ReadBuffer();
					if (log != null) log.Debug("op_response resp_data size: " + r.resp_data.Length);

					readStatusVector(db);
					if (log != null)
					{
						log.Debug("received");
						checkAllRead(db.Input);	//DEBUG
					}
			
					return r;
				} 
				else 
				{
					if (log != null)
					{
						log.Debug("not received: op is " + op);
						checkAllRead(db.Input);
					}

					return null;
				}
			} 
			catch (IOException ex) 
			{
				if (log != null) log.WarnEx("IOException in receiveResponse", ex);
				// ex.getMessage() makes little sense here, it will not be displayed
				// because error message for isc_net_read_err does not accept params
				throw new GDSException(GdsCodes.isc_arg_gds, GdsCodes.isc_net_read_err, ex.Message);
			}
		}

		private int nextOperation(isc_db_handle_impl db)
		{
			do 
			{
				/* loop as long as we are receiving dummy packets, just
				 * throwing them away--note that if we are a server we won't
				 * be receiving them, but it is better to check for them at
				 * this level rather than try to catch them in all places where
				 * this routine is called 
				 */
				db.Op = db.Input.ReadInt();
				if (log != null)
				{
					if (db.Op == op_dummy) 
					{
						log.Debug("op_dummy received");
					}
				}
			} while (db.Op == op_dummy);

			return db.Op;
		}

		private int readOperation(isc_db_handle_impl db)
		{
			int op = (db.Op >= 0) ? db.Op : nextOperation(db);			
			db.Op = -1;

			return op;
		}

		private void readStatusVector(isc_db_handle_impl db)
		{
			try 
			{
				GDSException exception = new GDSException();

				while (true) 
				{
					int arg = db.Input.ReadInt();
					switch (arg) 
					{
						case GdsCodes.isc_arg_gds: 
							int er = db.Input.ReadInt();
							if (log != null) log.Debug("readStatusVector arg: isc_arg_gds int: " + er);
							if (er != 0) 
							{
								exception.Errors.Add(arg, er);
							}
							break;

						case GdsCodes.isc_arg_end:
						{		
							if (exception.Errors.Count != 0 && !exception.IsWarning()) 
							{
								exception.BuildExceptionMessage();
								throw exception;
							}
							else
							{
								if (exception.Errors.Count != 0 && exception.IsWarning())
								{
									exception.BuildExceptionMessage();
									db.AddWarning(exception);
								}
							}
						}
						return;
						
						case GdsCodes.isc_arg_interpreted:						
						case GdsCodes.isc_arg_string:
						{
							string arg_value = db.Input.ReadString();
							if (log != null) log.Debug("readStatusVector string: " + arg_value);
							exception.Errors.Add(arg, arg_value);
						}
						break;
						
						case GdsCodes.isc_arg_number:
						{
							int arg_value = db.Input.ReadInt();
							if (log != null)log.Debug("readStatusVector int: " + arg_value);							
							exception.Errors.Add(arg, arg_value);
						}
						break;
						
						default:
						{
							int e = db.Input.ReadInt();
							if (log != null)log.Debug("readStatusVector int: " + e);
							if (e != 0) 
							{
								exception.Errors.Add(arg, e);
							}
						}
						break;
					}
				}
			}
			catch (IOException ioe)
			{
				/* ioe.getMessage() makes little sense here, it will not be displayed
				 * because error message for isc_net_read_err does not accept params
				 */
				throw new GDSException(GdsCodes.isc_arg_gds, GdsCodes.isc_net_read_err, ioe.Message);
			}
		}

		private void writeBLR(isc_db_handle_impl db,
							XSQLDA xsqlda)
		{
			int blr_len = 0;
			byte[] blr = null;

			if (xsqlda != null) 
			{
				// Determine the BLR length
				blr_len = 8;
				int par_count = 0;
				for (int i = 0; i < xsqlda.sqld; i++) 
				{
					int dtype = xsqlda.sqlvar[i].sqltype & ~1;
					if (dtype == GdsCodes.SQL_VARYING || dtype == GdsCodes.SQL_TEXT) 
					{
						blr_len += 3;
					} 
					else if (dtype == GdsCodes.SQL_SHORT || dtype == GdsCodes.SQL_LONG ||
								dtype == GdsCodes.SQL_INT64 ||
								dtype == GdsCodes.SQL_QUAD ||
								dtype == GdsCodes.SQL_BLOB || dtype == GdsCodes.SQL_ARRAY) {
							blr_len += 2;
						} 
						else 
						{
							blr_len++;
						}
					blr_len += 2;
					par_count += 2;
				}

				blr = new byte[blr_len];

				int n = 0;
				blr[n++] = 5;                   // blr_version5
				blr[n++] = 2;                   // blr_begin
				blr[n++] = 4;                   // blr_message
				blr[n++] = 0;

				blr[n++] = (byte) (par_count & 255);
				blr[n++] = (byte) (par_count >> 8);

				for (int i = 0; i < xsqlda.sqld; i++) 
				{
					int dtype = xsqlda.sqlvar[i].sqltype & ~1;
					int len = xsqlda.sqlvar[i].sqllen;
					if (dtype == GdsCodes.SQL_VARYING) {
						blr[n++] = 37;              // blr_varying
						blr[n++] = (byte) (len & 255);
						blr[n++] = (byte) (len >> 8);
					} else if (dtype == GdsCodes.SQL_TEXT) {
						blr[n++] = 14;              // blr_text
						blr[n++] = (byte) (len & 255);
						blr[n++] = (byte) (len >> 8);
					} else if (dtype == GdsCodes.SQL_DOUBLE) {
						blr[n++] = 27;              // blr_double
					} else if (dtype == GdsCodes.SQL_FLOAT) {
						blr[n++] = 10;              // blr_float
					} else if (dtype == GdsCodes.SQL_D_FLOAT) {
						blr[n++] = 11;              // blr_d_float
					} else if (dtype == GdsCodes.SQL_TYPE_DATE) {
						blr[n++] = 12;              // blr_sql_date
					} else if (dtype == GdsCodes.SQL_TYPE_TIME) {
						blr[n++] = 13;              // blr_sql_time
					} else if (dtype == GdsCodes.SQL_TIMESTAMP) {
						blr[n++] = 35;              // blr_timestamp
					} else if (dtype == GdsCodes.SQL_BLOB) {
						blr[n++] = 9;               // blr_quad
						blr[n++] = 0;
					} else if (dtype == GdsCodes.SQL_ARRAY) {
						blr[n++] = 9;               // blr_quad
						blr[n++] = 0;
					} else if (dtype == GdsCodes.SQL_LONG) {
						blr[n++] = 8;               // blr_long
						blr[n++] = (byte) xsqlda.sqlvar[i].sqlscale;
					} else if (dtype == GdsCodes.SQL_SHORT) {
						blr[n++] = 7;               // blr_short
						blr[n++] = (byte) xsqlda.sqlvar[i].sqlscale;
					} else if (dtype == GdsCodes.SQL_INT64) {
						blr[n++] = 16;              // blr_int64
						blr[n++] = (byte) xsqlda.sqlvar[i].sqlscale;
					} else if (dtype == GdsCodes.SQL_QUAD) {
						blr[n++] = 9;               // blr_quad
						blr[n++] = (byte) xsqlda.sqlvar[i].sqlscale;
					} else {
	//                    return error_dsql_804 (gds__dsql_sqlda_value_err);
					}

					blr[n++] = 7;               // blr_short
					blr[n++] = 0;
				}

				blr[n++] = (byte) 255;          // blr_end
				blr[n++] = 76;                  // blr_eoc
			}

			try 
			{
				db.Output.WriteBuffer(blr, blr_len);
			} 
			catch (IOException) 
			{
				throw new GDSException(GdsCodes.isc_net_write_err);
			}

		}

		private bool isSQLDataOK(XSQLDA xsqlda) 
		{
			if (xsqlda != null) 
			{
				for (int i = 0; i < xsqlda.sqld; i++) 
				{
					if ((xsqlda.sqlvar[i].sqlind != -1) &&
						(xsqlda.sqlvar[i].sqldata == null)) 
					{
						return false;
					}
				}
			}
			return true;
		}

		private void writeSQLData(isc_db_handle_impl db,
								XSQLDA xsqlda)
		{
			// This only works if not (port->port_flags & PORT_symmetric)
			for (int i = 0; i < xsqlda.sqld; i++) 
			{
				writeSQLDatum(db, xsqlda.sqlvar[i]);
			}
		}

		private void writeSQLDatum(isc_db_handle_impl db,
								XSQLVAR xsqlvar)
		{				
			if (log != null) 
			{
				if (db.Output == null) 
				{
					log.Debug("db.Output null in writeSQLDatum");
				}
				if (xsqlvar.sqldata == null) 
				{
					log.Debug("sqldata null in writeSQLDatum: " + xsqlvar);
				}
			}

			fixNull(xsqlvar);

			if (log != null) 
			{
				if (xsqlvar.sqldata == null) 
				{
					log.Debug("sqldata still null in writeSQLDatum: " + xsqlvar);
				}
			}

			try 
			{
				object sqldata = xsqlvar.sqldata;

				switch (xsqlvar.sqltype & ~1) 
				{
					case GdsCodes.SQL_TEXT:
						db.Output.WriteOpaque((byte[])sqldata, 
						                      xsqlvar.sqllen);
						break;

					case GdsCodes.SQL_VARYING:
						if (((byte[])sqldata).Length > xsqlvar.sqllen)
						{
							throw new GDSException(GdsCodes.isc_rec_size_err, 
							                       ((byte[])sqldata).Length);
						}
						db.Output.WriteInt(((byte[])sqldata).Length);
						db.Output.WriteOpaque((byte[])sqldata, 
						                      ((byte[])sqldata).Length);
						break;
					
					case GdsCodes.SQL_SHORT:
						db.Output.WriteShort(Convert.ToInt16(sqldata));
						break;
					
					case GdsCodes.SQL_LONG:						
						db.Output.WriteInt(Convert.ToInt32(sqldata));
						break;
					
					case GdsCodes.SQL_FLOAT:
						db.Output.WriteFloat(Convert.ToSingle(sqldata));
						break;
					
					case GdsCodes.SQL_DOUBLE:
						db.Output.WriteDouble(Convert.ToDouble(sqldata));
						break;
	
					case GdsCodes.SQL_TIMESTAMP:
						db.Output.WriteInt(encodeDate((System.DateTime) sqldata));
						db.Output.WriteInt(encodeTime((System.DateTime) sqldata));
						break;

					case GdsCodes.SQL_BLOB:						
						db.Output.WriteLong(Convert.ToInt64(sqldata));
						break;

					case GdsCodes.SQL_ARRAY:
						db.Output.WriteLong(Convert.ToInt64(sqldata));
						break;
					
					case GdsCodes.SQL_INT64:
					case GdsCodes.SQL_QUAD:
						db.Output.WriteLong(Convert.ToInt64(sqldata));
						break;
					
					case GdsCodes.SQL_TYPE_TIME:
						db.Output.WriteInt(encodeTime((System.DateTime) sqldata));
						break;
					
					case GdsCodes.SQL_TYPE_DATE:
						db.Output.WriteInt(encodeDate((System.DateTime) sqldata));
						break;
					
					default:
						throw new GDSException("Unknown sql data type: " + xsqlvar.sqltype);
				}

				db.Output.WriteInt(xsqlvar.sqlind);

			} 
			catch (IOException) 
			{
				throw new GDSException(GdsCodes.isc_net_write_err);
			}
		}

		private void fixNull(XSQLVAR xsqlvar)
		{
			if ((xsqlvar.sqlind == -1) && (xsqlvar.sqldata == null))
			{
				switch (xsqlvar.sqltype & ~1) 
				{
					case GdsCodes.SQL_TEXT:
						xsqlvar.sqldata = new byte[xsqlvar.sqllen];
						break;
					
					case GdsCodes.SQL_VARYING:
						xsqlvar.sqldata = new byte[0];
						break;
					
					case GdsCodes.SQL_SHORT:
						xsqlvar.sqldata = (short) 0;
						break;
					
					case GdsCodes.SQL_LONG:
						xsqlvar.sqldata = (int) 0;
						break;
					
					case GdsCodes.SQL_FLOAT:
						xsqlvar.sqldata = (float) 0;
						break;
					
					case GdsCodes.SQL_DOUBLE:
						xsqlvar.sqldata = (double) 0;
						break;

					case GdsCodes.SQL_TIMESTAMP:
						xsqlvar.sqldata  = new System.DateTime(0 * 10000L + 621355968000000000);
						break;
					
					case GdsCodes.SQL_BLOB:					
					case GdsCodes.SQL_ARRAY:
					case GdsCodes.SQL_QUAD:
					case GdsCodes.SQL_INT64:
						xsqlvar.sqldata = (long) 0;
						break;
					
					case GdsCodes.SQL_TYPE_TIME:
						xsqlvar.sqldata = new System.DateTime(0 * 10000L + 621355968000000000);
						break;

					case GdsCodes.SQL_TYPE_DATE:
						xsqlvar.sqldata = new System.DateTime(0 * 10000L + 621355968000000000);
						break;

					default:
						throw new GDSException("Unknown sql data type: " + xsqlvar.sqltype);
				}
			}
		}

		private int encodeTime(System.DateTime d) 
		{
			GregorianCalendar calendar = new GregorianCalendar();			

			long millisInDay = calendar.GetHour(d) * 60 * 60 * 1000	+
								calendar.GetMinute(d) * 60 * 1000	+
								calendar.GetSecond(d) * 1000;				
			
			int iTime = (int) (millisInDay * 10);

			return iTime;
		}

		private int encodeDate(System.DateTime d) 
		{			
			int day, month, year;
			int c, ya;

			GregorianCalendar calendar = new GregorianCalendar();

			day		= calendar.GetDayOfMonth(d);
			month	= calendar.GetMonth(d);
			year	= calendar.GetYear(d);

			if (month > 2) 
			{
				month -= 3;
			} 
			else
			{
				month	+= 9;
				year	-= 1;
			}

			c	= year / 100;
			ya	= year - 100 * c;

			return ((146097 * c) / 4		+
					(1461 * ya) / 4			+
					(153 * month + 2) / 5	+
					day + 1721119 - 2400001);
		}


		/* Now returns results in object[] and in xsqlda.data
		 * Nulls are represented by null values in object array,
		 * but by sqlind = -1 in xsqlda.
		 */
		private object[] readSQLData(isc_db_handle_impl db,
									XSQLDA xsqlda)
		{
			// This only works if not (port->port_flags & PORT_symmetric)
			object[] row = new object[xsqlda.sqld];
			for (int i = 0; i < xsqlda.sqld; i++) 
			{
				row[i] = readSQLDatum(db, xsqlda.sqlvar[i]);
			}
			return row;
		}

		private object readSQLDatum(isc_db_handle_impl db,
									XSQLVAR xsqlvar) 
		{
			try 
			{
				switch (xsqlvar.sqltype & ~1) 
				{
					case GdsCodes.SQL_TEXT:
						xsqlvar.sqldata = db.Input.ReadOpaque(xsqlvar.sqllen);
						break;
					
					case GdsCodes.SQL_VARYING:						
						xsqlvar.sqldata = db.Input.ReadOpaque(db.Input.ReadInt());
						break;
					
					case GdsCodes.SQL_SHORT:
						xsqlvar.sqldata = (short)db.Input.ReadInt();
						break;
					
					case GdsCodes.SQL_LONG:
						xsqlvar.sqldata = db.Input.ReadInt();
						break;
					
					case GdsCodes.SQL_FLOAT:
						xsqlvar.sqldata = (float)db.Input.ReadSingle();
						break;
					
					case GdsCodes.SQL_DOUBLE:	
						xsqlvar.sqldata = (double)db.Input.ReadDouble();
						break;
		
					case GdsCodes.SQL_TIMESTAMP:
						DateTime date = decodeDate(db.Input.ReadInt());
						DateTime time = decodeTime(db.Input.ReadInt());

						xsqlvar.sqldata = new System.DateTime(
										date.Year, date.Month, date.Day,
										time.Hour,time.Minute, time.Second, time.Millisecond);
						break;
										
					case GdsCodes.SQL_TYPE_TIME:
						xsqlvar.sqldata = decodeTime(db.Input.ReadInt());
						break;
					
					case GdsCodes.SQL_TYPE_DATE:
						xsqlvar.sqldata = decodeDate(db.Input.ReadInt());
						break;
					
					case GdsCodes.SQL_BLOB:					
					case GdsCodes.SQL_ARRAY:				
					case GdsCodes.SQL_QUAD:
					case GdsCodes.SQL_INT64:
						xsqlvar.sqldata = db.Input.ReadLong();
						break;
				}

				xsqlvar.sqlind = db.Input.ReadInt();

				if (xsqlvar.sqlind == 0) 
				{
					return xsqlvar.sqldata;
				}
				else if (xsqlvar.sqlind == -1) {
					return null;
				}
				else {
					throw new GDSException("invalid sqlind value: " + xsqlvar.sqlind);
				}
			} 
			catch (IOException) 
			{
				throw new GDSException(GdsCodes.isc_net_read_err);
			}
		}

		private System.DateTime decodeTime(int sql_time) 
		{
			return new System.DateTime((sql_time / 10000) * 1000 * 10000L + 621355968000000000);
		}

		private System.DateTime decodeDate(int sql_date) 
		{
			int year, month, day, century;

			sql_date	-= 1721119 - 2400001;
			century		= (4 * sql_date - 1) / 146097;
			sql_date	= 4 * sql_date - 1 - 146097 * century;
			day			= sql_date / 4;

			sql_date	= (4 * day + 3) / 1461;
			day			= 4 * day + 3 - 1461 * sql_date;
			day			= (day + 4) / 4;

			month		= (5 * day - 3) / 153;
			day			= 5 * day - 3 - 153 * month;
			day			= (day + 5) / 5;

			year		= 100 * century + sql_date;

			if (month < 10) 
			{
				month += 3;
			} 
			else 
			{
				month	-= 9;
				year	+= 1;
			}

			DateTime date = new System.DateTime(year, month, day);		

			return date.Date;
		}

		private XSQLDA parseSqlInfo(isc_stmt_handle stmt_handle,
									byte[] info,
									byte[] items) 
		{   
			if (log != null) log.Debug("parseSqlInfo started");

			XSQLDA xsqlda = new XSQLDA();
			int lastindex = 0;
			while ((lastindex = parseTruncSqlInfo(info, xsqlda, lastindex)) > 0) 
			{
				lastindex--;               // Is this OK ?
				
				byte[] new_items = new byte[4 + items.Length];
				new_items[0] = GdsCodes.isc_info_sql_sqlda_start;
				new_items[1] = 2;
				new_items[2] = (byte) (lastindex & 255);
				new_items[3] = (byte) (lastindex >> 8);

				Array.Copy(items, 0, new_items, 4, items.Length);
				info = isc_dsql_sql_info(stmt_handle, new_items.Length, new_items, info.Length);
			}
			if (log != null) log.Debug("parseSqlInfo ended");

			return xsqlda;
		}
	    
	    
		private int parseTruncSqlInfo(byte[] info,
									XSQLDA xsqlda,
									int lastindex)
		{
			byte item;
			int index = 0;
			if (log != null) log.Debug("parseSqlInfo: first 2 bytes are " + isc_vax_integer(info, 0, 2) + " or: " + info[0] + ", " + info[1]);

			int i = 2;

			int len = isc_vax_integer(info, i, 2);
			i += 2;
			int n = isc_vax_integer(info, i, len);
			i += len;
			if (xsqlda.sqlvar == null) 
			{
				xsqlda.sqld = xsqlda.sqln = n;
				xsqlda.sqlvar = new XSQLVAR[xsqlda.sqln];
			}
			if (log != null) log.Debug("xsqlda.sqln read as " + xsqlda.sqln);

			while (info[i] != GdsCodes.isc_info_end) 
			{
				while ((item = info[i++]) != GdsCodes.isc_info_sql_describe_end) 
				{
					switch (item) 
					{
						case GdsCodes.isc_info_sql_sqlda_seq:
							len = isc_vax_integer(info, i, 2);
							i += 2;
							index = isc_vax_integer(info, i, len);
							i += len;
							xsqlda.sqlvar[index - 1] = new XSQLVAR();
							break;
						
						case GdsCodes.isc_info_sql_type:
							len = isc_vax_integer(info, i, 2);
							i += 2;
							xsqlda.sqlvar[index - 1].sqltype = isc_vax_integer (info, i, len);
							i += len;
							break;

						case GdsCodes.isc_info_sql_sub_type:
							len = isc_vax_integer(info, i, 2);
							i += 2;
							xsqlda.sqlvar[index - 1].sqlsubtype = isc_vax_integer (info, i, len);
							i += len;
							break;
						
						case GdsCodes.isc_info_sql_scale:
							len = isc_vax_integer(info, i, 2);
							i += 2;
							xsqlda.sqlvar[index - 1].sqlscale = isc_vax_integer (info, i, len);
							i += len;
							break;
						
						case GdsCodes.isc_info_sql_length:
							len = isc_vax_integer(info, i, 2);
							i += 2;
							xsqlda.sqlvar[index - 1].sqllen = isc_vax_integer (info, i, len);
							i += len;
							break;

						case GdsCodes.isc_info_sql_field:
							len = isc_vax_integer(info, i, 2);
							i += 2;							
							xsqlda.sqlvar[index - 1].sqlname = 
										Encoding.Default.GetString(info,i,len);
							i += len;
							break;							
						
						case GdsCodes.isc_info_sql_relation:
							len = isc_vax_integer(info, i, 2);
							i += 2;
							xsqlda.sqlvar[index - 1].relname = 
										Encoding.Default.GetString(info,i,len);
							i += len;
							break;
						
						case GdsCodes.isc_info_sql_owner:
							len = isc_vax_integer(info, i, 2);
							i += 2;
							xsqlda.sqlvar[index - 1].ownname = 
												Encoding.Default.GetString(info,i,len);
							i += len;
							break;

						case GdsCodes.isc_info_sql_alias:
							len = isc_vax_integer(info, i, 2);
							i += 2;
							xsqlda.sqlvar[index - 1].aliasname = 
												Encoding.Default.GetString(info,i,len);
							i += len;
							break;

						case GdsCodes.isc_info_truncated:
							return lastindex;

						default:
							throw new GDSException(GdsCodes.isc_dsql_sqlda_err);
					}
				}
				lastindex = index;
			}
			return 0;
		}

		//DEBUG
		private void checkAllRead(BinaryReader input)
		{
			try 
			{
				int i = (int)input.PeekChar();
				if (i > 0) 
				{
					if (log != null) log.Debug("Extra bytes in packet read: " + i);
					byte[] b = new byte[i];					
					input.Read(b, 0, i);					
					for (int j = 0; j < ((b.Length < 16) ? b.Length : 16) ; j++) 
					{
						if (log != null) log.Debug("byte: " + b[j]);
					}
				}
			}
			catch (IOException e) 
			{
				throw new GDSException("IOException in checkAllRead: " + e);
			}
		}

		private void releaseObject(isc_db_handle_impl db, int op, int id)
		{
			lock (db) 
			{
				try 
				{
					db.Output.WriteInt(op);
					db.Output.WriteInt(id);
					db.Output.Flush();            
					receiveResponse(db);
				}
				catch (IOException) 
				{
					throw new GDSException(GdsCodes.isc_net_read_err);
				}
			}
		}

		#endregion

		#region INNER_CLASSES		

		internal class DbAttachInfo 
		{
			private string	server		= "localhost";
			private int		port		= 3050;
			private string	fileName	= String.Empty;

			public string Server
			{
				get { return server; }
			}

			public int Port
			{
				get { return port; }
			}

			public string FileName
			{
				get { return fileName; }
			}

			public DbAttachInfo(string connectInfo)
			{
				if (connectInfo == null) 
				{
					throw new GDSException("Connection string missing");
				}

				/* allows standard syntax //host:port/....
				 * and old fb syntax host/port:....
				 */
				connectInfo = connectInfo.Trim();
				char hostSepChar;
				char portSepChar;
				if (connectInfo.StartsWith("//"))
				{
					connectInfo = connectInfo.Substring(2);
					hostSepChar = '/';
					portSepChar = ':';
				}
				else 
				{
					hostSepChar = ':';
					portSepChar = '/';
				}

				int sep = connectInfo.IndexOf(hostSepChar);
				if (sep == 0 || sep == connectInfo.Length - 1) 
				{
					throw new GDSException("Bad connection string: '"+hostSepChar+"' at beginning or end of:" + connectInfo +  GdsCodes.isc_bad_db_format);
				}
				else if (sep > 0) {
					server = connectInfo.Substring(0, sep);
					fileName = connectInfo.Substring(sep + 1);
					int portSep = server.IndexOf(portSepChar);
					if (portSep == 0 || portSep == server.Length - 1) 
					{
						throw new GDSException("Bad server string: '"+portSepChar+"' at beginning or end of: " + server +  GdsCodes.isc_bad_db_format);
					}
					else if (portSep > 0) {
						port = int.Parse(server.Substring(portSep + 1));							
						server = server.Substring(0, portSep);
					}
				}
				else if (sep == -1) 
				{
					fileName = connectInfo;
				}
			}

			public DbAttachInfo(string server, int port, string fileName)
			{
				if (fileName == null || fileName.Equals(String.Empty)) 
				{
					throw new GDSException("null filename in DbAttachInfo");
				}
				if (server != null) 
				{
					this.server = server;
				}
				if (!port.Equals(null)) 
				{
					this.port = port;
				}
				this.fileName = fileName;
			}
		}
			
		internal class Response 
		{
			public int	  resp_object;
			public long	  resp_blob_id;
			public byte[] resp_data;
		}

		#endregion
	}
}
