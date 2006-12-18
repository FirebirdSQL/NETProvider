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

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Gds
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	class GdsStatement : StatementBase
	{
		#region Fields

		private int					handle;
		private GdsDbAttachment		db;
		private GdsTransaction		transaction;
		private RowDescriptor		parameters;
		private RowDescriptor		fields;
		private StatementState		state;
		private DbStatementType		statementType;
		private bool				allRowsFetched;
		private bool				isSingletonResult;
		private object[]			rows;
		private int					recordsAffected;
		private int					fetchSize;
		private int					rowIndex;
		
		#endregion

		#region Properties

		public override IDbAttachment DB
		{
			get { return this.db; }
			set { this.db = (GdsDbAttachment)value; }
		}

		public override ITransaction Transaction
		{
			get { return this.transaction; }
			set
			{
				if (value == null)
				{
					this.transaction = null;
				}
				else
				{
					bool addHandler = false;
					if (this.transaction != value)
					{
						if (this.TransactionUpdate != null &&
							this.transaction != null)
						{
							this.transaction.Update -= this.TransactionUpdate ;
							this.TransactionUpdate  = null;
						}
					
						// Add event handler for transaction updates
						this.TransactionUpdate = new TransactionUpdateEventHandler(this.TransactionUpdated);

						addHandler = true;
					}

					this.transaction = (GdsTransaction)value;

					if (addHandler && this.transaction != null)
					{
						this.transaction.Update += this.TransactionUpdate;
					}
				}
			}
		}
		
		public override RowDescriptor Parameters
		{
			get { return this.parameters; }
			set { this.parameters = value; }
		}

		public override RowDescriptor Fields
		{
			get { return this.fields; }
		}

		public override int RecordsAffected
		{
			get { return this.recordsAffected; }
		}

		public override bool IsPrepared
		{
			get 
			{
				if (this.state == StatementState.Deallocated ||
					this.state == StatementState.Error)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public override object[] Rows
		{
			get { return this.rows; }
		}

		public override DbStatementType StatementType
		{
			get { return this.statementType; }
			set { this.statementType = value; }
		}

		public override StatementState State
		{
			get { return this.state; }
			set { this.state = value; }
		}

		#endregion

		#region Constructors

		public GdsStatement(IDbAttachment db) : this(db, null)
		{
		}

		public GdsStatement(IDbAttachment db, ITransaction transaction)
		{
			if (!(db is GdsDbAttachment))
			{
				throw new ArgumentException("Specified argument is not of GdsDbAttachment type.");
			}

			this.fetchSize			= 200;
			this.recordsAffected	= -1;
			this.db					= (GdsDbAttachment)db;
			
			if (transaction != null)
			{
				this.Transaction	= transaction;
			}
		}

		#endregion

		#region Blob Creation Metods

		public override BlobBase CreateBlob()
		{
			return new GdsBlob(this.db, this.transaction);
		}

		public override BlobBase CreateBlob(long blobId)
		{
			return new GdsBlob(this.db, this.transaction, blobId);
		}

		#endregion

		#region Array Creation Methods

		public override ArrayBase CreateArray(string tableName, string fieldName)
		{
			return new GdsArray(this.db, this.transaction, tableName, fieldName);
		}

		public override ArrayBase CreateArray(
			long handle,
			string tableName,
			string fieldName)
		{
			return new GdsArray(this.db, this.transaction, handle, tableName, fieldName);
		}

		#endregion

		#region Methods

		public override void Prepare(string commandText)
		{
			// Clear data
			this.parameters	= null;
			this.fields		= null;
			this.rows		= null;
			this.rowIndex	= 0;

			lock (this.db)
			{
				if (this.state == StatementState.Deallocated)
				{
					// Allocate statement
					this.allocate();
				}

				try 
				{
					this.db.Send.Write(IscCodes.op_prepare_statement);
					this.db.Send.Write(this.transaction.Handle);
					this.db.Send.Write(this.handle);
					this.db.Send.Write((int)this.db.Parameters.Dialect);
					this.db.Send.Write(commandText);
					this.db.Send.WriteBuffer(
						DescribeInfoItems, 
						DescribeInfoItems.Length);
					this.db.Send.Write(IscCodes.MAX_BUFFER_SIZE);
					this.db.Send.Flush();

					GdsResponse r = this.db.ReceiveResponse();

					this.fields	= this.parseSqlInfo(r.Data, DescribeInfoItems);
					this.state	= StatementState.Prepared;
					
					// Get Statement type
					this.statementType = this.GetStatementType();
				} 
				catch (IOException)
				{
					this.state = StatementState.Error;
					throw new IscException(IscCodes.isc_net_read_err);					
				}				
			}
		}

		public override void Execute()
		{
			if (this.state == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statment is not correctly created.");
			}
	        
			lock (this.db) 
			{
				try 
				{
					byte[] descriptor = null;
					if (this.parameters != null)
					{
						XdrStream xdr = new XdrStream(this.db.Parameters.Charset);
						xdr.Write(this.parameters);

						descriptor = xdr.ToArray();

						xdr.Close();
					}

					if (this.statementType == DbStatementType.StoredProcedure)
					{
						this.db.Send.Write(IscCodes.op_execute2);
					}
					else
					{
						this.db.Send.Write(IscCodes.op_execute);
					}
					this.db.Send.Write(this.handle);
					this.db.Send.Write(this.transaction.Handle);

					if (this.parameters != null)
					{
						this.db.Send.WriteBuffer(this.parameters.ToBlrArray());
						this.db.Send.Write(0);	// message number = in_message_type
						this.db.Send.Write(1);	// stmt->rsr_bind_format
						this.db.Send.Write(descriptor, 0, descriptor.Length);
					}
					else
					{
						this.db.Send.WriteBuffer(null);
						this.db.Send.Write(0);
						this.db.Send.Write(0);
					}
					if (this.statementType == DbStatementType.StoredProcedure)
					{
						this.db.Send.WriteBuffer(this.fields == null ? null : this.fields.ToBlrArray());
						this.db.Send.Write(0);	//out_message_number = out_message_type
					}
					this.db.Send.Flush();            

					if (this.db.NextOperation() == IscCodes.op_sql_response) 
					{
						// This would be an Execute procedure
						this.rows		= new object[1];
						this.rows[0]	= this.receiveSqlResponse();
									
						this.allRowsFetched		= true;
						this.isSingletonResult	= true;
					}
					else 
					{
						this.isSingletonResult = false;
					}

					this.rowIndex = 0;

					GdsResponse r = this.db.ReceiveResponse();
					
					this.updateRecordsAffected();

					this.state = StatementState.Executed;
				} 
				catch (IOException) 
				{
					this.state = StatementState.Error;
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public override void ExecuteImmediate(string commandText)
		{
			lock (this.db) 
			{
				try 
				{
					byte[] descriptor = null;
					if (this.parameters != null)
					{
						XdrStream xdr = new XdrStream(this.db.Parameters.Charset);
						xdr.Write(this.parameters);

						descriptor = xdr.ToArray();

						xdr.Close();
					}

					if (this.parameters == null && this.fields == null) 
					{
						this.db.Send.Write(IscCodes.op_exec_immediate);
					} 
					else 
					{
						this.db.Send.Write(IscCodes.op_exec_immediate2);

						if (this.parameters != null)
						{
							this.db.Send.WriteBuffer(
								this.parameters.ToBlrArray());
							this.db.Send.Write(0);
							this.db.Send.Write(1);
							this.db.Send.Write(descriptor, 0, descriptor.Length);
						}
						else
						{
							this.db.Send.WriteBuffer(null);
							this.db.Send.Write(0);
							this.db.Send.Write(0);
						}
						if (this.fields != null)
						{
							this.db.Send.WriteBuffer(this.fields.ToBlrArray());
						}
						else
						{
							this.db.Send.WriteBuffer(null);
						}
						this.db.Send.Write(0);
					}

					this.db.Send.Write(this.transaction.Handle);
					this.db.Send.Write(0);
					this.db.Send.Write(this.db.Parameters.Dialect);
					this.db.Send.Write(commandText);
					this.db.Send.Write(String.Empty);
					this.db.Send.Write(0);
					this.db.Send.Flush();

					int op = this.db.NextOperation();
					if (op == IscCodes.op_sql_response) 
					{
						this.rows			= new object[1];
						this.rows[0]		= this.receiveSqlResponse();			
						this.allRowsFetched	= true;

						op = this.db.NextOperation();
					}

					this.rowIndex = 0;
					
					GdsResponse r = this.db.ReceiveResponse();

					this.updateRecordsAffected();
				} 
				catch (IOException) 
				{
					this.state = StatementState.Error;
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public override DbValue[] Fetch()
		{
			if (this.state == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statment is not correctly created.");
			}

			if (this.statementType == DbStatementType.Select			||
				this.statementType == DbStatementType.SelectForUpdate	||
				this.statementType == DbStatementType.StoredProcedure)
			{
				if ((!this.allRowsFetched && this.rowIndex == 0) ||
					(!this.allRowsFetched && this.rowIndex >= fetchSize))
				{
					// Fetch next batch of rows
					lock (this.db) 
					{
						try 
						{
							this.db.Send.Write(IscCodes.op_fetch);
							this.db.Send.Write(this.handle);
							this.db.Send.WriteBuffer(this.fields.ToBlrArray());
							this.db.Send.Write(0);
							this.db.Send.Write(fetchSize);
							this.db.Send.Flush();

							if (this.db.NextOperation() == IscCodes.op_fetch_response) 
							{
								// Initialize rows array
								this.rows		= new object[fetchSize];
								this.rowIndex	= 0;

								int status	 = 0;
								int count	 = 1;
								int index	 = 0;
								while (count > 0 && status == 0)
								{
									int op = this.db.ReadOperation();
									if (op == IscCodes.op_fetch_response)
									{
										status	= this.db.Receive.ReadInt32();
										count	= this.db.Receive.ReadInt32();

										if (count == 1) 
										{
											this.rows[index++] = this.readDataRow();
										}
									}
								}

								if (index == 0)
								{
									this.rows = null;
								}

								if (status == 100) 
								{
									this.allRowsFetched = true;
								}
							}
							else 
							{
								this.db.ReceiveResponse();
							}
						} 
						catch (IOException) 
						{
							throw new IscException(IscCodes.isc_net_read_err);
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
				return (DbValue[])this.rows[this.rowIndex++];
			}
			else 
			{
				// All readed clear rows and return null
				this.rows		= null;
				this.rowIndex	= 0;

				return null;
			}
		}

		public override void Describe()
		{
			try
			{
				byte[] buffer = this.GetSqlInfo(
					DescribeInfoItems, IscCodes.MAX_BUFFER_SIZE);

				this.fields = this.parseSqlInfo(buffer, DescribeInfoItems);
			}
			catch(IscException ge)
			{
				throw ge;
			}
		}

		public override void DescribeParameters()
		{
			try
			{		        
				byte[] buffer = this.GetSqlInfo(
					DescribeBindInfoItems, IscCodes.MAX_BUFFER_SIZE);
		        
				this.parameters = this.parseSqlInfo(buffer, DescribeBindInfoItems);
			}
			catch(IscException ge)
			{
				throw ge;
			}
		}

		public override byte[] GetSqlInfo(byte[] items, int bufferLength)
		{
			lock (this.db) 
			{
				try 
				{
					this.db.Send.Write(IscCodes.op_info_sql);
					this.db.Send.Write(this.handle);
					this.db.Send.Write(0);
					this.db.Send.WriteBuffer(items, items.Length);
					this.db.Send.Write(bufferLength);
					this.db.Send.Flush();

					GdsResponse r = this.db.ReceiveResponse();		
					
					return r.Data;
				} 
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}
		
		/*
		public override void SetCursorName(string name)
		{
			lock (this.db) 
			{
				try 
				{
					this.db.Send.Write(IscCodes.op_set_cursor);
					this.db.Send.Write(this.handle);
					this.db.Send.Write(name + '\0');
					this.db.Send.Write(0); // The type parameter is reserved for future use
					this.db.Send.Flush();

					GdsResponse r = this.db.ReceiveResponse();
				} 
				catch (IOException) 
				{
					state = StatementState.Error;
					throw new IscException(IscCodes.isc_net_read_err);
				}
				catch (Exception ex)
				{
					state = StatementState.Error;
					throw ex;
				}
			}
		}
		*/

		#endregion

		#region Protected Methods

		protected override void Free(int option)
		{
			// Does not seem to be possible or necessary to close
			// an execute procedure statement.
			if (this.isSingletonResult && option == IscCodes.DSQL_close) 
			{
				return;        
			}
	        
			lock (this.db) 
			{
				try 
				{
					this.db.Send.Write(IscCodes.op_free_statement);
					this.db.Send.Write(this.handle);
					this.db.Send.Write(option);
					this.db.Send.Flush();

					// Reset statement information
					if (option == IscCodes.DSQL_drop) 
					{
						this.parameters	= null;
						this.fields		= null;
					}
					
					this.rows				= null;
					this.rowIndex			= 0;
					this.isSingletonResult	= false;
					this.allRowsFetched		= false;

					GdsResponse r = this.db.ReceiveResponse();
				} 
				catch (IOException) 
				{
					this.state = StatementState.Error;
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		protected override void TransactionUpdated(object sender, EventArgs e)
		{
			lock (this)
			{
				if (this.Transaction != null && this.TransactionUpdate != null)
				{
					this.Transaction.Update -= this.TransactionUpdate;
				}
				this.State				= StatementState.Closed;
				this.TransactionUpdate	= null;
				this.rows				= null;
				this.rowIndex			= 0;
				this.allRowsFetched		= false;
				this.isSingletonResult	= false;
			}
		}

		protected override DbStatementType GetStatementType()
		{			
			DbStatementType	stmtType	= DbStatementType.None;
			int				pos			= 0;
			int				length		= 0;
			int				type		= 0;

			byte[] buffer = this.GetSqlInfo(
				StatementTypeInfoItems, 
				IscCodes.MAX_BUFFER_SIZE);

			while ((type = buffer[pos++]) != IscCodes.isc_info_end) 
			{
				length = this.db.VaxInteger(buffer, pos, 2);
				pos += 2;
				switch (type) 
				{
					case IscCodes.isc_info_sql_stmt_type:
						stmtType = (DbStatementType)this.db.VaxInteger(buffer, pos, length);
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

		#region Response Methods

		private DbValue[] receiveSqlResponse()
		{
			try 
			{
				if (this.db.ReadOperation() == IscCodes.op_sql_response) 
				{
					int messages = this.db.Receive.ReadInt32();
					if (messages > 0) 
					{
						return this.readDataRow();
					}
					else 
					{
						return null;
					}
				} 
				else 
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			} 
			catch (IOException) 
			{
				throw new IscException(IscCodes.isc_net_read_err);
			}
		}

		private DbValue[] readDataRow()
		{
			DbValue[] row = new DbValue[this.fields.Count];

			lock (this.db)
			{
				// This only works if not (port->port_flags & PORT_symmetric)				
				for (int i = 0; i < this.fields.Count; i++) 
				{
					try 
					{
						object value = this.db.Receive.ReadValue(this.fields[i]);

						row[i] = new DbValue(this, this.fields[i], value);
					} 
					catch (IOException) 
					{
						throw new IscException(IscCodes.isc_net_read_err);
					}
				}
			}

			return row;			
		}

		#endregion

		#region Private Methods

		private void allocate()
		{
			lock (this.db) 
			{
				try 
				{
					this.db.Send.Write(IscCodes.op_allocate_statement);
					this.db.Send.Write(this.db.Handle);
					this.db.Send.Flush();

					// Receive reponse
					GdsResponse r = this.db.ReceiveResponse();

					this.handle = r.ObjectHandle;

					this.allRowsFetched	= false;
					this.rowIndex		= 0;
					this.rows			= null;
					this.state			= StatementState.Allocated;
					this.statementType	= DbStatementType.None;
				} 
				catch (IOException) 
				{
					this.state = StatementState.Deallocated;
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		private void updateRecordsAffected()
		{
			if (this.StatementType == DbStatementType.Insert ||
				this.StatementType == DbStatementType.Delete ||
				this.StatementType == DbStatementType.Update)
			{
				this.recordsAffected = this.GetRecordsAffected();
			}
			else
			{
				this.recordsAffected = -1;
			}
		}

		private RowDescriptor parseSqlInfo(byte[] info, byte[] items) 
		{
			RowDescriptor	rowDesc		= null;
			int				lastindex	= 0;

			while ((lastindex = this.parseTruncSqlInfo(info, ref rowDesc, lastindex)) > 0) 
			{
				lastindex--;               // Is this OK ?
				
				byte[] new_items = new byte[4 + items.Length];
				new_items[0] = IscCodes.isc_info_sql_sqlda_start;
				new_items[1] = 2;
				new_items[2] = (byte) (lastindex & 255);
				new_items[3] = (byte) (lastindex >> 8);

				Array.Copy(items, 0, new_items, 4, items.Length);
				info = this.GetSqlInfo(new_items, info.Length);
			}

			return rowDesc;
		}
    	    
		private int parseTruncSqlInfo(
			byte[]			info,
			ref RowDescriptor	rowDesc, 
			int				lastindex)
		{
			byte	item	= 0;
			int		index	= 0;
			int		i		= 2;

			int len = this.db.VaxInteger(info, i, 2);
			i += 2;
			int n	= this.db.VaxInteger(info, i, len);
			i += len;

			if (rowDesc == null) 
			{
				rowDesc = new RowDescriptor((short)n);
			}

			while (info[i] != IscCodes.isc_info_end) 
			{
				while ((item = info[i++]) != IscCodes.isc_info_sql_describe_end) 
				{
					switch (item) 
					{
						case IscCodes.isc_info_sql_sqlda_seq:
							len = this.db.VaxInteger(info, i, 2);
							i += 2;
							index = this.db.VaxInteger(info, i, len);
							i += len;
							break;
						
						case IscCodes.isc_info_sql_type:
							len = this.db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].DataType = (short)this.db.VaxInteger(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_sub_type:
							len = this.db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].SubType = (short)this.db.VaxInteger(info, i, len);
							i += len;
							break;
						
						case IscCodes.isc_info_sql_scale:
							len = this.db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].NumericScale = (short)this.db.VaxInteger(info, i, len);
							i += len;
							break;
						
						case IscCodes.isc_info_sql_length:
							len = this.db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Length = (short)this.db.VaxInteger(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_field:
							len = this.db.VaxInteger(info, i, 2);
							i += 2;							
							rowDesc[index - 1].Name = Encoding.Default.GetString(info, i, len);
							i += len;
							break;							
						
						case IscCodes.isc_info_sql_relation:
							len = this.db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Relation = Encoding.Default.GetString(info, i, len);
							i += len;
							break;
						
						case IscCodes.isc_info_sql_owner:
							len = this.db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Owner = Encoding.Default.GetString(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_alias:
							len = this.db.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Alias = Encoding.Default.GetString(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_truncated:
							return lastindex;

						default:
							throw new IscException(IscCodes.isc_dsql_sqlda_err);
					}
				}

				lastindex = index;
			}
			
			return 0;
		}

		#endregion
	}
}
