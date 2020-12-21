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
using System.Data;
using System.Runtime.InteropServices;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.Client.Native.Handle;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Client.Native
{
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

		#region Events

		public override event EventHandler Update;

		#endregion

		#region Fields

		private TransactionHandle _handle;
		private FesDatabase _db;
		private TransactionState _state;
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

		public override TransactionState State
		{
			get { return _state; }
		}

		#endregion

		#region Constructors

		public FesTransaction(IDatabase db)
		{
			if (!(db is FesDatabase))
			{
				throw new ArgumentException($"Specified argument is not of {nameof(FesDatabase)} type.");
			}

			_db = (FesDatabase)db;
			_handle = new TransactionHandle();
			_state = TransactionState.NoTransaction;
			_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
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
				_db = null;
				_handle.Dispose();
				_state = TransactionState.NoTransaction;
				_statusVector = null;
				await base.Dispose2(async).ConfigureAwait(false);
			}
		}

		#endregion

		#region Methods

		public override Task BeginTransaction(TransactionParameterBuffer tpb, AsyncWrappingCommonArgs async)
		{
			if (_state != TransactionState.NoTransaction)
			{
				throw new InvalidOperationException();
			}

			var teb = new IscTeb();
			var tebData = IntPtr.Zero;

			try
			{
				ClearStatusVector();

				teb.dbb_ptr = Marshal.AllocHGlobal(4);
				Marshal.WriteInt32(teb.dbb_ptr, _db.Handle);

				teb.tpb_len = tpb.Length;

				teb.tpb_ptr = Marshal.AllocHGlobal(tpb.Length);
				Marshal.Copy(tpb.ToArray(), 0, teb.tpb_ptr, tpb.Length);

				var size = Marshal.SizeOf<IscTeb>();
				tebData = Marshal.AllocHGlobal(size);

				Marshal.StructureToPtr(teb, tebData, true);

				_db.FbClient.isc_start_multiple(
					_statusVector,
					ref _handle,
					1,
					tebData);

				_db.ProcessStatusVector(_statusVector);

				_state = TransactionState.Active;

				_db.TransactionCount++;
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

			return Task.CompletedTask;
		}

		public override Task Commit(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			ClearStatusVector();

			_db.FbClient.isc_commit_transaction(_statusVector, ref _handle);

			_db.ProcessStatusVector(_statusVector);

			_db.TransactionCount--;

			Update?.Invoke(this, new EventArgs());

			_state = TransactionState.NoTransaction;

			return Task.CompletedTask;
		}

		public override Task Rollback(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			ClearStatusVector();

			_db.FbClient.isc_rollback_transaction(_statusVector, ref _handle);

			_db.ProcessStatusVector(_statusVector);

			_db.TransactionCount--;

			Update?.Invoke(this, new EventArgs());

			_state = TransactionState.NoTransaction;

			return Task.CompletedTask;
		}

		public override Task CommitRetaining(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			ClearStatusVector();

			_db.FbClient.isc_commit_retaining(_statusVector, ref _handle);

			_db.ProcessStatusVector(_statusVector);

			_state = TransactionState.Active;

			return Task.CompletedTask;
		}

		public override Task RollbackRetaining(AsyncWrappingCommonArgs async)
		{
			EnsureActiveTransactionState();

			ClearStatusVector();

			_db.FbClient.isc_rollback_retaining(_statusVector, ref _handle);

			_db.ProcessStatusVector(_statusVector);

			_state = TransactionState.Active;

			return Task.CompletedTask;
		}

		#endregion

		#region Two Phase Commit Methods

		public override Task Prepare(AsyncWrappingCommonArgs async)
		{
			return Task.CompletedTask;
		}

		public override Task Prepare(byte[] buffer, AsyncWrappingCommonArgs async)
		{
			return Task.CompletedTask;
		}

		#endregion

		#region Private Methods

		private void ClearStatusVector()
		{
			Array.Clear(_statusVector, 0, _statusVector.Length);
		}

		#endregion
	}
}
