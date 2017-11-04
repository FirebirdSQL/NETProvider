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

		private FbConnection _connection;
		private TransactionBase _transaction;
		private bool _disposed;
		private bool _isCompleted;

		#endregion

		#region Properties

		public new FbConnection Connection
		{
			get { return !_isCompleted ? _connection : null; }
		}

		public override IsolationLevel IsolationLevel { get; }

		internal TransactionBase Transaction
		{
			get { return _transaction; }
		}

		internal bool IsCompleted
		{
			get { return _isCompleted; }
		}

		protected override DbConnection DbConnection
		{
			get { return _connection; }
		}

		#endregion

		#region Constructors

		internal FbTransaction(FbConnection connection)
			: this(connection, IsolationLevel.ReadCommitted)
		{ }

		internal FbTransaction(FbConnection connection, IsolationLevel il)
		{
			_connection = connection;
			IsolationLevel = il;
		}

		#endregion

		#region IDisposable methods

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!_disposed)
				{
					_disposed = true;
					if (_transaction != null)
					{
						if (!_isCompleted)
						{
							_transaction.Dispose();
						}
					}
					_connection = null;
					_transaction = null;
					_isCompleted = true;
					base.Dispose(disposing);
				}
			}
		}

		#endregion

		#region DbTransaction Methods

		public override void Commit()
		{
			EnsureCompleted();
			try
			{
				_transaction.Commit();
				CompleteTransaction();
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public override void Rollback()
		{
			EnsureCompleted();
			try
			{
				_transaction.Rollback();
				CompleteTransaction();
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		#endregion

		#region Methods

		public void Save(string savePointName)
		{
			EnsureSavePointName(savePointName);
			EnsureCompleted();
			try
			{
				using (var command = new FbCommand($"SAVEPOINT {savePointName}", _connection, this))
				{
					command.ExecuteNonQuery();
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public void Commit(string savePointName)
		{
			EnsureSavePointName(savePointName);
			EnsureCompleted();
			try
			{
				using (var command = new FbCommand($"RELEASE SAVEPOINT {savePointName}", _connection, this))
				{
					command.ExecuteNonQuery();
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public void Rollback(string savePointName)
		{
			EnsureSavePointName(savePointName);
			EnsureCompleted();
			try
			{
				using (var command = new FbCommand($"ROLLBACK WORK TO SAVEPOINT {savePointName}", _connection, this))
				{
					command.ExecuteNonQuery();
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public void CommitRetaining()
		{
			EnsureCompleted();
			try
			{
				_transaction.CommitRetaining();
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public void RollbackRetaining()
		{
			EnsureCompleted();
			try
			{
				_transaction.RollbackRetaining();
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		#endregion

		#region Internal Methods

		internal void BeginTransaction()
		{
			_transaction = _connection.InnerConnection.Database.BeginTransaction(BuildTpb());
		}

		internal void BeginTransaction(FbTransactionOptions options)
		{
			_transaction = _connection.InnerConnection.Database.BeginTransaction(BuildTpb(options));
		}

		#endregion

		#region Private Methods

		private void CompleteTransaction()
		{
			_connection?.InnerConnection?.TransactionCompleted();
			_connection = null;
			_transaction.Dispose();
			_transaction = null;
			_isCompleted = true;
		}

		private TransactionParameterBuffer BuildTpb()
		{
			FbTransactionOptions options = new FbTransactionOptions();
			options.WaitTimeout = null;
			options.TransactionBehavior = FbTransactionBehavior.Write;

			options.TransactionBehavior |= FbTransactionBehavior.NoWait;

			switch (IsolationLevel)
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

			return BuildTpb(options);
		}

		private void EnsureCompleted()
		{
			if (_isCompleted)
			{
				throw new InvalidOperationException("This transaction has completed and it is no longer usable.");
			}
		}

		private static TransactionParameterBuffer BuildTpb(FbTransactionOptions options)
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

		private static void EnsureSavePointName(string savePointName)
		{
			if (string.IsNullOrWhiteSpace(savePointName))
			{
				throw new ArgumentException("No transaction name was be specified.");
			}
		}

		#endregion
	}
}
