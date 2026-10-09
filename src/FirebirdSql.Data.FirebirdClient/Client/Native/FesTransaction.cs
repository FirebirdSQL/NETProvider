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
using System.IO;
using System.Data;
using System.Runtime.InteropServices;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.Client.Native.Handles;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Collections.Generic;

namespace FirebirdSql.Data.Client.Native;

internal sealed class FesTransaction : TransactionBase
{
	#region Inner Structs

	[StructLayout(LayoutKind.Sequential)]
	struct IscTeb
	{
		public IntPtr dbb_ptr;
		public int tpb_len;
		public IntPtr tpb_ptr;
	}

	#endregion

	#region Fields

	private TransactionHandle _handle;
	private FesDatabase _database;
	private bool _disposed;
	private IntPtr[] _statusVector;

	#endregion

	#region Properties

	public override int Handle
	{
		get { return _handle.DangerousGetHandle().AsInt(); }
	}

	public TransactionHandle HandlePtr
	{
		get { return _handle; }
	}

	#endregion

	#region Constructors

	public FesTransaction(FesDatabase database)
	{
		_database = database;
		_handle = new TransactionHandle();
		State = TransactionState.NoTransaction;
		_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
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
			_handle.Dispose();
			State = TransactionState.NoTransaction;
			_statusVector = null;
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
			_handle.Dispose();
			State = TransactionState.NoTransaction;
			_statusVector = null;
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

		var teb = new IscTeb();
		var tebData = IntPtr.Zero;

		try
		{
			ClearStatusVector();

			teb.dbb_ptr = Marshal.AllocHGlobal(4);
			Marshal.WriteInt32(teb.dbb_ptr, _database.Handle);

			teb.tpb_len = tpb.Length;

			teb.tpb_ptr = Marshal.AllocHGlobal(tpb.Length);
			Marshal.Copy(tpb.ToArray(), 0, teb.tpb_ptr, tpb.Length);

			var size = Marshal.SizeOf<IscTeb>();
			tebData = Marshal.AllocHGlobal(size);

			Marshal.StructureToPtr(teb, tebData, true);

			_database.FbClient.isc_start_multiple(
				_statusVector,
				ref _handle,
				1,
				tebData);

			_database.ProcessStatusVector(_statusVector);

			State = TransactionState.Active;

			_database.TransactionCount++;
		}
		finally
		{
			if (teb.dbb_ptr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(teb.dbb_ptr);
			}
			if (teb.tpb_ptr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(teb.tpb_ptr);
			}
			if (tebData != IntPtr.Zero)
			{
				Marshal.DestroyStructure<IscTeb>(tebData);
				Marshal.FreeHGlobal(tebData);
			}
		}
	}
	public override ValueTask BeginTransactionAsync(TransactionParameterBuffer tpb, CancellationToken cancellationToken = default)
	{
		if (State != TransactionState.NoTransaction)
		{
			throw new InvalidOperationException();
		}

		var teb = new IscTeb();
		var tebData = IntPtr.Zero;

		try
		{
			ClearStatusVector();

			teb.dbb_ptr = Marshal.AllocHGlobal(4);
			Marshal.WriteInt32(teb.dbb_ptr, _database.Handle);

			teb.tpb_len = tpb.Length;

			teb.tpb_ptr = Marshal.AllocHGlobal(tpb.Length);
			Marshal.Copy(tpb.ToArray(), 0, teb.tpb_ptr, tpb.Length);

			var size = Marshal.SizeOf<IscTeb>();
			tebData = Marshal.AllocHGlobal(size);

			Marshal.StructureToPtr(teb, tebData, true);

			_database.FbClient.isc_start_multiple(
				_statusVector,
				ref _handle,
				1,
				tebData);

			_database.ProcessStatusVector(_statusVector);

			State = TransactionState.Active;

			_database.TransactionCount++;
		}
		finally
		{
			if (teb.dbb_ptr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(teb.dbb_ptr);
			}
			if (teb.tpb_ptr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(teb.tpb_ptr);
			}
			if (tebData != IntPtr.Zero)
			{
				Marshal.DestroyStructure<IscTeb>(tebData);
				Marshal.FreeHGlobal(tebData);
			}
		}

		return ValueTask.CompletedTask;
	}

	public override void Commit()
	{
		EnsureActiveTransactionState();

		ClearStatusVector();

		_database.FbClient.isc_commit_transaction(_statusVector, ref _handle);

		_database.ProcessStatusVector(_statusVector);

		_database.TransactionCount--;

		OnUpdate(EventArgs.Empty);

		State = TransactionState.NoTransaction;
	}
	public override ValueTask CommitAsync(CancellationToken cancellationToken = default)
	{
		EnsureActiveTransactionState();

		ClearStatusVector();

		_database.FbClient.isc_commit_transaction(_statusVector, ref _handle);

		_database.ProcessStatusVector(_statusVector);

		_database.TransactionCount--;

		OnUpdate(EventArgs.Empty);

		State = TransactionState.NoTransaction;

		return ValueTask.CompletedTask;
	}

	public override void Rollback()
	{
		EnsureActiveTransactionState();

		ClearStatusVector();

		_database.FbClient.isc_rollback_transaction(_statusVector, ref _handle);

		_database.ProcessStatusVector(_statusVector);

		_database.TransactionCount--;

		OnUpdate(EventArgs.Empty);

		State = TransactionState.NoTransaction;
	}
	public override ValueTask RollbackAsync(CancellationToken cancellationToken = default)
	{
		EnsureActiveTransactionState();

		ClearStatusVector();

		_database.FbClient.isc_rollback_transaction(_statusVector, ref _handle);

		_database.ProcessStatusVector(_statusVector);

		_database.TransactionCount--;

		OnUpdate(EventArgs.Empty);

		State = TransactionState.NoTransaction;

		return ValueTask.CompletedTask;
	}

	public override void CommitRetaining()
	{
		EnsureActiveTransactionState();

		ClearStatusVector();

		_database.FbClient.isc_commit_retaining(_statusVector, ref _handle);

		_database.ProcessStatusVector(_statusVector);

		State = TransactionState.Active;
	}
	public override ValueTask CommitRetainingAsync(CancellationToken cancellationToken = default)
	{
		EnsureActiveTransactionState();

		ClearStatusVector();

		_database.FbClient.isc_commit_retaining(_statusVector, ref _handle);

		_database.ProcessStatusVector(_statusVector);

		State = TransactionState.Active;

		return ValueTask.CompletedTask;
	}

	public override void RollbackRetaining()
	{
		EnsureActiveTransactionState();

		ClearStatusVector();

		_database.FbClient.isc_rollback_retaining(_statusVector, ref _handle);

		_database.ProcessStatusVector(_statusVector);

		State = TransactionState.Active;
	}
	public override ValueTask RollbackRetainingAsync(CancellationToken cancellationToken = default)
	{
		EnsureActiveTransactionState();

		ClearStatusVector();

		_database.FbClient.isc_rollback_retaining(_statusVector, ref _handle);

		_database.ProcessStatusVector(_statusVector);

		State = TransactionState.Active;

		return ValueTask.CompletedTask;
	}

	public override void Prepare()
	{ }
	public override ValueTask PrepareAsync(CancellationToken cancellationToken = default)
	{
		return ValueTask.CompletedTask;
	}

	public override void Prepare(byte[] buffer)
	{ }
	public override ValueTask PrepareAsync(byte[] buffer, CancellationToken cancellationToken = default)
	{
		return ValueTask.CompletedTask;
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

		TransactionInfo(items, buffer, buffer.Length);

		return IscHelper.ParseTransactionInfo(buffer, _database.Charset);
	}
	public override ValueTask<List<object>> GetTransactionInfoAsync(byte[] items, int bufferLength, CancellationToken cancellationToken = default)
	{
		var buffer = new byte[bufferLength];

		TransactionInfo(items, buffer, buffer.Length);

		return ValueTask.FromResult(IscHelper.ParseTransactionInfo(buffer, _database.Charset));
	}

	#endregion

	#region Private Methods

	private void TransactionInfo(byte[] items, byte[] buffer, int bufferLength)
	{
		StatusVectorHelper.ClearStatusVector(_statusVector);

		_database.FbClient.isc_transaction_info(
			_statusVector,
			ref _handle,
			(short)items.Length,
			items,
			(short)bufferLength,
			buffer);

		ProcessStatusVector();
	}

	private void ClearStatusVector()
	{
		Array.Clear(_statusVector, 0, _statusVector.Length);
	}

	private void ProcessStatusVector()
	{
		StatusVectorHelper.ProcessStatusVector(_statusVector, _database.Charset, _database.WarningMessage);
	}

	private void ProcessStatusVector(Charset charset)
	{
		StatusVectorHelper.ProcessStatusVector(_statusVector, charset, _database.WarningMessage);
	}

	#endregion
}
