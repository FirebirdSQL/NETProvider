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
using System.Text;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Embedded
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class FesStatement : StatementBase
	{
		#region Fields

		private int					handle;
		private FesDbAttachment		db;
		private FesTransaction		transaction;
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
			set { this.db = (FesDbAttachment)value; }
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
							this.Transaction != null)
						{
							this.Transaction.Update -= this.TransactionUpdate ;
							this.TransactionUpdate  = null;
						}
					
						// Add event handler for transaction updates
						this.TransactionUpdate = new TransactionUpdateEventHandler(this.TransactionUpdated);

						addHandler = true;
					}

					this.transaction = (FesTransaction)value;

					if (addHandler)
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

		public FesStatement(IDbAttachment db) : this(db, null)
		{
		}

		public FesStatement(IDbAttachment db, ITransaction transaction)
		{
			if (!(db is FesDbAttachment))
			{
				throw new ArgumentException("Specified argument is not of FesDbAttachment type.");
			}

			this.fetchSize			= 200;
			this.recordsAffected	= -1;
			this.db					= (FesDbAttachment)db;
			
			if (transaction != null)
			{
				this.Transaction = transaction;
			}
		}

		#endregion

		#region Blob Creation Metods

		public override BlobBase CreateBlob()
		{
			return new FesBlob(this.db, this.transaction);
		}

		public override BlobBase CreateBlob(long blobId)
		{
			return new FesBlob(this.db, this.transaction, blobId);
		}

		#endregion

		#region Array Creation Methods

		public override ArrayBase CreateArray(string tableName, string fieldName)
		{
			return new FesArray(this.db, this.transaction, tableName, fieldName);
		}

		public override ArrayBase CreateArray(
			long handle,
			string tableName,
			string fieldName)
		{
			return new FesArray(this.db, this.transaction, handle, tableName, fieldName);
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

				// Marshal structures to pointer
				XsqldaMarshaler marshaler = XsqldaMarshaler.GetInstance();

				// Setup fields structure
				this.fields = new RowDescriptor(1);

				IntPtr sqlda = marshaler.MarshalManagedToNative(this.fields);

				int[]	statusVector	= FesAttachment.GetNewStatusVector();
				int		trHandle		= this.transaction.Handle;
				int		stmtHandle		= this.handle;

                byte[] buffer = this.db.Parameters.Charset.Encoding.GetBytes(commandText);

                int status = FbClient.isc_dsql_prepare(
					statusVector,
					ref trHandle,
					ref stmtHandle,
					(short)buffer.Length,
					buffer,
					this.db.Parameters.Dialect,
					sqlda);

				// Marshal Pointer
				RowDescriptor descriptor = marshaler.MarshalNativeToManaged(sqlda);

				// Free memory
				marshaler.CleanUpNativeData(ref sqlda);

				// Parse status vector
				this.db.ParseStatusVector(statusVector);

				// Update state
				this.state = StatementState.Prepared;

				// Get Statement type
				this.statementType = this.GetStatementType();

				// Describe fields
				this.fields = descriptor;
				if (this.fields.ActualCount > 0 &&
					this.fields.ActualCount != this.fields.Count)
				{
					this.Describe();
				}
				else
				{
					if (this.fields.ActualCount == 0)
					{
						this.fields = new RowDescriptor(0);
					}
				}

				// Reset actual field values
				this.fields.ResetValues();
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
				// Marshal structures to pointer
				XsqldaMarshaler marshaler = XsqldaMarshaler.GetInstance();

				IntPtr in_sqlda		= IntPtr.Zero;
				IntPtr out_sqlda	= IntPtr.Zero;

				if (this.parameters != null)
				{
					in_sqlda = marshaler.MarshalManagedToNative(this.parameters);
				}
				if (this.statementType == DbStatementType.StoredProcedure)
				{
					this.Fields.ResetValues();
					out_sqlda = marshaler.MarshalManagedToNative(this.fields);
				}

				int[]	statusVector	= FesAttachment.GetNewStatusVector();
				int		trHandle		= this.transaction.Handle;
				int		stmtHandle		= this.handle;

				int status = FbClient.isc_dsql_execute2(
					statusVector,
					ref trHandle,
					ref stmtHandle,
					IscCodes.SQLDA_VERSION1,
					in_sqlda,
					out_sqlda);

				if (out_sqlda != IntPtr.Zero)
				{
					RowDescriptor descriptor = marshaler.MarshalNativeToManaged(out_sqlda);

					// This would be an Execute procedure
					DbValue[] values = new DbValue[descriptor.Count];
					
					for (int i = 0; i < values.Length; i++)
					{
						values[i] = new DbValue(this, descriptor[i]);
					}

					this.rows				= new object[1];
					this.rows[0]			= values;
					this.allRowsFetched		= true;
					this.isSingletonResult	= true;
				}

				// Reset the row index
				this.rowIndex = 0;

				// Free memory
				marshaler.CleanUpNativeData(ref in_sqlda);
				marshaler.CleanUpNativeData(ref out_sqlda);

				this.db.ParseStatusVector(statusVector);

				this.updateRecordsAffected();

				this.state = StatementState.Executed;
			}
		}

		public override void ExecuteImmediate(string commandText)
		{
			lock (this.db)
			{
				// Marshal structures to pointer
				XsqldaMarshaler marshaler = XsqldaMarshaler.GetInstance();

				IntPtr in_sqlda		= IntPtr.Zero;
				IntPtr out_sqlda	= IntPtr.Zero;

				if (this.parameters != null)
				{
					in_sqlda = marshaler.MarshalManagedToNative(this.parameters);
				}
				if (this.fields != null)
				{
					this.Fields.ResetValues();
					out_sqlda = marshaler.MarshalManagedToNative(this.fields);
				}

				int[]	statusVector	= FesAttachment.GetNewStatusVector();
				int		dbHandle		= this.db.Handle;
				int		trHandle		= this.transaction.Handle;

				int status = FbClient.isc_dsql_exec_immed2(
					statusVector,
					ref dbHandle,
					ref trHandle,
					(short)commandText.Length,
					commandText,
					this.db.Parameters.Dialect,
					in_sqlda,
					out_sqlda);

				if (out_sqlda != IntPtr.Zero)
				{
					RowDescriptor descriptor = marshaler.MarshalNativeToManaged(out_sqlda);

					// Read result
					DbValue[] values = new DbValue[descriptor.ActualCount];
					
					for (int i = 0; i < values.Length; i++)
					{
						values[i] = new DbValue(this, descriptor[i]);
					}

					this.rows				= new object[1];
					this.rows[0]			= values;
					this.rowIndex			= 0;
					this.allRowsFetched		= true;
					this.isSingletonResult	= false;
				}

				// Free memory
				marshaler.CleanUpNativeData(ref in_sqlda);
				marshaler.CleanUpNativeData(ref out_sqlda);

				this.db.ParseStatusVector(statusVector);

				this.updateRecordsAffected();
			}
		}

		public override DbValue[] Fetch()
		{
			DbValue[] row = null;

			if (this.state == StatementState.Deallocated)
			{
				throw new InvalidOperationException("Statment is not correctly created.");
			}

			lock (this.db)
			{
				if (this.statementType == DbStatementType.Select			||
					this.statementType == DbStatementType.SelectForUpdate	||
					this.statementType == DbStatementType.StoredProcedure)
				{
					if ((!this.allRowsFetched && this.rowIndex == 0) ||
						(!this.allRowsFetched && this.rowIndex >= fetchSize))
					{
						// Marshal structures to pointer
						XsqldaMarshaler marshaler = XsqldaMarshaler.GetInstance();

						// Reset actual field values
						this.fields.ResetValues();

						IntPtr	sqlda = marshaler.MarshalManagedToNative(fields);

						int[]	statusVector	= FesAttachment.GetNewStatusVector();
						int		stmtHandle		= this.handle;

						int status = FbClient.isc_dsql_fetch(
							statusVector,
							ref stmtHandle,
							IscCodes.SQLDA_VERSION1,
							sqlda);

						// Obtain values
						this.fields = marshaler.MarshalNativeToManaged(sqlda);

						// Free memory
						marshaler.CleanUpNativeData(ref sqlda);
               
						// Parse status vector
						this.db.ParseStatusVector(statusVector);
				
						if (status == 100)
						{
							this.allRowsFetched = true;
						}
						else
						{
							// set row values
							row = new DbValue[this.fields.ActualCount];
							for (int i = 0; i < row.Length; i++)
							{
								row[i] = new DbValue(this, this.fields[i]);
							}
						}
					}
				}
			}

			return row;
		}

		public override void Describe()
		{
			lock (this.db) 
			{
				// Update structure
				this.fields = new RowDescriptor(this.fields.ActualCount);

				// Marshal structures to pointer
				XsqldaMarshaler marshaler = XsqldaMarshaler.GetInstance();

				IntPtr sqlda = marshaler.MarshalManagedToNative(fields);

				int[]	statusVector	= FesAttachment.GetNewStatusVector();
				int		stmtHandle		= this.handle;

				int status = FbClient.isc_dsql_describe(
					statusVector,
					ref stmtHandle,
					IscCodes.SQLDA_VERSION1,
					sqlda);

				// Marshal Pointer
				RowDescriptor descriptor = marshaler.MarshalNativeToManaged(
					sqlda);

				// Free memory
				marshaler.CleanUpNativeData(ref sqlda);

				// Parse status vector
				this.db.ParseStatusVector(statusVector);

				// Update field descriptor
				this.fields = descriptor;
			}
		}

		public override void DescribeParameters()
		{
			lock (this.db) 
			{
				// Marshal structures to pointer
				XsqldaMarshaler marshaler = XsqldaMarshaler.GetInstance();

				this.parameters = new RowDescriptor(1);

				IntPtr	sqlda = marshaler.MarshalManagedToNative(parameters);

				int[]	statusVector	= FesAttachment.GetNewStatusVector();
				int		stmtHandle		= this.handle;

				int status = FbClient.isc_dsql_describe_bind(
					statusVector,
					ref stmtHandle,
					IscCodes.SQLDA_VERSION1,
					sqlda);

				RowDescriptor descriptor = marshaler.MarshalNativeToManaged(
					sqlda);

				// Parse status vector
				this.db.ParseStatusVector(statusVector);
				
				if (descriptor.ActualCount != 0 &&
					descriptor.Count != descriptor.ActualCount)
				{
					short n = descriptor.ActualCount;
					descriptor = new RowDescriptor(n);

					// Fre memory
					marshaler.CleanUpNativeData(ref sqlda);

					// Marshal new structure
					sqlda = marshaler.MarshalManagedToNative(descriptor);

					status = FbClient.isc_dsql_describe_bind(
						statusVector,
						ref stmtHandle,
						IscCodes.SQLDA_VERSION1,
						sqlda);

					descriptor = marshaler.MarshalNativeToManaged(sqlda);

					// Free memory
					marshaler.CleanUpNativeData(ref sqlda);

					// Parse status vector
					this.db.ParseStatusVector(statusVector);
				}
				else
				{
					if (descriptor.ActualCount == 0)
					{
						descriptor = new RowDescriptor(0);
					}
				}

				// Free memory
				if (sqlda != IntPtr.Zero)
				{
					marshaler.CleanUpNativeData(ref sqlda);
				}

				// Update parameter descriptor
				this.parameters = descriptor;
			}
		}

		public override byte[] GetSqlInfo(byte[] items, int bufferLength)
		{
			lock (this.db) 
			{
				byte[]	buffer			= new byte[bufferLength];
				int[]	statusVector	= FesAttachment.GetNewStatusVector();
				int		stmtHandle		= this.handle;

				int status = FbClient.isc_dsql_sql_info(
					statusVector,
					ref stmtHandle,
					(short)items.Length,
					items,
					(short)bufferLength,
					buffer);

				this.db.ParseStatusVector(statusVector);

				return buffer;
			}
		}

		/*
		public override void SetCursorName(string name)
		{
			lock (this.db) 
			{
				int[]	statusVector	= FesAttachment.GetNewStatusVector();
				int		stmtHandle		= this.handle;

				int status = FbClient.isc_dsql_set_cursor_name(
					statusVector,
					ref stmtHandle,
					name,
					0);

				this.db.ParseStatusVector(statusVector);
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
				int[]	statusVector	= FesAttachment.GetNewStatusVector();
				int		stmtHandle		= this.handle;

				int status = FbClient.isc_dsql_free_statement(
					statusVector,
					ref stmtHandle,
					(short)option);

				this.handle = stmtHandle;

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

				this.db.ParseStatusVector(statusVector);
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

		#region Private Methods

		private void allocate()
		{
			lock (this.db) 
			{
				int[]	statusVector	= FesAttachment.GetNewStatusVector();
				int		dbHandle		= this.db.Handle;
				int		stmtHandle		= this.handle;

				int status = FbClient.isc_dsql_allocate_statement(
					statusVector,
					ref dbHandle,
					ref stmtHandle);

				this.db.ParseStatusVector(statusVector);

				this.handle			= stmtHandle;
				this.allRowsFetched	= false;
				this.rowIndex		= 0;
				this.rows			= null;
				this.state			= StatementState.Allocated;
				this.statementType	= DbStatementType.None;
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

		#endregion
	}
}