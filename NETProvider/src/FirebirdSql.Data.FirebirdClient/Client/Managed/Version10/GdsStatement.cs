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
 *	Copyright (c) 2002 - 2007 Carlos Guzman Alvarez
 *	Copyright (c) 2007 - 2009 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal class GdsStatement : StatementBase
	{
		#region Fields

		protected int _handle;
		protected GdsDatabase _database;
		private GdsTransaction _transaction;
		protected Descriptor _parameters;
		protected Descriptor _fields;
		protected StatementState _state;
		protected DbStatementType _statementType;
		protected bool _allRowsFetched;
		private Queue<DbValue[]> _rows;
		private Queue<DbValue[]> _outputParams;
		private int _fetchSize;
		private bool _returnRecordsAffected;

		#endregion

		#region Properties

		public override IDatabase Database
		{
			get { return _database; }
		}

		public override ITransaction Transaction
		{
			get { return _transaction; }
			set
			{
				if (_transaction != value)
				{
					if (_TransactionUpdate != null && _transaction != null)
					{
						_transaction.Update -= _TransactionUpdate;
						_TransactionUpdate = null;
					}

					if (value == null)
					{
						_transaction = null;
					}
					else
					{
						_transaction = (GdsTransaction)value;
						_TransactionUpdate = new TransactionUpdateEventHandler(TransactionUpdated);
						_transaction.Update += _TransactionUpdate;
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

		public override int RecordsAffected { get; protected set; }

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
			get { return _fetchSize; }
			set { _fetchSize = value; }
		}

		public override bool ReturnRecordsAffected
		{
			get { return _returnRecordsAffected; }
			set { _returnRecordsAffected = value; }
		}

		#endregion

		#region Constructors

		public GdsStatement(IDatabase db)
			: this(db, null)
		{
		}

		public GdsStatement(IDatabase db, ITransaction transaction)
		{
			if (!(db is GdsDatabase))
			{
				throw new ArgumentException("Specified argument is not of GdsDatabase type.");
			}
			if (transaction != null && !(transaction is GdsTransaction))
			{
				throw new ArgumentException("Specified argument is not of GdsTransaction type.");
			}

			_handle = IscCodes.INVALID_OBJECT;
			RecordsAffected = -1;
			_fetchSize = 200;
			_rows = new Queue<DbValue[]>();
			_outputParams = new Queue<DbValue[]>();

			_database = (GdsDatabase)db;

			if (transaction != null)
			{
				Transaction = transaction;
			}
		}

		#endregion

		#region IDisposable methods

		protected override void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!IsDisposed)
				{
					try
					{
						Release();
					}
					catch
					{ }

					if (disposing)
					{
						Clear();

						_rows = null;
						_outputParams = null;
						_database = null;
						_fields = null;
						_parameters = null;
						_transaction = null;
						_allRowsFetched = false;
						_state = StatementState.Deallocated;
						_statementType = DbStatementType.None;
						_handle = 0;
						_fetchSize = 0;
						RecordsAffected = 0;
					}

					base.Dispose(disposing);
				}
			}
		}

		#endregion

		#region Blob Creation Metods

		public override BlobBase CreateBlob()
		{
			return new GdsBlob(_database, _transaction);
		}

		public override BlobBase CreateBlob(long blobId)
		{
			return new GdsBlob(_database, _transaction, blobId);
		}

		#endregion

		#region Array Creation Methods

		public override ArrayBase CreateArray(ArrayDesc descriptor)
		{
			return new GdsArray(descriptor);
		}

		public override ArrayBase CreateArray(string tableName, string fieldName)
		{
			return new GdsArray(_database, _transaction, tableName, fieldName);
		}

		public override ArrayBase CreateArray(long handle, string tableName, string fieldName)
		{
			return new GdsArray(_database, _transaction, handle, tableName, fieldName);
		}

		#endregion

		#region Methods

		public override void Prepare(string commandText)
		{
			// Clear data
			ClearAll();

			lock (_database.SyncObject)
			{
				try
				{
					if (_state == StatementState.Deallocated)
					{
						// Allocate statement
						SendAllocateToBuffer();
						_database.Flush();
						ProcessAllocateResponce(_database.ReadGenericResponse());
					}

					SendPrepareToBuffer(commandText);
					_database.Flush();
					ProcessPrepareResponse(_database.ReadGenericResponse());

					// Grab statement type
					SendInfoSqlToBuffer(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE);
					_database.Flush();
					_statementType = ProcessStatementTypeInfoBuffer(ProcessInfoSqlResponse(_database.ReadGenericResponse()));


					_state = StatementState.Prepared;
				}
				catch (IOException)
				{
					// if the statement has been already allocated, it's now in error
					if (_state == StatementState.Allocated)
						_state = StatementState.Error;
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public override void Execute()
		{
			if (_state == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statement is not correctly created.");
			}

			// Clear data
			Clear();

			lock (_database.SyncObject)
			{
				try
				{
					RecordsAffected = -1;

					SendExecuteToBuffer();

					_database.Flush();

					if (_statementType == DbStatementType.StoredProcedure)
					{
						ProcessStoredProcedureExecuteResponse(_database.ReadSqlResponse());
					}

					GenericResponse executeResponse = _database.ReadGenericResponse();
					ProcessExecuteResponse(executeResponse);

					// Updated number of records affected by the statement execution
					if (ReturnRecordsAffected &&
						(StatementType == DbStatementType.Insert ||
						StatementType == DbStatementType.Delete ||
						StatementType == DbStatementType.Update ||
						StatementType == DbStatementType.StoredProcedure))
					{
						SendInfoSqlToBuffer(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE);
						_database.Flush();
						RecordsAffected = ProcessRecordsAffectedBuffer(ProcessInfoSqlResponse(_database.ReadGenericResponse()));
					}

					_state = StatementState.Executed;
				}
				catch (IOException)
				{
					_state = StatementState.Error;
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public override DbValue[] Fetch()
		{
			if (_state == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statement is not correctly created.");
			}
			if (_statementType != DbStatementType.Select &&
				_statementType != DbStatementType.SelectForUpdate)
			{
				return null;
			}

			if (!_allRowsFetched && _rows.Count == 0)
			{
				// Fetch next batch of rows
				lock (_database.SyncObject)
				{
					try
					{
						_database.Write(IscCodes.op_fetch);
						_database.Write(_handle);
						_database.WriteBuffer(_fields.ToBlrArray());
						_database.Write(0);             // p_sqldata_message_number
						_database.Write(_fetchSize);    // p_sqldata_messages
						_database.Flush();

						if (_database.NextOperation() == IscCodes.op_fetch_response)
						{
							IResponse response = null;

							while (!_allRowsFetched)
							{
								response = _database.ReadResponse();

								if (response is FetchResponse)
								{
									FetchResponse fetchResponse = (FetchResponse)response;

									if (fetchResponse.Count > 0 && fetchResponse.Status == 0)
									{
										_rows.Enqueue(ReadDataRow());
									}
									else if (fetchResponse.Status == 100)
									{
										_allRowsFetched = true;
									}
									else
									{
										break;
									}
								}
								else
								{
									break;
								}
							}
						}
						else
						{
							_database.ReadResponse();
						}
					}
					catch (IOException)
					{
						throw new IscException(IscCodes.isc_net_read_err);
					}
				}
			}

			if (_rows != null && _rows.Count > 0)
			{
				return _rows.Dequeue();
			}
			else
			{
				_rows.Clear();

				return null;
			}
		}

		public override DbValue[] GetOutputParameters()
		{
			if (_outputParams.Count > 0)
			{
				return _outputParams.Dequeue();
			}

			return null;
		}

		public override void Describe()
		{
			System.Diagnostics.Debug.Assert(true);
		}
		// these are not needed for Gds, because it's pre-fetched in Prepare
		// maybe we can fetch these also for Fes and Ext etc.
		public override void DescribeParameters()
		{
			System.Diagnostics.Debug.Assert(true);
		}

		#endregion

		#region Protected Methods

		#region op_prepare methods
		protected void SendPrepareToBuffer(string commandText)
		{
			_database.Write(IscCodes.op_prepare_statement);
			_database.Write(_transaction.Handle);
			_database.Write(_handle);
			_database.Write((int)_database.Dialect);
			_database.Write(commandText);
			_database.WriteBuffer(DescribeInfoAndBindInfoItems, DescribeInfoAndBindInfoItems.Length);
			_database.Write(IscCodes.PREPARE_INFO_BUFFER_SIZE);
		}

		protected void ProcessPrepareResponse(GenericResponse response)
		{
			Descriptor[] descriptors = new Descriptor[] { null, null };
			ParseSqlInfo(response.Data, DescribeInfoAndBindInfoItems, ref descriptors);
			_fields = descriptors[0];
			_parameters = descriptors[1];
		}
		#endregion

		#region op_info_sql methods
		protected override byte[] GetSqlInfo(byte[] items, int bufferLength)
		{
			lock (_database.SyncObject)
			{
				DoInfoSqlPacket(items, bufferLength);
				_database.Flush();
				return ProcessInfoSqlResponse(_database.ReadGenericResponse());
			}
		}

		protected void DoInfoSqlPacket(byte[] items, int bufferLength)
		{
			try
			{
				SendInfoSqlToBuffer(items, bufferLength);
			}
			catch (IOException)
			{
				throw new IscException(IscCodes.isc_net_read_err);
			}
		}

		protected void SendInfoSqlToBuffer(byte[] items, int bufferLength)
		{
			_database.Write(IscCodes.op_info_sql);
			_database.Write(_handle);
			_database.Write(0);
			_database.WriteBuffer(items, items.Length);
			_database.Write(bufferLength);
		}

		protected byte[] ProcessInfoSqlResponse(GenericResponse respose)
		{
			System.Diagnostics.Debug.Assert(respose.Data != null && respose.Data.Length > 0);
			return respose.Data;
		}
		#endregion

		#region op_free_statement methods
		protected override void Free(int option)
		{
			if (FreeNotNeeded(option))
				return;

			lock (_database.SyncObject)
			{
				DoFreePacket(option);
				_database.Flush();
				ProcessFreeResponse(_database.ReadResponse());
			}
		}

		protected bool FreeNotNeeded(int option)
		{
			// Does not seem to be possible or necessary to close
			// an execute procedure statement.
			if (StatementType == DbStatementType.StoredProcedure && option == IscCodes.DSQL_close)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		protected void DoFreePacket(int option)
		{
			try
			{
				SendFreeToBuffer(option);

				// Reset statement information
				if (option == IscCodes.DSQL_drop)
				{
					_parameters = null;
					_fields = null;
				}

				Clear();
			}
			catch (IOException)
			{
				_state = StatementState.Error;
				throw new IscException(IscCodes.isc_net_read_err);
			}
		}

		protected void SendFreeToBuffer(int option)
		{
			_database.Write(IscCodes.op_free_statement);
			_database.Write(_handle);
			_database.Write(option);
		}

		protected void ProcessFreeResponse(IResponse response)
		{

		}
		#endregion

		#region op_allocate_statement methods
		protected void SendAllocateToBuffer()
		{
			_database.Write(IscCodes.op_allocate_statement);
			_database.Write(_database.Handle);
		}

		protected void ProcessAllocateResponce(GenericResponse response)
		{
			_handle = response.ObjectHandle;
			_allRowsFetched = false;
			_state = StatementState.Allocated;
			_statementType = DbStatementType.None;
		}
		#endregion

		#region op_execute/op_execute2 methods
		protected void SendExecuteToBuffer()
		{
			// this may throw error, so it needs to be before any writing
			byte[] descriptor = null;
			if (_parameters != null)
			{
				using (XdrStream xdr = new XdrStream(_database.Charset))
				{
					xdr.Write(_parameters);
					descriptor = xdr.ToArray();
				}
			}

			// Write the message
			if (_statementType == DbStatementType.StoredProcedure)
			{
				_database.Write(IscCodes.op_execute2);
			}
			else
			{
				_database.Write(IscCodes.op_execute);
			}

			_database.Write(_handle);
			_database.Write(_transaction.Handle);

			if (_parameters != null)
			{
				_database.WriteBuffer(_parameters.ToBlrArray());
				_database.Write(0); // Message number
				_database.Write(1); // Number of messages
				_database.Write(descriptor, 0, descriptor.Length);
			}
			else
			{
				_database.WriteBuffer(null);
				_database.Write(0);
				_database.Write(0);
			}

			if (_statementType == DbStatementType.StoredProcedure)
			{
				_database.WriteBuffer((_fields == null) ? null : _fields.ToBlrArray());
				_database.Write(0); // Output message number
			}
		}

		protected void ProcessExecuteResponse(GenericResponse response)
		{
			// nothing to do here
		}

		protected void ProcessStoredProcedureExecuteResponse(SqlResponse response)
		{
			try
			{
				if (response.Count > 0)
				{
					_outputParams.Enqueue(ReadDataRow());
				}
			}
			catch (IOException)
			{
				throw new IscException(IscCodes.isc_net_read_err);
			}
		}
		#endregion

		protected override void TransactionUpdated(object sender, EventArgs e)
		{
			lock (this)
			{
				if (Transaction != null && _TransactionUpdate != null)
				{
					Transaction.Update -= _TransactionUpdate;
				}

				_state = StatementState.Closed;
				_TransactionUpdate = null;
				_allRowsFetched = false;
			}
		}

		protected DbValue[] ReadDataRow()
		{
			DbValue[] row = new DbValue[_fields.Count];
			object value = null;

			lock (_database.SyncObject)
			{
				// This only works if not (port->port_flags & PORT_symmetric)
				for (int i = 0; i < _fields.Count; i++)
				{
					try
					{
						value = _database.ReadValue(_fields[i]);
						row[i] = new DbValue(this, _fields[i], value);
					}
					catch (IOException)
					{
						throw new IscException(IscCodes.isc_net_read_err);
					}
				}
			}

			return row;
		}

		protected void ParseSqlInfo(byte[] info, byte[] items, ref Descriptor[] rowDescs)
		{
			ParseTruncSqlInfo(info, items, ref rowDescs);
		}

		protected void ParseTruncSqlInfo(byte[] info, byte[] items, ref Descriptor[] rowDescs)
		{
			int currentPosition = 0;
			int currentDescriptorIndex = -1;
			int currentItemIndex = 0;
			while (info[currentPosition] != IscCodes.isc_info_end)
			{
				bool jumpOutOfInnerLoop = false;
				byte item;
				while ((item = info[currentPosition++]) != IscCodes.isc_info_sql_describe_end)
				{
					switch (item)
					{
						case IscCodes.isc_info_truncated:
							currentItemIndex--;

							List<byte> newItems = new List<byte>(items.Length);
							int part = 0;
							int chock = 0;
							for (int i = 0; i < items.Length; i++)
							{
								if (items[i] == IscCodes.isc_info_sql_describe_end)
								{
									newItems.Insert(chock, IscCodes.isc_info_sql_sqlda_start);
									newItems.Insert(chock + 1, 2);

									short processedItems = (rowDescs[part] != null ? rowDescs[part].Count : (short)0);
									newItems.Insert(chock + 2, (byte)((part == currentDescriptorIndex ? currentItemIndex : processedItems) & 255));
									newItems.Insert(chock + 3, (byte)((part == currentDescriptorIndex ? currentItemIndex : processedItems) >> 8));

									part++;
									chock = i + 4 + 1;
								}
								newItems.Add(items[i]);
							}

							info = GetSqlInfo(newItems.ToArray(), info.Length);

							currentPosition = 0;
							currentDescriptorIndex = -1;
							jumpOutOfInnerLoop = true;
							break;

						case IscCodes.isc_info_sql_select:
						case IscCodes.isc_info_sql_bind:
							currentDescriptorIndex++;

							if (info[currentPosition] == IscCodes.isc_info_truncated)
								break;

							currentPosition++;
							int len = IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							if (rowDescs[currentDescriptorIndex] == null)
							{
								int n = IscHelper.VaxInteger(info, currentPosition, len);
								rowDescs[currentDescriptorIndex] = new Descriptor((short)n);
								jumpOutOfInnerLoop = (n == 0);
							}
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_sqlda_seq:
							len = IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							currentItemIndex = IscHelper.VaxInteger(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_type:
							len = IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].DataType = (short)IscHelper.VaxInteger(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_sub_type:
							len = IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].SubType = (short)IscHelper.VaxInteger(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_scale:
							len = IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].NumericScale = (short)IscHelper.VaxInteger(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_length:
							len = IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].Length = (short)IscHelper.VaxInteger(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_field:
							len = IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].Name = _database.Charset.GetString(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_relation:
							len = IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].Relation = _database.Charset.GetString(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_owner:
							len = IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].Owner = _database.Charset.GetString(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_alias:
							len = IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].Alias = _database.Charset.GetString(info, currentPosition, len);
							currentPosition += len;
							break;

						default:
							throw new IscException(IscCodes.isc_dsql_sqlda_err);
					}
					if (jumpOutOfInnerLoop)
						break;
				}
			}
		}

		protected void Clear()
		{
			if (_rows != null && _rows.Count > 0)
			{
				_rows.Clear();
			}
			if (_outputParams != null && _outputParams.Count > 0)
			{
				_outputParams.Clear();
			}

			_allRowsFetched = false;
		}

		protected void ClearAll()
		{
			Clear();

			_parameters = null;
			_fields = null;
		}

		#endregion
	}
}
