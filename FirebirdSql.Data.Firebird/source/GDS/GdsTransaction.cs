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
using System.Data;
using System.IO;

namespace FirebirdSql.Data.Firebird.Gds
{
	#region ENUMS

	internal enum GdsTransactionState
	{
		NoTransaction,
		TrasactionStarting,
		TransactionStarted,
		TransactionPreparing,
		TransactionPrepared,
		TransactionCommiting,
		TransactionRollbacking
	}

	#endregion

	internal delegate void TransactionUpdateEventHandler(object sender, EventArgs e);

	internal class GdsTransaction
	{
		#region FIELDS

		private int					handle;
		private GdsDbAttachment	db;		
		private GdsTransactionState	state;
		private IsolationLevel		isolationLevel;

		#endregion

		#region EVENTS

		public event TransactionUpdateEventHandler Update;

		#endregion

		#region PROPERTIES

		public int Handle
		{
			get { return handle; }
		}

		public GdsTransactionState State
		{
			get { return state; }
		}

		#endregion

		#region CONSTRUCTORS

		public GdsTransaction(GdsDbAttachment db)
		{
			this.db				= db;
			this.isolationLevel	= IsolationLevel.ReadCommitted;
		}

		public GdsTransaction(GdsDbAttachment db, IsolationLevel isolationLevel) : this(db)
		{
			this.isolationLevel	= isolationLevel;
		}

		#endregion

		#region METHODS

		public void BeginTransaction()
		{
			lock (db) 
			{
				if (state != GdsTransactionState.NoTransaction)
				{
					throw new GdsException(
						GdsCodes.isc_arg_gds, 
						GdsCodes.isc_tra_state, 
						this.handle,
						"no valid");
				}
				
				state = GdsTransactionState.TrasactionStarting;

				try 
				{
					GdsDpbBuffer tpb = buildTpb();

					db.Send.WriteInt(GdsCodes.op_transaction);
					db.Send.WriteInt(db.Handle);
					db.Send.WriteBuffer(tpb.ToArray());
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();
					handle = r.ObjectHandle;

					state = GdsTransactionState.TransactionStarted;

					db.TransactionCount++;
				}
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
			}
		}

		public void Commit()
		{
			lock (db) 
			{
				if (state != GdsTransactionState.TransactionStarted && 
					state != GdsTransactionState.TransactionPrepared)
				{
					throw new GdsException(
						GdsCodes.isc_arg_gds, 
						GdsCodes.isc_tra_state, 
						this.handle,
						"no valid");
				}
				
				state = GdsTransactionState.TransactionCommiting;

				try 
				{					
					db.Send.WriteInt(GdsCodes.op_commit);
					db.Send.WriteInt(handle);
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();

					db.TransactionCount--;

					if (Update != null)
					{
						Update(this, new EventArgs());
					}
					state = GdsTransactionState.NoTransaction;
				} 
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
			}
		}

		public void Rollback()
		{
			lock (db)
			{
				if (state == GdsTransactionState.NoTransaction)
				{
					throw new GdsException(
						GdsCodes.isc_arg_gds, 
						GdsCodes.isc_tra_state, 
						this.handle,
						"no valid");
				}

				state = GdsTransactionState.TransactionRollbacking;

				try 
				{
					db.Send.WriteInt(GdsCodes.op_rollback);
					db.Send.WriteInt(handle);
					db.Send.Flush();            

					GdsResponse r = db.ReceiveResponse();

					db.TransactionCount--;

					if (Update != null)
					{
						Update(this, new EventArgs());
					}
					state = GdsTransactionState.NoTransaction;
				} 
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
			}
		}

		public void CommitRetaining()
		{
			lock (db) 
			{
				if (state != GdsTransactionState.TransactionStarted && 
					state != GdsTransactionState.TransactionPrepared)
				{
					throw new GdsException(
						GdsCodes.isc_arg_gds, 
						GdsCodes.isc_tra_state, 
						this.handle,
						"no valid");
				}

				state = GdsTransactionState.TransactionCommiting;

				try 
				{
					db.Send.WriteInt(GdsCodes.op_commit_retaining);
					db.Send.WriteInt(handle);
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();

					state = GdsTransactionState.TransactionStarted;
				} 
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}				
			}
		}

		public void RollbackRetaining()
		{
			lock (db) 
			{
				if (state != GdsTransactionState.TransactionStarted && 
					state != GdsTransactionState.TransactionPrepared)
				{
					throw new GdsException(
						GdsCodes.isc_arg_gds, 
						GdsCodes.isc_tra_state, 
						this.handle,
						"no valid");
				}

				state = GdsTransactionState.TransactionRollbacking;

				try 
				{
					db.Send.WriteInt(GdsCodes.op_rollback_retaining);
					db.Send.WriteInt(handle);
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();

					state = GdsTransactionState.TransactionStarted;
				} 
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}				
			}
		}

		public void Prepare()
		{
			lock (db) 
			{
				if (state != GdsTransactionState.TransactionStarted) 
				{
					throw new GdsException(
						GdsCodes.isc_arg_gds, 
						GdsCodes.isc_tra_state, 
						this.handle,
						"no valid");
				}

				state = GdsTransactionState.TransactionPreparing;

				try 
				{
					db.Send.WriteInt(GdsCodes.op_prepare);
					db.Send.WriteInt(handle);
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();

					state = GdsTransactionState.TransactionPrepared;
				} 
				catch (IOException)
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}				
			}
		}

		public void Prepare(byte[] buffer)
		{
			lock (db) 
			{
				if (state != GdsTransactionState.TransactionStarted) 
				{
					throw new GdsException(
						GdsCodes.isc_arg_gds, 
						GdsCodes.isc_tra_state, 
						this.handle,
						"no valid");
				}

				state = GdsTransactionState.TransactionPreparing;

				try 
				{
					db.Send.WriteInt(GdsCodes.op_prepare2);
					db.Send.WriteInt(handle);
					db.Send.WriteBuffer(buffer, buffer.Length);
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();

					state = GdsTransactionState.TransactionStarted;
				} 
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}				
			}
		}

		#endregion

		#region PRIVATE_METHODS

		private GdsDpbBuffer buildTpb()
		{
			GdsDpbBuffer tpb = new GdsDpbBuffer();

			tpb.Append(GdsCodes.isc_tpb_version3);
			tpb.Append(GdsCodes.isc_tpb_write);
			tpb.Append(GdsCodes.isc_tpb_wait);

			/* Isolation level */
			switch(this.isolationLevel)
			{
				case IsolationLevel.Serializable:
					tpb.Append(GdsCodes.isc_tpb_consistency);
					break;

				case IsolationLevel.RepeatableRead:			
					tpb.Append(GdsCodes.isc_tpb_concurrency);
					break;

				case IsolationLevel.ReadUncommitted:
					tpb.Append(GdsCodes.isc_tpb_read_committed);
					tpb.Append(GdsCodes.isc_tpb_rec_version);
					break;

				case IsolationLevel.ReadCommitted:
				default:					
					tpb.Append(GdsCodes.isc_tpb_read_committed);
					tpb.Append(GdsCodes.isc_tpb_no_rec_version);
					break;
			}

			return tpb;
		}

		#endregion
	}
}
