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
using System.Collections;
using FirebirdSql.Data.Firebird.Gds;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbTransaction"]/overview/*'/>
	public sealed class FbTransaction : MarshalByRefObject, IDbTransaction, IDisposable
	{
		#region FIELDS

		private GdsTransaction	transaction;
		private bool			disposed;
		private FbConnection	connection;
		private bool			isUpdated;
		private IsolationLevel	isolationLevel;

		#endregion

		#region PROPERTIES

		IDbConnection IDbTransaction.Connection
		{
			get { return this.Connection; }
		}

		/// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbTransaction"]/property[@name="Connection"]/*'/>
		public FbConnection Connection
		{
			get 
			{ 
				if (!this.isUpdated)
				{
					return this.connection; 
				}
				else
				{
					return null;
				}
			}
		}

		/// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbTransaction"]/property[@name="IsolationLevel"]/*'/>
		public IsolationLevel IsolationLevel 
		{
			get { return this.isolationLevel; }
		}

		#endregion

		#region INTERNAL_PROPERTIES

		internal GdsTransaction Transaction
		{
			get { return this.transaction; }
		}

		internal bool IsUpdated
		{
			get { return this.isUpdated; }
			set
			{
				if (value && connection != null)
				{
					this.connection.ActiveTransaction	= null;
					this.connection						= null;
				}
				this.isUpdated = value;
			}
		}

		#endregion

		#region CONSTRUCTORS

		private FbTransaction()
		{
			this.isolationLevel = IsolationLevel.ReadCommitted;
		}
		
		internal FbTransaction(FbConnection connection) : this()
		{
			this.connection = connection;
		}				

		internal FbTransaction(FbConnection connection, IsolationLevel il) : this()
		{
			this.isolationLevel = il;
			this.connection		= connection;			
		}				

		#endregion

		#region DESTRUCTORS

		/// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbCommandBuilder"]/destructor[@name="Finalize"]/*'/>
		~FbTransaction()
		{
			this.Dispose(false);
		}

		#endregion

		#region IDISPOSABLE_METHODS

		/// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbTransaction"]/method[@name="Dispose"]/*'/>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this.disposed)			
			{
				if (disposing)
				{
					try
					{
						if (this.connection != null)
						{
							if ((this.transaction.State == GdsTransactionState.TransactionStarted
								|| this.transaction.State == GdsTransactionState.TransactionPrepared)
								&& !this.isUpdated)
							{
								if (this.transaction != null)
								{
									this.transaction.Rollback();									
								}
							}
						}
					}
					finally
					{
						if (this.connection != null)
						{
							this.connection.ActiveTransaction	= null;
							this.connection						= null;
						}
						this.transaction = null;
						this.isUpdated	 = true;									
					}
				}
			}			

			this.disposed = true;
		}
		
		#endregion

		#region METHODS

		/// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbTransaction"]/method[@name="Commit"]/*'/>
		public void Commit()
		{
			if (this.isUpdated)
			{
				throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
			}
			if (this.Connection.DataReader != null)
			{
				throw new InvalidOperationException("FbCommand is currently busy Open, Fetching.");
			}

			try
			{
				this.transaction.Commit();
				this.IsUpdated = true;
			}
			catch(GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbTransaction"]/method[@name="Save(System.String)"]/*'/>
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
			if (this.isUpdated)
			{
				throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
			}
			if (this.Connection.DataReader != null)
			{
				throw new InvalidOperationException("FbCommand is currently busy Open, Fetching.");
			}

			try
			{
				FbCommand command = new FbCommand(
					"SAVEPOINT " + savePointName,
					connection, 
					this);
				command.InternalPrepare();
				command.InternalExecute();
				command.Dispose();
			}
			catch(GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbTransaction"]/method[@name="Rollback(System.String)"]/*'/>
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
			if (this.isUpdated)
			{
				throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
			}
			if (this.Connection.DataReader != null)
			{
				throw new InvalidOperationException("FbCommand is currently busy Open, Fetching.");
			}

			try
			{
				FbCommand command = new FbCommand(
								"ROLLBACK WORK TO SAVEPOINT " + savePointName,
								connection, 
								this);
				command.InternalPrepare();
				command.InternalExecute();
				command.Dispose();
			}
			catch(GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbTransaction"]/method[@name="Rollback"]/*'/>
		public void Rollback()
		{
			if (this.isUpdated)
			{
				throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
			}
			if (this.Connection.DataReader != null)
			{
				throw new InvalidOperationException("FbCommand is currently busy Open, Fetching.");
			}

			try
			{
				this.transaction.Rollback();
				this.IsUpdated = true;
			}
			catch(GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbTransaction"]/method[@name="CommitRetaining"]/*'/>
		public void CommitRetaining()
		{
			if (this.isUpdated)
			{
				throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
			}
			if (this.Connection.DataReader != null)
			{
				throw new InvalidOperationException("FbCommand is currently busy Open, Fetching.");
			}

			try
			{
				this.transaction.CommitRetaining();
			}
			catch(GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbTransaction"]/method[@name="RollbackRetaining"]/*'/>
		public void RollbackRetaining()
		{
			if (this.isUpdated)
			{
				throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
			}

			if (this.Connection.DataReader != null)
			{
				throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
			}

			try
			{
				this.transaction.RollbackRetaining();
			}
			catch(GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		internal void BeginTransaction()
		{
			try
			{				
				this.transaction = connection.DbConnection.DB.CreateTransaction(this.IsolationLevel);
				this.transaction.BeginTransaction();
				this.IsUpdated = false;
			}
			catch(GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		#endregion
	}
}
