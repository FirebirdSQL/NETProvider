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

		private int handle;
		private FesDatabase db;
		private TransactionState state;
		private bool disposed;
		private IntPtr[] statusVector;

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

		public FesTransaction(IDatabase db)
		{
			if (!(db is FesDatabase))
			{
				throw new ArgumentException("Specified argument is not of FesDatabase type.");
			}

			this.db = (FesDatabase)db;
			this.state = TransactionState.NoTransaction;
			this.statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];

			GC.SuppressFinalize(this);
		}

		#endregion

		#region Finalizer

		~FesTransaction()
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
					}
					catch
					{
					}
					finally
					{
						// release any managed resources
						if (disposing)
						{
							this.db = null;
							this.handle = 0;
							this.state = TransactionState.NoTransaction;
							this.statusVector = null;
						}

						this.disposed = true;
					}
				}
			}
		}

		#endregion

		#region Methods

		public void BeginTransaction(TransactionParameterBuffer tpb)
		{
			if (this.state != TransactionState.NoTransaction)
			{
				throw new IscException(IscCodes.isc_arg_gds, IscCodes.isc_tra_state, this.handle, "no valid");
			}

			lock (this.db)
			{
				IscTeb teb = new IscTeb();
				IntPtr tebData = IntPtr.Zero;

				try
				{
					// Clear the status vector
					this.ClearStatusVector();

					// Set db handle
					teb.dbb_ptr = Marshal.AllocHGlobal(4);
					Marshal.WriteInt32(teb.dbb_ptr, this.db.Handle);

					// Set tpb length
					teb.tpb_len = tpb.Length;

					// Set TPB data
					teb.tpb_ptr = Marshal.AllocHGlobal(tpb.Length);
					Marshal.Copy(tpb.ToArray(), 0, teb.tpb_ptr, tpb.Length);

					// Alloc memory	for	the	IscTeb structure
					int size = Marshal.SizeOf(typeof(IscTeb));
					tebData = Marshal.AllocHGlobal(size);

					Marshal.StructureToPtr(teb, tebData, true);

					int trHandle = this.handle;

					db.FbClient.isc_start_multiple(
						this.statusVector,
						ref	trHandle,
						1,
						tebData);

					this.handle = trHandle;

					// Parse status	vector
					this.db.ParseStatusVector(this.statusVector);

					// Update transaction state
					this.state = TransactionState.Active;

					// Update transaction count
					this.db.TransactionCount++;
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
			this.CheckTransactionState();

			lock (this.db)
			{
				// Clear the status vector
				this.ClearStatusVector();

				int trHandle = this.handle;

				db.FbClient.isc_commit_transaction(this.statusVector, ref trHandle);

				this.handle = trHandle;

				this.db.ParseStatusVector(this.statusVector);

				this.db.TransactionCount--;

				if (this.Update != null)
				{
					this.Update(this, new EventArgs());
				}

				this.state = TransactionState.NoTransaction;
			}
		}

		public void Rollback()
		{
			this.CheckTransactionState();

			lock (this.db)
			{
				// Clear the status vector
				this.ClearStatusVector();

				int trHandle = this.handle;

				db.FbClient.isc_rollback_transaction(this.statusVector, ref trHandle);

				this.handle = trHandle;

				this.db.ParseStatusVector(this.statusVector);

				this.db.TransactionCount--;

				if (this.Update != null)
				{
					this.Update(this, new EventArgs());
				}

				this.state = TransactionState.NoTransaction;
			}
		}

		public void CommitRetaining()
		{
			this.CheckTransactionState();

			lock (this.db)
			{
				// Clear the status vector
				this.ClearStatusVector();

				int trHandle = this.handle;

				db.FbClient.isc_commit_retaining(this.statusVector, ref trHandle);

				this.db.ParseStatusVector(this.statusVector);

				this.state = TransactionState.Active;
			}
		}

		public void RollbackRetaining()
		{
			this.CheckTransactionState();

			lock (this.db)
			{
				// Clear the status vector
				this.ClearStatusVector();

				int trHandle = this.handle;

				db.FbClient.isc_rollback_retaining(this.statusVector, ref trHandle);

				this.db.ParseStatusVector(this.statusVector);

				this.state = TransactionState.Active;
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
			Array.Clear(this.statusVector, 0, this.statusVector.Length);
		}

		private void CheckTransactionState()
		{
			if (this.state != TransactionState.Active)
			{
				throw new IscException(IscCodes.isc_arg_gds, IscCodes.isc_tra_state, this.handle, "no valid");
			}
		}

		#endregion
	}
}
