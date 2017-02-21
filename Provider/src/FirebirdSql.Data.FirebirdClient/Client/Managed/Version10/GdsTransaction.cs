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
	internal sealed class GdsTransaction : TransactionBase
	{
		#region Events

		public override event EventHandler Update;

		#endregion

		#region Fields

		private int _handle;
		private bool _disposed;
		private GdsDatabase _database;
		private TransactionState _state;

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

		public GdsTransaction(IDatabase db)
		{
			if (!(db is GdsDatabase))
			{
				throw new ArgumentException($"Specified argument is not of {nameof(GdsDatabase)} type.");
			}

			_database = (GdsDatabase)db;
			_state = TransactionState.NoTransaction;
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
				_database = null;
				_handle = 0;
				_state = TransactionState.NoTransaction;
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

			try
			{
				GenericResponse response;
				_database.XdrStream.Write(IscCodes.op_transaction);
				_database.XdrStream.Write(_database.Handle);
				_database.XdrStream.WriteBuffer(tpb.ToArray());
				_database.XdrStream.Flush();

				response = _database.ReadGenericResponse();

				_database.TransactionCount++;

				_handle = response.ObjectHandle;
				_state = TransactionState.Active;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		public override void Commit()
		{
			EnsureActiveTransactionState();

			try
			{
				_database.XdrStream.Write(IscCodes.op_commit);
				_database.XdrStream.Write(_handle);
				_database.XdrStream.Flush();

				_database.ReadResponse();

				_database.TransactionCount--;

				Update?.Invoke(this, new EventArgs());

				_state = TransactionState.NoTransaction;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		public override void Rollback()
		{
			EnsureActiveTransactionState();

			try
			{
				_database.XdrStream.Write(IscCodes.op_rollback);
				_database.XdrStream.Write(_handle);
				_database.XdrStream.Flush();

				_database.ReadResponse();

				_database.TransactionCount--;

				Update?.Invoke(this, new EventArgs());

				_state = TransactionState.NoTransaction;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		public override void CommitRetaining()
		{
			EnsureActiveTransactionState();

			try
			{
				_database.XdrStream.Write(IscCodes.op_commit_retaining);
				_database.XdrStream.Write(_handle);
				_database.XdrStream.Flush();

				_database.ReadResponse();

				_state = TransactionState.Active;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		public override void RollbackRetaining()
		{
			EnsureActiveTransactionState();

			try
			{
				_database.XdrStream.Write(IscCodes.op_rollback_retaining);
				_database.XdrStream.Write(_handle);
				_database.XdrStream.Flush();

				_database.ReadResponse();

				_state = TransactionState.Active;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		#endregion

		#region Two Phase Commit Methods

		public override void Prepare()
		{
			EnsureActiveTransactionState();

			try
			{
				_state = TransactionState.NoTransaction;

				_database.XdrStream.Write(IscCodes.op_prepare);
				_database.XdrStream.Write(_handle);
				_database.XdrStream.Flush();

				_database.ReadResponse();

				_state = TransactionState.Prepared;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		public override void Prepare(byte[] buffer)
		{
			EnsureActiveTransactionState();

			try
			{
				_state = TransactionState.NoTransaction;

				_database.XdrStream.Write(IscCodes.op_prepare2);
				_database.XdrStream.Write(_handle);
				_database.XdrStream.WriteBuffer(buffer, buffer.Length);
				_database.XdrStream.Flush();

				_database.ReadResponse();

				_state = TransactionState.Prepared;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		#endregion
	}
}
