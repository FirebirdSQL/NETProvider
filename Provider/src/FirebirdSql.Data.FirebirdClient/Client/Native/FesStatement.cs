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
using System.Collections;
using System.Text;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.Client.Native.Marshalers;
using FirebirdSql.Data.Client.Native.Handle;

namespace FirebirdSql.Data.Client.Native
{
	internal sealed class FesStatement : StatementBase
	{
		#region Fields

		private StatementHandle _handle;
		private bool _disposed;
		private FesDatabase _db;
		private FesTransaction _transaction;
		private Descriptor _parameters;
		private Descriptor _fields;
		private StatementState _state;
		private DbStatementType _statementType;
		private bool _allRowsFetched;
		private Queue _outputParams;
		private int _recordsAffected;
		private bool _returnRecordsAffected;
		private IntPtr[] _statusVector;
		private IntPtr _fetchSqlDa;

		#endregion

		#region Properties

		public override IDatabase Database
		{
			get { return _db; }
		}

		public override TransactionBase Transaction
		{
			get { return _transaction; }
			set
			{
				if (_transaction != value)
				{
					if (TransactionUpdate != null && _transaction != null)
					{
						_transaction.Update -= TransactionUpdate;
						TransactionUpdate = null;
					}

					if (value == null)
					{
						_transaction = null;
					}
					else
					{
						_transaction = (FesTransaction)value;
						TransactionUpdate = new EventHandler(TransactionUpdated);
						_transaction.Update += TransactionUpdate;
					}
				}
			}
		}

		public override Descriptor Parameters
		{
			get { return _parameters; }
			set { _parameters = value; }
		}

		public override Descriptor Fields
		{
			get { return _fields; }
		}

		public override int RecordsAffected
		{
			get { return _recordsAffected; }
			protected set { _recordsAffected = value; }
		}

		public override bool IsPrepared
		{
			get
			{
				if (_state == StatementState.Deallocated || _state == StatementState.Error)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public override DbStatementType StatementType
		{
			get { return _statementType; }
			protected set { _statementType = value; }
		}

		public override StatementState State
		{
			get { return _state; }
			protected set { _state = value; }
		}

		public override int FetchSize
		{
			get { return 200; }
			set { }
		}

		public override bool ReturnRecordsAffected
		{
			get { return _returnRecordsAffected; }
			set { _returnRecordsAffected = value; }
		}

		#endregion

		#region Constructors

		public FesStatement(IDatabase db)
			: this(db, null)
		{
		}

		public FesStatement(IDatabase db, TransactionBase transaction)
		{
			if (!(db is FesDatabase))
			{
				throw new ArgumentException($"Specified argument is not of {nameof(FesDatabase)} type.");
			}

			_recordsAffected = -1;
			_db = (FesDatabase)db;
			_handle = new StatementHandle();
			_outputParams = new Queue();
			_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
			_fetchSqlDa = IntPtr.Zero;

			if (transaction != null)
			{
				Transaction = transaction;
			}
		}

		#endregion

		#region IDisposable methods

		public override void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				Release();
				Clear();
				_db = null;
				_fields = null;
				_parameters = null;
				_transaction = null;
				_outputParams = null;
				_statusVector = null;
				_allRowsFetched = false;
				_state = StatementState.Deallocated;
				_statementType = DbStatementType.None;
				_recordsAffected = 0;
				_handle.Dispose();
				FetchSize = 0;
				base.Dispose();
			}
		}

		#endregion

		#region Blob Creation Metods

		public override BlobBase CreateBlob()
		{
			return new FesBlob(_db, _transaction);
		}

		public override BlobBase CreateBlob(long blobId)
		{
			return new FesBlob(_db, _transaction, blobId);
		}

		#endregion

		#region Array Creation Methods

		public override ArrayBase CreateArray(ArrayDesc descriptor)
		{
			return new FesArray(descriptor);
		}

		public override ArrayBase CreateArray(string tableName, string fieldName)
		{
			return new FesArray(_db, _transaction, tableName, fieldName);
		}

		public override ArrayBase CreateArray(long handle, string tableName, string fieldName)
		{
			return new FesArray(_db, _transaction, handle, tableName, fieldName);
		}

		#endregion

		#region Methods

		public override void Release()
		{
			XsqldaMarshaler.CleanUpNativeData(ref _fetchSqlDa);

			base.Release();
		}

		public override void Close()
		{
			XsqldaMarshaler.CleanUpNativeData(ref _fetchSqlDa);

			base.Close();
		}

		public override void Prepare(string commandText)
		{
			ClearAll();

			ClearStatusVector();

			if (_state == StatementState.Deallocated)
			{
				Allocate();
			}

			_fields = new Descriptor(1);

			IntPtr sqlda = XsqldaMarshaler.MarshalManagedToNative(_db.Charset, _fields);
			TransactionHandle trHandle = _transaction.HandlePtr;

			byte[] buffer = _db.Charset.GetBytes(commandText);

			_db.FbClient.isc_dsql_prepare(
				_statusVector,
				ref trHandle,
				ref _handle,
				(short)buffer.Length,
				buffer,
				_db.Dialect,
				sqlda);

			Descriptor descriptor = XsqldaMarshaler.MarshalNativeToManaged(_db.Charset, sqlda);

			XsqldaMarshaler.CleanUpNativeData(ref sqlda);

			_db.ProcessStatusVector(_statusVector);

			_fields = descriptor;

			if (_fields.ActualCount > 0 && _fields.ActualCount != _fields.Count)
			{
				Describe();
			}
			else
			{
				if (_fields.ActualCount == 0)
				{
					_fields = new Descriptor(0);
				}
			}

			_fields.ResetValues();

			_statementType = GetStatementType();

			_state = StatementState.Prepared;
		}

		public override void Execute()
		{
			if (_state == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statment is not correctly created.");
			}

			ClearStatusVector();

			IntPtr inSqlda = IntPtr.Zero;
			IntPtr outSqlda = IntPtr.Zero;

			if (_parameters != null)
			{
				inSqlda = XsqldaMarshaler.MarshalManagedToNative(_db.Charset, _parameters);
			}
			if (_statementType == DbStatementType.StoredProcedure)
			{
				Fields.ResetValues();
				outSqlda = XsqldaMarshaler.MarshalManagedToNative(_db.Charset, _fields);
			}

			TransactionHandle trHandle = _transaction.HandlePtr;

			_db.FbClient.isc_dsql_execute2(
				_statusVector,
				ref trHandle,
				ref _handle,
				IscCodes.SQLDA_VERSION1,
				inSqlda,
				outSqlda);

			if (outSqlda != IntPtr.Zero)
			{
				Descriptor descriptor = XsqldaMarshaler.MarshalNativeToManaged(_db.Charset, outSqlda, true);

				DbValue[] values = new DbValue[descriptor.Count];

				for (int i = 0; i < values.Length; i++)
				{
					values[i] = new DbValue(this, descriptor[i]);
				}

				_outputParams.Enqueue(values);
			}

			XsqldaMarshaler.CleanUpNativeData(ref inSqlda);
			XsqldaMarshaler.CleanUpNativeData(ref outSqlda);

			_db.ProcessStatusVector(_statusVector);

			UpdateRecordsAffected();

			_state = StatementState.Executed;
		}

		public override DbValue[] Fetch()
		{
			DbValue[] row = null;

			if (_state == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statement is not correctly created.");
			}
			if (_statementType != DbStatementType.Select &&
				_statementType != DbStatementType.SelectForUpdate)
			{
				return null;
			}

			if (!_allRowsFetched)
			{
				_fields.ResetValues();

				if (_fetchSqlDa == IntPtr.Zero)
				{
					_fetchSqlDa = XsqldaMarshaler.MarshalManagedToNative(_db.Charset, _fields);
				}

				ClearStatusVector();

				IntPtr status = _db.FbClient.isc_dsql_fetch(_statusVector, ref _handle, IscCodes.SQLDA_VERSION1, _fetchSqlDa);

				Descriptor rowDesc = XsqldaMarshaler.MarshalNativeToManaged(_db.Charset, _fetchSqlDa, true);

				if (_fields.Count == rowDesc.Count)
				{
					for (int i = 0; i < _fields.Count; i++)
					{
						if (_fields[i].IsArray() && _fields[i].ArrayHandle != null)
						{
							rowDesc[i].ArrayHandle = _fields[i].ArrayHandle;
						}
					}
				}

				_fields = rowDesc;

				_db.ProcessStatusVector(_statusVector);

				if (status == new IntPtr(100))
				{
					_allRowsFetched = true;

					XsqldaMarshaler.CleanUpNativeData(ref _fetchSqlDa);
				}
				else
				{
					row = new DbValue[_fields.ActualCount];
					for (int i = 0; i < row.Length; i++)
					{
						row[i] = new DbValue(this, _fields[i]);
					}
				}
			}

			return row;
		}

		public override DbValue[] GetOutputParameters()
		{
			if (_outputParams != null && _outputParams.Count > 0)
			{
				return (DbValue[])_outputParams.Dequeue();
			}

			return null;
		}

		public override void Describe()
		{
			ClearStatusVector();

			_fields = new Descriptor(_fields.ActualCount);

			IntPtr sqlda = XsqldaMarshaler.MarshalManagedToNative(_db.Charset, _fields);


			_db.FbClient.isc_dsql_describe(
				_statusVector,
				ref _handle,
				IscCodes.SQLDA_VERSION1,
				sqlda);

			Descriptor descriptor = XsqldaMarshaler.MarshalNativeToManaged(_db.Charset, sqlda);

			XsqldaMarshaler.CleanUpNativeData(ref sqlda);

			_db.ProcessStatusVector(_statusVector);

			_fields = descriptor;
		}

		public override void DescribeParameters()
		{
			ClearStatusVector();

			_parameters = new Descriptor(1);

			IntPtr sqlda = XsqldaMarshaler.MarshalManagedToNative(_db.Charset, _parameters);


			_db.FbClient.isc_dsql_describe_bind(
				_statusVector,
				ref _handle,
				IscCodes.SQLDA_VERSION1,
				sqlda);

			Descriptor descriptor = XsqldaMarshaler.MarshalNativeToManaged(_db.Charset, sqlda);

			_db.ProcessStatusVector(_statusVector);

			if (descriptor.ActualCount != 0 && descriptor.Count != descriptor.ActualCount)
			{
				short n = descriptor.ActualCount;
				descriptor = new Descriptor(n);

				XsqldaMarshaler.CleanUpNativeData(ref sqlda);

				sqlda = XsqldaMarshaler.MarshalManagedToNative(_db.Charset, descriptor);

				_db.FbClient.isc_dsql_describe_bind(
					_statusVector,
					ref _handle,
					IscCodes.SQLDA_VERSION1,
					sqlda);

				descriptor = XsqldaMarshaler.MarshalNativeToManaged(_db.Charset, sqlda);

				XsqldaMarshaler.CleanUpNativeData(ref sqlda);

				_db.ProcessStatusVector(_statusVector);
			}
			else
			{
				if (descriptor.ActualCount == 0)
				{
					descriptor = new Descriptor(0);
				}
			}

			if (sqlda != IntPtr.Zero)
			{
				XsqldaMarshaler.CleanUpNativeData(ref sqlda);
			}

			_parameters = descriptor;
		}

		#endregion

		#region Protected Methods

		protected override void Free(int option)
		{
			// Does	not	seem to	be possible	or necessary to	close
			// an execute procedure	statement.
			if (StatementType == DbStatementType.StoredProcedure && option == IscCodes.DSQL_close)
			{
				return;
			}

			ClearStatusVector();

			_db.FbClient.isc_dsql_free_statement(
				_statusVector,
				ref _handle,
				(short)option);

			if (option == IscCodes.DSQL_drop)
			{
				_parameters = null;
				_fields = null;
			}

			Clear();
			_allRowsFetched = false;

			_db.ProcessStatusVector(_statusVector);
		}

		protected override void TransactionUpdated(object sender, EventArgs e)
		{
			if (Transaction != null && TransactionUpdate != null)
			{
				Transaction.Update -= TransactionUpdate;
			}
			Clear();
			State = StatementState.Closed;
			TransactionUpdate = null;
			_allRowsFetched = false;
		}

		protected override byte[] GetSqlInfo(byte[] items, int bufferLength)
		{
			ClearStatusVector();

			byte[] buffer = new byte[bufferLength];


			_db.FbClient.isc_dsql_sql_info(
				_statusVector,
				ref _handle,
				(short)items.Length,
				items,
				(short)bufferLength,
				buffer);

			_db.ProcessStatusVector(_statusVector);

			return buffer;
		}

		#endregion

		#region Private Methods

		private void ClearStatusVector()
		{
			Array.Clear(_statusVector, 0, _statusVector.Length);
		}

		private void Clear()
		{
			if (_outputParams != null && _outputParams.Count > 0)
			{
				_outputParams.Clear();
			}
		}

		private void ClearAll()
		{
			Clear();

			_parameters = null;
			_fields = null;
		}

		private void Allocate()
		{
			ClearStatusVector();

			DatabaseHandle dbHandle = _db.HandlePtr;

			_db.FbClient.isc_dsql_allocate_statement(
				_statusVector,
				ref dbHandle,
				ref _handle);

			_db.ProcessStatusVector(_statusVector);

			_allRowsFetched = false;
			_state = StatementState.Allocated;
			_statementType = DbStatementType.None;
		}

		private void UpdateRecordsAffected()
		{
			if (ReturnRecordsAffected &&
				(StatementType == DbStatementType.Insert ||
				StatementType == DbStatementType.Delete ||
				StatementType == DbStatementType.Update ||
				StatementType == DbStatementType.StoredProcedure))
			{
				_recordsAffected = GetRecordsAffected();
			}
			else
			{
				_recordsAffected = -1;
			}
		}

		#endregion
	}
}
