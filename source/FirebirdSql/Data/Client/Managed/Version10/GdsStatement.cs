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
 *	Copyright (c) 2007 - 2008 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
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
		#region · Fields ·

		protected int			    handle;
        protected GdsDatabase       database;
		private GdsTransaction      transaction;
		protected Descriptor	    parameters;
		protected Descriptor	    fields;
		protected StatementState	state;
		protected DbStatementType   statementType;
		protected bool			    allRowsFetched;
		private Queue			    rows;
		private Queue			    outputParams;
		private int				    recordsAffected;
		private int				    fetchSize;
        private bool                returnRecordsAffected;

		#endregion

		#region · Properties ·

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

        #region · Constructors ·

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

            this.handle = IscCodes.INVALID_OBJECT;
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

		#region · IDisposable methods ·

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

		#region · Blob Creation Metods ·

		public override BlobBase CreateBlob()
		{
			return new GdsBlob(this.database, this.transaction);
		}

		public override BlobBase CreateBlob(long blobId)
		{
			return new GdsBlob(this.database, this.transaction, blobId);
		}

		#endregion

		#region · Array Creation Methods ·

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

		#region · Methods ·

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

                    this.SendPrepareToBuffer(commandText);
                    this.database.Flush();
                    this.ProcessPrepareResponse(this.database.ReadGenericResponse());

                    // Grab statement type
                    this.SendInfoSqlToBuffer(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE);
                    this.database.Flush(); 
                    this.statementType = this.ProcessStatementTypeInfoBuffer(this.ProcessInfoSqlResponse(this.database.ReadGenericResponse()));


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
				throw new InvalidOperationException("Statement is not correctly created.");
			}

			// Clear data
			this.Clear();

			lock (this.database.SyncObject)
			{
				try
				{
                    this.recordsAffected = -1;

                    this.SendExecuteToBuffer();

					this.database.Flush();

                    if (this.statementType == DbStatementType.StoredProcedure)
					{
						this.ProcessStoredProcedureExecuteResponse(this.database.ReadSqlResponse());
					}

                    GenericResponse executeResponse = this.database.ReadGenericResponse();
 
					// Updated number of records affected by the statement execution			
					if (this.ReturnRecordsAffected &&
                        (this.StatementType == DbStatementType.Insert ||
						this.StatementType == DbStatementType.Delete ||
						this.StatementType == DbStatementType.Update ||
                        this.StatementType == DbStatementType.StoredProcedure))
					{
                        this.SendInfoSqlToBuffer(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE);
                        this.database.Flush();
                        this.RecordsAffected = this.ProcessRecordsAffectedBuffer(this.ProcessInfoSqlResponse(this.database.ReadGenericResponse()));
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
            System.Diagnostics.Debug.Assert(true);
		}
        // these are not needed for Gds, because it's pre-fetched in Prepare
        // maybe we can fetch these also for Fes and Ext etc.
        public override void DescribeParameters()
        {
            System.Diagnostics.Debug.Assert(true);
        }

		#endregion

		#region · Protected Methods ·

        #region op_prepare methods
        protected void SendPrepareToBuffer(string commandText)
        {
            this.database.Write(IscCodes.op_prepare_statement);
            this.database.Write(this.transaction.Handle);
            this.database.Write(this.handle);
            this.database.Write((int)this.database.Dialect);
            this.database.Write(commandText);
            this.database.WriteBuffer(DescribeInfoAndBindInfoItems, DescribeInfoAndBindInfoItems.Length);
            this.database.Write(IscCodes.MAX_BUFFER_SIZE);
        }

        protected void ProcessPrepareResponse(GenericResponse response)
        {
            int lastPosition = 0;
            this.fields = this.ParseSqlInfo(response.Data, DescribeInfoAndBindInfoItems, ref lastPosition);
            this.parameters = this.ParseSqlInfo(response.Data, DescribeInfoAndBindInfoItems, ref lastPosition);
        }
        #endregion

        #region op_info_sql methods
        protected override byte[] GetSqlInfo(byte[] items, int bufferLength)
        {
            lock (this.database.SyncObject)
            {
                DoInfoSqlPacket(items, bufferLength);
                this.database.Flush();
                return ProcessInfoSqlResponse(this.database.ReadGenericResponse());
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
            this.database.Write(IscCodes.op_info_sql);
            this.database.Write(this.handle);
            this.database.Write(0);
            this.database.WriteBuffer(items, items.Length);
            this.database.Write(bufferLength);
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

			lock (this.database.SyncObject)
			{
				DoFreePacket(option);
                this.database.Flush();
                ProcessFreeResponse(this.database.ReadResponse());
			}
		}

        protected bool FreeNotNeeded(int option)
        {
            // Does	not	seem to	be possible	or necessary to	close
            // an execute procedure	statement.
            if (this.StatementType == DbStatementType.StoredProcedure && option == IscCodes.DSQL_close)
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
                    this.parameters = null;
                    this.fields = null;
                }

                this.Clear();
            }
            catch (IOException)
            {
                this.state = StatementState.Error;
                throw new IscException(IscCodes.isc_net_read_err);
            }
        }

        protected void SendFreeToBuffer(int option)
        {
            this.database.Write(IscCodes.op_free_statement);
            this.database.Write(this.handle);
            this.database.Write(option);
        }

        protected void ProcessFreeResponse(IResponse response)
        {

        }
        #endregion

        #region op_allocate_statement methods
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
        #endregion

        #region op_execute/op_execute2 methods
        protected void SendExecuteToBuffer()
        {
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
                byte[] descriptor = this.BuildParameterDescriptor();

                this.database.WriteBuffer(this.parameters.ToBlrArray());
                this.database.Write(0);	// Message number
                this.database.Write(1);	// Number of messages
                this.database.Write(descriptor, 0, descriptor.Length);
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
        }

        protected void ProcessStoredProcedureExecuteResponse(SqlResponse response)
        {
            try
            {
                if (response.Count > 0)
                {
                    this.outputParams.Enqueue(this.ReadDataRow());
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
				if (this.Transaction != null && this.TransactionUpdate != null)
				{
					this.Transaction.Update -= this.TransactionUpdate;
				}

				this.state              = StatementState.Closed;
				this.TransactionUpdate  = null;
				this.allRowsFetched     = false;
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

        protected Descriptor ParseSqlInfo(byte[] info, byte[] items, ref int lastPosition)
		{
			Descriptor rowDesc = null;
            int lastIndex;

			while (!this.ParseTruncSqlInfo(info, ref rowDesc, out lastIndex, ref lastPosition))
			{
				lastIndex--;			   // Is this OK ?

				byte[] new_items = new byte[4 + items.Length];

				new_items[0] = IscCodes.isc_info_sql_sqlda_start;
				new_items[1] = 2;
				new_items[2] = (byte)(lastIndex & 255);
				new_items[3] = (byte)(lastIndex >> 8);

				Array.Copy(items, 0, new_items, 4, items.Length);

				info = this.GetSqlInfo(new_items, info.Length);
			}

			return rowDesc;
		}

        protected bool ParseTruncSqlInfo(byte[] info, ref Descriptor rowDesc, out int lastIndex, ref int currentPosition)
		{
			lastIndex = 0;
            currentPosition = currentPosition + 2;

			int len = IscHelper.VaxInteger(info, currentPosition, 2);
			currentPosition += 2;
			int n = IscHelper.VaxInteger(info, currentPosition, len);
			currentPosition += len;

			if (rowDesc == null)
			{
				rowDesc = new Descriptor((short)n);
			}

            byte item;
            while (info[currentPosition] != IscCodes.isc_info_end &&
                info[currentPosition] != IscCodes.isc_info_sql_select && info[currentPosition] != IscCodes.isc_info_sql_bind)
            {
                while ((item = info[currentPosition++]) != IscCodes.isc_info_sql_describe_end)
                {
                    switch (item)
                    {
                        case IscCodes.isc_info_sql_sqlda_seq:
                            len = IscHelper.VaxInteger(info, currentPosition, 2);
                            currentPosition += 2;
                            lastIndex = IscHelper.VaxInteger(info, currentPosition, len);
                            currentPosition += len;
                            break;

                        case IscCodes.isc_info_sql_type:
                            len = IscHelper.VaxInteger(info, currentPosition, 2);
                            currentPosition += 2;
                            rowDesc[lastIndex - 1].DataType = (short)IscHelper.VaxInteger(info, currentPosition, len);
                            currentPosition += len;
                            break;

                        case IscCodes.isc_info_sql_sub_type:
                            len = IscHelper.VaxInteger(info, currentPosition, 2);
                            currentPosition += 2;
                            rowDesc[lastIndex - 1].SubType = (short)IscHelper.VaxInteger(info, currentPosition, len);
                            currentPosition += len;
                            break;

                        case IscCodes.isc_info_sql_scale:
                            len = IscHelper.VaxInteger(info, currentPosition, 2);
                            currentPosition += 2;
                            rowDesc[lastIndex - 1].NumericScale = (short)IscHelper.VaxInteger(info, currentPosition, len);
                            currentPosition += len;
                            break;

                        case IscCodes.isc_info_sql_length:
                            len = IscHelper.VaxInteger(info, currentPosition, 2);
                            currentPosition += 2;
                            rowDesc[lastIndex - 1].Length = (short)IscHelper.VaxInteger(info, currentPosition, len);
                            currentPosition += len;
                            break;

                        case IscCodes.isc_info_sql_field:
                            len = IscHelper.VaxInteger(info, currentPosition, 2);
                            currentPosition += 2;
                            rowDesc[lastIndex - 1].Name = this.database.Charset.GetString(info, currentPosition, len);
                            currentPosition += len;
                            break;

                        case IscCodes.isc_info_sql_relation:
                            len = IscHelper.VaxInteger(info, currentPosition, 2);
                            currentPosition += 2;
                            rowDesc[lastIndex - 1].Relation = this.database.Charset.GetString(info, currentPosition, len);
                            currentPosition += len;
                            break;

                        case IscCodes.isc_info_sql_owner:
                            len = IscHelper.VaxInteger(info, currentPosition, 2);
                            currentPosition += 2;
                            rowDesc[lastIndex - 1].Owner = this.database.Charset.GetString(info, currentPosition, len);
                            currentPosition += len;
                            break;

                        case IscCodes.isc_info_sql_alias:
                            len = IscHelper.VaxInteger(info, currentPosition, 2);
                            currentPosition += 2;
                            rowDesc[lastIndex - 1].Alias = this.database.Charset.GetString(info, currentPosition, len);
                            currentPosition += len;
                            break;

                        case IscCodes.isc_info_truncated:
                            return false;

                        default:
                            throw new IscException(IscCodes.isc_dsql_sqlda_err);
                    }
                }
            }

            return true;
		}

        protected byte[] BuildParameterDescriptor()
        {
            if (this.parameters == null)
            {
                throw new InvalidOperationException("Cannot build descriptor from null parameters.");
            }

            using (XdrStream xdr = new XdrStream(this.database.Charset))
            {
                xdr.Write(this.parameters);
                return xdr.ToArray();
            }
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

        protected void ClearAll()
        {
            this.Clear();

            this.parameters = null;
            this.fields = null;
        }

		#endregion
	}
}
