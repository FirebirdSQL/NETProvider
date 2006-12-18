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
using FirebirdSql.Data.INGDS;
using FirebirdSql.Data.NGDS;
using System.Collections;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="T:FbTransaction"]/*'/>
	public sealed class FbTransaction : MarshalByRefObject, IDbTransaction, 
										IDisposable
	{
		#region FIELDS

		private isc_tr_handle_impl	iscTransaction;
		private bool				disposed;
		private bool				isUpdated;
		private FbConnection		connection;
		private IsolationLevel		isolationLevel;

		#endregion

		#region PROPERTIES

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="P:Connection"]/*'/>
		IDbConnection IDbTransaction.Connection
		{
			get { return Connection; }
		}

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="P:Connection"]/*'/>
		public FbConnection Connection
		{
			get { return connection; }
			set { connection = value; }
		}

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="P:IsolationLevel"]/*'/>
		public IsolationLevel IsolationLevel 
		{
			get { return isolationLevel; }
			set { isolationLevel = value; }
		}

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="P:Transaction"]/*'/>
		internal isc_tr_handle_impl IscTransaction
		{
			get { return iscTransaction; }
		}

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="P:IsUpdated"]/*'/>
		internal bool IsUpdated
		{
			get { return isUpdated; }
		}

		#endregion

		#region CONSTRUCTORS

		private FbTransaction()
		{
			iscTransaction	= null;
			disposed		= false;
			isUpdated		= false;		
			connection		= null;
			isolationLevel	= IsolationLevel.ReadCommitted;
		}
		
		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:#ctor(FirebirdSql.Data.Firebird.FbConnection)"]/*'/>
		internal FbTransaction(FbConnection connection) : this()
		{
			this.connection = connection;
		}				

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:#ctor(FirebirdSql.Data.Firebird.FbConnection,System.Data.IsolationLevel)"]/*'/>
		internal FbTransaction(FbConnection connection, IsolationLevel il) : this()
		{
			this.isolationLevel = il;
			this.connection = connection;			
		}				

		#endregion

		#region DESTRUCTORS

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:#Finalize())"]/*'/>
		~FbTransaction()
		{
			Dispose(false);
		}

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:Dispose"]/*'/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:Dispose(bool)"]/*'/>
		private void Dispose(bool disposing)
		{
			if (!disposed)			
			{
				if (disposing)
				{
					try
					{
						if (connection != null)
						{
							if ((iscTransaction.State == TxnState.TRANSACTIONSTARTED		
								|| iscTransaction.State == TxnState.TRANSACTIONPREPARED)
								&& !IsUpdated)
							{
								if (this.iscTransaction != null)
								{
									InternalRollback(false);
									isUpdated = true;									
								}
							}
						}
					}
					finally
					{
						connection		= null;
						iscTransaction	= null;

						disposed		= true;
					}
				}
			}			
		}
		
		#endregion

		#region METHODS

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:Commit"]/*'/>
		public void Commit()
		{
			if (isUpdated)
			{
				throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
			}

			if (Connection.DataReader != null)
			{
				throw new InvalidOperationException("FbCommand is currently busy Open, Fetching.");
			}

			try
			{
				InternalCommit(false);
				isUpdated	= true;
				connection	= null;
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:Save(System.String)"]/*'/>
		public void Save(string savePointName)
		{
			if (savePointName == null)
			{
				throw new ArgumentException("No transaction name was be specified.");
			}
			else
			{
				if (savePointName.Length == 0)
				{
					throw new ArgumentException("No transaction name was be specified.");
				}
			}

			if (isUpdated)
			{
				throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
			}

			if (Connection.DataReader != null)
			{
				throw new InvalidOperationException("FbCommand is currently busy Open, Fetching.");
			}

			try
			{
				FbStatement statement = new FbStatement(
					connection, this, 
					new FbParameterCollection(),
					"SAVEPOINT " + savePointName,
					CommandType.Text);

				statement.Execute();

				statement.Dispose();
			}
			catch (Exception)
			{
				throw new Exception("An error occurred while trying to commit the transaction.");
			}
		}

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:Rollback(System.String)"]/*'/>
		public void Rollback(string savePointName)
		{
			if (savePointName == null)
			{
				throw new ArgumentException("No transaction name was be specified.");
			}
			else
			{
				if (savePointName.Length == 0)
				{
					throw new ArgumentException("No transaction name was be specified.");
				}
			}

			if (isUpdated)
			{
				throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
			}

			if (Connection.DataReader != null)
			{
				throw new InvalidOperationException("FbCommand is currently busy Open, Fetching.");
			}

			try
			{
				FbStatement statement = new FbStatement(
					connection, this,
					new FbParameterCollection(),
					"ROLLBACK WORK TO SAVEPOINT " + savePointName,
					CommandType.Text);

				statement.Execute();

				statement.Dispose();
			}
			catch (Exception)
			{
				throw new Exception();
			}
		}
	
		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:Rollback"]/*'/>
		public void Rollback()
		{
			if (isUpdated)
			{
				throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
			}

			if (Connection.DataReader != null)
			{
				throw new InvalidOperationException("FbCommand is currently busy Open, Fetching.");
			}

			try
			{
				InternalRollback(false);
				isUpdated	= true;
				connection	= null;
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:CommitRetaining"]/*'/>
		public void CommitRetaining()
		{
			if (isUpdated)
			{
				throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
			}

			if (Connection.DataReader != null)
			{
				throw new InvalidOperationException("FbCommand is currently busy Open, Fetching.");
			}

			try
			{
				InternalCommit(true);				
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:RollbackRetaining"]/*'/>
		public void RollbackRetaining()
		{
			if (isUpdated)
			{
				throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
			}

			if (Connection.DataReader != null)
			{
				throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
			}

			try
			{
				InternalRollback(true);
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:BeginTransaction"]/*'/>
		internal void BeginTransaction()
		{
			try
			{
				// Initialization of transaction handler
				iscTransaction = 
					(isc_tr_handle_impl)connection.IscConnection.GDS.get_new_isc_tr_handle();

				ArrayList iscTpb = new ArrayList();

				iscTpb.Add(GdsCodes.isc_tpb_version3);
				iscTpb.Add(GdsCodes.isc_tpb_write);
				iscTpb.Add(GdsCodes.isc_tpb_wait);

				/* Isolation level */
				switch(this.isolationLevel)
				{
					case IsolationLevel.Serializable:
						iscTpb.Add(GdsCodes.isc_tpb_consistency);						
						break;

					case IsolationLevel.RepeatableRead:				
						iscTpb.Add(GdsCodes.isc_tpb_concurrency);						
						break;

					case IsolationLevel.ReadUncommitted:
						iscTpb.Add(GdsCodes.isc_tpb_read_committed);
						iscTpb.Add(GdsCodes.isc_tpb_rec_version);
						break;

					case IsolationLevel.ReadCommitted:
					default:					
						iscTpb.Add(GdsCodes.isc_tpb_read_committed);
						iscTpb.Add(GdsCodes.isc_tpb_no_rec_version);
						break;
				}

				connection.IscConnection.GDS.isc_start_transaction(iscTransaction,
													connection.IscConnection.db, 
													iscTpb);				
				isUpdated = false;
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}


		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:InternalCommit(System.Boolean)"]/*'/>
		internal void InternalCommit(bool retaining)
		{
			try
			{
				if (retaining)
				{
					connection.IscConnection.GDS.isc_commit_retaining(iscTransaction);
				}
				else
				{
					connection.IscConnection.GDS.isc_commit_transaction(iscTransaction);
				}
			}
			catch(GDSException ex)
			{
				throw ex;
			}
		}

		/// <include file='xmldoc/fbtransaction.xml' path='doc/member[@name="M:InternalRollback(System.Boolean)"]/*'/>
		internal void InternalRollback(bool retaining)
		{			
			try
			{
				if (retaining)
				{
					connection.IscConnection.GDS.isc_rollback_retaining(iscTransaction);
				}
				else
				{
					connection.IscConnection.GDS.isc_rollback_transaction(iscTransaction);
				}
			}
			catch(GDSException ex)
			{
				throw ex;
			}
		}

		#endregion
	}
}
