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
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

using FirebirdSql.Data.INGDS;
using FirebirdSql.Data.NGDS;

namespace FirebirdSql.Data.Firebird
{
	/// <summary>
	/// fbstatement States
	/// </summary>
	internal enum CommandState 
	{
		Deallocated	,
		Allocated	,
		Preparing	,
		Prepared	,
		Executing	,
		Executed
	};

	/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="T:FbStatement"]/*'/>
	internal class FbStatement : IDisposable
	{
		#region FIELDS

		private isc_stmt_handle_impl	statement;

		private FbConnection			connection;
		private FbTransaction			transaction;
		private FbResultset				resultset;
		private FbParameterCollection	parameters;
				
		private string					commandText;
		private CommandType				commandType;

		private CommandState			state;
		private bool					disposed;
		private int						statementType;
		private Regex					searchNamedParameters;
		private MatchCollection			namedParameters;

		// Statement info size
		private const int INFO_SIZE = 128;

		// Statement info
		private byte[] stmtInfo = new byte[]
		{
			GdsCodes.isc_info_sql_records	,
			GdsCodes.isc_info_sql_stmt_type	,
			GdsCodes.isc_info_end
		};

		#endregion

		#region PROPERTIES

		public isc_stmt_handle_impl Statement
		{
			get { return statement; }
		}
		
		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="P:CommandType"]/*'/>
		public CommandType CommandType
		{
			get { return commandType; }
			set { commandType = value; }
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="P:Connection"]/*'/>
		public FbConnection Connection
		{
			get { return connection; }
			set { connection = value; }
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="P:Transaction"]/*'/>
		public FbTransaction Transaction
		{
			get { return transaction; }
			set { transaction = value; }
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="P:Parameters"]/*'/>
		public FbParameterCollection Parameters
		{
			get { return parameters; }
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="P:CommandText"]/*'/>
		public string CommandText
		{
			get { return commandText; }
			set { commandText = value; }
		}

		public FbResultset Resultset
		{
			get { return resultset; }
		}

		public CommandState State
		{
			get { return state; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public FbStatement()
		{
			statement	= null;	
			connection	= null;
			transaction	= null;
			resultset	= null;
			parameters	= new FbParameterCollection();					
			commandText = String.Empty;
			commandType	= CommandType.Text;
			state		= CommandState.Deallocated;
			disposed	= false;
			statementType = 0;

			this.searchNamedParameters = new Regex("(@([a-zA-Z-$][a-zA-Z0-9_$]*))");			
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="M:#ctorctor(System.String,FirebirdSql.Data.Firebird.FbConnection,FirebirdSql.Data.Firebird.FbTransaction,FirebirdSql.Data.Firebird.FbParameters, System.Data.CommandType)"]/*'/>
		public FbStatement(FbConnection connection, FbTransaction transaction, 
							FbParameterCollection parameters, string commandText, 
							CommandType commandType) : this()
		{
			this.connection  = connection;
			this.transaction = transaction;
			this.parameters  = parameters;
			this.commandText = commandText;
			this.commandType = commandType;			
		}

		#endregion

		#region DESTRUCTORS

		~FbStatement()
		{
			Dispose(false);
		}

		#endregion

		#region IDISPOSABLE_METHODS

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (!disposed)
			{
				try
				{
					if (disposing)
					{
						// release any managed resources
						if (statement != null)
						{
							DropStatement();						
						}
					}

					// release any unmanaged resources
				}
				finally
				{
				}
								
				disposed = true;
			}
		}

		#endregion

		#region METHODS

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="M:Close"]/*'/>
		public void Close()
		{
			if (statement != null)
			{
				try
				{
					if (!Transaction.IsUpdated)
					{
						/* Close the statement if its executed 
						 * and it have columns
						 */
						if (statement.out_sqlda.sqld != 0 && 
							state == CommandState.Executed)
						{
							connection.IscConnection.GDS.isc_dsql_free_statement(statement, GdsCodes.DSQL_close);

							state = CommandState.Prepared;
						}
					}
				}
				catch(GDSException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="M:DropStatement"]/*'/>
		public void DropStatement()
		{
			if (statement != null)
			{
				try
				{
					if (this.state != CommandState.Deallocated)
					{
						connection.IscConnection.GDS.isc_dsql_free_statement(statement, GdsCodes.DSQL_drop);
					
						statement  = null;
						parameters = null;
						resultset  = null;
					}

					state = CommandState.Deallocated;				
				}
				catch(GDSException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="M:Prepare"]/*'/>
		public void Prepare()
		{
			try
			{
				if (state == CommandState.Deallocated)
				{
					allocateStatement();
				}

				state = CommandState.Preparing;

				/* Get named parameters in CommandText	*/
				namedParameters = searchNamedParameters.Matches(CommandText);

				/* Prepare Statement */
				XSQLDA output = connection.IscConnection.GDS.isc_dsql_prepare(
					transaction.IscTransaction		, 
					statement						, 
					parseCommandText(CommandText)	, 
					connection.Encoding, 
					connection.IscConnection.Dialect);
						
				// sqln: number of fields allocated
				// sqld: actual number of fields
				if (output.sqld != output.sqln)
				{
					throw new FbException("Invalid number of allocated columns in resultset.");
				}
			
				connection.IscConnection.GDS.isc_dsql_describe(statement, 
					GdsCodes.SQLDA_VERSION1);
		
				XSQLDA input = connection.IscConnection.GDS.isc_dsql_describe_bind(
					statement, 
					GdsCodes.SQLDA_VERSION1);

				// sqln: number of fields allocated
				// sqld: actual number of fields
				if (input.sqld != input.sqln)
				{
					throw new FbException("Invalid number of allocated columns in resultset.");
				}		

				state = CommandState.Prepared;				

				/* Create new resultset */
				resultset = new FbResultset(connection, transaction, statement);
			}
			catch(GDSException ex)
			{
				throw ex;
			}
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="M:Execute"]/*'/>
		public void Execute()
		{
			try
			{
				if (state == CommandState.Deallocated)
				{
					Prepare();
				}

				state = CommandState.Executing;

				switch(CommandType)
				{
					case System.Data.CommandType.StoredProcedure:
						connection.IscConnection.GDS.isc_dsql_execute2(
							transaction.IscTransaction	, 
							statement					, 
							GdsCodes.SQLDA_VERSION1		,									
							getInSqlda()				, 
							statement.OutSqlda);
						break;

					default:
						connection.IscConnection.GDS.isc_dsql_execute(
							transaction.IscTransaction	, 
							statement					, 
							GdsCodes.SQLDA_VERSION1		, 								
							getInSqlda());
						break;
				}			
				
				state = CommandState.Executed;
			}
			catch(GDSException ex)
			{
				throw ex;
			}
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="M:IsSelect"]/*'/>
		public bool IsSelect()
		{
			return statement.OutSqlda.sqld > 0  ? true : false;
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="M:SetOutputParameterValues"]/*'/>
		public void SetOutputParameterValues()
		{
			if (this.CommandType == CommandType.StoredProcedure)
			{
				IEnumerator paramEnumerator = Parameters.GetEnumerator();
				int i = 0;

				Resultset.FillOutputParameters();

				while (paramEnumerator.MoveNext())
				{					
					if (((FbParameter)paramEnumerator.Current).Direction == ParameterDirection.Output ||
						((FbParameter)paramEnumerator.Current).Direction == ParameterDirection.ReturnValue)
					{
						((FbParameter)paramEnumerator.Current).Value = this.Resultset.GetValue(i);
						i++;
					}					
				}
			}
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="M:GetRecordsAffected"]/*'/>
		public int GetRecordsAffected()
		{
			int recordsAffected;
			SqlInfo info;

			try
			{				
				info = new SqlInfo(connection.IscConnection.GDS.isc_dsql_sql_info(
					statement			, 
					stmtInfo.Length		, 
					stmtInfo			, 
					INFO_SIZE)			, 
					connection.IscConnection.GDS);
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			statementType	= info.StatementType;

			if (statementType == GdsCodes.isc_info_sql_stmt_select)
			{
				recordsAffected = -1;
			}
			else
			{
				recordsAffected = (info.InsertCount + info.UpdateCount + info.DeleteCount);
			}

			return recordsAffected;
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="M:AllocateStatement"]/*'/>
		private void allocateStatement()
		{
			try
			{
				if (statement == null)
				{
					statement = (isc_stmt_handle_impl)connection.IscConnection.GDS.get_new_isc_stmt_handle();
				}
				connection.IscConnection.GDS.isc_dsql_allocate_statement(connection.IscConnection.db, statement);
			}
			catch(GDSException ex)
			{
				throw ex;
			}
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="M:GetInSqlda"]/*'/>		
		private XSQLDA getInSqlda()
		{
			XSQLDA		in_sqlda		= statement.InSqlda;
			Encoding	encoding		= Connection.Encoding;
			string		parameterName	= String.Empty;
			
			if (in_sqlda == null)
			{
				return in_sqlda;
			}
			
			for (int i = 0; i < in_sqlda.sqln; i++)
			{
				parameterName = parameters[i].ParameterName;
				if (namedParameters.Count != 0)
				{
					try
					{
						parameterName = namedParameters[i].Value.Trim();
					}
					catch
					{
						parameterName = parameters[i].ParameterName;
					}
				}

				if (parameters[parameterName].Value == System.DBNull.Value)
				{
					if ((in_sqlda.sqlvar[i].sqltype & 1 ) == 0)
					{
						throw new InvalidOperationException("Input parameter value cannot be null.");
					}
					else
					{
						in_sqlda.sqlvar[i].sqlind	= -1;
						in_sqlda.sqlvar[i].sqldata	= null;
					}
				}
				else
				{
					long multiplier = 1;
					if (in_sqlda.sqlvar[i].sqlscale < 0)
					{
						int exp = in_sqlda.sqlvar[i].sqlscale*(-1);
						multiplier = (long)System.Math.Pow(10, exp);
					}

					switch (in_sqlda.sqlvar[i].sqltype & ~1)
					{
						case GdsCodes.SQL_TEXT:
							in_sqlda.sqlvar[i].sqldata = encoding.GetBytes((string)parameters[parameterName].Value);
							in_sqlda.sqlvar[i].sqllen  = encoding.GetMaxByteCount(in_sqlda.sqlvar[i].sqllen);
							break;
						
						case GdsCodes.SQL_VARYING:						
							in_sqlda.sqlvar[i].sqldata = encoding.GetBytes((string)parameters[parameterName].Value);
							in_sqlda.sqlvar[i].sqllen  = encoding.GetByteCount((string)parameters[parameterName].Value);
							break;

						case GdsCodes.SQL_SHORT:
							if (in_sqlda.sqlvar[i].sqlscale < 0)
							{
								decimal paramValue = Convert.ToDecimal(parameters[parameterName].Value.ToString());
								in_sqlda.sqlvar[i].sqldata = 
									Convert.ToInt16(paramValue*multiplier);
							}
							else
							{										
								in_sqlda.sqlvar[i].sqldata = Convert.ToInt16(parameters[parameterName].Value.ToString());
							}
							break;

						case GdsCodes.SQL_LONG:
							if (in_sqlda.sqlvar[i].sqlscale < 0)
							{
								decimal paramValue = Convert.ToDecimal(parameters[parameterName].Value.ToString());
								in_sqlda.sqlvar[i].sqldata = 
									Convert.ToInt32(paramValue*multiplier);
							}
							else
							{
								in_sqlda.sqlvar[i].sqldata = Convert.ToInt32(parameters[parameterName].Value.ToString());
							}
							break;

						case GdsCodes.SQL_FLOAT:
							in_sqlda.sqlvar[i].sqldata = float.Parse(parameters[parameterName].Value.ToString());
							break;

						case GdsCodes.SQL_DOUBLE:
						case GdsCodes.SQL_D_FLOAT:
							in_sqlda.sqlvar[i].sqldata = 
								Convert.ToDouble(parameters[parameterName].Value.ToString());
							break;

						case GdsCodes.SQL_BLOB:
							if (in_sqlda.sqlvar[i].sqlsubtype == 1)
							{
								FbClob clob = new FbClob(Connection, Transaction);
								in_sqlda.sqlvar[i].sqldata = clob.Write(
									Convert.ToString(parameters[parameterName].Value));
							}
							else
							{
								FbBlob blob = new FbBlob(Connection, Transaction);
								in_sqlda.sqlvar[i].sqldata = blob.Write((byte[])parameters[parameterName].Value);
							}

							break;

						case GdsCodes.SQL_ARRAY:
							// TODO: Write array data
							break;

						case GdsCodes.SQL_QUAD:
						case GdsCodes.SQL_INT64:
							if (in_sqlda.sqlvar[i].sqlscale < 0)
							{
								decimal paramValue = Convert.ToDecimal(parameters[parameterName].Value.ToString());
								in_sqlda.sqlvar[i].sqldata = 
									Convert.ToInt64(paramValue*multiplier);
							}
							else
							{										
								in_sqlda.sqlvar[i].sqldata = Convert.ToInt64(parameters[parameterName].Value.ToString());
							}
							break;

						case GdsCodes.SQL_TIMESTAMP:
						case GdsCodes.SQL_TYPE_TIME:			
						case GdsCodes.SQL_TYPE_DATE:				
							in_sqlda.sqlvar[i].sqldata = DateTime.Parse(parameters[parameterName].Value.ToString());
							break;

						default:
							throw new NotSupportedException("Unknown data type");
					}							
				}
			}

			return in_sqlda;
		}

		/// <include file='xmldoc/fbstatement.xml' path='doc/member[@name="M:parseCommandText(System.String)"]/*'/>		
		private string parseCommandText(string commandText)
		{
			string	sqlText	= commandText;

			if(Parameters.Count != 0)
			{
				if (namedParameters.Count != 0)
				{					
					sqlText = searchNamedParameters.Replace(sqlText, "?");
				}
			}

			return sqlText;
		}

		#endregion
	}
}
