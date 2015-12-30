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
	internal sealed class ExtTransaction : TransactionBase
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

		private int _handle;
		private ExtDatabase _db;
		private TransactionState _state;
		private bool _disposed;
		private int[] _statusVector;

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

		#region IDisposable methods

		protected override void Dispose(bool disposing)
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
					base.Dispose(disposing);
				}
			}
		}

		#endregion

		#region Methods

		public override void BeginTransaction(TransactionParameterBuffer tpb)
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

		public override void Commit()
		{
			if (Update != null)
			{
				Update(this, new EventArgs());
			}
		}

		public override void Rollback()
		{
			if (Update != null)
			{
				Update(this, new EventArgs());
			}
		}

		public override void CommitRetaining()
		{ }

		public override void RollbackRetaining()
		{ }

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
