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

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Gds
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class GdsTransaction : ITransaction
	{
		#region Events

		public event TransactionUpdateEventHandler Update;

		#endregion

		#region Fields

		private int					handle;
		private GdsDbAttachment		db;		
		private TransactionState	state;

		#endregion

		#region Properties

		public int Handle
		{
			get { return this.handle; }
		}

		public TransactionState State
		{
			get { return this.state; }
		}

		#endregion

		#region Constructors

		public GdsTransaction(IDbAttachment db)			
		{
			if (!(db is GdsDbAttachment))
			{
				throw new ArgumentException("Specified argument is not of GdsDbAttachment type.");
			}
			this.db		= (GdsDbAttachment)db;
			this.state	= TransactionState.NoTransaction;
		}

		#endregion

		#region Methods

		public void BeginTransaction(DpbBuffer tpb)
		{			
			lock (this.db) 
			{
				if (this.state != TransactionState.NoTransaction)
				{
					throw new IscException(
						IscCodes.isc_arg_gds, 
						IscCodes.isc_tra_state, 
						this.handle,
						"no valid");
				}
				
				this.state = TransactionState.TrasactionStarting;

				try 
				{
					this.db.Send.Write(IscCodes.op_transaction);
					this.db.Send.Write(this.db.Handle);
					this.db.Send.WriteBuffer(tpb.ToArray());
					this.db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();
					this.handle = r.ObjectHandle;

					this.state = TransactionState.TransactionStarted;

					this.db.TransactionCount++;
				} 
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}				
			}
		}

		public void Commit()
		{
			lock (this.db) 
			{
				if (this.state != TransactionState.TransactionStarted && 
					this.state != TransactionState.TransactionPrepared)
				{
					throw new IscException(
						IscCodes.isc_arg_gds, 
						IscCodes.isc_tra_state, 
						this.handle,
						"no valid");
				}
				
				this.state = TransactionState.TransactionCommiting;

				try 
				{					
					this.db.Send.Write(IscCodes.op_commit);
					this.db.Send.Write(this.handle);
					this.db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();

					this.db.TransactionCount--;

					if (this.Update != null)
					{
						this.Update(this, new EventArgs());
					}

					this.state = TransactionState.NoTransaction;
				} 
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public void Rollback()
		{
			lock (this.db)
			{
				if (this.state == TransactionState.NoTransaction)
				{
					throw new IscException(
						IscCodes.isc_arg_gds, 
						IscCodes.isc_tra_state,
						this.handle,
						"no valid");
				}

				this.state = TransactionState.TransactionRollbacking;

				try 
				{
					this.db.Send.Write(IscCodes.op_rollback);
					this.db.Send.Write(this.handle);
					this.db.Send.Flush();            

					GdsResponse r = db.ReceiveResponse();

					this.db.TransactionCount--;

					if (this.Update != null)
					{
						this.Update(this, new EventArgs());
					}

					this.state = TransactionState.NoTransaction;
				} 
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public void CommitRetaining()
		{
			lock (this.db) 
			{
				if (this.state != TransactionState.TransactionStarted && 
					this.state != TransactionState.TransactionPrepared)
				{
					throw new IscException(
						IscCodes.isc_arg_gds, 
						IscCodes.isc_tra_state, 
						this.handle,
						"no valid");
				}

				this.state = TransactionState.TransactionCommiting;

				try 
				{
					this.db.Send.Write(IscCodes.op_commit_retaining);
					this.db.Send.Write(this.handle);
					this.db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();

					this.state = TransactionState.TransactionStarted;
				} 
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}				
			}
		}

		public void RollbackRetaining()
		{
			lock (this.db) 
			{
				if (this.state != TransactionState.TransactionStarted && 
					this.state != TransactionState.TransactionPrepared)
				{
					throw new IscException(
						IscCodes.isc_arg_gds, 
						IscCodes.isc_tra_state, 
						this.handle,
						"no valid");
				}

				this.state = TransactionState.TransactionRollbacking;

				try 
				{
					this.db.Send.Write(IscCodes.op_rollback_retaining);
					this.db.Send.Write(this.handle);
					this.db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();

					this.state = TransactionState.TransactionStarted;
				} 
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}				
			}
		}

		public void Prepare()
		{
			lock (this.db)
			{
				if (this.state != TransactionState.TransactionStarted) 
				{
					throw new IscException(
						IscCodes.isc_arg_gds, 
						IscCodes.isc_tra_state, 
						this.handle,
						"no valid");
				}

				this.state = TransactionState.TransactionPreparing;

				try 
				{
					this.db.Send.Write(IscCodes.op_prepare);
					this.db.Send.Write(this.handle);
					this.db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();

					this.state = TransactionState.TransactionPrepared;
				} 
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}				
			}
		}

		public void Prepare(byte[] buffer)
		{
			lock (this.db) 
			{
				if (this.state != TransactionState.TransactionStarted) 
				{
					throw new IscException(
						IscCodes.isc_arg_gds, 
						IscCodes.isc_tra_state, 
						this.handle,
						"no valid");
				}

				this.state = TransactionState.TransactionPreparing;

				try 
				{
					this.db.Send.Write(IscCodes.op_prepare2);
					this.db.Send.Write(this.handle);
					this.db.Send.WriteBuffer(buffer, buffer.Length);
					this.db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();

					this.state = TransactionState.TransactionStarted;
				} 
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}				
			}
		}

		#endregion
	}
}
