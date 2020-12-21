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
using System.Threading.Tasks;
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

		#region Dispose2

		public override async Task Dispose2(AsyncWrappingCommonArgs async)
		{
			if (!_disposed)
			{
				_disposed = true;
				await Release(async).ConfigureAwait(false);
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
				await base.Dispose2(async).ConfigureAwait(false);
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

		public override Task<ArrayBase> CreateArray(ArrayDesc descriptor, AsyncWrappingCommonArgs async)
		{
			var array = new GdsArray(descriptor);
			return Task.FromResult((ArrayBase)array);
		}

		public override async Task<ArrayBase> CreateArray(string tableName, string fieldName, AsyncWrappingCommonArgs async)
		{
			var array = new GdsArray(_database, _transaction, tableName, fieldName);
			await array.Initialize(async).ConfigureAwait(false);
			return array;
		}

		public override async Task<ArrayBase> CreateArray(long handle, string tableName, string fieldName, AsyncWrappingCommonArgs async)
		{
			var array = new GdsArray(_database, _transaction, handle, tableName, fieldName);
			await array.Initialize(async).ConfigureAwait(false);
			return array;
		}

		#endregion

		#region Methods

		public override async Task Prepare(string commandText, AsyncWrappingCommonArgs async)
		{
			ClearAll();

			try
			{
				if (State == StatementState.Deallocated)
				{
					await SendAllocateToBuffer(async).ConfigureAwait(false);
					await _database.Xdr.Flush(async).ConfigureAwait(false);
					await ProcessAllocateResponse((GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false), async).ConfigureAwait(false);
				}

				await SendPrepareToBuffer(commandText, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);
				await ProcessPrepareResponse((GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false), async).ConfigureAwait(false);

				await SendInfoSqlToBuffer(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);
				StatementType = ProcessStatementTypeInfoBuffer(await ProcessInfoSqlResponse((GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false), async).ConfigureAwait(false));

				State = StatementState.Prepared;
			}
			catch (IOException ex)
			{
				State = State == StatementState.Allocated ? StatementState.Error : State;
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		public override async Task Execute(AsyncWrappingCommonArgs async)
		{
			EnsureNotDeallocated();

			Clear();

			try
			{
				await SendExecuteToBuffer(async).ConfigureAwait(false);

				await _database.Xdr.Flush(async).ConfigureAwait(false);

				if (StatementType == DbStatementType.StoredProcedure)
				{
					await ProcessStoredProcedureExecuteResponse((SqlResponse)await _database.ReadResponse(async).ConfigureAwait(false), async).ConfigureAwait(false);
				}

				var executeResponse = (GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false);
				await ProcessExecuteResponse(executeResponse, async).ConfigureAwait(false);

				if (DoRecordsAffected)
				{
					await SendInfoSqlToBuffer(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE, async).ConfigureAwait(false);
					await _database.Xdr.Flush(async).ConfigureAwait(false);
					RecordsAffected = ProcessRecordsAffectedBuffer(await ProcessInfoSqlResponse((GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false), async).ConfigureAwait(false));
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

		public override async Task<DbValue[]> Fetch(AsyncWrappingCommonArgs async)
		{
			EnsureNotDeallocated();

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
					await _database.Xdr.Write(IscCodes.op_fetch, async).ConfigureAwait(false);
					await _database.Xdr.Write(_handle, async).ConfigureAwait(false);
					await _database.Xdr.WriteBuffer(_fields.ToBlrArray(), async).ConfigureAwait(false);
					await _database.Xdr.Write(0, async).ConfigureAwait(false); // p_sqldata_message_number
					await _database.Xdr.Write(_fetchSize, async).ConfigureAwait(false); // p_sqldata_messages
					await _database.Xdr.Flush(async).ConfigureAwait(false);

					var operation = await _database.ReadOperation(async).ConfigureAwait(false);
					if (operation == IscCodes.op_fetch_response)
					{
						var hasOperation = true;
						while (!_allRowsFetched)
						{
							var response = hasOperation
								? await _database.ReadResponse(operation, async).ConfigureAwait(false)
								: await _database.ReadResponse(async).ConfigureAwait(false);
							hasOperation = false;
							if (response is FetchResponse fetchResponse)
							{
								if (fetchResponse.Count > 0 && fetchResponse.Status == 0)
								{
									_rows.Enqueue(await ReadRow(async).ConfigureAwait(false));
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
						await _database.ReadResponse(operation, async).ConfigureAwait(false);
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

		public override Task Describe(AsyncWrappingCommonArgs async)
		{
			// Nothing for Gds, because it's pre-fetched in Prepare.
			return Task.CompletedTask;
		}

		public override Task DescribeParameters(AsyncWrappingCommonArgs async)
		{
			// Nothing for Gds, because it's pre-fetched in Prepare.
			return Task.CompletedTask;
		}

		#endregion

		#region Protected Methods

		#region op_prepare methods
		protected async Task SendPrepareToBuffer(string commandText, AsyncWrappingCommonArgs async)
		{
			await _database.Xdr.Write(IscCodes.op_prepare_statement, async).ConfigureAwait(false);
			await _database.Xdr.Write(_transaction.Handle, async).ConfigureAwait(false);
			await _database.Xdr.Write(_handle, async).ConfigureAwait(false);
			await _database.Xdr.Write((int)_database.Dialect, async).ConfigureAwait(false);
			await _database.Xdr.Write(commandText, async).ConfigureAwait(false);
			await _database.Xdr.WriteBuffer(DescribeInfoAndBindInfoItems, DescribeInfoAndBindInfoItems.Length, async).ConfigureAwait(false);
			await _database.Xdr.Write(IscCodes.PREPARE_INFO_BUFFER_SIZE, async).ConfigureAwait(false);
		}

		protected async Task ProcessPrepareResponse(GenericResponse response, AsyncWrappingCommonArgs async)
		{
			var descriptors = await ParseSqlInfo(response.Data, DescribeInfoAndBindInfoItems, new Descriptor[] { null, null }, async).ConfigureAwait(false);
			_fields = descriptors[0];
			_parameters = descriptors[1];
		}
		#endregion

		#region op_info_sql methods
		protected override async Task<byte[]> GetSqlInfo(byte[] items, int bufferLength, AsyncWrappingCommonArgs async)
		{
			await DoInfoSqlPacket(items, bufferLength, async).ConfigureAwait(false);
			await _database.Xdr.Flush(async).ConfigureAwait(false);
			return await ProcessInfoSqlResponse((GenericResponse)await _database.ReadResponse(async).ConfigureAwait(false), async).ConfigureAwait(false);
		}

		protected async Task DoInfoSqlPacket(byte[] items, int bufferLength, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendInfoSqlToBuffer(items, bufferLength, async).ConfigureAwait(false);
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		protected async Task SendInfoSqlToBuffer(byte[] items, int bufferLength, AsyncWrappingCommonArgs async)
		{
			await _database.Xdr.Write(IscCodes.op_info_sql, async).ConfigureAwait(false);
			await _database.Xdr.Write(_handle, async).ConfigureAwait(false);
			await _database.Xdr.Write(0, async).ConfigureAwait(false);
			await _database.Xdr.WriteBuffer(items, items.Length, async).ConfigureAwait(false);
			await _database.Xdr.Write(bufferLength, async).ConfigureAwait(false);
		}

		protected Task<byte[]> ProcessInfoSqlResponse(GenericResponse response, AsyncWrappingCommonArgs async)
		{
			Debug.Assert(response.Data != null && response.Data.Length > 0);

			return Task.FromResult(response.Data);
		}
		#endregion

		#region op_free_statement methods
		protected override async Task Free(int option, AsyncWrappingCommonArgs async)
		{
			if (FreeNotNeeded(option))
				return;

			await DoFreePacket(option, async).ConfigureAwait(false);
			await ProcessFreeResponse(await _database.ReadResponse(async).ConfigureAwait(false), async).ConfigureAwait(false);
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

		protected async Task DoFreePacket(int option, AsyncWrappingCommonArgs async)
		{
			try
			{
				await _database.Xdr.Write(IscCodes.op_free_statement, async).ConfigureAwait(false);
				await _database.Xdr.Write(_handle, async).ConfigureAwait(false);
				await _database.Xdr.Write(option, async).ConfigureAwait(false);
				await _database.Xdr.Flush(async).ConfigureAwait(false);

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

		protected Task ProcessFreeResponse(IResponse response, AsyncWrappingCommonArgs async)
		{
			return Task.CompletedTask;
		}
		#endregion

		#region op_allocate_statement methods
		protected async Task SendAllocateToBuffer(AsyncWrappingCommonArgs async)
		{
			await _database.Xdr.Write(IscCodes.op_allocate_statement, async).ConfigureAwait(false);
			await _database.Xdr.Write(_database.Handle, async).ConfigureAwait(false);
		}

		protected Task ProcessAllocateResponse(GenericResponse response, AsyncWrappingCommonArgs async)
		{
			_handle = response.ObjectHandle;
			_allRowsFetched = false;
			State = StatementState.Allocated;
			StatementType = DbStatementType.None;
			return Task.CompletedTask;
		}
		#endregion

		#region op_execute/op_execute2 methods
		protected async Task SendExecuteToBuffer(AsyncWrappingCommonArgs async)
		{
			// this may throw error, so it needs to be before any writing
			var descriptor = await WriteParameters(async).ConfigureAwait(false);

			if (StatementType == DbStatementType.StoredProcedure)
			{
				await _database.Xdr.Write(IscCodes.op_execute2, async).ConfigureAwait(false);
			}
			else
			{
				await _database.Xdr.Write(IscCodes.op_execute, async).ConfigureAwait(false);
			}

			await _database.Xdr.Write(_handle, async).ConfigureAwait(false);
			await _database.Xdr.Write(_transaction.Handle, async).ConfigureAwait(false);

			if (_parameters != null)
			{
				await _database.Xdr.WriteBuffer(_parameters.ToBlrArray(), async).ConfigureAwait(false);
				await _database.Xdr.Write(0, async).ConfigureAwait(false); // Message number
				await _database.Xdr.Write(1, async).ConfigureAwait(false); // Number of messages
				await _database.Xdr.WriteBytes(descriptor, descriptor.Length, async).ConfigureAwait(false);
			}
			else
			{
				await _database.Xdr.WriteBuffer(null, async).ConfigureAwait(false);
				await _database.Xdr.Write(0, async).ConfigureAwait(false);
				await _database.Xdr.Write(0, async).ConfigureAwait(false);
			}

			if (StatementType == DbStatementType.StoredProcedure)
			{
				await _database.Xdr.WriteBuffer(_fields?.ToBlrArray(), async).ConfigureAwait(false);
				await _database.Xdr.Write(0, async).ConfigureAwait(false); // Output message number
			}
		}

		protected Task ProcessExecuteResponse(GenericResponse response, AsyncWrappingCommonArgs async)
		{
			// nothing to do here
			return Task.CompletedTask;
		}

		protected async Task ProcessStoredProcedureExecuteResponse(SqlResponse response, AsyncWrappingCommonArgs async)
		{
			try
			{
				if (response.Count > 0)
				{
					_outputParams.Enqueue(await ReadRow(async).ConfigureAwait(false));
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

		protected Task<Descriptor[]> ParseSqlInfo(byte[] info, byte[] items, Descriptor[] rowDescs, AsyncWrappingCommonArgs async)
		{
			return ParseTruncSqlInfo(info, items, rowDescs, async);
		}

		protected async Task<Descriptor[]> ParseTruncSqlInfo(byte[] info, byte[] items, Descriptor[] rowDescs, AsyncWrappingCommonArgs async)
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

							info = await GetSqlInfo(newItems.ToArray(), info.Length, async).ConfigureAwait(false);

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
			return rowDescs;
		}

		protected async Task WriteRawParameter(IXdrWriter xdr, DbField field, AsyncWrappingCommonArgs async)
		{
			if (field.DbDataType != DbDataType.Null)
			{
				await field.FixNull(async).ConfigureAwait(false);

				switch (field.DbDataType)
				{
					case DbDataType.Char:
						if (field.Charset.IsOctetsCharset)
						{
							await xdr.WriteOpaque(await field.DbValue.GetBinary(async).ConfigureAwait(false), field.Length, async).ConfigureAwait(false);
						}
						else if (field.Charset.IsNoneCharset)
						{
							var bvalue = field.Charset.GetBytes(await field.DbValue.GetString(async).ConfigureAwait(false));
							if (bvalue.Length > field.Length)
							{
								throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
							}
							await xdr.WriteOpaque(bvalue, field.Length, async).ConfigureAwait(false);
						}
						else
						{
							var svalue = await field.DbValue.GetString(async).ConfigureAwait(false);
							if ((field.Length % field.Charset.BytesPerCharacter) == 0 && svalue.Length > field.CharCount)
							{
								throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
							}
							await xdr.WriteOpaque(field.Charset.GetBytes(svalue), field.Length, async).ConfigureAwait(false);
						}
						break;

					case DbDataType.VarChar:
						if (field.Charset.IsOctetsCharset)
						{
							await xdr.WriteBuffer(await field.DbValue.GetBinary(async).ConfigureAwait(false), async).ConfigureAwait(false);
						}
						else if (field.Charset.IsNoneCharset)
						{
							var bvalue = field.Charset.GetBytes(await field.DbValue.GetString(async).ConfigureAwait(false));
							if (bvalue.Length > field.Length)
							{
								throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
							}
							await xdr.WriteBuffer(bvalue, async).ConfigureAwait(false);
						}
						else
						{
							var svalue = await field.DbValue.GetString(async).ConfigureAwait(false);
							if ((field.Length % field.Charset.BytesPerCharacter) == 0 && svalue.Length > field.CharCount)
							{
								throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
							}
							await xdr.WriteBuffer(field.Charset.GetBytes(svalue), async).ConfigureAwait(false);
						}
						break;

					case DbDataType.SmallInt:
						await xdr.Write(await field.DbValue.GetInt16(async).ConfigureAwait(false), async).ConfigureAwait(false);
						break;

					case DbDataType.Integer:
						await xdr.Write(await field.DbValue.GetInt32(async).ConfigureAwait(false), async).ConfigureAwait(false);
						break;

					case DbDataType.BigInt:
					case DbDataType.Array:
					case DbDataType.Binary:
					case DbDataType.Text:
						await xdr.Write(await field.DbValue.GetInt64(async).ConfigureAwait(false), async).ConfigureAwait(false);
						break;

					case DbDataType.Decimal:
					case DbDataType.Numeric:
						await xdr.Write(await field.DbValue.GetDecimal(async).ConfigureAwait(false), field.DataType, field.NumericScale, async).ConfigureAwait(false);
						break;

					case DbDataType.Float:
						await xdr.Write(await field.DbValue.GetFloat(async).ConfigureAwait(false), async).ConfigureAwait(false);
						break;

					case DbDataType.Guid:
						await xdr.Write(await field.DbValue.GetGuid(async).ConfigureAwait(false), async).ConfigureAwait(false);
						break;

					case DbDataType.Double:
						await xdr.Write(await field.DbValue.GetDouble(async).ConfigureAwait(false), async).ConfigureAwait(false);
						break;

					case DbDataType.Date:
						await xdr.Write(await field.DbValue.GetDate(async).ConfigureAwait(false), async).ConfigureAwait(false);
						break;

					case DbDataType.Time:
						await xdr.Write(await field.DbValue.GetTime(async).ConfigureAwait(false), async).ConfigureAwait(false);
						break;

					case DbDataType.TimeStamp:
						await xdr.Write(await field.DbValue.GetDate(async).ConfigureAwait(false), async).ConfigureAwait(false);
						await xdr.Write(await field.DbValue.GetTime(async).ConfigureAwait(false), async).ConfigureAwait(false);
						break;

					case DbDataType.Boolean:
						await xdr.Write(await field.DbValue.GetBoolean(async).ConfigureAwait(false), async).ConfigureAwait(false);
						break;

					case DbDataType.TimeStampTZ:
						await xdr.Write(await field.DbValue.GetDate(async).ConfigureAwait(false), async).ConfigureAwait(false);
						await xdr.Write(await field.DbValue.GetTime(async).ConfigureAwait(false), async).ConfigureAwait(false);
						await xdr.Write(await field.DbValue.GetTimeZoneId(async).ConfigureAwait(false), async).ConfigureAwait(false);
						break;

					case DbDataType.TimeStampTZEx:
						await xdr.Write(await field.DbValue.GetDate(async).ConfigureAwait(false), async).ConfigureAwait(false);
						await xdr.Write(await field.DbValue.GetTime(async).ConfigureAwait(false), async).ConfigureAwait(false);
						await xdr.Write(await field.DbValue.GetTimeZoneId(async).ConfigureAwait(false), async).ConfigureAwait(false);
						await xdr.Write((short)0, async).ConfigureAwait(false);
						break;

					case DbDataType.TimeTZ:
						await xdr.Write(await field.DbValue.GetTime(async).ConfigureAwait(false), async).ConfigureAwait(false);
						await xdr.Write(await field.DbValue.GetTimeZoneId(async).ConfigureAwait(false), async).ConfigureAwait(false);
						break;

					case DbDataType.TimeTZEx:
						await xdr.Write(await field.DbValue.GetTime(async).ConfigureAwait(false), async).ConfigureAwait(false);
						await xdr.Write(await field.DbValue.GetTimeZoneId(async).ConfigureAwait(false), async).ConfigureAwait(false);
						await xdr.Write((short)0, async).ConfigureAwait(false);
						break;

					case DbDataType.Dec16:
						await xdr.Write(await field.DbValue.GetDec16(async).ConfigureAwait(false), 16, async).ConfigureAwait(false);
						break;

					case DbDataType.Dec34:
						await xdr.Write(await field.DbValue.GetDec34(async).ConfigureAwait(false), 34, async).ConfigureAwait(false);
						break;

					case DbDataType.Int128:
						await xdr.Write(await field.DbValue.GetInt128(async).ConfigureAwait(false), async).ConfigureAwait(false);
						break;

					default:
						throw IscException.ForStrParam($"Unknown SQL data type: {field.DataType}.");
				}
			}
		}

		protected async Task<object> ReadRawValue(IXdrReader xdr, DbField field, AsyncWrappingCommonArgs async)
		{
			var innerCharset = !_database.Charset.IsNoneCharset ? _database.Charset : field.Charset;

			switch (field.DbDataType)
			{
				case DbDataType.Char:
					if (field.Charset.IsOctetsCharset)
					{
						return await xdr.ReadOpaque(field.Length, async).ConfigureAwait(false);
					}
					else
					{
						var s = await xdr.ReadString(innerCharset, field.Length, async).ConfigureAwait(false);
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
						return await xdr.ReadBuffer(async).ConfigureAwait(false);
					}
					else
					{
						return await xdr.ReadString(innerCharset, async).ConfigureAwait(false);
					}

				case DbDataType.SmallInt:
					return await xdr.ReadInt16(async).ConfigureAwait(false);

				case DbDataType.Integer:
					return await xdr.ReadInt32(async).ConfigureAwait(false);

				case DbDataType.Array:
				case DbDataType.Binary:
				case DbDataType.Text:
				case DbDataType.BigInt:
					return await xdr.ReadInt64(async).ConfigureAwait(false);

				case DbDataType.Decimal:
				case DbDataType.Numeric:
					return await xdr.ReadDecimal(field.DataType, field.NumericScale, async).ConfigureAwait(false);

				case DbDataType.Float:
					return await xdr.ReadSingle(async).ConfigureAwait(false);

				case DbDataType.Guid:
					return await xdr.ReadGuid(async).ConfigureAwait(false);

				case DbDataType.Double:
					return await xdr.ReadDouble(async).ConfigureAwait(false);

				case DbDataType.Date:
					return await xdr.ReadDate(async).ConfigureAwait(false);

				case DbDataType.Time:
					return await xdr.ReadTime(async).ConfigureAwait(false);

				case DbDataType.TimeStamp:
					return await xdr.ReadDateTime(async).ConfigureAwait(false);

				case DbDataType.Boolean:
					return await xdr.ReadBoolean(async).ConfigureAwait(false);

				case DbDataType.TimeStampTZ:
					return await xdr.ReadZonedDateTime(false, async).ConfigureAwait(false);

				case DbDataType.TimeStampTZEx:
					return await xdr.ReadZonedDateTime(true, async).ConfigureAwait(false);

				case DbDataType.TimeTZ:
					return await xdr.ReadZonedTime(false, async).ConfigureAwait(false);

				case DbDataType.TimeTZEx:
					return await xdr.ReadZonedTime(true, async).ConfigureAwait(false);

				case DbDataType.Dec16:
					return await xdr.ReadDec16(async).ConfigureAwait(false);

				case DbDataType.Dec34:
					return await xdr.ReadDec34(async).ConfigureAwait(false);

				case DbDataType.Int128:
					return await xdr.ReadInt128(async).ConfigureAwait(false);

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

		protected virtual async Task<byte[]> WriteParameters(AsyncWrappingCommonArgs async)
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
						await WriteRawParameter(xdr, field, async).ConfigureAwait(false);
						await xdr.Write(field.NullFlag, async).ConfigureAwait(false);
					}
					catch (IOException ex)
					{
						throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
					}
				}
				await xdr.Flush(async).ConfigureAwait(false);
				return ms.ToArray();
			}
		}

		protected virtual async Task<DbValue[]> ReadRow(AsyncWrappingCommonArgs async)
		{
			var row = new DbValue[_fields.Count];
			try
			{
				for (var i = 0; i < _fields.Count; i++)
				{
					var value = await ReadRawValue(_database.Xdr, _fields[i], async).ConfigureAwait(false);
					var sqlInd = await _database.Xdr.ReadInt32(async).ConfigureAwait(false);
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
