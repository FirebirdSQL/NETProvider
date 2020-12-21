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
		#region Events

		public override event EventHandler Update;

		#endregion

		#region Fields

		private int _handle;
		private bool _disposed;
		private GdsDatabase _database;
		private TransactionState _state;

		#endregion

		#region Properties

		public override int Handle
		{
			get { return _handle; }
		}

		public override TransactionState State
		{
			get { return _state; }
		}

		#endregion

		#region Constructors

		public GdsTransaction(IDatabase db)
		{
			if (!(db is GdsDatabase))
			{
				throw new ArgumentException($"Specified argument is not of {nameof(GdsDatabase)} type.");
			}

			_database = (GdsDatabase)db;
			_state = TransactionState.NoTransaction;
		}

		#endregion

		#region Dispose2

		public override async Task Dispose2(AsyncWrappingCommonArgs async)
		{
			if (!_disposed)
			{
				_disposed = true;
				if (_state != TransactionState.NoTransaction)
				{
					await Rollback(async).ConfigureAwait(false);
				}
				_database = null;
				_handle = 0;
				_state = TransactionState.NoTransaction;
				await base.Dispose2(async).ConfigureAwait(false);
			}
		}

		#endregion

		#region Methods

		public override async Task BeginTransaction(TransactionParameterBuffer tpb, AsyncWrappingCommonArgs async)
		{
			if (_state != TransactionState.NoTransaction)
			{
				throw new InvalidOperationException();
			}

			try
			{
				await _database.Xdr.Write(IscCodes.op_transaction, async).ConfigureAwait(false);
				await _database.Xdr.Write(_database.Handle, async).ConfigureAwait(false);
				await _database.Xdr.WriteBuffer(tpb.ToArray(), async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				var response = (GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false);

				_database.TransactionCount++;

				_handle = response.ObjectHandle;
				_state = TransactionState.Active;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		public override async Task Commit(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			try
			{
				await _database.Xdr.Write(IscCodes.op_commit, async).ConfigureAwait(false);
				await _database.Xdr.Write(_handle, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				await _database.ReadResponse(async).ConfigureAwait(false);

				_database.TransactionCount--;

				Update?.Invoke(this, new EventArgs());

				_state = TransactionState.NoTransaction;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		public override async Task Rollback(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			try
			{
				await _database.Xdr.Write(IscCodes.op_rollback, async).ConfigureAwait(false);
				await _database.Xdr.Write(_handle, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				await _database.ReadResponse(async).ConfigureAwait(false);

				_database.TransactionCount--;

				Update?.Invoke(this, new EventArgs());

				_state = TransactionState.NoTransaction;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		public override async Task CommitRetaining(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			try
			{
				await _database.Xdr.Write(IscCodes.op_commit_retaining, async).ConfigureAwait(false);
				await _database.Xdr.Write(_handle, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				await _database.ReadResponse(async).ConfigureAwait(false);

				_state = TransactionState.Active;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		public override async Task RollbackRetaining(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			try
			{
				await _database.Xdr.Write(IscCodes.op_rollback_retaining, async).ConfigureAwait(false);
				await _database.Xdr.Write(_handle, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				await _database.ReadResponse(async).ConfigureAwait(false);

				_state = TransactionState.Active;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		#endregion

		#region Two Phase Commit Methods

		public override async Task Prepare(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			try
			{
				_state = TransactionState.NoTransaction;

				await _database.Xdr.Write(IscCodes.op_prepare, async).ConfigureAwait(false);
				await _database.Xdr.Write(_handle, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				await _database.ReadResponse(async).ConfigureAwait(false);

				_state = TransactionState.Prepared;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		public override async Task Prepare(byte[] buffer, AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			try
			{
				_state = TransactionState.NoTransaction;

				await _database.Xdr.Write(IscCodes.op_prepare2, async).ConfigureAwait(false);
				await _database.Xdr.Write(_handle, async).ConfigureAwait(false);
				await _database.Xdr.WriteBuffer(buffer, buffer.Length, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

				await _database.ReadResponse(async).ConfigureAwait(false);

				_state = TransactionState.Prepared;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		#endregion
	}
}
