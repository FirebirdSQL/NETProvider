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
 *  
 *  Contributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Data;
using System.Data.Common;
using System.Collections;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	public sealed class FbTransaction : DbTransaction
	{
		#region Fields

		private FbConnection connection;
		private ITransaction transaction;
		private IsolationLevel isolationLevel;
		private bool disposed;
		private bool isUpdated;

		#endregion

		#region Properties

		public new FbConnection Connection
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

		public override IsolationLevel IsolationLevel
		{
			get { return this.isolationLevel; }
		}

		#endregion

		#region Internal Properties

		internal ITransaction Transaction
		{
			get { return this.transaction; }
		}

		internal bool IsUpdated
		{
			get { return this.isUpdated; }
		}

		#endregion

		#region DbTransaction Protected properties

		protected override DbConnection DbConnection
		{
			get { return this.connection; }
		}

		#endregion

		#region Constructors

		internal FbTransaction(FbConnection connection)
			: this(connection, IsolationLevel.ReadCommitted)
		{
		}

		internal FbTransaction(FbConnection connection, IsolationLevel il)
		{
			this.isolationLevel = il;
			this.connection = connection;
		}

		#endregion

		#region Finalizer

		~FbTransaction()
		{
			this.Dispose(false);
		}

		#endregion

		#region IDisposable methods

		protected override void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!this.disposed)
				{
					try
					{
						// release any unmanaged resources
						if (this.transaction != null)
						{
							if (this.transaction.State == TransactionState.Active && !this.isUpdated)
							{
								this.transaction.Dispose();
								this.transaction = null;
							}
						}

						// release any managed resources
						if (disposing)
						{
							this.connection = null;
							this.transaction = null;
						}
					}
					catch
					{
					}
					finally
					{
						this.isolationLevel = IsolationLevel.ReadCommitted;
						this.isUpdated = true;
						this.disposed = true;
					}
				}
			}
		}

		#endregion

		#region DbTransaction Methods

		public override void Commit()
		{
			lock (this)
			{
				if (this.isUpdated)
				{
					throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
				}

				try
				{
					this.transaction.Commit();
					this.UpdateTransaction();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		public override void Rollback()
		{
			lock (this)
			{
				if (this.isUpdated)
				{
					throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
				}

				try
				{
					this.transaction.Rollback();
					this.UpdateTransaction();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		#endregion

		#region Methods

		public void Save(string savePointName)
		{
			lock (this)
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

				try
				{
					FbCommand command = new FbCommand(
						"SAVEPOINT " + savePointName,
						this.connection,
						this);
					command.ExecuteNonQuery();
					command.Dispose();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		public void Commit(string savePointName)
		{
			lock (this)
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

				try
				{
					FbCommand command = new FbCommand(
						"RELEASE SAVEPOINT " + savePointName,
						this.connection,
						this);
					command.ExecuteNonQuery();
					command.Dispose();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		public void Rollback(string savePointName)
		{
			lock (this)
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

				try
				{
					FbCommand command = new FbCommand(
						"ROLLBACK WORK TO SAVEPOINT " + savePointName,
						this.connection,
						this);
					command.ExecuteNonQuery();
					command.Dispose();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		public void CommitRetaining()
		{
			lock (this)
			{
				if (this.isUpdated)
				{
					throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
				}

				try
				{
					this.transaction.CommitRetaining();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		public void RollbackRetaining()
		{
			lock (this)
			{
				if (this.isUpdated)
				{
					throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
				}

				try
				{
					this.transaction.RollbackRetaining();
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		#endregion

		#region Internal Methods

		internal void BeginTransaction()
		{
			lock (this)
			{
				try
				{
					IDatabase database = this.connection.InnerConnection.Database;
					this.transaction = database.BeginTransaction(this.BuildTpb());
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		internal void BeginTransaction(FbTransactionOptions options)
		{
			lock (this)
			{
				try
				{
					IDatabase database = this.connection.InnerConnection.Database;
					this.transaction = database.BeginTransaction(this.BuildTpb(options));
				}
				catch (IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		#endregion

		#region Private Methods

		private void UpdateTransaction()
		{
			if (this.connection != null && this.connection.InnerConnection != null)
			{
				this.connection.InnerConnection.TransactionUpdated();
			}

			this.isUpdated = true;
			this.connection = null;
			this.transaction = null;
		}

		private TransactionParameterBuffer BuildTpb()
		{
			FbTransactionOptions options = new FbTransactionOptions();
			options.WaitTimeout = null;
			options.TransactionBehavior = FbTransactionBehavior.Write;

			options.TransactionBehavior |= FbTransactionBehavior.NoWait;

			switch (this.isolationLevel)
			{
				case IsolationLevel.Serializable:
					options.TransactionBehavior |= FbTransactionBehavior.Consistency;
					break;

				case IsolationLevel.RepeatableRead:
				case IsolationLevel.Snapshot:
					options.TransactionBehavior |= FbTransactionBehavior.Concurrency;
					break;

				case IsolationLevel.ReadCommitted:
				case IsolationLevel.ReadUncommitted:
				default:
					options.TransactionBehavior |= FbTransactionBehavior.ReadCommitted;
					options.TransactionBehavior |= FbTransactionBehavior.RecVersion;
					break;
			}

			return this.BuildTpb(options);
		}

		private TransactionParameterBuffer BuildTpb(FbTransactionOptions options)
		{
			TransactionParameterBuffer tpb = new TransactionParameterBuffer();

			tpb.Append(IscCodes.isc_tpb_version3);

			if ((options.TransactionBehavior & FbTransactionBehavior.Consistency) == FbTransactionBehavior.Consistency)
			{
				tpb.Append(IscCodes.isc_tpb_consistency);
			}
			if ((options.TransactionBehavior & FbTransactionBehavior.Concurrency) == FbTransactionBehavior.Concurrency)
			{
				tpb.Append(IscCodes.isc_tpb_concurrency);
			}
			if ((options.TransactionBehavior & FbTransactionBehavior.Wait) == FbTransactionBehavior.Wait)
			{
				tpb.Append(IscCodes.isc_tpb_wait);
				if (options.WaitTimeoutTPBValue.HasValue)
				{
					tpb.Append(IscCodes.isc_tpb_lock_timeout, (short)options.WaitTimeoutTPBValue);
				}
			}
			if ((options.TransactionBehavior & FbTransactionBehavior.NoWait) == FbTransactionBehavior.NoWait)
			{
				tpb.Append(IscCodes.isc_tpb_nowait);
			}
			if ((options.TransactionBehavior & FbTransactionBehavior.Read) == FbTransactionBehavior.Read)
			{
				tpb.Append(IscCodes.isc_tpb_read);
			}
			if ((options.TransactionBehavior & FbTransactionBehavior.Write) == FbTransactionBehavior.Write)
			{
				tpb.Append(IscCodes.isc_tpb_write);
			}
			foreach (var table in options.LockTables)
			{
				int lockType;
				if ((table.Value & FbTransactionBehavior.LockRead) == FbTransactionBehavior.LockRead)
				{
					lockType = IscCodes.isc_tpb_lock_read;
				}
				else if ((table.Value & FbTransactionBehavior.LockWrite) == FbTransactionBehavior.LockWrite)
				{
					lockType = IscCodes.isc_tpb_lock_write;
				}
				else
				{
					throw new ArgumentException("Must specify either LockRead or LockWrite.");
				}				
				tpb.Append(lockType, table.Key);

				int? lockBehavior = null;
				if ((table.Value & FbTransactionBehavior.Exclusive) == FbTransactionBehavior.Exclusive)
				{
					lockBehavior = IscCodes.isc_tpb_exclusive;
				}
				else if ((table.Value & FbTransactionBehavior.Protected) == FbTransactionBehavior.Protected)
				{
					lockBehavior = IscCodes.isc_tpb_protected;
				}
				else if ((table.Value & FbTransactionBehavior.Shared) == FbTransactionBehavior.Shared)
				{
					lockBehavior = IscCodes.isc_tpb_shared;
				}
				if (lockBehavior.HasValue)
					tpb.Append((int)lockBehavior);
			}
			if ((options.TransactionBehavior & FbTransactionBehavior.ReadCommitted) == FbTransactionBehavior.ReadCommitted)
			{
				tpb.Append(IscCodes.isc_tpb_read_committed);
			}
			if ((options.TransactionBehavior & FbTransactionBehavior.Autocommit) == FbTransactionBehavior.Autocommit)
			{
				tpb.Append(IscCodes.isc_tpb_autocommit);
			}
			if ((options.TransactionBehavior & FbTransactionBehavior.RecVersion) == FbTransactionBehavior.RecVersion)
			{
				tpb.Append(IscCodes.isc_tpb_rec_version);
			}
			if ((options.TransactionBehavior & FbTransactionBehavior.NoRecVersion) == FbTransactionBehavior.NoRecVersion)
			{
				tpb.Append(IscCodes.isc_tpb_no_rec_version);
			}
			if ((options.TransactionBehavior & FbTransactionBehavior.RestartRequests) == FbTransactionBehavior.RestartRequests)
			{
				tpb.Append(IscCodes.isc_tpb_restart_requests);
			}
			if ((options.TransactionBehavior & FbTransactionBehavior.NoAutoUndo) == FbTransactionBehavior.NoAutoUndo)
			{
				tpb.Append(IscCodes.isc_tpb_no_auto_undo);
			}
			
			return tpb;
		}

		#endregion
	}
}
