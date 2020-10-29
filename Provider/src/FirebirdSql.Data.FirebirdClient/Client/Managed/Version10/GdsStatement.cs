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
using System.Diagnostics;
using System.IO;
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
		protected bool _allRowsFetched;
		private Queue<DbValue[]> _rows;
		private Queue<DbValue[]> _outputParams;
		private int _fetchSize;

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

		public override int FetchSize
		{
			get { return _fetchSize; }
			set { _fetchSize = value; }
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
				_handle = 0;
				_fetchSize = 0;
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
				if (State == StatementState.Deallocated)
				{
					SendAllocateToBuffer();
					_database.Xdr.Flush();
					ProcessAllocateResponce(_database.ReadResponse<GenericResponse>());
				}

				SendPrepareToBuffer(commandText);
				_database.Xdr.Flush();
				ProcessPrepareResponse(_database.ReadResponse<GenericResponse>());

				SendInfoSqlToBuffer(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE);
				_database.Xdr.Flush();
				StatementType = ProcessStatementTypeInfoBuffer(ProcessInfoSqlResponse(_database.ReadResponse<GenericResponse>()));

				State = StatementState.Prepared;
			}
			catch (IOException ex)
			{
				State = State == StatementState.Allocated ? StatementState.Error : State;
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		public override void Execute()
		{
			if (State == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statement is not correctly created.");
			}

			Clear();

			try
			{
				SendExecuteToBuffer();

				_database.Xdr.Flush();

				if (StatementType == DbStatementType.StoredProcedure)
				{
					ProcessStoredProcedureExecuteResponse(_database.ReadResponse<SqlResponse>());
				}

				var executeResponse = _database.ReadResponse<GenericResponse>();
				ProcessExecuteResponse(executeResponse);

				if (DoRecordsAffected)
				{
					SendInfoSqlToBuffer(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE);
					_database.Xdr.Flush();
					RecordsAffected = ProcessRecordsAffectedBuffer(ProcessInfoSqlResponse(_database.ReadResponse<GenericResponse>()));
				}
				else
				{
					RecordsAffected = -1;
				}

				State = StatementState.Executed;
			}
			catch (IOException ex)
			{
				State = StatementState.Error;
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		public override DbValue[] Fetch()
		{
			if (State == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statement is not correctly created.");
			}
			if (StatementType == DbStatementType.StoredProcedure && !_allRowsFetched)
			{
				_allRowsFetched = true;
				return GetOutputParameters();
			}
			else if (StatementType == DbStatementType.Insert && _allRowsFetched)
			{
				return null;
			}
			else if (StatementType != DbStatementType.Select && StatementType != DbStatementType.SelectForUpdate)
			{
				return null;
			}

			if (!_allRowsFetched && _rows.Count == 0)
			{
				try
				{
					_database.Xdr.Write(IscCodes.op_fetch);
					_database.Xdr.Write(_handle);
					_database.Xdr.WriteBuffer(_fields.ToBlrArray());
					_database.Xdr.Write(0); // p_sqldata_message_number
					_database.Xdr.Write(_fetchSize); // p_sqldata_messages
					_database.Xdr.Flush();

					var operation = _database.ReadOperation();
					if (operation == IscCodes.op_fetch_response)
					{
						var hasOperation = true;
						while (!_allRowsFetched)
						{
							var response = hasOperation ? _database.ReadResponse(operation) : _database.ReadResponse();
							hasOperation = false;
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
						_database.ReadResponse(operation);
					}
				}
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
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
			_database.Xdr.Write(IscCodes.op_prepare_statement);
			_database.Xdr.Write(_transaction.Handle);
			_database.Xdr.Write(_handle);
			_database.Xdr.Write((int)_database.Dialect);
			_database.Xdr.Write(commandText);
			_database.Xdr.WriteBuffer(DescribeInfoAndBindInfoItems, DescribeInfoAndBindInfoItems.Length);
			_database.Xdr.Write(IscCodes.PREPARE_INFO_BUFFER_SIZE);
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
			_database.Xdr.Flush();
			return ProcessInfoSqlResponse(_database.ReadResponse<GenericResponse>());
		}

		protected void DoInfoSqlPacket(byte[] items, int bufferLength)
		{
			try
			{
				SendInfoSqlToBuffer(items, bufferLength);
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		protected void SendInfoSqlToBuffer(byte[] items, int bufferLength)
		{
			_database.Xdr.Write(IscCodes.op_info_sql);
			_database.Xdr.Write(_handle);
			_database.Xdr.Write(0);
			_database.Xdr.WriteBuffer(items, items.Length);
			_database.Xdr.Write(bufferLength);
		}

		protected byte[] ProcessInfoSqlResponse(GenericResponse response)
		{
			Debug.Assert(response.Data != null && response.Data.Length > 0);
			return response.Data;
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
			// does not seem to be possible or necessary to close an execute procedure statement
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
				_database.Xdr.Write(IscCodes.op_free_statement);
				_database.Xdr.Write(_handle);
				_database.Xdr.Write(option);
				_database.Xdr.Flush();

				if (option == IscCodes.DSQL_drop)
				{
					_parameters = null;
					_fields = null;
				}

				Clear();
			}
			catch (IOException ex)
			{
				State = StatementState.Error;
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		protected void ProcessFreeResponse(IResponse response)
		{

		}
		#endregion

		#region op_allocate_statement methods
		protected void SendAllocateToBuffer()
		{
			_database.Xdr.Write(IscCodes.op_allocate_statement);
			_database.Xdr.Write(_database.Handle);
		}

		protected void ProcessAllocateResponce(GenericResponse response)
		{
			_handle = response.ObjectHandle;
			_allRowsFetched = false;
			State = StatementState.Allocated;
			StatementType = DbStatementType.None;
		}
		#endregion

		#region op_execute/op_execute2 methods
		protected void SendExecuteToBuffer()
		{
			// this may throw error, so it needs to be before any writing
			var descriptor = WriteParameters();

			if (StatementType == DbStatementType.StoredProcedure)
			{
				_database.Xdr.Write(IscCodes.op_execute2);
			}
			else
			{
				_database.Xdr.Write(IscCodes.op_execute);
			}

			_database.Xdr.Write(_handle);
			_database.Xdr.Write(_transaction.Handle);

			if (_parameters != null)
			{
				_database.Xdr.WriteBuffer(_parameters.ToBlrArray());
				_database.Xdr.Write(0); // Message number
				_database.Xdr.Write(1); // Number of messages
				_database.Xdr.WriteBytes(descriptor, descriptor.Length);
			}
			else
			{
				_database.Xdr.WriteBuffer(null);
				_database.Xdr.Write(0);
				_database.Xdr.Write(0);
			}

			if (StatementType == DbStatementType.StoredProcedure)
			{
				_database.Xdr.WriteBuffer(_fields?.ToBlrArray());
				_database.Xdr.Write(0); // Output message number
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
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}
		#endregion

		protected override void TransactionUpdated(object sender, EventArgs e)
		{
			if (Transaction != null && TransactionUpdate != null)
			{
				Transaction.Update -= TransactionUpdate;
			}

			State = StatementState.Closed;
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
							var len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
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
							len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							currentItemIndex = (int)IscHelper.VaxInteger(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_type:
							len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].DataType = (short)IscHelper.VaxInteger(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_sub_type:
							len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].SubType = (short)IscHelper.VaxInteger(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_scale:
							len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].NumericScale = (short)IscHelper.VaxInteger(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_length:
							len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].Length = (short)IscHelper.VaxInteger(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_field:
							len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].Name = _database.Charset.GetString(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_relation:
							len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].Relation = _database.Charset.GetString(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_owner:
							len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
							currentPosition += 2;
							rowDescs[currentDescriptorIndex][currentItemIndex - 1].Owner = _database.Charset.GetString(info, currentPosition, len);
							currentPosition += len;
							break;

						case IscCodes.isc_info_sql_alias:
							len = (int)IscHelper.VaxInteger(info, currentPosition, 2);
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

		protected void WriteRawParameter(IXdrWriter xdr, DbField field)
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
						else if (field.Charset.IsNoneCharset)
						{
							var bvalue = field.Charset.GetBytes(field.DbValue.GetString());
							if (bvalue.Length > field.Length)
							{
								throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
							}
							xdr.WriteOpaque(bvalue, field.Length);
						}
						else
						{
							var svalue = field.DbValue.GetString();
							if ((field.Length % field.Charset.BytesPerCharacter) == 0 && svalue.Length > field.CharCount)
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
						else if (field.Charset.IsNoneCharset)
						{
							var bvalue = field.Charset.GetBytes(field.DbValue.GetString());
							if (bvalue.Length > field.Length)
							{
								throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
							}
							xdr.WriteBuffer(bvalue);
						}
						else
						{
							var svalue = field.DbValue.GetString();
							if ((field.Length % field.Charset.BytesPerCharacter) == 0 && svalue.Length > field.CharCount)
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

					case DbDataType.TimeStampTZ:
						xdr.Write(field.DbValue.GetDate());
						xdr.Write(field.DbValue.GetTime());
						xdr.Write(field.DbValue.GetTimeZoneId());
						break;

					case DbDataType.TimeStampTZEx:
						xdr.Write(field.DbValue.GetDate());
						xdr.Write(field.DbValue.GetTime());
						xdr.Write(field.DbValue.GetTimeZoneId());
						xdr.Write((short)0);
						break;

					case DbDataType.TimeTZ:
						xdr.Write(field.DbValue.GetTime());
						xdr.Write(field.DbValue.GetTimeZoneId());
						break;

					case DbDataType.TimeTZEx:
						xdr.Write(field.DbValue.GetTime());
						xdr.Write(field.DbValue.GetTimeZoneId());
						xdr.Write((short)0);
						break;

					case DbDataType.Dec16:
						xdr.Write(field.DbValue.GetDec16(), 16);
						break;

					case DbDataType.Dec34:
						xdr.Write(field.DbValue.GetDec34(), 34);
						break;

					case DbDataType.Int128:
						xdr.Write(field.DbValue.GetInt128());
						break;

					default:
						throw IscException.ForStrParam($"Unknown SQL data type: {field.DataType}.");
				}
			}
		}

		protected object ReadRawValue(IXdrReader xdr, DbField field)
		{
			var innerCharset = !_database.Charset.IsNoneCharset ? _database.Charset : field.Charset;

			switch (field.DbDataType)
			{
				case DbDataType.Char:
					if (field.Charset.IsOctetsCharset)
					{
						return xdr.ReadOpaque(field.Length);
					}
					else
					{
						var s = xdr.ReadString(innerCharset, field.Length);
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
						return xdr.ReadBuffer();
					}
					else
					{
						return xdr.ReadString(innerCharset);
					}

				case DbDataType.SmallInt:
					return xdr.ReadInt16();

				case DbDataType.Integer:
					return xdr.ReadInt32();

				case DbDataType.Array:
				case DbDataType.Binary:
				case DbDataType.Text:
				case DbDataType.BigInt:
					return xdr.ReadInt64();

				case DbDataType.Decimal:
				case DbDataType.Numeric:
					return xdr.ReadDecimal(field.DataType, field.NumericScale);

				case DbDataType.Float:
					return xdr.ReadSingle();

				case DbDataType.Guid:
					return xdr.ReadGuid();

				case DbDataType.Double:
					return xdr.ReadDouble();

				case DbDataType.Date:
					return xdr.ReadDate();

				case DbDataType.Time:
					return xdr.ReadTime();

				case DbDataType.TimeStamp:
					return xdr.ReadDateTime();

				case DbDataType.Boolean:
					return xdr.ReadBoolean();

				case DbDataType.TimeStampTZ:
					return xdr.ReadZonedDateTime(false);

				case DbDataType.TimeStampTZEx:
					return xdr.ReadZonedDateTime(true);

				case DbDataType.TimeTZ:
					return xdr.ReadZonedTime(false);

				case DbDataType.TimeTZEx:
					return xdr.ReadZonedTime(true);

				case DbDataType.Dec16:
					return xdr.ReadDec16();

				case DbDataType.Dec34:
					return xdr.ReadDec34();

				case DbDataType.Int128:
					return xdr.ReadInt128();

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

			using (var ms = new MemoryStream())
			{
				var xdr = new XdrReaderWriter(ms, _database.Charset);
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
						throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
					}
				}
				xdr.Flush();
				return ms.ToArray();
			}
		}

		protected virtual DbValue[] ReadRow()
		{
			var row = new DbValue[_fields.Count];
			try
			{
				for (var i = 0; i < _fields.Count; i++)
				{
					var value = ReadRawValue(_database.Xdr, _fields[i]);
					var sqlInd = _database.Xdr.ReadInt32();
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
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
			return row;
		}

		#endregion
	}
}
