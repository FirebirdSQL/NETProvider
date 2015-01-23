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

		private int handle;
		private ExtDatabase db;
		private TransactionState state;
		private bool disposed;
		private int[] statusVector;

		#endregion

		#region Properties

		public int Handle
		{
			get { return this.handle; }
		}

		public TransactionState State
		{
			get { return this.state; }
		}

		#endregion

		#region Constructors

		public ExtTransaction(IDatabase db)
		{
			if (!(db is ExtDatabase))
			{
				throw new ArgumentException("Specified argument is not of FesDatabase type.");
			}

			this.db = (ExtDatabase)db;
			this.state = TransactionState.NoTransaction;
			this.statusVector = new int[IscCodes.ISC_STATUS_LENGTH];

			GC.SuppressFinalize(this);
		}

		#endregion

		#region Finalizer

		~ExtTransaction()
		{
			this.Dispose(false);
		}

		#endregion

		#region IDisposable methods

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!this.disposed)
				{
					try
					{
						// release any unmanaged resources
						this.Rollback();

						// release any managed resources
						if (disposing)
						{
							this.db = null;
							this.handle = 0;
							this.state = TransactionState.NoTransaction;
							this.statusVector = null;
						}
					}
					finally
					{
						this.disposed = true;
					}
				}
			}
		}

		#endregion

		#region Methods

		public void BeginTransaction(TransactionParameterBuffer tpb)
		{
			// Clear the status vector
			this.ClearStatusVector();

			int trHandle = 0;

			lock (this.db)
			{
				SafeNativeMethods.isc_get_current_transaction(this.statusVector, ref trHandle);

				this.handle = trHandle;
				this.state = TransactionState.Active;
			}
		}

		public void Commit()
		{
			if (this.Update != null)
			{
				this.Update(this, new EventArgs());
			}
		}

		public void Rollback()
		{
			if (this.Update != null)
			{
				this.Update(this, new EventArgs());
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
			Array.Clear(this.statusVector, 0, this.statusVector.Length);
		}

		#endregion
	}
}
