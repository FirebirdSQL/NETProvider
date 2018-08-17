/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal class GdsStatement : StatementBase
	{
		#region Fields

		protected int _handle;
		private bool _disposed;
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
						_transaction = (GdsTransaction)value;
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

		public GdsStatement(IDatabase db, TransactionBase transaction)
		{
			if (!(db is GdsDatabase))
			{
				throw new ArgumentException($"Specified argument is not of {nameof(GdsDatabase)} type.");
			}
			if (transaction != null && !(transaction is GdsTransaction))
			{
				throw new ArgumentException($"Specified argument is not of {nameof(GdsTransaction)} type.");
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

		public override void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				Release();
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
				base.Dispose();
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
			ClearAll();

			try
			{
				if (_state == StatementState.Deallocated)
				{
					SendAllocateToBuffer();
					_database.XdrStream.Flush();
					ProcessAllocateResponce(_database.ReadGenericResponse());
				}

				SendPrepareToBuffer(commandText);
				_database.XdrStream.Flush();
				ProcessPrepareResponse(_database.ReadGenericResponse());

				SendInfoSqlToBuffer(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE);
				_database.XdrStream.Flush();
				_statementType = ProcessStatementTypeInfoBuffer(ProcessInfoSqlResponse(_database.ReadGenericResponse()));


				_state = StatementState.Prepared;
			}
			catch (IOException ex)
			{
				if (_state == StatementState.Allocated)
					_state = StatementState.Error;
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		public override void Execute()
		{
			if (_state == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statement is not correctly created.");
			}

			Clear();

			try
			{
				RecordsAffected = -1;

				SendExecuteToBuffer();

				_database.XdrStream.Flush();

				if (_statementType == DbStatementType.StoredProcedure)
				{
					ProcessStoredProcedureExecuteResponse(_database.ReadSqlResponse());
				}

				var executeResponse = _database.ReadGenericResponse();
				ProcessExecuteResponse(executeResponse);

				if (ReturnRecordsAffected &&
					(StatementType == DbStatementType.Insert ||
					StatementType == DbStatementType.Delete ||
					StatementType == DbStatementType.Update ||
					StatementType == DbStatementType.StoredProcedure))
				{
					SendInfoSqlToBuffer(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE);
					_database.XdrStream.Flush();
					RecordsAffected = ProcessRecordsAffectedBuffer(ProcessInfoSqlResponse(_database.ReadGenericResponse()));
				}

				_state = StatementState.Executed;
			}
			catch (IOException ex)
			{
				_state = StatementState.Error;
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		public override DbValue[] Fetch()
		{
			if (_state == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statement is not correctly created.");
			}
			if (_statementType == DbStatementType.StoredProcedure && !_allRowsFetched)
			{
				_allRowsFetched = true;
				return GetOutputParameters();
			}
			else if (_statementType == DbStatementType.Insert && _allRowsFetched)
			{
				return null;
			}
			else if (_statementType != DbStatementType.Select && _statementType != DbStatementType.SelectForUpdate)
			{
				return null;
			}

			if (!_allRowsFetched && _rows.Count == 0)
			{
				try
				{
					_database.XdrStream.Write(IscCodes.op_fetch);
					_database.XdrStream.Write(_handle);
					_database.XdrStream.WriteBuffer(_fields.ToBlrArray());
					_database.XdrStream.Write(0); // p_sqldata_message_number
					_database.XdrStream.Write(_fetchSize); // p_sqldata_messages
					_database.XdrStream.Flush();

					if (_database.NextOperation() == IscCodes.op_fetch_response)
					{
						IResponse response = null;

						while (!_allRowsFetched)
						{
							response = _database.ReadResponse();

							if (response is FetchResponse fetchResponse)
							{
								if (fetchResponse.Count > 0 && fetchResponse.Status == 0)
								{
									_rows.Enqueue(ReadRow());
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
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
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
			Debug.Assert(false, "Nothing for Gds, because it's pre-fetched in Prepare.");
		}

		public override void DescribeParameters()
		{
			Debug.Assert(false, "Nothing for Gds, because it's pre-fetched in Prepare.");
		}

		#endregion

		#region Protected Methods

		#region op_prepare methods
		protected void SendPrepareToBuffer(string commandText)
		{
			_database.XdrStream.Write(IscCodes.op_prepare_statement);
			_database.XdrStream.Write(_transaction.Handle);
			_database.XdrStream.Write(_handle);
			_database.XdrStream.Write((int)_database.Dialect);
			_database.XdrStream.Write(commandText);
			_database.XdrStream.WriteBuffer(DescribeInfoAndBindInfoItems, DescribeInfoAndBindInfoItems.Length);
			_database.XdrStream.Write(IscCodes.PREPARE_INFO_BUFFER_SIZE);
		}

		protected void ProcessPrepareResponse(GenericResponse response)
		{
			var descriptors = new Descriptor[] { null, null };
			ParseSqlInfo(response.Data, DescribeInfoAndBindInfoItems, ref descriptors);
			_fields = descriptors[0];
			_parameters = descriptors[1];
		}
		#endregion

		#region op_info_sql methods
		protected override byte[] GetSqlInfo(byte[] items, int bufferLength)
		{
			DoInfoSqlPacket(items, bufferLength);
			_database.XdrStream.Flush();
			return ProcessInfoSqlResponse(_database.ReadGenericResponse());
		}

		protected void DoInfoSqlPacket(byte[] items, int bufferLength)
		{
			try
			{
				SendInfoSqlToBuffer(items, bufferLength);
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		protected void SendInfoSqlToBuffer(byte[] items, int bufferLength)
		{
			_database.XdrStream.Write(IscCodes.op_info_sql);
			_database.XdrStream.Write(_handle);
			_database.XdrStream.Write(0);
			_database.XdrStream.WriteBuffer(items, items.Length);
			_database.XdrStream.Write(bufferLength);
		}

		protected byte[] ProcessInfoSqlResponse(GenericResponse respose)
		{
			Debug.Assert(respose.Data != null && respose.Data.Length > 0);
			return respose.Data;
		}
		#endregion

		#region op_free_statement methods
		protected override void Free(int option)
		{
			if (FreeNotNeeded(option))
				return;

			DoFreePacket(option);
			ProcessFreeResponse(_database.ReadResponse());
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
				_database.XdrStream.Write(IscCodes.op_free_statement);
				_database.XdrStream.Write(_handle);
				_database.XdrStream.Write(option);
				_database.XdrStream.Flush();

				if (option == IscCodes.DSQL_drop)
				{
					_parameters = null;
					_fields = null;
				}

				Clear();
			}
			catch (IOException ex)
			{
				_state = StatementState.Error;
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		protected void ProcessFreeResponse(IResponse response)
		{

		}
		#endregion

		#region op_allocate_statement methods
		protected void SendAllocateToBuffer()
		{
			_database.XdrStream.Write(IscCodes.op_allocate_statement);
			_database.XdrStream.Write(_database.Handle);
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
			var descriptor = WriteParameters();

			if (_statementType == DbStatementType.StoredProcedure)
			{
				_database.XdrStream.Write(IscCodes.op_execute2);
			}
			else
			{
				_database.XdrStream.Write(IscCodes.op_execute);
			}

			_database.XdrStream.Write(_handle);
			_database.XdrStream.Write(_transaction.Handle);

			if (_parameters != null)
			{
				_database.XdrStream.WriteBuffer(_parameters.ToBlrArray());
				_database.XdrStream.Write(0); // Message number
				_database.XdrStream.Write(1); // Number of messages
				_database.XdrStream.Write(descriptor, 0, descriptor.Length);
			}
			else
			{
				_database.XdrStream.WriteBuffer(null);
				_database.XdrStream.Write(0);
				_database.XdrStream.Write(0);
			}

			if (_statementType == DbStatementType.StoredProcedure)
			{
				_database.XdrStream.WriteBuffer(_fields?.ToBlrArray());
				_database.XdrStream.Write(0); // Output message number
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
					_outputParams.Enqueue(ReadRow());
				}
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}
		#endregion

		protected override void TransactionUpdated(object sender, EventArgs e)
		{
			if (Transaction != null && TransactionUpdate != null)
			{
				Transaction.Update -= TransactionUpdate;
			}

			_state = StatementState.Closed;
			TransactionUpdate = null;
			_allRowsFetched = false;
		}

		protected void ParseSqlInfo(byte[] info, byte[] items, ref Descriptor[] rowDescs)
		{
			ParseTruncSqlInfo(info, items, ref rowDescs);
		}

		protected void ParseTruncSqlInfo(byte[] info, byte[] items, ref Descriptor[] rowDescs)
		{
			var currentPosition = 0;
			var currentDescriptorIndex = -1;
			var currentItemIndex = 0;
			while (info[currentPosition] != IscCodes.isc_info_end)
			{
				byte item;
				while ((item = info[currentPosition++]) != IscCodes.isc_info_sql_describe_end)
				{
					switch (item)
					{
						case IscCodes.isc_info_truncated:
							currentItemIndex--;

							var newItems = new List<byte>(items.Length);
							var part = 0;
							var chock = 0;
							for (var i = 0; i < items.Length; i++)
							{
								if (items[i] == IscCodes.isc_info_sql_describe_end)
								{
									newItems.Insert(chock, IscCodes.isc_info_sql_sqlda_start);
									newItems.Insert(chock + 1, 2);

									var processedItems = (rowDescs[part] != null ? rowDescs[part].Count : (short)0);
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
							goto Break;

						case IscCodes.isc_info_sql_select:
						case IscCodes.isc_info_sql_bind:
							currentDescriptorIndex++;

							if (info[currentPosition] == IscCodes.isc_info_truncated)
								break;

							currentPosition++;
							var len = IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							if (rowDescs[currentDescriptorIndex] == null)
							{
								var n = IscHelper.VaxInteger(info, currentPosition, len);
								rowDescs[currentDescriptorIndex] = new Descriptor((short)n);
								if (n == 0)
								{
									currentPosition += len;
									goto Break;
								}
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
							throw IscException.ForErrorCode(IscCodes.isc_dsql_sqlda_err);
					}
				}
				// just to get out of the loop
				Break:
				{ }
			}
		}

		protected void WriteRawParameter(XdrStream xdr, DbField field)
		{
			if (field.DbDataType != DbDataType.Null)
			{
				field.FixNull();

				switch (field.DbDataType)
				{
					case DbDataType.Char:
						if (field.Charset.IsOctetsCharset)
						{
							xdr.WriteOpaque(field.DbValue.GetBinary(), field.Length);
						}
						else
						{
							var svalue = field.DbValue.GetString();

							if ((field.Length % field.Charset.BytesPerCharacter) == 0 &&
								svalue.Length > field.CharCount)
							{
								throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
							}

							xdr.WriteOpaque(field.Charset.GetBytes(svalue), field.Length);
						}
						break;

					case DbDataType.VarChar:
						if (field.Charset.IsOctetsCharset)
						{
							xdr.WriteBuffer(field.DbValue.GetBinary());
						}
						else
						{
							var svalue = field.DbValue.GetString();

							if ((field.Length % field.Charset.BytesPerCharacter) == 0 &&
								svalue.Length > field.CharCount)
							{
								throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
							}

							xdr.WriteBuffer(field.Charset.GetBytes(svalue));
						}
						break;

					case DbDataType.SmallInt:
						xdr.Write(field.DbValue.GetInt16());
						break;

					case DbDataType.Integer:
						xdr.Write(field.DbValue.GetInt32());
						break;

					case DbDataType.BigInt:
					case DbDataType.Array:
					case DbDataType.Binary:
					case DbDataType.Text:
						xdr.Write(field.DbValue.GetInt64());
						break;

					case DbDataType.Decimal:
					case DbDataType.Numeric:
						xdr.Write(field.DbValue.GetDecimal(), field.DataType, field.NumericScale);
						break;

					case DbDataType.Float:
						xdr.Write(field.DbValue.GetFloat());
						break;

					case DbDataType.Guid:
						xdr.Write(field.DbValue.GetGuid());
						break;

					case DbDataType.Double:
						xdr.Write(field.DbValue.GetDouble());
						break;

					case DbDataType.Date:
						xdr.Write(field.DbValue.GetDate());
						break;

					case DbDataType.Time:
						xdr.Write(field.DbValue.GetTime());
						break;

					case DbDataType.TimeStamp:
						xdr.Write(field.DbValue.GetDate());
						xdr.Write(field.DbValue.GetTime());
						break;

					case DbDataType.Boolean:
						xdr.Write(field.DbValue.GetBoolean());
						break;

					default:
						throw IscException.ForStrParam($"Unknown SQL data type: {field.DataType}.");
				}
			}
		}

		protected object ReadRawValue(DbField field)
		{
			var innerCharset = !_database.Charset.IsNoneCharset ? _database.Charset : field.Charset;

			switch (field.DbDataType)
			{
				case DbDataType.Char:
					if (field.Charset.IsOctetsCharset)
					{
						return _database.XdrStream.ReadOpaque(field.Length);
					}
					else
					{
						var s = _database.XdrStream.ReadString(innerCharset, field.Length);
						if ((field.Length % field.Charset.BytesPerCharacter) == 0 &&
							s.Length > field.CharCount)
						{
							return s.Substring(0, field.CharCount);
						}
						else
						{
							return s;
						}
					}

				case DbDataType.VarChar:
					if (field.Charset.IsOctetsCharset)
					{
						return _database.XdrStream.ReadBuffer();
					}
					else
					{
						return _database.XdrStream.ReadString(innerCharset);
					}

				case DbDataType.SmallInt:
					return _database.XdrStream.ReadInt16();

				case DbDataType.Integer:
					return _database.XdrStream.ReadInt32();

				case DbDataType.Array:
				case DbDataType.Binary:
				case DbDataType.Text:
				case DbDataType.BigInt:
					return _database.XdrStream.ReadInt64();

				case DbDataType.Decimal:
				case DbDataType.Numeric:
					return _database.XdrStream.ReadDecimal(field.DataType, field.NumericScale);

				case DbDataType.Float:
					return _database.XdrStream.ReadSingle();

				case DbDataType.Guid:
					return _database.XdrStream.ReadGuid();

				case DbDataType.Double:
					return _database.XdrStream.ReadDouble();

				case DbDataType.Date:
					return _database.XdrStream.ReadDate();

				case DbDataType.Time:
					return _database.XdrStream.ReadTime();

				case DbDataType.TimeStamp:
					return _database.XdrStream.ReadDateTime();

				case DbDataType.Boolean:
					return _database.XdrStream.ReadBoolean();

				default:
					throw TypeHelper.InvalidDataType((int)field.DbDataType);
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

		protected virtual byte[] WriteParameters()
		{
			if (_parameters == null)
				return null;

			using (var xdr = new XdrStream(_database.Charset))
			{
				for (var i = 0; i < _parameters.Count; i++)
				{
					var field = _parameters[i];
					try
					{
						WriteRawParameter(xdr, field);
						xdr.Write(field.NullFlag);
					}
					catch (IOException ex)
					{
						throw IscException.ForErrorCode(IscCodes.isc_net_write_err, ex);
					}
				}

				return xdr.ToArray();
			}
		}

		protected virtual DbValue[] ReadRow()
		{
			var row = new DbValue[_fields.Count];
			try
			{
				for (var i = 0; i < _fields.Count; i++)
				{
					var value = ReadRawValue(_fields[i]);
					var sqlInd = _database.XdrStream.ReadInt32();
					if (sqlInd == -1)
					{
						row[i] = new DbValue(this, _fields[i], null);
					}
					else if (sqlInd == 0)
					{
						row[i] = new DbValue(this, _fields[i], value);
					}
					else
					{
						throw IscException.ForStrParam($"Invalid {nameof(sqlInd)} value: {sqlInd}.");
					}
				}
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
			return row;
		}

		#endregion
	}
}
