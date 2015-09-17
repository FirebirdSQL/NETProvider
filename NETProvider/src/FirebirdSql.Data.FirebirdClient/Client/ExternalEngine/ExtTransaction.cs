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
 *	Copyright (c) 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.IO;
using System.Data;
using System.Runtime.InteropServices;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.ExternalEngine
{
	internal sealed class ExtTransaction : ITransaction, IDisposable
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
		private ExtDatabase _db;
		private TransactionState _state;
		private bool _disposed;
		private int[] _statusVector;

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

		public ExtTransaction(IDatabase db)
		{
			if (!(db is ExtDatabase))
			{
				throw new ArgumentException("Specified argument is not of FesDatabase type.");
			}

			_db = (ExtDatabase)db;
			_state = TransactionState.NoTransaction;
			_statusVector = new int[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region Finalizer

		~ExtTransaction()
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
			// Clear the status vector
			ClearStatusVector();

			int trHandle = 0;

			lock (_db)
			{
				SafeNativeMethods.isc_get_current_transaction(_statusVector, ref trHandle);

				_handle = trHandle;
				_state = TransactionState.Active;
			}
		}

		public void Commit()
		{
			if (Update != null)
			{
				Update(this, new EventArgs());
			}
		}

		public void Rollback()
		{
			if (Update != null)
			{
				Update(this, new EventArgs());
			}
		}

		public void CommitRetaining()
		{
		}

		public void RollbackRetaining()
		{
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

		#endregion
	}
}
