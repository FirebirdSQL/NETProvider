/*
 *	Firebird ADO.NET Data provider for .NET and Mono
 *
 *	   The contents of this file are subject to the Initial
 *	   Developer's Public License Version 1.0 (the "License");
 *	   you may not use this file except in compliance with the
 *	   License. You may obtain a copy of the License at
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *	   express or implied. See the License for the specific
 *	   language governing rights and limitations under the License.
 *
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.IO;
using System.Data;
using System.Runtime.InteropServices;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.Client.Native.Handle;

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

		#region IDisposable methods

		public override void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				if (_state != TransactionState.NoTransaction)
				{
					Rollback();
				}
				_db = null;
				_handle.Dispose();
				_state = TransactionState.NoTransaction;
				_statusVector = null;
				base.Dispose();
			}
		}

		#endregion

		#region Methods

		public override void BeginTransaction(TransactionParameterBuffer tpb)
		{
			if (_state != TransactionState.NoTransaction)
			{
				throw new InvalidOperationException();
			}

			IscTeb teb = new IscTeb();
			IntPtr tebData = IntPtr.Zero;

			try
			{
				ClearStatusVector();

				teb.dbb_ptr = Marshal.AllocHGlobal(4);
				Marshal.WriteInt32(teb.dbb_ptr, _db.Handle);

				teb.tpb_len = tpb.Length;

				teb.tpb_ptr = Marshal.AllocHGlobal(tpb.Length);
				Marshal.Copy(tpb.ToArray(), 0, teb.tpb_ptr, tpb.Length);

				int size = Marshal2.SizeOf<IscTeb>();
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
			catch
			{
				throw;
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
					Marshal2.DestroyStructure<IscTeb>(tebData);
					Marshal.FreeHGlobal(tebData);
				}
			}
		}

		public override void Commit()
		{
			EnsureActiveTransactionState();

			ClearStatusVector();

			_db.FbClient.isc_commit_transaction(_statusVector, ref _handle);

			_db.ProcessStatusVector(_statusVector);

			_db.TransactionCount--;

			Update?.Invoke(this, new EventArgs());

			_state = TransactionState.NoTransaction;
		}

		public override void Rollback()
		{
			EnsureActiveTransactionState();

			ClearStatusVector();

			_db.FbClient.isc_rollback_transaction(_statusVector, ref _handle);

			_db.ProcessStatusVector(_statusVector);

			_db.TransactionCount--;

			Update?.Invoke(this, new EventArgs());

			_state = TransactionState.NoTransaction;
		}

		public override void CommitRetaining()
		{
			EnsureActiveTransactionState();

			ClearStatusVector();

			_db.FbClient.isc_commit_retaining(_statusVector, ref _handle);

			_db.ProcessStatusVector(_statusVector);

			_state = TransactionState.Active;
		}

		public override void RollbackRetaining()
		{
			EnsureActiveTransactionState();

			ClearStatusVector();

			_db.FbClient.isc_rollback_retaining(_statusVector, ref _handle);

			_db.ProcessStatusVector(_statusVector);

			_state = TransactionState.Active;
		}

		#endregion

		#region Two Phase Commit Methods

		public override void Prepare()
		{ }

		public override void Prepare(byte[] buffer)
		{ }

		#endregion

		#region Private Methods

		private void ClearStatusVector()
		{
			Array.Clear(_statusVector, 0, _statusVector.Length);
		}

		#endregion
	}
}
