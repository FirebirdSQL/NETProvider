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
 *  Contributors
 *      Jiri Cincura (jiri@cincura.net)
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
		{
		}

		public GdsStatement(IDatabase db, ITransaction transaction) : 
            base(db, transaction)
		{
		}

        #endregion

        #region · Overriden Methods ·

        public override void Prepare(string commandText)
        {
            // Clear data
            this.ClearAll();

            lock (this.Database.SyncObject)
            {
                GdsDatabase database = (GdsDatabase)this.Database;

                if (this.State == StatementState.Deallocated)
                {
                    // Allocate statement
                    this.Allocate();
                }

                try
                {
                    // Parameter descriptor
                    byte[] descriptor = null;

                    if (this.Parameters != null)
                    {
                        using (XdrStream xdr = new XdrStream(database.Charset))
                        {
                            xdr.Write(this.Parameters);

                            descriptor = xdr.ToArray();

                            xdr.Close();
                        }
                    }

                    // Prepare the statement
                    database.Write(IscCodes.op_prepare_statement);
                    database.Write(this.Transaction.Handle);
                    database.Write(this.Handle);
                    database.Write((int)this.Database.Dialect);
                    database.Write(commandText);
                    database.WriteBuffer(DescribeInfoItems, DescribeInfoItems.Length);
                    database.Write(IscCodes.MAX_BUFFER_SIZE);

                    // Grab statement type
                    this.WriteSqlInfoRequest(StatementTypeInfoItems, IscCodes.STATEMENT_TYPE_BUFFER_SIZE);

                    // Flush data
                    database.Flush();

                    // Read Responses
                    List<IResponse> responses = database.ReadResponses(2);

                    GenericResponse prepareResponse         = (GenericResponse)responses[0];
                    GenericResponse statementTypeResponse   = (GenericResponse)responses[1];

                    if (prepareResponse.Exception != null && !prepareResponse.Exception.IsWarning)
                    {
                        throw prepareResponse.Exception;
                    }
                    else
                    {
                        this.ProcessPrepareResponse(prepareResponse);
                    }

                    // Statement type information processing
                    if (statementTypeResponse.Exception != null && !statementTypeResponse.Exception.IsWarning)
                    {
                        throw statementTypeResponse.Exception;
                    }
                    else
                    {
                        this.StatementType = this.ParseStatementTypeInfo(statementTypeResponse.Data);
                    }

                    this.State = StatementState.Prepared;
                }
                catch (IOException)
                {
                    this.State = StatementState.Error;
                    throw new IscException(IscCodes.isc_net_read_err);
                }
            }
        }

        public override void Execute()
        {
            if (this.State == StatementState.Deallocated)
            {
                throw new InvalidOperationException("Statment is not correctly created.");
            }

            // Clear data
            this.Clear();

            lock (this.Database.SyncObject)
            {
                GdsDatabase database                = (GdsDatabase)this.Database;
                bool        rowsAffectedResponse    = false;
                int         responseCount           = 1;

                try
                {
                    this.RecordsAffected = -1;

                    // Build Parameter description
                    byte[] descriptor = null;

                    if (this.Parameters != null)
                    {
                        using (XdrStream xdr = new XdrStream(database.Charset))
                        {
                            xdr.Write(this.Parameters);

                            descriptor = xdr.ToArray();

                            xdr.Close();
                        }
                    }

                    // Write the message
                    if (this.StatementType == DbStatementType.StoredProcedure)
                    {
                        database.Write(IscCodes.op_execute2);
                    }
                    else
                    {
                        database.Write(IscCodes.op_execute);
                    }

                    database.Write(this.Handle);
                    database.Write(this.Transaction.Handle);

                    if (this.Parameters != null)
                    {
                        database.WriteBuffer(this.Parameters.ToBlrArray());
                        database.Write(0);    // Message number
                        database.Write(1);    // Number of messages
                        database.Write(descriptor, 0, descriptor.Length);
                    }
                    else
                    {
                        database.WriteBuffer(null);
                        database.Write(0);
                        database.Write(0);
                    }

                    if (this.StatementType == DbStatementType.StoredProcedure)
                    {
                        database.WriteBuffer((this.Fields == null) ? null : this.Fields.ToBlrArray());
                        database.Write(0);    // Output message number
                    }

                    // Obtain records affected by query execution
                    if (this.ReturnRecordsAffected &&
                        (this.StatementType == DbStatementType.Insert ||
                        this.StatementType == DbStatementType.Delete ||
                        this.StatementType == DbStatementType.Update ||
                        this.StatementType == DbStatementType.StoredProcedure ||
                        this.StatementType == DbStatementType.Select))
                    {
                        // Grab statement type
                        this.WriteSqlInfoRequest(RecordsAffectedInfoItems, IscCodes.ROWS_AFFECTED_BUFFER_SIZE);

                        responseCount++;
                        rowsAffectedResponse = true;
                    }

                    database.Flush();

                    SqlResponse     sqlResponse     = null;
                    GenericResponse executeResponse = null;
                    GenericResponse raResponse      = null;

                    if (this.StatementType == DbStatementType.StoredProcedure)
                    {
                        sqlResponse = database.ReadSqlResponse();

                        this.ProcessStoredProcedureResponse(sqlResponse);
                    }

                    executeResponse = database.ReadGenericResponse();
                    
                    // Process Rows Affected Response
                    if (rowsAffectedResponse)
                    {
                        raResponse = database.ReadGenericResponse();

                        if (raResponse.Data != null && raResponse.Data.Length > 0)
                        {
                            this.RecordsAffected = this.ProcessRecordsAffectedBuffer(raResponse.Data);
                        }
                    }

                    // Process responses
                    database.ProcessResponse(sqlResponse);
                    database.ProcessResponse(executeResponse);
                    database.ProcessResponse(raResponse);

                    this.State = StatementState.Executed;
                }
                catch (IOException)
                {
                    this.State = StatementState.Error;
                    throw new IscException(IscCodes.isc_net_read_err);
                }
            }
        }

        #endregion

        #region · Private Methods ·

        private void WriteSqlInfoRequest(byte[] buffer, int bufferSize)
        {
            lock (this.Database.SyncObject)
            {
                GdsDatabase database = (GdsDatabase)this.Database;
             
                database.Write(IscCodes.op_info_sql);
                database.Write(this.Handle);
                database.Write(0);
                database.WriteBuffer(buffer, buffer.Length);
                database.Write(bufferSize);
            }
        }

        #endregion
    }
}
