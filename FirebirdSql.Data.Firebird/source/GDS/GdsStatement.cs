/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Collections;
using System.Text;
using System.IO;

namespace FirebirdSql.Data.Firebird.Gds
{
	#region ENUMERATIONS

	internal enum GdsStatementType : int
	{
		None			= 0,
		Select			= GdsCodes.isc_info_sql_stmt_select,
		Insert			= GdsCodes.isc_info_sql_stmt_insert,
		Update			= GdsCodes.isc_info_sql_stmt_update,
		Delete			= GdsCodes.isc_info_sql_stmt_delete,
		DDL				= GdsCodes.isc_info_sql_stmt_ddl,
		GetSegment		= GdsCodes.isc_info_sql_stmt_get_segment,
		PutSegment		= GdsCodes.isc_info_sql_stmt_put_segment,
		StoredProcedure = GdsCodes.isc_info_sql_stmt_exec_procedure,
		StartTrans		= GdsCodes.isc_info_sql_stmt_start_trans,
		Commit			= GdsCodes.isc_info_sql_stmt_commit,
		Rollback		= GdsCodes.isc_info_sql_stmt_rollback,
		SelectForUpdate	= GdsCodes.isc_info_sql_stmt_select_for_upd,
		SetGenerator	= GdsCodes.isc_info_sql_stmt_set_generator,
		SavePoint		= GdsCodes.isc_info_sql_stmt_savepoint
	}

	#endregion

	internal class GdsStatement
	{
		#region EVENT_HANDLER_FIELDS

		private TransactionUpdateEventHandler transactionUpdate;

		#endregion

		#region FIELDS

		private int					handle;
		private GdsDbAttachment		db;
		private GdsTransaction		transaction;
		private GdsRowDescription	parameters;
		private GdsRowDescription	fields;
		private string				commandText;
		private bool				allRowsFetched;
		private bool				isSingletonResult;
		private object[]			rows;
		private GdsStatementState	state;
		private int					recordsAffected;
		private string				cursorName;
		private GdsStatementType	statementType;
		private int					fetchSize;
		private int					rowIndex;
		
		#endregion

		#region PROPERTIES

		public GdsDbAttachment DB
		{
			get { return this.db; }
			set { this.db = value; }
		}
		
		public GdsRowDescription Parameters
		{
			get { return this.parameters; }
			set { this.parameters = value; }
		}

		public GdsRowDescription Fields
		{
			get { return fields; }
		}

		public int RecordsAffected
		{
			get { return this.recordsAffected; }
		}

		public GdsStatementType StatementType
		{
			get { return this.statementType; }
		}

		public bool IsPrepared
		{
			get 
			{
				if (state == GdsStatementState.Deallocated ||
					state == GdsStatementState.Error)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public string CommandText
		{
			get { return this.commandText; }
			set { this.commandText = value; }
		}

		public object[] Rows
		{
			get { return this.rows; }
		}

		public GdsTransaction Transaction
		{
			get { return this.transaction; }
			set
			{
				bool addHandler = false;
				if (this.transaction != value)
				{
					if (this.transactionUpdate != null &&
						this.transaction != null)
					{
						this.Transaction.Update -= this.transactionUpdate;
						this.transactionUpdate = null;
					}
					
					// Add event handler for transaction updates
					this.transactionUpdate = new TransactionUpdateEventHandler(this.transactionUpdated);

					addHandler = true;
				}

				this.transaction = value;

				if (addHandler && this.transaction != null)
				{
					this.transaction.Update += this.transactionUpdate;
				}
			}
		}

		public string CursorName
		{
			get { return this.cursorName; }
			set { this.cursorName = value; }
		}

		#endregion

		#region CONSTRUCTORS

		public GdsStatement(GdsDbAttachment db)
		{
			this.fetchSize			= 200;
			this.recordsAffected	= -1;
			this.db					= db;
			this.cursorName			= String.Empty;
			this.commandText			= String.Empty;
		}

		public GdsStatement(
			string			commandText, 
			GdsDbAttachment db, 
			GdsTransaction	transaction) : this(db)
		{
			this.commandText		= commandText;
			this.Transaction	= transaction;
		}

		public GdsStatement(
			string			commandText, 
			GdsDbAttachment db, 
			GdsTransaction	transaction, 
			string			cursorName) : this(db)
		{
			this.commandText		= commandText;
			this.cursorName		= cursorName;
			this.Transaction	= transaction;			
		}

		#endregion

		#region MISC_METHODS

		public string GetExecutionPlan()
		{
			string plan = String.Empty;
			
			byte[] describe_plan_info = new byte[] { 
						GdsCodes.isc_info_sql_get_plan,
						GdsCodes.isc_info_end };

			try
			{		        
				byte[] buffer = GetSqlInfo(describe_plan_info, 
											GdsCodes.MAX_BUFFER_SIZE);			

				int len = buffer[1];				
				len += buffer[2] << 8;
									
				if (len > 0)
				{
					plan = Encoding.Default.GetString(buffer, 4, --len);
				}
			}
			catch(GdsException ge)
			{
				throw ge;
			}

			return plan;
		}

		public void UpdateRecordsAffected()
		{
			byte[] items = new byte[]
			{
				GdsCodes.isc_info_sql_records,
				GdsCodes.isc_info_end
			};
			
			int insertCount		= 0;
			int updateCount		= 0;
			int deleteCount		= 0;
			int selectCount		= 0;
			int pos				= 0;
			int length			= 0;
			int type			= 0;

			byte[] buffer = GetSqlInfo(items, GdsCodes.MAX_BUFFER_SIZE);

			while ((type = buffer[pos++]) != GdsCodes.isc_info_end) 
			{
				length = db.VaxInteger(buffer, pos, 2);
				pos += 2;
				switch (type) 
				{
					case GdsCodes.isc_info_sql_records:
						int l;
						int t;
						while ((t = buffer[pos++]) != GdsCodes.isc_info_end) 
						{
							l = db.VaxInteger(buffer, pos, 2);
							pos += 2;
							switch (t) 
							{
								case GdsCodes.isc_info_req_insert_count:
									insertCount = db.VaxInteger(buffer, pos, l);
									break;
								
								case GdsCodes.isc_info_req_update_count:
									updateCount = db.VaxInteger(buffer, pos, l);
									break;
								
								case GdsCodes.isc_info_req_delete_count:
									deleteCount = db.VaxInteger(buffer, pos, l);
									break;
								
								case GdsCodes.isc_info_req_select_count:
									selectCount = db.VaxInteger(buffer, pos, l);
									break;
							}
							pos += l;
						}
						break;
										
					default:
						pos += length;
						break;
				}
			}

			if (statementType == GdsStatementType.Select ||
				statementType == GdsStatementType.SelectForUpdate ||
				statementType == GdsStatementType.StoredProcedure)
			{
				recordsAffected = -1;
			}
			else
			{
				recordsAffected = insertCount + updateCount + deleteCount;
			}			
		}

		#endregion

		#region API_METHODS

		/// <summary>
		/// isc_dsql_allocate_statement
		/// </summary>
		public void Allocate()
		{
			lock (db) 
			{
				try 
				{
					db.Send.WriteInt(GdsCodes.op_allocate_statement);
					db.Send.WriteInt(db.Handle);
					db.Send.Flush();

					// Receive reponse
					GdsResponse r = db.ReceiveResponse();

					handle = r.ObjectHandle;

					allRowsFetched	= false;
					rowIndex		= 0;
					rows			= null;
					state			= GdsStatementState.Allocated;
					statementType	= GdsStatementType.None;
				} 
				catch (IOException) 
				{
					state = GdsStatementState.Deallocated;
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
				catch (Exception ex)
				{
					state = GdsStatementState.Error;
					throw ex;
				}
			}
		}

		public void Close()
		{
			if (this.state == GdsStatementState.Executed ||
				this.state == GdsStatementState.Error)
			{
				if (statementType == GdsStatementType.Select			||
					statementType == GdsStatementType.SelectForUpdate	||
					statementType == GdsStatementType.StoredProcedure)
				{
					this.free(GdsCodes.DSQL_close);
					this.state = GdsStatementState.Closed;
				}
			}
		}

		public void Drop()
		{
			if (transactionUpdate != null && transaction != null)
			{
				this.transaction.Update -= transactionUpdate;
				this.transactionUpdate = null;
			}

			this.free(GdsCodes.DSQL_drop);
			this.state			= GdsStatementState.Deallocated;
			this.statementType	= GdsStatementType.None;
		}

		public byte[] GetSqlInfo(byte[] items, int bufferLength)
		{
			lock (db) 
			{
				try 
				{
					db.Send.WriteInt(GdsCodes.op_info_sql);
					db.Send.WriteInt(handle);
					db.Send.WriteInt(0);
					db.Send.WriteBuffer(items, items.Length);
					db.Send.WriteInt(bufferLength);
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();		
					
					return r.Data;
				} 
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
			}
		}

		public GdsRowDescription Describe()
		{
			byte[] describe_select_info = new byte[] { 
					GdsCodes.isc_info_sql_select,
					GdsCodes.isc_info_sql_describe_vars,
					GdsCodes.isc_info_sql_sqlda_seq,
					GdsCodes.isc_info_sql_type,
					GdsCodes.isc_info_sql_sub_type,
					GdsCodes.isc_info_sql_scale,
					GdsCodes.isc_info_sql_length,
					GdsCodes.isc_info_sql_field,
					GdsCodes.isc_info_sql_relation,
					GdsCodes.isc_info_sql_owner,
					GdsCodes.isc_info_sql_alias,
					GdsCodes.isc_info_sql_describe_end};

			try
			{
				byte[] buffer = GetSqlInfo(describe_select_info, GdsCodes.MAX_BUFFER_SIZE);

				return parseSqlInfo(buffer, describe_select_info);
			}
			catch (GdsException ge)
			{
				throw ge;
			}
		}

		public void DescribeParameters()
		{
			byte[] describe_bind_info = new byte[] { 
					GdsCodes.isc_info_sql_bind,
					GdsCodes.isc_info_sql_describe_vars,
					GdsCodes.isc_info_sql_sqlda_seq,
					GdsCodes.isc_info_sql_type,
					GdsCodes.isc_info_sql_sub_type,
					GdsCodes.isc_info_sql_scale,
					GdsCodes.isc_info_sql_length,
					GdsCodes.isc_info_sql_field,
					GdsCodes.isc_info_sql_relation,
					GdsCodes.isc_info_sql_owner,
					GdsCodes.isc_info_sql_alias,
					GdsCodes.isc_info_sql_describe_end };

			try
			{		        
				byte[] buffer = GetSqlInfo(describe_bind_info, GdsCodes.MAX_BUFFER_SIZE);
		        
				parameters = parseSqlInfo(buffer, describe_bind_info);
			}
			catch (GdsException ge)
			{
				throw ge;
			}
		}

		public void Prepare()
		{
			// Clear data
			parameters	= null;
			fields		= null;
			rows		= null;
			rowIndex	= 0;

			byte[] sql_prepare_info = new byte[] { 
													 GdsCodes.isc_info_sql_select,
													 GdsCodes.isc_info_sql_describe_vars,
													 GdsCodes.isc_info_sql_sqlda_seq,
													 GdsCodes.isc_info_sql_type,
													 GdsCodes.isc_info_sql_sub_type,
													 GdsCodes.isc_info_sql_scale,
													 GdsCodes.isc_info_sql_length,
													 GdsCodes.isc_info_sql_field,
													 GdsCodes.isc_info_sql_relation,
													 GdsCodes.isc_info_sql_owner,
													 GdsCodes.isc_info_sql_alias,
													 GdsCodes.isc_info_sql_describe_end};

			lock (db)
			{
				try 
				{
					db.Send.WriteInt(GdsCodes.op_prepare_statement);
					db.Send.WriteInt(transaction.Handle);
					db.Send.WriteInt(handle);
					db.Send.WriteInt(db.Parameters.Dialect);
					db.Send.WriteString(commandText);
					db.Send.WriteBuffer(sql_prepare_info, sql_prepare_info.Length);
					db.Send.WriteInt(GdsCodes.MAX_BUFFER_SIZE);
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();

					fields	= parseSqlInfo(r.Data, sql_prepare_info);
					state	= GdsStatementState.Prepared;
					
					// Get Statement type
					this.statementType = this.getStatementType();
				} 
				catch (IOException)
				{
					state = GdsStatementState.Error;
					throw new GdsException(GdsCodes.isc_net_read_err);					
				}				
				catch (Exception ex)
				{
					state = GdsStatementState.Error;
					throw ex;
				}
			}
		}

		public void Execute()
		{
			execute(false);			
		}

		public void ExecuteStoredProc()
		{
			if (this.commandText.Trim().ToLower().StartsWith("execute procedure "))
			{
				execute(true);
			}
			else
			{
				execute(false);
			}
		}

		public GdsValue[] Fetch()
		{
			if (state == GdsStatementState.Deallocated)
			{
				throw new InvalidOperationException("Statment is not correctly created.");
			}

			if (statementType == GdsStatementType.Select			||
				statementType == GdsStatementType.SelectForUpdate	||
				statementType == GdsStatementType.StoredProcedure)
			{
				if ((!allRowsFetched && rowIndex == 0) ||
					(!allRowsFetched && rowIndex >= fetchSize))
				{
					// Fetch next batch of rows
					lock (db) 
					{
						try 
						{
							db.Send.WriteInt(GdsCodes.op_fetch);
							db.Send.WriteInt(handle);
							writeBLR(fields);
							db.Send.WriteInt(0);				// p_sqldata_message_number						
							db.Send.WriteInt(fetchSize);		// p_sqldata_messages
							db.Send.Flush();

							if (db.NextOperation() == GdsCodes.op_fetch_response) 
							{
								// Initialize rows array
								rows		= new object[fetchSize];
								rowIndex	= 0;

								int sqldata_status	 = 0;
								int sqldata_messages = 1;
								int index			 = 0;
								while (sqldata_messages > 0 && sqldata_status == 0)
								{
									int op = db.ReadOperation();
									sqldata_status		= db.Receive.ReadInt();
									sqldata_messages	= db.Receive.ReadInt();

									if (sqldata_messages > 0 && sqldata_status == 0) 
									{
										rows[index++] = readDataRow();
									}
								}

								if (index == 0)
								{
									rows = null;
								}

								if (sqldata_status == 100) 
								{
									allRowsFetched = true;
								}
							}
							else 
							{
								db.ReceiveResponse();
							}
						} 
						catch (IOException) 
						{
							state = GdsStatementState.Error;
							throw new GdsException(GdsCodes.isc_net_read_err);
						}
						catch (Exception ex)
						{
							state = GdsStatementState.Error;
							throw ex;
						}
					}
				}
			}

			if (this.rows != null					&&
				this.rowIndex < fetchSize			&&
				this.rowIndex < this.rows.Length	&&
				this.rows[this.rowIndex] != null)
			{
				// return current row
				return (GdsValue[])rows[rowIndex++];
			}
			else 
			{
				// All readed clear rows and return null
				rows		= null;
				rowIndex	= 0;

				return null;
			}
		}

		// The type parameter is reserved for futureuse
		public void SetCursorName(string cursorName, int type)
		{
			lock (db) 
			{
				try 
				{
					db.Send.WriteInt(GdsCodes.op_set_cursor);
					db.Send.WriteInt(handle);
					db.Send.WriteString(cursorName + '\0');
					db.Send.WriteInt(type);
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();
				} 
				catch (IOException) 
				{
					state = GdsStatementState.Error;
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
				catch (Exception ex)
				{
					state = GdsStatementState.Error;
					throw ex;
				}
			}
		}

		#endregion

		#region RESPONSE_METHODS

		private GdsValue[] receiveSqlResponse()
		{
			try 
			{
				if (db.ReadOperation() == GdsCodes.op_sql_response) 
				{
					int messages = db.Receive.ReadInt();
					if (messages > 0) 
					{
						return readDataRow();
					}
					else 
					{
						return null;
					}
				} 
				else 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
			} 
			catch (IOException ex) 
			{
				// ex.getMessage() makes little sense here, it will not be displayed
				// because error message for isc_net_read_err does not accept params
				throw new GdsException(
					GdsCodes.isc_arg_gds, 
					GdsCodes.isc_net_read_err, 
					ex.Message);
			}
		}

		private GdsValue[] readDataRow()
		{
			GdsValue[] row = new GdsValue[fields.SqlD];

			lock (db)
			{
				// This only works if not (port->port_flags & PORT_symmetric)				
				for (int i = 0; i < fields.SqlD; i++) 
				{
					try 
					{
						row[i] = new GdsValue(
							this,
							fields.SqlVar[i],
							db.Receive.ReadValue(fields.SqlVar[i]));
					} 
					catch (IOException) 
					{
						throw new GdsException(GdsCodes.isc_net_read_err);
					}
				}
			}

			return row;			
		}

		#endregion

		#region EVENT_HANDLER_METHODS

		private void transactionUpdated(object sender, EventArgs e)
		{
			lock (this)
			{
				this.state				= GdsStatementState.Closed;
				this.transaction.Update -= this.transactionUpdate;
				this.transactionUpdate	= null;
				this.rows				= null;
				this.rowIndex			= 0;
				this.isSingletonResult	= false;
				this.allRowsFetched		= false;
			}
		}

		#endregion

		#region PRIVATE_METHODS

		private void execute(bool isSP)
		{
			if (state == GdsStatementState.Deallocated)
			{
				throw new InvalidOperationException("Statment is not correctly created.");
			}
	        
			lock (db) 
			{
				try 
				{
					byte[] paramBuffer = null;
					if (parameters != null)
					{
						MemoryStream ms = new MemoryStream();
						GdsInetWriter wr = new GdsInetWriter(ms, this.db.Parameters.Charset.Encoding);
						for (int i = 0; i < parameters.SqlD; i++) 
						{
							wr.WriteParameter(parameters.SqlVar[i]);
						}
						paramBuffer = ms.ToArray();

						wr.Close();
					}

					db.Send.WriteInt(isSP ? GdsCodes.op_execute2 : GdsCodes.op_execute);
					db.Send.WriteInt(handle);
					db.Send.WriteInt(transaction.Handle);

					writeBLR(parameters);
					db.Send.WriteInt(0);								// message number = in_message_type
					db.Send.WriteInt(((parameters == null) ? 0 : 1));	// stmt->rsr_bind_format

					if (parameters != null)
					{
						db.Send.Write(paramBuffer);
					}

					if (isSP)
					{
						writeBLR(fields);
						db.Send.WriteInt(0);		//out_message_number = out_message_type
					}
					db.Send.Flush();            

					if (db.NextOperation() == GdsCodes.op_sql_response) 
					{
						// This would be an Execute procedure
						rows	= new object[1];
						rows[0] = receiveSqlResponse();
			
						rowIndex			= 0;
						allRowsFetched		= true;
						isSingletonResult	= true;
					}
					else 
					{
						isSingletonResult = false;
					}

					GdsResponse r = db.ReceiveResponse();

					state = GdsStatementState.Executed;
				} 
				catch (IOException) 
				{
					state = GdsStatementState.Error;
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
				catch (Exception ex)
				{
					state = GdsStatementState.Error;
					throw ex;
				}
			}
		}

		private void free(int option)
		{
			// Does not seem to be possible or necessary to close
			// an execute procedure statement.
			if (isSingletonResult && option == GdsCodes.DSQL_close) 
			{
				return;        
			}
	        
			lock (db) 
			{
				try 
				{
					db.Send.WriteInt(GdsCodes.op_free_statement);
					db.Send.WriteInt(handle);
					db.Send.WriteInt(option);
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();
					if (option == GdsCodes.DSQL_drop) 
					{
						parameters	= null;
						fields		= null;
					}
					
					rows				= null;
					rowIndex			= 0;
					isSingletonResult	= false;
					allRowsFetched		= false;
				} 
				catch (IOException) 
				{
					state = GdsStatementState.Error;
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
				catch (Exception ex)
				{
					state = GdsStatementState.Error;
					throw ex;
				}
			}
		}

		private void writeBLR(GdsRowDescription rowDesc)
		{
			int		blr_len = 0;
			byte[]	blr		= null;

			if (rowDesc != null) 
			{
				// Determine the BLR length
				blr_len = 8;
				int par_count = 0;

				for (int i = 0; i < rowDesc.SqlD; i++) 
				{
					int dtype = rowDesc.SqlVar[i].SqlType & ~1;
					switch (dtype)
					{
						case GdsCodes.SQL_VARYING:
						case GdsCodes.SQL_TEXT:
							blr_len += 3;
							break;

						case GdsCodes.SQL_SHORT:
						case GdsCodes.SQL_LONG:
						case GdsCodes.SQL_INT64:
						case GdsCodes.SQL_QUAD:
						case GdsCodes.SQL_BLOB:
						case GdsCodes.SQL_ARRAY:
							blr_len += 2;
							break;

						default:
							blr_len++;
							break;
					}

					blr_len		+= 2;
					par_count	+= 2;
				}

				blr = new byte[blr_len];

				int n = 0;
				blr[n++] = GdsCodes.blr_version5;
				blr[n++] = GdsCodes.blr_begin;
				blr[n++] = GdsCodes.blr_message;
				blr[n++] = 0;

				blr[n++] = (byte) (par_count & 255);
				blr[n++] = (byte) (par_count >> 8);

				for (int i = 0; i < rowDesc.SqlD; i++) 
				{
					int dtype	= rowDesc.SqlVar[i].SqlType & ~1;
					int len		= rowDesc.SqlVar[i].SqlLen;

					switch (dtype)
					{
						case GdsCodes.SQL_VARYING:
							blr[n++] = GdsCodes.blr_varying;
							blr[n++] = (byte) (len & 255);
							blr[n++] = (byte) (len >> 8);
							break;

						case GdsCodes.SQL_TEXT:
							blr[n++] = GdsCodes.blr_text;
							blr[n++] = (byte) (len & 255);
							blr[n++] = (byte) (len >> 8);
							break;

						case GdsCodes.SQL_DOUBLE:
							blr[n++] = GdsCodes.blr_double;
							break;

						case GdsCodes.SQL_FLOAT:
							blr[n++] = GdsCodes.blr_float;
							break;

						case GdsCodes.SQL_D_FLOAT:
							blr[n++] = GdsCodes.blr_d_float;
							break;

						case GdsCodes.SQL_TYPE_DATE:
							blr[n++] = GdsCodes.blr_sql_date;
							break;

						case GdsCodes.SQL_TYPE_TIME:
							blr[n++] = GdsCodes.blr_sql_time;
							break;

						case GdsCodes.SQL_TIMESTAMP:
							blr[n++] = GdsCodes.blr_timestamp;
							break;

						case GdsCodes.SQL_BLOB:
							blr[n++] = GdsCodes.blr_quad;
							blr[n++] = 0;
							break;

						case GdsCodes.SQL_ARRAY:
							blr[n++] = GdsCodes.blr_quad;
							blr[n++] = 0;
							break;

						case GdsCodes.SQL_LONG:
							blr[n++] = GdsCodes.blr_long;
							blr[n++] = (byte)rowDesc.SqlVar[i].SqlScale;
							break;

						case GdsCodes.SQL_SHORT:
							blr[n++] = GdsCodes.blr_short;
							blr[n++] = (byte) rowDesc.SqlVar[i].SqlScale;
							break;

						case GdsCodes.SQL_INT64:
							blr[n++] = GdsCodes.blr_int64;
							blr[n++] = (byte) rowDesc.SqlVar[i].SqlScale;
							break;

						case GdsCodes.SQL_QUAD:
							blr[n++] = GdsCodes.blr_quad;
							blr[n++] = (byte) rowDesc.SqlVar[i].SqlScale;
							break;
					}

					blr[n++] = GdsCodes.blr_short;
					blr[n++] = 0;
				}

				blr[n++] = GdsCodes.blr_end;
				blr[n++] = GdsCodes.blr_eoc;
			}

			try 
			{
				db.Send.WriteBuffer(blr, blr_len);
			} 
			catch (IOException) 
			{
				throw new GdsException(GdsCodes.isc_net_write_err);
			}
		}

		private GdsRowDescription parseSqlInfo(byte[] info, byte[] items) 
		{
			GdsRowDescription rowDesc = new GdsRowDescription();
			int lastindex = 0;
			while ((lastindex = parseTruncSqlInfo(info, rowDesc, lastindex)) > 0) 
			{
				lastindex--;               // Is this OK ?
				
				byte[] new_items = new byte[4 + items.Length];
				new_items[0] = GdsCodes.isc_info_sql_sqlda_start;
				new_items[1] = 2;
				new_items[2] = (byte) (lastindex & 255);
				new_items[3] = (byte) (lastindex >> 8);

				Array.Copy(items, 0, new_items, 4, items.Length);
				info = GetSqlInfo(new_items, info.Length);
			}

			return rowDesc;
		}
    	    
		private int parseTruncSqlInfo(byte[] info, GdsRowDescription rowDesc, int lastindex)
		{
			byte	item	= 0;
			int		index	= 0;
			int		i		= 2;

			int len = db.VaxInteger(info, i, 2);
			i += 2;
			int n	= db.VaxInteger(info, i, len);
			i += len;
			if (rowDesc.SqlVar == null) 
			{
				rowDesc.SqlD	= rowDesc.SqlN = n;
				rowDesc.SqlVar	= new GdsField[rowDesc.SqlN];
			}

			while (info[i] != GdsCodes.isc_info_end) 
			{
				while ((item = info[i++]) != GdsCodes.isc_info_sql_describe_end) 
				{
					switch (item) 
					{
						case GdsCodes.isc_info_sql_sqlda_seq:
							len = db.VaxInteger(info, i, 2);
							i += 2;
							index = db.VaxInteger(info, i, len);
							i += len;
							rowDesc.SqlVar[index - 1] = new GdsField();
							break;
						
						case GdsCodes.isc_info_sql_type:
							len = db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc.SqlVar[index - 1].SqlType = db.VaxInteger(info, i, len);
							i += len;
							break;

						case GdsCodes.isc_info_sql_sub_type:
							len = db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc.SqlVar[index - 1].SqlSubType = db.VaxInteger(info, i, len);
							i += len;
							break;
						
						case GdsCodes.isc_info_sql_scale:
							len = db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc.SqlVar[index - 1].SqlScale = db.VaxInteger(info, i, len);
							i += len;
							break;
						
						case GdsCodes.isc_info_sql_length:
							len = db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc.SqlVar[index - 1].SqlLen = db.VaxInteger(info, i, len);
							i += len;
							break;

						case GdsCodes.isc_info_sql_field:
							len = db.VaxInteger(info, i, 2);
							i += 2;							
							rowDesc.SqlVar[index - 1].SqlName = 
								Encoding.Default.GetString(info, i, len);
							i += len;
							break;							
						
						case GdsCodes.isc_info_sql_relation:
							len = db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc.SqlVar[index - 1].RelName = Encoding.Default.GetString(info, i, len);
							i += len;
							break;
						
						case GdsCodes.isc_info_sql_owner:
							len = db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc.SqlVar[index - 1].OwnerName = Encoding.Default.GetString(info, i, len);
							i += len;
							break;

						case GdsCodes.isc_info_sql_alias:
							len = db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc.SqlVar[index - 1].AliasName = Encoding.Default.GetString(info, i, len);
							i += len;
							break;

						case GdsCodes.isc_info_truncated:
							return lastindex;

						default:
							throw new GdsException(GdsCodes.isc_dsql_sqlda_err);
					}
				}

				lastindex = index;
			}
			
			return 0;
		}

		private GdsStatementType getStatementType()
		{
			byte[] items = new byte[]
			{
				GdsCodes.isc_info_sql_stmt_type	,
				GdsCodes.isc_info_end
			};
			
			GdsStatementType	stmtType	= GdsStatementType.None;
			int					pos			= 0;
			int					length		= 0;
			int					type		= 0;

			byte[] buffer = GetSqlInfo(items, GdsCodes.MAX_BUFFER_SIZE);

			while ((type = buffer[pos++]) != GdsCodes.isc_info_end) 
			{
				length = db.VaxInteger(buffer, pos, 2);
				pos += 2;
				switch (type) 
				{
					case GdsCodes.isc_info_sql_stmt_type:
						stmtType = (GdsStatementType)db.VaxInteger(buffer, pos, length);
						pos += length;
						break;
					
					default:
						pos += length;
						break;
				}
			}

			return stmtType;
		}

		#endregion
	}
}
