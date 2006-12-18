/*
 *  Firebird BDP - Borland Data provider Firebird
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Text;

using FirebirdSql.Data.Common;
using Borland.Data.Common;

namespace FirebirdSql.Data.Bdp
{
	public class FbCommand : ISQLCommand
	{
		#region Fields

		private FbConnection		connection;
		private StatementBase		statement;
		private IscException		lastError;
		private int					transactionId;

		#endregion

		#region Internal Properties

		internal int RowsAffected
		{
			get 
			{
				if (this.statement != null)
				{
					return this.statement.RecordsAffected; 
				}
				return -1;
			}
		}

		#endregion

		#region Constructors

		public FbCommand(FbConnection connection)
		{
			this.connection	= connection;
		}

		#endregion

		#region ISQLCommand Methods

#if (DIAMONDBACK)

		public int SetOptions(string[] options)
		{
			return 0;
		}

#endif

		public int Prepare(string sql, short paramCount)
		{
			try
			{
				if (this.statement != null)
				{
					this.Release();
				}

				this.transactionId = this.connection.GetTransactionId();
				if (this.transactionId < 0)
				{
					this.connection.BeginTransaction(this.transactionId, (int)(IsolationLevel.ReadCommitted));
				}

				ITransaction transaction = (ITransaction)this.connection.Transactions[this.transactionId];

				this.statement = this.connection.Database.CreateStatement(transaction);
				
				this.statement.Prepare(sql);
				this.statement.Describe();
				this.statement.DescribeParameters();
			}
			catch (IscException e)
			{
				this.lastError = e;
				this.RollbackImplicitTransaction();
			}

			return this.GetErrorCode();
		}

		public int PrepareProc(string spName, short paramCount)
		{
			try
			{
				if (this.statement != null)
				{
					this.Release();
				}

				this.transactionId = this.connection.GetTransactionId();
				if (this.transactionId < 0)
				{
					this.connection.BeginTransaction(this.transactionId, (int)(IsolationLevel.ReadCommitted));
				}

				ITransaction transaction = (ITransaction)this.connection.Transactions[this.transactionId];

				this.statement = this.connection.Database.CreateStatement(transaction);

				this.statement.Prepare(spName);
				this.statement.Describe();
				this.statement.DescribeParameters();
			}
			catch (IscException e)
			{
				this.lastError = e;
				this.RollbackImplicitTransaction();
			}

			return this.GetErrorCode();
		}

		public int Execute(out ISQLCursor cursor, ref short resultCols)
		{
			cursor		= null;
			resultCols	= 0;

			try
			{
				if (this.statement == null)
				{
					throw new InvalidOperationException("Command needs to be prepared before execution.");
				}

				if (this.transactionId == 0)
				{
					this.transactionId = this.connection.GetTransactionId();
					if (this.transactionId < 0)
					{
						this.connection.BeginTransaction(this.transactionId, (int)(IsolationLevel.ReadCommitted));
					}
				}

				ITransaction transaction = (ITransaction)this.connection.Transactions[this.transactionId];
				
				if (this.statement.IsPrepared)
				{
					this.Close();
				}

				this.statement.Execute();

				cursor = new FbCursor(this);

				if (this.statement.StatementType != DbStatementType.Select &&
					this.statement.StatementType != DbStatementType.SelectForUpdate &&
					this.statement.StatementType != DbStatementType.StoredProcedure)
				{
					this.CommitImplicitTransaction();
				}
				else
				{
					resultCols = this.statement.Fields.Count;
				}
			}
			catch (IscException e)
			{
				this.lastError = e;
				this.RollbackImplicitTransaction();
			}
			
			return this.GetErrorCode();
		}

		public int Close()
		{
			try
			{
				if (this.statement != null)
				{
					this.statement.Close();
				}
			}
			catch (IscException e)
			{
				this.lastError = e;
			}

			return this.GetErrorCode();
		}

		public int Release()
		{
			try
			{
				this.RollbackImplicitTransaction();
				if (this.statement != null)
				{
					this.statement.Release();
				}
			}
			catch (IscException e)
			{
				this.lastError = e;
			}
			finally
			{
				this.statement = null;
			}

			return this.GetErrorCode();
		}

		public int GetStoredProcedureSQL(StringBuilder sql, ArrayList paramList)
		{
			try
			{
                string commandText = sql.ToString().Trim();

                if (!commandText.StartsWith("SELECT ") &&
                    !commandText.StartsWith("EXECUTE PROCEDURE "))
                {
                    sql.Insert(0, "SELECT * FROM ");

                    if (paramList.Count > 0)
                    {
                        sql.Append("(");
                        foreach (BdpSPParam parameter in paramList)
                        {
                            if (parameter.Direction == ParameterDirection.Input ||
                                parameter.Direction == ParameterDirection.InputOutput)
                            {
                                sql.Append("?, ");
                            }
                        }
                        sql.Remove(sql.Length - 2, 2);
                        sql.Append(")");
                    }
                }
            }
			catch (IscException e)
			{
				this.lastError = e;
			}

			return this.GetErrorCode();
		}

		public int GetRowsAffected(ref int rowsAffected)
		{
			try
			{
				if (this.statement != null)
				{
					rowsAffected = this.statement.RecordsAffected;
				}
			}
			catch (IscException e)
			{
				this.lastError = e;
			}

			return this.GetErrorCode();
		}

		public int GetParameter(
			short		index, 
			short		childPos, 
			ref object	value, 
			ref bool	isNull)
		{
			try
			{
                int realIndex = index - this.statement.Parameters.Count;
                DbValue[] values = (DbValue[])this.statement.Fetch();

                if (values != null && values.Length > 0 && realIndex < values.Length)
                {
                    value   = values[realIndex].Value;
                    isNull  = values[realIndex].IsDBNull();
                }
            }
			catch (IscException e)
			{
				this.lastError = e;
			}

			return this.GetErrorCode();
		}

		public int SetParameter(
			short				index, 
			short				childPos, 
			ParameterDirection	paramDir, 
			BdpType				dataType, 
			BdpType				subType, 
			int					maxPrecision, 
			int					maxScale, 
			int					length, 
			object				value, 
			bool				isNullable)
		{
			try
			{
                if (paramDir == ParameterDirection.Input ||
                    paramDir == ParameterDirection.InputOutput)
                {
                    this.SetInputParameter(
			                index, 
			                childPos, 
			                paramDir, 
			                dataType, 
                            subType, 
                            maxPrecision, 
			                maxScale, 
			                length, 
			                value, 
			                isNullable);
                }
            }
			catch (IscException e)
			{
				this.lastError = e;
			}

			return this.GetErrorCode();
		}

		public int GetErrorMessage(ref StringBuilder errorMessage)
		{
			if (this.lastError != null)
			{
				errorMessage.Append(this.lastError.Message);

				this.lastError = null;
			}

			return 0;
		}
        
		#endregion

		#region Internal Methods

		internal int GetNextResult(out ISQLCursor cursor, ref short resultCols)
		{
			cursor = new FbCursor(this);
			resultCols = 0;

			return -1;
		}

		internal DbValue[] Fetch()
		{
			return this.statement.Fetch();			
		}

		internal Descriptor GetFieldsDescriptor()
		{
			if (this.statement != null)
			{
				return this.statement.Fields;
			}

			return null;
		}

		internal void CommitImplicitTransaction()
		{
			if (this.transactionId < 0)
			{
				try
				{
					int error = this.connection.Commit(this.transactionId);
					if (error != 0)
					{
						StringBuilder msg = new StringBuilder();
						this.connection.GetErrorMessage(ref msg);

						throw new IscException(msg.ToString());
					}
				}
				catch
				{
					this.connection.Rollback(this.transactionId);
					throw;
				}
				finally
				{
					this.transactionId = 0;
				}
			}
		}

		internal void RollbackImplicitTransaction()
		{
			if (this.transactionId < 0)
			{
				try
				{
					this.connection.Rollback(this.transactionId);
				}
				catch
				{
				}
				finally
				{
					this.transactionId = 0;
				}
			}
		}

		#endregion

		#region Private Methods

        private void SetInputParameter(
            short				index, 
			short				childPos, 
			ParameterDirection	paramDir, 
			BdpType				dataType, 
			BdpType				subType, 
			int					maxPrecision, 
			int					maxScale, 
			int					length, 
			object				value, 
			bool				isNullable)
        {
            // Set null flag
            if (value == null || value == DBNull.Value)
            {
                this.statement.Parameters[index].NullFlag = -1;
                this.statement.Parameters[index].Value = DBNull.Value;
            }
            else
            {
                this.statement.Parameters[index].NullFlag = 0;

                // Set parameter value
                switch (this.statement.Parameters[index].DbDataType)
                {
                    case DbDataType.Array:
                        if (this.statement.Parameters[index].ArrayHandle == null)
                        {
                            this.statement.Parameters[index].ArrayHandle =
                                this.statement.CreateArray(
                                this.statement.Parameters[index].Relation,
                                this.statement.Parameters[index].Name);
                        }
                        else
                        {
                            this.statement.Parameters[index].ArrayHandle.DB = this.statement.DB;
                            this.statement.Parameters[index].ArrayHandle.Transaction = this.statement.Transaction;
                        }

                        this.statement.Parameters[index].ArrayHandle.Handle = 0;
                        this.statement.Parameters[index].ArrayHandle.Write((System.Array)value);
                        this.statement.Parameters[index].Value = this.statement.Parameters[index].ArrayHandle.Handle;
                        break;

                    case DbDataType.Binary:
                        BlobBase blob = this.statement.CreateBlob();
                        blob.Write((byte[])value);
                        this.statement.Parameters[index].Value = blob.Id;
                        break;

                    case DbDataType.Text:
                        BlobBase clob = this.statement.CreateBlob();
						if (value is char[])
						{
							clob.Write(new String((char[])value));
						}
						else
						{
							clob.Write((string)value);
						}
						this.statement.Parameters[index].Value = clob.Id;
                        break;

                    default:
                        this.statement.Parameters[index].Value = value;
                        break;
                }
            }
        }

        private int GetErrorCode()
		{
			return (this.lastError != null ? this.lastError.ErrorCode : 0);
		}

		#endregion
	}
}
