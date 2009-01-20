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
    internal class GdsStatement : Version10.GdsStatement
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
                    this.SendPrepareToBuffer(commandText);

                    // Grab statement type
                    this.SendInfoSqlToBuffer(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE);

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
                        bool deferredExecute = ((prepareResponse.ObjectHandle & IscCodes.STMT_DEFER_EXECUTE) == IscCodes.STMT_DEFER_EXECUTE);
                        this.ProcessPrepareResponse(prepareResponse);

                        numberOfResponses--;
                        GenericResponse statementTypeResponse = this.database.ReadGenericResponse();
                        this.statementType = this.ProcessStatementTypeInfoBuffer(this.ProcessInfoSqlResponse(statementTypeResponse));
                    }
                    finally
                    {
                        SafeFinishFetching(ref numberOfResponses);
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

                    this.SendExecuteToBuffer();

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
                        this.SendInfoSqlToBuffer(RowsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE);

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
                            this.ProcessStoredProcedureExecuteResponse(sqlResponse);
                        }

                        numberOfResponses--;
                        GenericResponse executeResponse = this.database.ReadGenericResponse();

                        if (readRowsAffectedResponse)
                        {
                            numberOfResponses--;
                            GenericResponse rowsAffectedResponse = this.database.ReadGenericResponse();
                            this.RecordsAffected = this.ProcessRecordsAffectedBuffer(this.ProcessInfoSqlResponse(rowsAffectedResponse));
                        }
                    }
                    finally
                    {
                        SafeFinishFetching(ref numberOfResponses);
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

        private void SafeFinishFetching(ref int numberOfResponses)
        {
            while (numberOfResponses > 0)
            {
                numberOfResponses--;
                try
                {
                    this.database.ReadResponse();
                }
                catch (IscException) 
                { }
            }
        }

        #endregion

        #region Protected methods
        protected override void Free(int option)
        {
            if (FreeNotNeeded(option))
                return;

            lock (this.database.SyncObject)
            {
                DoFreePacket(option);
#warning Lock?
                (this.Database as GdsDatabase).DeferredPackets.Enqueue(ProcessFreeResponse);
            }
        }
        #endregion
    }
}
