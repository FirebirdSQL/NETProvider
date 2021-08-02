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
using System.IO;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal class GdsTransaction : TransactionBase
	{
		#region Fields

		private int _handle;
		private bool _disposed;
		private GdsDatabase _database;

		#endregion

		#region Properties

		public override int Handle
		{
			get { return _handle; }
		}

		#endregion

		#region Constructors

		public GdsTransaction(DatabaseBase db)
		{
			if (!(db is GdsDatabase))
			{
				throw new ArgumentException($"Specified argument is not of {nameof(GdsDatabase)} type.");
			}

			_database = (GdsDatabase)db;
			State = TransactionState.NoTransaction;
		}

		#endregion

		#region Dispose2

		public override async ValueTask Dispose2Async(AsyncWrappingCommonArgs async)
		{
			if (!_disposed)
			{
				_disposed = true;
				if (State != TransactionState.NoTransaction)
				{
					await RollbackAsync(async).ConfigureAwait(false);
				}
				_database = null;
				_handle = 0;
				State = TransactionState.NoTransaction;
				await base.Dispose2Async(async).ConfigureAwait(false);
			}
		}

		#endregion

		#region Methods

		public override async ValueTask BeginTransactionAsync(TransactionParameterBuffer tpb, AsyncWrappingCommonArgs async)
		{
			if (State != TransactionState.NoTransaction)
			{
				throw new InvalidOperationException();
			}

			try
			{
				await _database.Xdr.WriteAsync(IscCodes.op_transaction, async).ConfigureAwait(false);
				await _database.Xdr.WriteAsync(_database.Handle, async).ConfigureAwait(false);
				await _database.Xdr.WriteBufferAsync(tpb.ToArray(), async).ConfigureAwait(false);
				await _database.Xdr.FlushAsync(async).ConfigureAwait(false);

				var response = (GenericResponse)await _database.ReadResponseAsync(async).ConfigureAwait(false);

				_database.TransactionCount++;

				_handle = response.ObjectHandle;
				State = TransactionState.Active;
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		public override async ValueTask CommitAsync(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			try
			{
				await _database.Xdr.WriteAsync(IscCodes.op_commit, async).ConfigureAwait(false);
				await _database.Xdr.WriteAsync(_handle, async).ConfigureAwait(false);
				await _database.Xdr.FlushAsync(async).ConfigureAwait(false);

				await _database.ReadResponseAsync(async).ConfigureAwait(false);

				_database.TransactionCount--;

				OnUpdate(EventArgs.Empty);

				State = TransactionState.NoTransaction;
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		public override async ValueTask RollbackAsync(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			try
			{
				await _database.Xdr.WriteAsync(IscCodes.op_rollback, async).ConfigureAwait(false);
				await _database.Xdr.WriteAsync(_handle, async).ConfigureAwait(false);
				await _database.Xdr.FlushAsync(async).ConfigureAwait(false);

				await _database.ReadResponseAsync(async).ConfigureAwait(false);

				_database.TransactionCount--;

				OnUpdate(EventArgs.Empty);

				State = TransactionState.NoTransaction;
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		public override async ValueTask CommitRetainingAsync(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			try
			{
				await _database.Xdr.WriteAsync(IscCodes.op_commit_retaining, async).ConfigureAwait(false);
				await _database.Xdr.WriteAsync(_handle, async).ConfigureAwait(false);
				await _database.Xdr.FlushAsync(async).ConfigureAwait(false);

				await _database.ReadResponseAsync(async).ConfigureAwait(false);

				State = TransactionState.Active;
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		public override async ValueTask RollbackRetainingAsync(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			try
			{
				await _database.Xdr.WriteAsync(IscCodes.op_rollback_retaining, async).ConfigureAwait(false);
				await _database.Xdr.WriteAsync(_handle, async).ConfigureAwait(false);
				await _database.Xdr.FlushAsync(async).ConfigureAwait(false);

				await _database.ReadResponseAsync(async).ConfigureAwait(false);

				State = TransactionState.Active;
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		#endregion

		#region Two Phase Commit Methods

		public override async ValueTask PrepareAsync(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			try
			{
				State = TransactionState.NoTransaction;

				await _database.Xdr.WriteAsync(IscCodes.op_prepare, async).ConfigureAwait(false);
				await _database.Xdr.WriteAsync(_handle, async).ConfigureAwait(false);
				await _database.Xdr.FlushAsync(async).ConfigureAwait(false);

				await _database.ReadResponseAsync(async).ConfigureAwait(false);

				State = TransactionState.Prepared;
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		public override async ValueTask PrepareAsync(byte[] buffer, AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			try
			{
				State = TransactionState.NoTransaction;

				await _database.Xdr.WriteAsync(IscCodes.op_prepare2, async).ConfigureAwait(false);
				await _database.Xdr.WriteAsync(_handle, async).ConfigureAwait(false);
				await _database.Xdr.WriteBufferAsync(buffer, buffer.Length, async).ConfigureAwait(false);
				await _database.Xdr.FlushAsync(async).ConfigureAwait(false);

				await _database.ReadResponseAsync(async).ConfigureAwait(false);

				State = TransactionState.Prepared;
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		#endregion
	}
}
