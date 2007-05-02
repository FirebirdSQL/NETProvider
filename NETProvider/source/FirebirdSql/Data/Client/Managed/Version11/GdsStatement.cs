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
 */

using System;
using System.Collections;
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

                    // Prepare statement info processing
                    this.ReadFields();

                    // Statement type information processing
                    GenericResponse stmtmTypeResponse = (GenericResponse)this.DatabaseStream.ReadResponse();

                    this.StatementType = this.ParseStatementTypeInfo(stmtmTypeResponse.Data);

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
                GdsDatabase database = (GdsDatabase)this.Database;

                try
                {
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
                        database.Write(this.Parameters);
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
                        this.RecordsAffected = 0;
                    }
                    else
                    {
                        this.RecordsAffected = -1;
                    }

                    database.Flush();

                    if (this.DatabaseStream.NextOperation() == IscCodes.op_sql_response)
                    {
                        // This would be an Execute procedure
                        this.OutputParams.Enqueue(this.ReadStoredProcedureOutput());
                    }

                    this.DatabaseStream.ReadResponse();

                    if (this.RecordsAffected == 0)
                    {
                        GenericResponse response = (GenericResponse)this.DatabaseStream.ReadResponse();
                        this.ProcessRecordsAffectedBuffer(response.Data);
                    }

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
