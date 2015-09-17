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
using System.Data;
using System.IO;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal sealed class GdsTransaction : ITransaction, IDisposable
	{
		#region Events

		public event TransactionUpdateEventHandler Update;

		#endregion

		#region Fields

		private int _handle;
		private bool _disposed;
		private GdsDatabase _database;
		private TransactionState _state;
		private object _stateSyncRoot;

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

		private GdsTransaction()
		{
			_stateSyncRoot = new object();
		}

		public GdsTransaction(IDatabase db)
			: this()
		{
			if (!(db is GdsDatabase))
			{
				throw new ArgumentException("Specified argument is not of GdsDatabase type.");
			}

			_database = (GdsDatabase)db;
			_state = TransactionState.NoTransaction;
		}

		#endregion

		#region Finalizer

		~GdsTransaction()
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
			lock (_stateSyncRoot)
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
						_database = null;
						_handle = 0;
						_state = TransactionState.NoTransaction;
					}

					_disposed = true;
				}
			}
		}

		#endregion

		#region Methods

		public void BeginTransaction(TransactionParameterBuffer tpb)
		{
			lock (_stateSyncRoot)
			{
				if (_state != TransactionState.NoTransaction)
				{
					throw GetNoValidTransactionException();
				}

				try
				{
					GenericResponse response;
					lock (_database.SyncObject)
					{
						_database.Write(IscCodes.op_transaction);
						_database.Write(_database.Handle);
						_database.WriteBuffer(tpb.ToArray());
						_database.Flush();

						response = _database.ReadGenericResponse();

						_database.TransactionCount++;
					}

					_handle = response.ObjectHandle;
					_state = TransactionState.Active;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public void Commit()
		{
			lock (_stateSyncRoot)
			{
				CheckTransactionState();

				try
				{
					lock (_database.SyncObject)
					{
						_database.Write(IscCodes.op_commit);
						_database.Write(_handle);
						_database.Flush();

						_database.ReadResponse();

						_database.TransactionCount--;
					}

					if (Update != null)
					{
						Update(this, new EventArgs());
					}

					_state = TransactionState.NoTransaction;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public void Rollback()
		{
			lock (_stateSyncRoot)
			{
				CheckTransactionState();

				try
				{
					lock (_database.SyncObject)
					{
						_database.Write(IscCodes.op_rollback);
						_database.Write(_handle);
						_database.Flush();

						_database.ReadResponse();

						_database.TransactionCount--;
					}

					if (Update != null)
					{
						Update(this, new EventArgs());
					}

					_state = TransactionState.NoTransaction;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public void CommitRetaining()
		{
			lock (_stateSyncRoot)
			{
				CheckTransactionState();

				try
				{
					lock (_database.SyncObject)
					{
						_database.Write(IscCodes.op_commit_retaining);
						_database.Write(_handle);
						_database.Flush();

						_database.ReadResponse();
					}

					_state = TransactionState.Active;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public void RollbackRetaining()
		{
			lock (_stateSyncRoot)
			{
				CheckTransactionState();

				try
				{
					lock (_database.SyncObject)
					{
						_database.Write(IscCodes.op_rollback_retaining);
						_database.Write(_handle);
						_database.Flush();

						_database.ReadResponse();
					}

					_state = TransactionState.Active;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		#endregion

		#region Two Phase Commit Methods

		public void Prepare()
		{
			lock (_stateSyncRoot)
			{
				CheckTransactionState();

				try
				{
					_state = TransactionState.NoTransaction;

					lock (_database.SyncObject)
					{
						_database.Write(IscCodes.op_prepare);
						_database.Write(_handle);
						_database.Flush();

						_database.ReadResponse();
					}

					_state = TransactionState.Prepared;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public void Prepare(byte[] buffer)
		{
			lock (_stateSyncRoot)
			{
				CheckTransactionState();

				try
				{
					_state = TransactionState.NoTransaction;

					lock (_database.SyncObject)
					{
						_database.Write(IscCodes.op_prepare2);
						_database.Write(_handle);
						_database.WriteBuffer(buffer, buffer.Length);
						_database.Flush();

						_database.ReadResponse();
					}

					_state = TransactionState.Prepared;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		#endregion

		#region Private Methods

		private void CheckTransactionState()
		{
			if (_state != TransactionState.Active)
			{
				throw GetNoValidTransactionException();
			}
		}

		private IscException GetNoValidTransactionException()
		{
			return new IscException(IscCodes.isc_arg_gds, IscCodes.isc_tra_state, _handle, "no valid");
		}

		#endregion
	}
}
