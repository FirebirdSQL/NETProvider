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

namespace FirebirdSql.Data.Client.Native
{
	internal sealed class FesTransaction : ITransaction, IDisposable
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

		public event TransactionUpdateEventHandler Update;

		#endregion

		#region Fields

		private int _handle;
		private FesDatabase _db;
		private TransactionState _state;
		private bool _disposed;
		private IntPtr[] _statusVector;

		#endregion

		#region Properties

		public int Handle
		{
			get { return _handle; }
		}

		public TransactionState State
		{
			get { return _state; }
		}

		#endregion

		#region Constructors

		public FesTransaction(IDatabase db)
		{
			if (!(db is FesDatabase))
			{
				throw new ArgumentException("Specified argument is not of FesDatabase type.");
			}

			_db = (FesDatabase)db;
			_state = TransactionState.NoTransaction;
			_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region Finalizer

		~FesTransaction()
		{
			Dispose(false);
		}

		#endregion

		#region IDisposable methods

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!_disposed)
				{
					try
					{
						Rollback();
					}
					catch
					{ }

					if (disposing)
					{
						_db = null;
						_handle = 0;
						_state = TransactionState.NoTransaction;
						_statusVector = null;
					}

					_disposed = true;
				}
			}
		}

		#endregion

		#region Methods

		public void BeginTransaction(TransactionParameterBuffer tpb)
		{
			if (_state != TransactionState.NoTransaction)
			{
				throw new IscException(IscCodes.isc_arg_gds, IscCodes.isc_tra_state, _handle, "no valid");
			}

			lock (_db)
			{
				IscTeb teb = new IscTeb();
				IntPtr tebData = IntPtr.Zero;

				try
				{
					// Clear the status vector
					ClearStatusVector();

					// Set db handle
					teb.dbb_ptr = Marshal.AllocHGlobal(4);
					Marshal.WriteInt32(teb.dbb_ptr, _db.Handle);

					// Set tpb length
					teb.tpb_len = tpb.Length;

					// Set TPB data
					teb.tpb_ptr = Marshal.AllocHGlobal(tpb.Length);
					Marshal.Copy(tpb.ToArray(), 0, teb.tpb_ptr, tpb.Length);

					// Alloc memory	for	the	IscTeb structure
					int size = Marshal.SizeOf(typeof(IscTeb));
					tebData = Marshal.AllocHGlobal(size);

					Marshal.StructureToPtr(teb, tebData, true);

					int trHandle = _handle;

					_db.FbClient.isc_start_multiple(
						_statusVector,
						ref trHandle,
						1,
						tebData);

					_handle = trHandle;

					// Parse status	vector
					_db.ParseStatusVector(_statusVector);

					// Update transaction state
					_state = TransactionState.Active;

					// Update transaction count
					_db.TransactionCount++;
				}
				catch
				{
					throw;
				}
				finally
				{
					// Free	memory
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
						Marshal.DestroyStructure(tebData, typeof(IscTeb));
						Marshal.FreeHGlobal(tebData);
					}
				}
			}
		}

		public void Commit()
		{
			CheckTransactionState();

			lock (_db)
			{
				// Clear the status vector
				ClearStatusVector();

				int trHandle = _handle;

				_db.FbClient.isc_commit_transaction(_statusVector, ref trHandle);

				_handle = trHandle;

				_db.ParseStatusVector(_statusVector);

				_db.TransactionCount--;

				if (Update != null)
				{
					Update(this, new EventArgs());
				}

				_state = TransactionState.NoTransaction;
			}
		}

		public void Rollback()
		{
			CheckTransactionState();

			lock (_db)
			{
				// Clear the status vector
				ClearStatusVector();

				int trHandle = _handle;

				_db.FbClient.isc_rollback_transaction(_statusVector, ref trHandle);

				_handle = trHandle;

				_db.ParseStatusVector(_statusVector);

				_db.TransactionCount--;

				if (Update != null)
				{
					Update(this, new EventArgs());
				}

				_state = TransactionState.NoTransaction;
			}
		}

		public void CommitRetaining()
		{
			CheckTransactionState();

			lock (_db)
			{
				// Clear the status vector
				ClearStatusVector();

				int trHandle = _handle;

				_db.FbClient.isc_commit_retaining(_statusVector, ref trHandle);

				_db.ParseStatusVector(_statusVector);

				_state = TransactionState.Active;
			}
		}

		public void RollbackRetaining()
		{
			CheckTransactionState();

			lock (_db)
			{
				// Clear the status vector
				ClearStatusVector();

				int trHandle = _handle;

				_db.FbClient.isc_rollback_retaining(_statusVector, ref trHandle);

				_db.ParseStatusVector(_statusVector);

				_state = TransactionState.Active;
			}
		}

		#endregion

		#region Two Phase Commit Methods

		void ITransaction.Prepare()
		{
		}

		void ITransaction.Prepare(byte[] buffer)
		{
		}

		#endregion

		#region Private Methods

		private void ClearStatusVector()
		{
			Array.Clear(_statusVector, 0, _statusVector.Length);
		}

		private void CheckTransactionState()
		{
			if (_state != TransactionState.Active)
			{
				throw new IscException(IscCodes.isc_arg_gds, IscCodes.isc_tra_state, _handle, "no valid");
			}
		}

		#endregion
	}
}
