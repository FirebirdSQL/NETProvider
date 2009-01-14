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
using System.Collections.Generic;
using System.Text;
using System.IO;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.Client.Managed.Version10;

namespace FirebirdSql.Data.Client.Managed.Version11
{
    internal class GdsStatement : FirebirdSql.Data.Client.Managed.Version10.GdsStatement
    {
        #region · Constructors ·

        public GdsStatement(IDatabase db)
            : base(db)
        { }

        public GdsStatement(IDatabase db, ITransaction transaction)
            : base(db, transaction)
        { }

        #endregion

        #region · Overriden Methods ·

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
                        // Allocate statement
                        this.SendAllocateToBuffer();
                    }

                    // Prepare the statement
                    this.database.Write(IscCodes.op_prepare_statement);
                    this.database.Write(this.Transaction.Handle);
                    this.database.Write((int)IscCodes.INVALID_OBJECT);
                    this.database.Write((int)this.database.Dialect);
                    this.database.Write(commandText);
                    this.database.WriteBuffer(DescribeInfoAndBindInfoItems, DescribeInfoAndBindInfoItems.Length);
                    this.database.Write(IscCodes.MAX_BUFFER_SIZE);

                    // Grab statement type
                    this.WriteSqlInfoRequest(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE, IscCodes.INVALID_OBJECT);

                    this.database.Flush();

                    // allocate, prepare, statement type
                    int numberOfResponses = 3;
                    try
                    {
                        numberOfResponses--;
                        GenericResponse allocateResponse = this.database.ReadGenericResponse();
                        this.ProcessAllocateResponce(allocateResponse);

                        numberOfResponses--;
                        GenericResponse prepareResponse = this.database.ReadGenericResponse();
                        this.ProcessPrepareResponse(prepareResponse);
                        
                        numberOfResponses--;
                        GenericResponse statementTypeResponse = this.database.ReadGenericResponse();
                        this.statementType = this.ParseStatementTypeInfo(statementTypeResponse.Data);
                    }
                    finally
                    {
                        FinishFetching(ref numberOfResponses);
                    }

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
                    this.RecordsAffected = -1;

                    // Build Parameter description
                    byte[] descriptor = null;

                    if (this.parameters != null)
                    {
                        using (XdrStream xdr = new XdrStream(this.database.Charset))
                        {
                            xdr.Write(this.parameters);
                            descriptor = xdr.ToArray();
                            xdr.Close();
                        }
                    }

                    // Write the message
                    if (this.StatementType == DbStatementType.StoredProcedure)
                    {
                        this.database.Write(IscCodes.op_execute2);
                    }
                    else
                    {
                        this.database.Write(IscCodes.op_execute);
                    }

                    this.database.Write(this.handle);
                    this.database.Write(this.Transaction.Handle);

                    if (this.parameters != null)
                    {
                        this.database.WriteBuffer(this.parameters.ToBlrArray());
                        this.database.Write(0);    // Message number
                        this.database.Write(1);    // Number of messages
                        this.database.Write(descriptor, 0, descriptor.Length);
                    }
                    else
                    {
                        this.database.WriteBuffer(null);
                        this.database.Write(0);
                        this.database.Write(0);
                    }

                    if (this.StatementType == DbStatementType.StoredProcedure)
                    {
                        this.database.WriteBuffer((this.Fields == null) ? null : this.Fields.ToBlrArray());
                        this.database.Write(0);    // Output message number
                    }

                    bool readRowsAffectedResponse = false;
                    // Obtain records affected by query execution
                    if (this.ReturnRecordsAffected &&
                        (this.StatementType == DbStatementType.Insert ||
                        this.StatementType == DbStatementType.Delete ||
                        this.StatementType == DbStatementType.Update ||
                        this.StatementType == DbStatementType.StoredProcedure ||
                        this.StatementType == DbStatementType.Select))
                    {
                        // Grab rows affected
                        this.WriteSqlInfoRequest(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE, this.handle);

                        readRowsAffectedResponse = true;
                    }

                    this.database.Flush();

                    // sql?, execute, rows affected?
                    int numberOfResponses = 
                        (this.StatementType == DbStatementType.StoredProcedure ? 1 : 0) + 1 + (readRowsAffectedResponse ? 1 : 0);
                    try
                    {
                        if (this.StatementType == DbStatementType.StoredProcedure)
                        {
                            numberOfResponses--;
                            SqlResponse sqlResponse = this.database.ReadSqlResponse();
                            this.ProcessStoredProcedureResponse(sqlResponse);
                        }

                        numberOfResponses--;
                        GenericResponse executeResponse = this.database.ReadGenericResponse();

                        if (readRowsAffectedResponse)
                        {
                            numberOfResponses--;
                            GenericResponse rowsAffectedResponse = this.database.ReadGenericResponse();
                            if (rowsAffectedResponse.Data != null && rowsAffectedResponse.Data.Length > 0)
                            {
                                this.RecordsAffected = this.ProcessRecordsAffectedBuffer(rowsAffectedResponse.Data);
                            }
                        }
                    }
                    finally
                    {
                        FinishFetching(ref numberOfResponses);
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

        #endregion

        #region · Private Methods ·

        private void WriteSqlInfoRequest(byte[] buffer, int bufferSize, int handle)
        {
            lock (this.database.SyncObject)
            {
                this.database.Write(IscCodes.op_info_sql);
                this.database.Write(handle);
                this.database.Write(0);
                this.database.WriteBuffer(buffer, buffer.Length);
                this.database.Write(bufferSize);
            }
        }

        private void FinishFetching(ref int numberOfResponses)
        {
            while (numberOfResponses > 0)
            {
#warning Or ReadSingle to not get exception again?
                this.database.ReadResponse();
                numberOfResponses--;
            }
        }

        #endregion

        #region Protected methods
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
                DoFreePacket(option);
#warning This isn't in lock anymore later
                (this.Database as GdsDatabase).DefferedPackets.Enqueue(ProcessFreeResponse);
            }
        }
        #endregion
    }
}
