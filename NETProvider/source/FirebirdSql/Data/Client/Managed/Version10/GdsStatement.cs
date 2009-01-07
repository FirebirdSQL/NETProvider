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
 *	
 *  Contributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections;
using System.Text;
using System.IO;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal class GdsStatement : StatementBase
	{
		#region � Fields �

		protected int			    handle;
        protected GdsDatabase       database;
		private GdsTransaction      transaction;
		protected Descriptor	    parameters;
		private Descriptor		    fields;
		protected StatementState	state;
		protected DbStatementType   statementType;
		protected bool			    allRowsFetched;
		private Queue			    rows;
		private Queue			    outputParams;
		private int				    recordsAffected;
		private int				    fetchSize;
        private bool                returnRecordsAffected;

		#endregion

		#region � Properties �

        public override IDatabase Database
        {
            get { return this.database; }
        }

		public override ITransaction Transaction
		{
			get { return this.transaction; }
			set
			{
				if (this.transaction != value)
				{
					if (this.TransactionUpdate != null && this.transaction != null)
					{
						this.transaction.Update -= this.TransactionUpdate;
						this.TransactionUpdate	= null;
					}

					if (value == null)
					{
						this.transaction = null;
					}
					else
					{
						this.transaction		= (GdsTransaction)value;
						this.TransactionUpdate	= new TransactionUpdateEventHandler(this.TransactionUpdated);
						this.transaction.Update += this.TransactionUpdate;
					}
				}
			}
		}

        public override Descriptor Parameters
        {
            get { return this.parameters; }
            set { this.parameters = value; }
        }

		public override Descriptor Fields
		{
			get { return this.fields; }
		}

		public override int RecordsAffected
		{
			get { return this.recordsAffected; }
            protected set { this.recordsAffected = value; }
		}

		public override bool IsPrepared
		{
			get
			{
				if (this.state == StatementState.Deallocated || this.state == StatementState.Error)
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
			get { return this.statementType; }
			protected set { this.statementType = value; }
		}

		public override StatementState State
		{
			get { return this.state; }
			protected set { this.state = value; }
		}

		public override int FetchSize
		{
			get { return this.fetchSize; }
			set { this.fetchSize = value; }
		}

        public override bool ReturnRecordsAffected
        {
            get { return this.returnRecordsAffected; }
            set { this.returnRecordsAffected = value; }
        }

		#endregion

        #region � Protected Properties �

        protected int Handle
        {
            get { return this.handle; }
        }

        protected IDatabaseStream DatabaseStream
        {
            get { return this.database; }
        }

        protected Queue OutputParams
        {
            get { return this.outputParams; }
        }

        #endregion

        #region � Constructors �

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

			this.recordsAffected = -1;
			this.fetchSize		= 200;
			this.rows			= new Queue();
			this.outputParams	= new Queue();

			this.database = (GdsDatabase)db;

			if (transaction != null)
			{
				this.Transaction = transaction;
			}

			GC.SuppressFinalize(this);
		}

		#endregion

		#region � IDisposable methods �

		protected override void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!this.IsDisposed)
				{
                    try
                    {
                        // release any unmanaged resources
                        this.Release();
                    }
                    catch
                    {
                    }
					finally
					{
                        // release any managed resources
                        if (disposing)
                        {
                            this.Clear();

                            this.rows               = null;
                            this.outputParams       = null;
                            this.database           = null;
                            this.fields             = null;
                            this.parameters         = null;
                            this.transaction        = null;
                            this.allRowsFetched     = false;
                            this.state              = StatementState.Deallocated;
                            this.statementType      = DbStatementType.None;
                            this.handle             = 0;
                            this.fetchSize          = 0;
                            this.recordsAffected    = 0;
                        }
                        
                        base.Dispose(disposing);
					}
				}
			}
		}

		#endregion

		#region � Blob Creation Metods �

		public override BlobBase CreateBlob()
		{
			return new GdsBlob(this.database, this.transaction);
		}

		public override BlobBase CreateBlob(long blobId)
		{
			return new GdsBlob(this.database, this.transaction, blobId);
		}

		#endregion

		#region � Array Creation Methods �

		public override ArrayBase CreateArray(ArrayDesc descriptor)
		{
			return new GdsArray(descriptor);
		}

		public override ArrayBase CreateArray(string tableName, string fieldName)
		{
			return new GdsArray(this.database, this.transaction, tableName, fieldName);
		}

		public override ArrayBase CreateArray(long handle, string tableName, string fieldName)
		{
			return new GdsArray(this.database, this.transaction, handle, tableName, fieldName);
		}

		#endregion

		#region � Methods �

		public override void Prepare(string commandText)
		{
			// Clear data
            this.ClearAll();

			lock (this.database.SyncObject)
			{
                try
                {
                    if (this.state == StatementState.Deallocated)
                    {
                        // Allocate	statement
                        this.SendAllocateToBuffer();
                        this.database.Flush();
                        this.ProcessAllocateResponce(this.database.ReadGenericResponse());
                    }

                    this.database.Write(IscCodes.op_prepare_statement);
                    this.database.Write(this.transaction.Handle);
                    this.database.Write(this.handle);
                    this.database.Write((int)this.database.Dialect);
                    this.database.Write(commandText);
                    this.database.WriteBuffer(DescribeInfoItems, DescribeInfoItems.Length);
                    this.database.Write(IscCodes.MAX_BUFFER_SIZE);

                    this.database.Flush();

                    // Read Fields Information
                    this.ProcessPrepareResponse(this.database.ReadGenericResponse());

                    // Determine the statement type
                    this.statementType = this.GetStatementType();

                    this.state = StatementState.Prepared;
                }
                catch (IOException)
                {
                    // if the statement has been already allocated, it's now in error
                    if (this.state == StatementState.Allocated)
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

			// Clear data
			this.Clear();

			lock (this.database.SyncObject)
			{
				try
				{
                    this.recordsAffected = -1;

                    // Build Parameter description
                    byte[] descriptor = null;

                    if (this.parameters != null)
                    {
                        using (XdrStream xdr = new XdrStream(database.Charset))
                        {
                            xdr.Write(this.parameters);
                            descriptor = xdr.ToArray();
                            xdr.Close();
                        }
                    }

                    // Write the message
					if (this.statementType == DbStatementType.StoredProcedure)
					{
						this.database.Write(IscCodes.op_execute2);
					}
					else
					{
						this.database.Write(IscCodes.op_execute);
					}

					this.database.Write(this.handle);
					this.database.Write(this.transaction.Handle);

					if (this.parameters != null)
					{
						this.database.WriteBuffer(this.parameters.ToBlrArray());
						this.database.Write(0);	// Message number
						this.database.Write(1);	// Number of messages
                        database.Write(descriptor, 0, descriptor.Length);
					}
					else
					{
						this.database.WriteBuffer(null);
						this.database.Write(0);
						this.database.Write(0);
					}

					if (this.statementType == DbStatementType.StoredProcedure)
					{
						this.database.WriteBuffer((this.fields == null) ? null : this.fields.ToBlrArray());
						this.database.Write(0);	// Output message number
					}

					this.database.Flush();

                    if (this.statementType == DbStatementType.StoredProcedure)
					{
						this.ProcessStoredProcedureResponse(this.database.ReadResponse());
					}

                    this.database.ReadResponse();
 
					// Updated number of records affected by the statement execution			
					if (this.ReturnRecordsAffected &&
                        (this.StatementType == DbStatementType.Insert ||
						this.StatementType == DbStatementType.Delete ||
						this.StatementType == DbStatementType.Update ||
                        this.StatementType == DbStatementType.StoredProcedure))
					{
						this.recordsAffected = this.GetRecordsAffected();
					}

					this.state = StatementState.Executed;
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
				throw new InvalidOperationException("Statement is not correctly created.");
			}
			if (this.statementType != DbStatementType.Select &&
				this.statementType != DbStatementType.SelectForUpdate)
			{
				return null;
			}

			if (!this.allRowsFetched && this.rows.Count == 0)
			{
				// Fetch next batch	of rows
				lock (this.database.SyncObject)
				{
					try
					{
						this.database.Write(IscCodes.op_fetch);
						this.database.Write(this.handle);
						this.database.WriteBuffer(this.fields.ToBlrArray());
						this.database.Write(0);			// p_sqldata_message_number						
						this.database.Write(fetchSize);	// p_sqldata_messages
						this.database.Flush();

						if (this.database.NextOperation() == IscCodes.op_fetch_response)
						{
                            IResponse response = null;

                            while (!this.allRowsFetched)
							{
                                response = this.database.ReadResponse();

                                if (response is FetchResponse)
                                {
                                    FetchResponse fetchResponse = (FetchResponse)response;

                                    if (fetchResponse.Count > 0 && fetchResponse.Status == 0)
                                    {
                                        this.rows.Enqueue(this.ReadDataRow());
                                    }
                                    else if (fetchResponse.Status == 100)
                                    {
                                        this.allRowsFetched = true;
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
							this.database.ReadResponse();
						}
					}
					catch (IOException)
					{
						throw new IscException(IscCodes.isc_net_read_err);
					}
				}
			}

			if (this.rows != null && this.rows.Count > 0)
			{
				return (DbValue[])this.rows.Dequeue();
			}
			else
			{
				this.rows.Clear();

				return null;
			}
		}

		public override DbValue[] GetOutputParameters()
		{
			if (this.outputParams.Count > 0)
			{
				return (DbValue[])this.outputParams.Dequeue();
			}

			return null;
		}

		public override void Describe()
		{
			try
			{
				byte[] buffer = this.GetSqlInfo(DescribeInfoItems);
				this.fields = this.ParseSqlInfo(buffer, DescribeInfoItems);
			}
			catch (IscException)
			{
				throw;
			}
		}

		public override void DescribeParameters()
		{
			try
			{
				byte[] buffer = this.GetSqlInfo(DescribeBindInfoItems);
				this.parameters = this.ParseSqlInfo(buffer, DescribeBindInfoItems);
			}
			catch (IscException)
			{
				throw;
			}
		}

		public override byte[] GetSqlInfo(byte[] items, int bufferLength)
		{
			lock (this.database.SyncObject)
			{
				try
				{
					this.database.Write(IscCodes.op_info_sql);
					this.database.Write(this.handle);
					this.database.Write(0);
					this.database.WriteBuffer(items, items.Length);
					this.database.Write(bufferLength);
					this.database.Flush();

                    GenericResponse response = this.database.ReadGenericResponse();

					return response.Data;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		#endregion

		#region � Protected Methods �

        protected virtual void ProcessPrepareResponse(GenericResponse response)
        {
            this.fields = this.ParseSqlInfo(response.Data, DescribeInfoItems);
        }

		protected override void Free(int option)
		{
			// Does	not	seem to	be possible	or necessary to	close
			// an execute procedure	statement.
			if (this.StatementType == DbStatementType.StoredProcedure && option == IscCodes.DSQL_close)
			{
				return;
			}

			lock (this.database.SyncObject)
			{
				try
				{
					this.database.Write(IscCodes.op_free_statement);
					this.database.Write(this.handle);
					this.database.Write(option);
					this.database.Flush();

					// Reset statement information
					if (option == IscCodes.DSQL_drop)
					{
						this.parameters = null;
						this.fields     = null;
					}

					this.Clear();

					this.database.ReadResponse();
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

				this.state              = StatementState.Closed;
				this.TransactionUpdate  = null;
				this.allRowsFetched     = false;
			}
		}

		#endregion

		#region � Protected Methods �

        protected void ProcessStoredProcedureResponse(IResponse response)
		{
			try
			{
				if (response is SqlResponse)
				{
					if (((SqlResponse)response).Count > 0)
					{
						this.OutputParams.Enqueue(this.ReadDataRow());
					}
				}
			}
			catch (IOException)
			{
				throw new IscException(IscCodes.isc_net_read_err);
			}
		}

        protected DbValue[] ReadDataRow()
		{
			DbValue[] row = new DbValue[this.fields.Count];
			object value = null;

			lock (this.database.SyncObject)
			{
				// This	only works if not (port->port_flags	& PORT_symmetric)				
				for (int i = 0; i < this.fields.Count; i++)
				{
					try
					{
						value = this.database.ReadValue(this.fields[i]);
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

        protected void SendAllocateToBuffer()
        {
            this.database.Write(IscCodes.op_allocate_statement);
            this.database.Write(this.database.Handle);
        }

        protected void ProcessAllocateResponce(GenericResponse response)
        {
            this.handle = response.ObjectHandle;
            this.allRowsFetched = false;
            this.state = StatementState.Allocated;
            this.statementType = DbStatementType.None;
        }

        protected Descriptor ParseSqlInfo(byte[] info, byte[] items)
		{
			Descriptor rowDesc = null;
			int lastindex = 0;

			while ((lastindex = this.ParseTruncSqlInfo(info, ref rowDesc, lastindex)) > 0)
			{
				lastindex--;			   // Is this OK ?

				byte[] new_items = new byte[4 + items.Length];

				new_items[0] = IscCodes.isc_info_sql_sqlda_start;
				new_items[1] = 2;
				new_items[2] = (byte)(lastindex & 255);
				new_items[3] = (byte)(lastindex >> 8);

				Array.Copy(items, 0, new_items, 4, items.Length);

				info = this.GetSqlInfo(new_items, info.Length);
			}

			return rowDesc;
		}

        protected int ParseTruncSqlInfo(byte[] info, ref Descriptor rowDesc, int lastindex)
		{
			byte	item	= 0;
			int		index	= 0;
			int		i		= 2;

			int len = IscHelper.VaxInteger(info, i, 2);
			i += 2;
			int n = IscHelper.VaxInteger(info, i, len);
			i += len;

			if (rowDesc == null)
			{
				rowDesc = new Descriptor((short)n);
			}

			while (info[i] != IscCodes.isc_info_end)
			{
				while ((item = info[i++]) != IscCodes.isc_info_sql_describe_end)
				{
					switch (item)
					{
						case IscCodes.isc_info_sql_sqlda_seq:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							index = IscHelper.VaxInteger(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_type:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].DataType = (short)IscHelper.VaxInteger(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_sub_type:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].SubType = (short)IscHelper.VaxInteger(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_scale:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].NumericScale = (short)IscHelper.VaxInteger(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_length:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Length = (short)IscHelper.VaxInteger(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_field:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Name = this.database.Charset.GetString(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_relation:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Relation = this.database.Charset.GetString(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_owner:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Owner = this.database.Charset.GetString(info, i, len);
							i += len;
							break;

						case IscCodes.isc_info_sql_alias:
							len = IscHelper.VaxInteger(info, i, 2);
							i += 2;
							rowDesc[index - 1].Alias = this.database.Charset.GetString(info, i, len);
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

        protected void Clear()
		{
			if (this.rows != null && this.rows.Count > 0)
			{
				this.rows.Clear();
			}
			if (this.outputParams != null && this.outputParams.Count > 0)
			{
				this.outputParams.Clear();
			}

            this.allRowsFetched = false;
		}

#warning Finish implementation
        protected void ClearAll()
        {
            this.Clear();

            this.parameters = null;
            this.fields     = null;
        }

		#endregion
	}
}