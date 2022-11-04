/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10;

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

	public GdsTransaction(GdsDatabase database)
	{
		_database = database;
		State = TransactionState.NoTransaction;
	}

	#endregion

	#region Dispose2

	public override void Dispose2()
	{
		if (!_disposed)
		{
			_disposed = true;
			if (State != TransactionState.NoTransaction)
			{
				Rollback();
			}
			_database = null;
			_handle = 0;
			State = TransactionState.NoTransaction;
			base.Dispose2();
		}
	}
	public override async ValueTask Dispose2Async(CancellationToken cancellationToken = default)
	{
		if (!_disposed)
		{
			_disposed = true;
			if (State != TransactionState.NoTransaction)
			{
				await RollbackAsync(cancellationToken).ConfigureAwait(false);
			}
			_database = null;
			_handle = 0;
			State = TransactionState.NoTransaction;
			await base.Dispose2Async(cancellationToken).ConfigureAwait(false);
		}
	}

	#endregion

	#region Methods

	public override void BeginTransaction(TransactionParameterBuffer tpb)
	{
		if (State != TransactionState.NoTransaction)
		{
			throw new InvalidOperationException();
		}

		try
		{
			_database.Xdr.Write(IscCodes.op_transaction);
			_database.Xdr.Write(_database.Handle);
			_database.Xdr.WriteBuffer(tpb.ToArray());
			_database.Xdr.Flush();

			var response = (GenericResponse)_database.ReadResponse();

			_database.TransactionCount++;

			_handle = response.ObjectHandle;
			State = TransactionState.Active;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask BeginTransactionAsync(TransactionParameterBuffer tpb, CancellationToken cancellationToken = default)
	{
		if (State != TransactionState.NoTransaction)
		{
			throw new InvalidOperationException();
		}

		try
		{
			await _database.Xdr.WriteAsync(IscCodes.op_transaction, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_database.Handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteBufferAsync(tpb.ToArray(), cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			var response = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			_database.TransactionCount++;

			_handle = response.ObjectHandle;
			State = TransactionState.Active;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	public override void Commit()
	{
		EnsureActiveTransactionState();

		try
		{
			_database.Xdr.Write(IscCodes.op_commit);
			_database.Xdr.Write(_handle);
			_database.Xdr.Flush();

			_database.ReadResponse();

			_database.TransactionCount--;

			OnUpdate(EventArgs.Empty);

			State = TransactionState.NoTransaction;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask CommitAsync(CancellationToken cancellationToken = default)
	{
		EnsureActiveTransactionState();

		try
		{
			await _database.Xdr.WriteAsync(IscCodes.op_commit, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			_database.TransactionCount--;

			OnUpdate(EventArgs.Empty);

			State = TransactionState.NoTransaction;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	public override void Rollback()
	{
		EnsureActiveTransactionState();

		try
		{
			_database.Xdr.Write(IscCodes.op_rollback);
			_database.Xdr.Write(_handle);
			_database.Xdr.Flush();

			_database.ReadResponse();

			_database.TransactionCount--;

			OnUpdate(EventArgs.Empty);

			State = TransactionState.NoTransaction;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask RollbackAsync(CancellationToken cancellationToken = default)
	{
		EnsureActiveTransactionState();

		try
		{
			await _database.Xdr.WriteAsync(IscCodes.op_rollback, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			_database.TransactionCount--;

			OnUpdate(EventArgs.Empty);

			State = TransactionState.NoTransaction;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	public override void CommitRetaining()
	{
		EnsureActiveTransactionState();

		try
		{
			_database.Xdr.Write(IscCodes.op_commit_retaining);
			_database.Xdr.Write(_handle);
			_database.Xdr.Flush();

			_database.ReadResponse();

			State = TransactionState.Active;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask CommitRetainingAsync(CancellationToken cancellationToken = default)
	{
		EnsureActiveTransactionState();

		try
		{
			await _database.Xdr.WriteAsync(IscCodes.op_commit_retaining, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			State = TransactionState.Active;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	public override void RollbackRetaining()
	{
		EnsureActiveTransactionState();

		try
		{
			_database.Xdr.Write(IscCodes.op_rollback_retaining);
			_database.Xdr.Write(_handle);
			_database.Xdr.Flush();

			_database.ReadResponse();

			State = TransactionState.Active;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask RollbackRetainingAsync(CancellationToken cancellationToken = default)
	{
		EnsureActiveTransactionState();

		try
		{
			await _database.Xdr.WriteAsync(IscCodes.op_rollback_retaining, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			State = TransactionState.Active;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	public override void Prepare()
	{
		EnsureActiveTransactionState();

		try
		{
			State = TransactionState.NoTransaction;

			_database.Xdr.Write(IscCodes.op_prepare);
			_database.Xdr.Write(_handle);
			_database.Xdr.Flush();

			_database.ReadResponse();

			State = TransactionState.Prepared;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask PrepareAsync(CancellationToken cancellationToken = default)
	{
		EnsureActiveTransactionState();

		try
		{
			State = TransactionState.NoTransaction;

			await _database.Xdr.WriteAsync(IscCodes.op_prepare, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			State = TransactionState.Prepared;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	public override void Prepare(byte[] buffer)
	{
		EnsureActiveTransactionState();

		try
		{
			State = TransactionState.NoTransaction;

			_database.Xdr.Write(IscCodes.op_prepare2);
			_database.Xdr.Write(_handle);
			_database.Xdr.WriteBuffer(buffer, buffer.Length);
			_database.Xdr.Flush();

			_database.ReadResponse();

			State = TransactionState.Prepared;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask PrepareAsync(byte[] buffer, CancellationToken cancellationToken = default)
	{
		EnsureActiveTransactionState();

		try
		{
			State = TransactionState.NoTransaction;

			await _database.Xdr.WriteAsync(IscCodes.op_prepare2, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteBufferAsync(buffer, buffer.Length, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			State = TransactionState.Prepared;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	public override List<object> GetTransactionInfo(byte[] items)
	{
		return GetTransactionInfo(items, IscCodes.DEFAULT_MAX_BUFFER_SIZE);
	}
	public override ValueTask<List<object>> GetTransactionInfoAsync(byte[] items, CancellationToken cancellationToken = default)
	{
		return GetTransactionInfoAsync(items, IscCodes.DEFAULT_MAX_BUFFER_SIZE, cancellationToken);
	}

	public override List<object> GetTransactionInfo(byte[] items, int bufferLength)
	{
		var buffer = new byte[bufferLength];
		DatabaseInfo(items, buffer, buffer.Length);
		return IscHelper.ParseTransactionInfo(buffer, _database.Charset);
	}
	public override async ValueTask<List<object>> GetTransactionInfoAsync(byte[] items, int bufferLength, CancellationToken cancellationToken = default)
	{
		var buffer = new byte[bufferLength];
		await DatabaseInfoAsync(items, buffer, buffer.Length, cancellationToken).ConfigureAwait(false);
		return IscHelper.ParseTransactionInfo(buffer, _database.Charset);
	}

	#endregion

	#region Private Methods

	private void DatabaseInfo(byte[] items, byte[] buffer, int bufferLength)
	{
		try
		{
			_database.Xdr.Write(IscCodes.op_info_transaction);
			_database.Xdr.Write(_handle);
			_database.Xdr.Write(GdsDatabase.Incarnation);
			_database.Xdr.WriteBuffer(items, items.Length);
			_database.Xdr.Write(bufferLength);

			_database.Xdr.Flush();

			var response = (GenericResponse)_database.ReadResponse();

			var responseLength = bufferLength;

			if (response.Data.Length < bufferLength)
			{
				responseLength = response.Data.Length;
			}

			Buffer.BlockCopy(response.Data, 0, buffer, 0, responseLength);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	private async ValueTask DatabaseInfoAsync(byte[] items, byte[] buffer, int bufferLength, CancellationToken cancellationToken = default)
	{
		try
		{
			await _database.Xdr.WriteAsync(IscCodes.op_info_transaction, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(GdsDatabase.Incarnation, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteBufferAsync(items, items.Length, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(bufferLength, cancellationToken).ConfigureAwait(false);

			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			var response = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			var responseLength = bufferLength;

			if (response.Data.Length < bufferLength)
			{
				responseLength = response.Data.Length;
			}

			Buffer.BlockCopy(response.Data, 0, buffer, 0, responseLength);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	#endregion
}
