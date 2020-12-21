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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
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

#if !(NET48 || NETSTANDARD2_0 || NETSTANDARD2_1)
		public override bool SupportsSavepoints
		{
			get { return true; }
		}
#endif

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

		#region IDisposable, IAsyncDisposable methods

		protected override void Dispose(bool disposing)
		{
			DisposeHelper(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
			base.Dispose(disposing);
		}
#if !(NET48 || NETSTANDARD2_0)
		public override async ValueTask DisposeAsync()
		{
			await DisposeHelper(new AsyncWrappingCommonArgs(true)).ConfigureAwait(false);
			await base.DisposeAsync().ConfigureAwait(false);
		}
#endif
		private async Task DisposeHelper(AsyncWrappingCommonArgs async)
		{
			if (!_disposed)
			{
				_disposed = true;
				if (_transaction != null)
				{
					if (!_isCompleted)
					{
						try
						{
							await _transaction.Dispose2(async).ConfigureAwait(false);
						}
						catch (IscException ex)
						{
							throw new FbException(ex.Message, ex);
						}
					}
				}
				_connection = null;
				_transaction = null;
				_isCompleted = true;
			}
		}

		#endregion

		#region Methods

		public override void Commit() => CommitImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
#if NET48 || NETSTANDARD2_0
		public Task CommitAsync(CancellationToken cancellationToken = default)
#else
		public override Task CommitAsync(CancellationToken cancellationToken = default)
#endif
			=> CommitImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		internal async Task CommitImpl(AsyncWrappingCommonArgs async)
		{
			EnsureCompleted();
			try
			{
				await _transaction.Commit(async).ConfigureAwait(false);
				await CompleteTransaction(async).ConfigureAwait(false);
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public override void Rollback() => RollbackImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
#if NET48 || NETSTANDARD2_0
		public Task RollbackAsync(CancellationToken cancellationToken = default)
#else
		public override Task RollbackAsync(CancellationToken cancellationToken = default)
#endif
			=> RollbackImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		internal async Task RollbackImpl(AsyncWrappingCommonArgs async)
		{
			EnsureCompleted();
			try
			{
				await _transaction.Rollback(async).ConfigureAwait(false);
				await CompleteTransaction(async).ConfigureAwait(false);
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
		public void Save(string savePointName)
#else
		public override void Save(string savePointName)
#endif
			=> SaveImpl(savePointName, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
		public Task SaveAsync(string savePointName, CancellationToken cancellationToken = default)
#else
		public override Task SaveAsync(string savePointName, CancellationToken cancellationToken = default)
#endif
			=> SaveImpl(savePointName, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task SaveImpl(string savePointName, AsyncWrappingCommonArgs async)
		{
			EnsureSavePointName(savePointName);
			EnsureCompleted();
			try
			{
				var command = new FbCommand($"SAVEPOINT {savePointName}", _connection, this);
#if NET48 || NETSTANDARD2_0
				using (command)
#else
				await using (command)
#endif
				{
					await async.AsyncSyncCall(command.ExecuteNonQueryAsync, command.ExecuteNonQuery).ConfigureAwait(false);
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
		public void Release(string savePointName)
#else
		public override void Release(string savePointName)
#endif
			=> ReleaseImpl(savePointName, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
		public Task ReleaseAsync(string savePointName, CancellationToken cancellationToken = default)
#else
		public override Task ReleaseAsync(string savePointName, CancellationToken cancellationToken = default)
#endif
			=> ReleaseImpl(savePointName, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task ReleaseImpl(string savePointName, AsyncWrappingCommonArgs async)
		{
			EnsureSavePointName(savePointName);
			EnsureCompleted();
			try
			{
				var command = new FbCommand($"RELEASE SAVEPOINT {savePointName}", _connection, this);
#if NET48 || NETSTANDARD2_0
				using (command)
#else
				await using (command)
#endif
				{
					await async.AsyncSyncCall(command.ExecuteNonQueryAsync, command.ExecuteNonQuery).ConfigureAwait(false);
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
		public void Rollback(string savePointName)
#else
		public override void Rollback(string savePointName)
#endif
			=> RollbackImpl(savePointName, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
		public Task RollbackAsync(string savePointName, CancellationToken cancellationToken = default)
#else
		public override Task RollbackAsync(string savePointName, CancellationToken cancellationToken = default)
#endif
			=> RollbackImpl(savePointName, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task RollbackImpl(string savePointName, AsyncWrappingCommonArgs async)
		{
			EnsureSavePointName(savePointName);
			EnsureCompleted();
			try
			{
				var command = new FbCommand($"ROLLBACK WORK TO SAVEPOINT {savePointName}", _connection, this);
#if NET48 || NETSTANDARD2_0
				using (command)
#else
				await using (command)
#endif
				{
					await async.AsyncSyncCall(command.ExecuteNonQueryAsync, command.ExecuteNonQuery).ConfigureAwait(false);
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public void CommitRetaining() => CommitRetainingImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task CommitRetainingAsync(CancellationToken cancellationToken = default) => CommitRetainingImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task CommitRetainingImpl(AsyncWrappingCommonArgs async)
		{
			EnsureCompleted();
			try
			{
				await _transaction.CommitRetaining(async).ConfigureAwait(false);
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public void RollbackRetaining() => RollbackRetainingImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task RollbackRetainingAsync(CancellationToken cancellationToken = default) => RollbackRetainingImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task RollbackRetainingImpl(AsyncWrappingCommonArgs async)
		{
			EnsureCompleted();
			try
			{
				await _transaction.RollbackRetaining(async).ConfigureAwait(false);
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		#endregion

		#region Internal Methods

		internal async Task BeginTransaction(AsyncWrappingCommonArgs async)
		{
			_transaction = await _connection.InnerConnection.Database.BeginTransaction(BuildTpb(), async).ConfigureAwait(false);
		}

		internal async Task BeginTransaction(FbTransactionOptions options, AsyncWrappingCommonArgs async)
		{
			_transaction = await _connection.InnerConnection.Database.BeginTransaction(BuildTpb(options), async).ConfigureAwait(false);
		}

		#endregion

		#region Private Methods

		private async Task CompleteTransaction(AsyncWrappingCommonArgs async)
		{
			var innerConnection = _connection?.InnerConnection;
			if (innerConnection != null)
			{
				await innerConnection.TransactionCompleted(async).ConfigureAwait(false);
			}
			_connection = null;
			await _transaction.Dispose2(async).ConfigureAwait(false);
			_transaction = null;
			_isCompleted = true;
		}

		private TransactionParameterBuffer BuildTpb()
		{
			var options = new FbTransactionOptions();
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
			var tpb = new TransactionParameterBuffer();

			tpb.Append(IscCodes.isc_tpb_version3);

			if (options.TransactionBehavior.HasFlag(FbTransactionBehavior.Consistency))
			{
				tpb.Append(IscCodes.isc_tpb_consistency);
			}
			if (options.TransactionBehavior.HasFlag(FbTransactionBehavior.Concurrency))
			{
				tpb.Append(IscCodes.isc_tpb_concurrency);
			}
			if (options.TransactionBehavior.HasFlag(FbTransactionBehavior.Wait))
			{
				tpb.Append(IscCodes.isc_tpb_wait);
				if (options.WaitTimeoutTPBValue.HasValue)
				{
					tpb.Append(IscCodes.isc_tpb_lock_timeout, (short)options.WaitTimeoutTPBValue);
				}
			}
			if (options.TransactionBehavior.HasFlag(FbTransactionBehavior.NoWait))
			{
				tpb.Append(IscCodes.isc_tpb_nowait);
			}
			if (options.TransactionBehavior.HasFlag(FbTransactionBehavior.Read))
			{
				tpb.Append(IscCodes.isc_tpb_read);
			}
			if (options.TransactionBehavior.HasFlag(FbTransactionBehavior.Write))
			{
				tpb.Append(IscCodes.isc_tpb_write);
			}
			foreach (var table in options.LockTables)
			{
				int lockType;
				if (table.Value.HasFlag(FbTransactionBehavior.LockRead))
				{
					lockType = IscCodes.isc_tpb_lock_read;
				}
				else if (table.Value.HasFlag(FbTransactionBehavior.LockWrite))
				{
					lockType = IscCodes.isc_tpb_lock_write;
				}
				else
				{
					throw new ArgumentException("Must specify either LockRead or LockWrite.");
				}
				tpb.Append(lockType, table.Key);

				int? lockBehavior = null;
				if (table.Value.HasFlag(FbTransactionBehavior.Exclusive))
				{
					lockBehavior = IscCodes.isc_tpb_exclusive;
				}
				else if (table.Value.HasFlag(FbTransactionBehavior.Protected))
				{
					lockBehavior = IscCodes.isc_tpb_protected;
				}
				else if (table.Value.HasFlag(FbTransactionBehavior.Shared))
				{
					lockBehavior = IscCodes.isc_tpb_shared;
				}
				if (lockBehavior.HasValue)
					tpb.Append((int)lockBehavior);
			}
			if (options.TransactionBehavior.HasFlag(FbTransactionBehavior.ReadCommitted))
			{
				tpb.Append(IscCodes.isc_tpb_read_committed);
			}
			if (options.TransactionBehavior.HasFlag(FbTransactionBehavior.Autocommit))
			{
				tpb.Append(IscCodes.isc_tpb_autocommit);
			}
			if (options.TransactionBehavior.HasFlag(FbTransactionBehavior.RecVersion))
			{
				tpb.Append(IscCodes.isc_tpb_rec_version);
			}
			if (options.TransactionBehavior.HasFlag(FbTransactionBehavior.NoRecVersion))
			{
				tpb.Append(IscCodes.isc_tpb_no_rec_version);
			}
			if (options.TransactionBehavior.HasFlag(FbTransactionBehavior.RestartRequests))
			{
				tpb.Append(IscCodes.isc_tpb_restart_requests);
			}
			if (options.TransactionBehavior.HasFlag(FbTransactionBehavior.NoAutoUndo))
			{
				tpb.Append(IscCodes.isc_tpb_no_auto_undo);
			}
			if (options.TransactionBehavior.HasFlag(FbTransactionBehavior.ReadConsistency))
			{
				tpb.Append(IscCodes.isc_tpb_read_consistency);
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
