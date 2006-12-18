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
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using FirebirdSql.Data.Firebird.Gds;

namespace FirebirdSql.Data.Firebird
{		
	/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/overview/*'/>
	[ToolboxItem(true),
	ToolboxBitmap(typeof(FbCommand), "Resources.ToolboxBitmaps.FbCommand.bmp")]
	public sealed class FbCommand : Component, IDbCommand, ICloneable
	{				
		#region FIELDS
		
		private FbConnection			connection;
		private FbTransaction			transaction;
		private FbParameterCollection	parameters;
		private UpdateRowSource			updatedRowSource;
		private CommandBehavior			commandBehavior;
		private GdsStatement			statement;
		private bool					disposed;
		private int						actualCommand;
		private string[]				commands;
		private string					commandText;
		private CommandType				commandType;
		private bool					designTimeVisible;
		private int						commandTimeout;
		private StringCollection		namedParameters;
		private bool					returnsSet;
		private bool					implicitTransaction;

		#endregion

		#region PROPERTIES

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/property[@name="CommandText"]/*'/>
		[Category("Data"),
		DefaultValue(""),
		RefreshProperties(RefreshProperties.All)]
		#if (!_MONO)
		[Editor(typeof(Design.CommandTextUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
		#endif
		public string CommandText
		{
			get { return this.commandText; }
			set 
			{ 				
				if (this.statement != null && 
					this.commandText != null &&
					this.commandText != value && 
					this.commandText.Length != 0)
				{
					this.statement.Drop();
					this.statement = null;
				}
				
				this.commandText	= value;
				this.actualCommand	= 0;
				this.commands		= null;
			}
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/property[@name="CommandType"]/*'/>
		[Category("Data"),
		DefaultValue(CommandType.Text),
		RefreshProperties(RefreshProperties.All)]		
		public CommandType CommandType
		{
			get { return this.commandType; }
			set { this.commandType = value; }
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/property[@name="CommandTimeout"]/*'/>
		[ReadOnly(true),		
		DefaultValue(30)]		
		public int CommandTimeout
		{
			get { return this.commandTimeout; }
			set
			{
				if (value < 0) 
				{
					throw new ArgumentException("The property value assigned is less than 0.");
				}
				else
				{					
					throw new NotSupportedException();
				}
			}
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/property[@name="CommandPlan"]/*'/>
		[Browsable(false)]		
		public string CommandPlan
		{
			get 
			{
				if (this.statement != null)
				{
					return this.statement.GetExecutionPlan();
				}
				else
				{
					return String.Empty;
				}
			}
		}

		IDbConnection IDbCommand.Connection
		{
			get { return this.Connection; }
			set { this.Connection = (FbConnection)value; }
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/property[@name="Connection"]/*'/>
		[Category("Behavior"), DefaultValue(null)]
		public FbConnection Connection
		{
			get { return this.connection; }
			set
			{
				if (this.connection != null && 
					this.connection.DataReader != null)
				{
					throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
				}

				if (this.transaction != null &&
					!this.transaction.IsUpdated)
				{
					throw new InvalidOperationException("The Connection property was changed while a transaction was in progress.");
				}

				if (this.connection != value)
				{										
					if (this.Transaction != null)
					{
						this.Transaction = null;
					}

					if (this.statement != null)
					{
						this.statement.Drop();
						this.statement = null;
					}
				}

				this.connection = value;
			}
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/property[@name="DesignTimeVisible"]/*'/>
		[Browsable(false),
		DesignOnly(true),
		DefaultValue(true)]
		public bool DesignTimeVisible
		{
			get { return this.designTimeVisible; }
			set { this.designTimeVisible = value; }
		}

		IDataParameterCollection IDbCommand.Parameters
		{
			get { return this.Parameters; }
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/property[@name="Parameters"]/*'/>
		[Category("Data"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FbParameterCollection Parameters
		{
			get { return this.parameters; }
		}

		IDbTransaction IDbCommand.Transaction
		{
			get { return this.Transaction; }
			set { this.Transaction = (FbTransaction)value; }
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/property[@name="Transaction"]/*'/>
		[Browsable(false),
		DataSysDescription("Tansaction context used by the command."),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]		
		public FbTransaction Transaction
		{
			get { return this.transaction; }
			set
			{
				if (this.connection != null && 
					this.connection.DataReader != null)
				{
					throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
				}

				if (this.implicitTransaction && 
					this.transaction != null)
				{
					this.transaction.Commit();
					this.implicitTransaction = false;
				}

				this.transaction = value;

				if (this.statement != null && 
					this.transaction != null) 
				{
					this.statement.Transaction = this.transaction.Transaction;
				}
			}
		}
		
		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/property[@name="UpdatedRowSource"]/*'/>
		[Category("Behavior"), DefaultValue(UpdateRowSource.Both)]
		public UpdateRowSource UpdatedRowSource
		{
			get { return this.updatedRowSource; }
			set { this.updatedRowSource = value; }
		}

		#endregion

		#region INTERNAL_PROPERTIES
		
		internal CommandBehavior CommandBehavior
		{
			get { return this.commandBehavior; }
		}

		internal GdsStatement Statement
		{
			get { return this.statement; }
			set { this.statement = value; }
		}

		internal int RecordsAffected
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

		internal bool IsDisposed
		{
			get { return this.disposed; }
		}

		internal FbTransaction ActiveTransaction
		{
			get { return this.transaction; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/constructor[@name="ctor"]/*'/>
		public FbCommand() : base()
		{
			this.parameters			= new FbParameterCollection();
			this.updatedRowSource	= UpdateRowSource.Both;
			this.commandBehavior	= CommandBehavior.Default;
			this.actualCommand		= -1;
			this.commandText		= String.Empty;
			this.commandType		= CommandType.Text;		
			this.designTimeVisible	= true;
			this.commandTimeout		= 30;
			this.namedParameters	= new StringCollection();

			GC.SuppressFinalize(this);
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/constructor[@name="ctor(System.String)"]/*'/>
		public FbCommand(string cmdText) : this()
		{
			this.CommandText = cmdText;
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/constructor[@name="ctor(System.String,FbConnection)"]/*'/>
		public FbCommand(string cmdText, FbConnection connection) : this()
		{
			this.CommandText	= cmdText;
			this.connection		= connection;
		}
		
		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/constructor[@name="ctor(System.String,FbConnection,Transaction)"]/*'/>
		public FbCommand(string cmdText, FbConnection connection, FbTransaction transaction) : this()
		{
			this.CommandText	= cmdText;
			this.connection		= connection;
			this.transaction	= transaction;
		}				 

		#endregion

		#region DESTRUCTORS

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/method[@name="Dispose(System.Boolean)"]/*'/>
		protected override void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				try
				{
					if (disposing)
					{
						// Clear active commands
						if (this.connection != null &&
							this.connection.ActiveCommands != null)
						{
							this.connection.ActiveCommands.Remove(this);
						}

						// Commit transaction if needed
						if (this.implicitTransaction &&
							this.transaction != null &&
							!this.transaction.IsUpdated)
						{
							this.RollbackImplicitTransaction();
						}

						// release any managed resources
						if (this.statement != null)
						{
							this.statement.Drop();
							this.statement = null;
						}

						this.commandText			= String.Empty;
						this.actualCommand			= -1;
						this.namedParameters		= null;
						this.commands				= null;
						this.connection				= null;
						this.transaction			= null;
						this.parameters				= null;
						this.implicitTransaction	= false;
					}
					
					// release any unmanaged resources
					this.disposed = true;
				}
				finally 
				{
					base.Dispose(disposing);
				}
			}
		}

		#endregion

		#region ICLONEABLE_METHODS

		object ICloneable.Clone()
		{
			FbCommand command = new FbCommand();
			
			command.CommandText			= this.commandText;
			command.Connection			= this.connection;
			command.Transaction			= this.transaction;
			command.CommandType			= this.CommandType;
			command.UpdatedRowSource	= this.UpdatedRowSource;
			// command.CommandTimeout		= this.CommandTimeout;

			for (int i=0; i < this.Parameters.Count; i++)
			{
				command.Parameters.Add(((ICloneable)this.Parameters[i]).Clone());
			}

			return command;
		}

		#endregion

		#region METHODS

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/method[@name="Cancel"]/*'/>
		public void Cancel()
		{			
			throw new NotSupportedException();
		}
		
		IDbDataParameter IDbCommand.CreateParameter()
		{
			return CreateParameter();
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/method[@name="CreateParameter"]/*'/>
		public FbParameter CreateParameter()
		{
			return new FbParameter();
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/method[@name="ExecuteNonQuery"]/*'/>
		public int ExecuteNonQuery()
		{
			this.checkCommand();

			try
			{
				this.actualCommand	= 0;
				this.returnsSet		= false;
				
				this.splitBatchCommands(false);
				this.InternalPrepare();
				this.InternalExecute();
				
				// Retrive information about records affected by command execution
				this.statement.UpdateRecordsAffected();

				// Set output parameter values
				this.InternalSetOutputParameters();

				// Commit implicit transaction
				this.CommitImplicitTransaction();
			}
			catch (GdsException ex)
			{
				this.RollbackImplicitTransaction();

				throw new FbException(ex.Message, ex);
			}			

			return this.statement.RecordsAffected;
		}
				
		IDataReader IDbCommand.ExecuteReader()
		{	
			return this.ExecuteReader();			
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/method[@name="ExecuteReader"]/*'/>
		public FbDataReader ExecuteReader()
		{	
			return this.ExecuteReader(CommandBehavior.Default);			
		}
		
		IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
		{
			return this.ExecuteReader(behavior);
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/method[@name="ExecuteReader(System.Data.CommandBehavior)"]/*'/>
		public FbDataReader ExecuteReader(CommandBehavior behavior)
		{
			this.checkCommand();

			try
			{
				this.commandBehavior = behavior;
				this.returnsSet		= true;

				this.splitBatchCommands(true);
				this.InternalPrepare();

				if ((behavior & System.Data.CommandBehavior.SequentialAccess) == System.Data.CommandBehavior.SequentialAccess ||
					(behavior & System.Data.CommandBehavior.SingleResult) == System.Data.CommandBehavior.SingleResult ||
					(behavior & System.Data.CommandBehavior.SingleRow) == System.Data.CommandBehavior.SingleRow ||
					(behavior & System.Data.CommandBehavior.CloseConnection) == System.Data.CommandBehavior.CloseConnection ||
					behavior == System.Data.CommandBehavior.Default)
				{
					this.InternalExecute();

					// Retrive information about records affected by command execution
					this.statement.UpdateRecordsAffected();
				}
			}
			catch (GdsException ex)
			{
				this.RollbackImplicitTransaction();

				throw new FbException(ex.Message, ex);
			}

			return new FbDataReader(this, this.connection);
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/method[@name="ExecuteScalar"]/*'/>
		public object ExecuteScalar()
		{
			this.checkCommand();
			
			object val = null;

			try
			{
				this.actualCommand	= 0;
				if (this.CommandType == CommandType.StoredProcedure)
				{
					this.returnsSet	= false;
				}
				else
				{
					this.returnsSet	= true;
				}

				this.splitBatchCommands(false);
				this.InternalPrepare();
				this.InternalExecute();

				// Gets only the values of the first row
				GdsValue[] values;
				if ((values = statement.Fetch()) != null)
				{
					val = values[0].Value;
				}

				// Retrive information about records affected by command execution
				this.statement.UpdateRecordsAffected();

				// Set output parameter values
				this.InternalSetOutputParameters();

				// Commit implicit transaction
				this.CommitImplicitTransaction();
			}
			catch (GdsException ex)
			{
				this.RollbackImplicitTransaction();

				throw new FbException(ex.Message, ex);
			}

			return val;
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/method[@name="Prepare"]/*'/>
		public void Prepare()
		{
			this.checkCommand();

			try
			{
				this.returnsSet = false;

				this.splitBatchCommands(false);
				this.InternalPrepare();
			}
			catch (GdsException ex)
			{
				this.RollbackImplicitTransaction();

				throw new FbException(ex.Message, ex);
			}
		}

		#endregion

		#region INTERNAL_METHODS

		internal void InternalPrepare()
		{
			if (this.transaction == null)
			{
				this.implicitTransaction = true;

				this.transaction = new FbTransaction(this.connection);
				this.transaction.BeginTransaction();
			}

			if (this.commands == null)
			{
				this.splitBatchCommands(false);
			}

			if (this.statement == null)
			{
				if (this.commandType == CommandType.StoredProcedure)
				{
					this.commands[actualCommand] = this.parseSPCommandText();
				}

				this.statement = this.connection.DbConnection.DB.CreateStatement(
					this.commands[actualCommand],
					this.Transaction.Transaction);
			}
			else
			{
				if (this.implicitTransaction)
				{
					this.statement.Transaction = this.Transaction.Transaction;
				}
			}

			if (!this.statement.IsPrepared)
			{
				this.parseCommandText();

				this.statement.Allocate();
				this.statement.Prepare();
				if (this.parameters.Count > 0)
				{
					this.statement.DescribeParameters();
				}

				// Add this command to the active command list
				if (this.connection.ActiveCommands != null)
				{
					if (!this.connection.ActiveCommands.Contains(this))
					{
						this.connection.ActiveCommands.Add(this);
					}
				}
			}
			else
			{
				// Close statement for subsequently executions
				this.statement.Close();
			}
		}

		internal void InternalExecute()
		{
			if (this.parameters.Count > 0)
			{
				this.parseParameters();
			}

			switch (CommandType)
			{
				case CommandType.Text:
				case CommandType.TableDirect:
					this.statement.Execute();
					break;

				case CommandType.StoredProcedure:
					this.statement.ExecuteStoredProc();
					break;
			}
		}

		internal bool NextResult()
		{
			bool returnValue = false;

			this.statement.Drop();
			this.statement = null;

			if ((this.commandBehavior & CommandBehavior.SingleResult) != CommandBehavior.SingleResult)
			{
				this.actualCommand++;
				
				if (this.actualCommand >= commands.Length)
				{
					this.actualCommand--;					
				}
				else
				{
					string commandText = this.commands[actualCommand];
					if (commandText != null && commandText.Trim().Length > 0)
					{
						this.InternalPrepare();
						this.InternalExecute();

						returnValue = true;
					}
				}
			}		

			return returnValue;
		}
			
		internal void InternalSetOutputParameters()
		{
			if (this.CommandType == CommandType.StoredProcedure &&
				this.parameters.Count > 0						&&
				this.statement != null)
			{
				IEnumerator paramEnumerator = this.parameters.GetEnumerator();
				int i = 0;

				if (this.statement != null		&&
					this.statement.Rows != null &&
					this.statement.Rows.Length > 0)
				{
					GdsValue[] values = (GdsValue[])this.statement.Rows[0];

					if (values.Length > 0)
					{
						while (paramEnumerator.MoveNext())
						{
							FbParameter parameter = (FbParameter)paramEnumerator.Current;

							if (parameter.Direction == ParameterDirection.Output ||
								parameter.Direction == ParameterDirection.InputOutput ||
								parameter.Direction == ParameterDirection.ReturnValue)
							{
								parameter.Value = values[i].Value;
								i++;
							}
						}
					}
				}
			}
		}

		internal void CommitImplicitTransaction()
		{
			if (this.implicitTransaction &&
				this.transaction != null)
			{
				try
				{
					this.transaction.Commit();
				}
				catch (Exception ex)
				{
					this.RollbackImplicitTransaction();

					throw ex;
				}
				finally
				{
					this.implicitTransaction	= false;
					this.transaction			= null;
					if (this.statement != null)
					{
						this.statement.Transaction	= null;
					}
				}
			}
		}

		internal void RollbackImplicitTransaction()
		{
			if (this.implicitTransaction &&
				this.transaction != null)
			{
				try
				{
					this.transaction.Rollback();
				}
				catch (Exception)
				{
				}
				finally
				{
					this.implicitTransaction	= false;
					this.transaction			= null;
					if (this.statement != null)
					{
						this.statement.Transaction	= null;
					}
				}
			}
		}

		#endregion

		#region PRIVATE_METHODS

		private void checkCommand()
		{
			if (this.transaction != null &&
				this.transaction.IsUpdated)
			{
				this.transaction = null;
			}

			if (this.connection == null || this.connection.State != ConnectionState.Open)
			{
				throw new InvalidOperationException("Connection must valid and open");
			}

			if (this.connection.DataReader != null)
			{
				throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
			}

			if (this.connection.ActiveTransaction != null &&
				this.transaction == null)
			{
				throw new InvalidOperationException("Execute requires the Command object to have a Transaction object when the Connection object assigned to the command is in a pending local transaction.  The Transaction property of the Command has not been initialized.");
			}

			if (this.transaction != null	&&
				!this.transaction.IsUpdated &&
				!this.connection.Equals(transaction.Connection))
			{
				throw new InvalidOperationException("Command Connection is not equal to Transaction Connection.");
			}

			if (this.commandText == String.Empty || this.commandText == null)
			{
				throw new InvalidOperationException ("The command text for this Command has not been set.");
			}
		}

		private string parseSPCommandText()
		{
			string commandText = this.commands[actualCommand];

			if (commandText != null &&
				!commandText.Trim().ToLower().StartsWith("execute procedure ") &&
				!commandText.Trim().ToLower().StartsWith("select "))
			{
				StringBuilder paramsText = new StringBuilder();

				// Append the stored proc parameter name
				paramsText.Append(commandText);
				if (parameters.Count > 0)
				{
					paramsText.Append("(");
					for (int i = 0; i < this.parameters.Count; i++)
					{
						if (this.parameters[i].Direction == ParameterDirection.Input ||
							this.parameters[i].Direction == ParameterDirection.InputOutput)
						{
							// Append parameter name to parameter list
							paramsText.Append(this.parameters[i].ParameterName);
							if (i != parameters.Count - 1)
							{
								paramsText = paramsText.Append(",");
							}
						}
					}
					paramsText.Append(")");
					paramsText.Replace(",)", ")");
					paramsText.Replace("()", "");
				}
				
				if (returnsSet)
				{
					commandText = "select * from "  + paramsText.ToString();
				}
				else
				{
					commandText = "execute procedure " + paramsText.ToString();
				}
			}

			return commandText;
		}

		private void parseCommandText()
		{
			string sql = this.commands[actualCommand];
			
			this.namedParameters.Clear();

			if (sql.IndexOf("@") != -1)
			{
	 			string pattern = @"(('[^']*?\@[^']*')*[^'@]*?)*(?<param>@\w+)*([^'@]*?('[^']*?\@*[^']*'))*";

				Regex r = new Regex(pattern, RegexOptions.ExplicitCapture);

				MatchEvaluator me = new MatchEvaluator(matchEvaluator);

				sql = r.Replace(sql, me);
			}

			this.statement.CommandText = sql;
		}

		private string matchEvaluator(Match match)
		{
			string input = match.Value;

			if (match.Groups["param"].Success)
			{				
				Group g = match.Groups["param"];

				this.namedParameters.Add(g.Value);
								
				return Regex.Replace(input, g.Value, "?");
			}
			else
			{
				return match.Value;
			}
		}

		private void parseParameters()
		{
			GdsRowDescription	gdsParams		= this.statement.Parameters;			
			string				parameterName	= String.Empty;
			int					index			= -1;
			
			if (gdsParams != null)
			{
				for (int i = 0; i < gdsParams.SqlD; i++)
				{
					index			= -1;
					parameterName	= null;

					if (this.namedParameters != null &&
						this.namedParameters.Count != 0)
					{
						try
						{
							parameterName = this.namedParameters[i].Trim();
						}
						catch
						{
							if (i < this.parameters.Count )
							{
								parameterName = this.parameters[i].ParameterName;
							}
						}

						index = this.parameters.IndexOf(parameterName);
					}
					else
					{
						index = i;
					}

					if (index != -1)
					{
						if (this.parameters[index].Value == System.DBNull.Value)
						{
							gdsParams.SqlVar[i].SqlInd	= -1;
							gdsParams.SqlVar[i].SqlData	= null;
						}
						else
						{
							// Parameter value is not null
							gdsParams.SqlVar[i].SqlInd	= 0;

							switch (gdsParams.SqlVar[i].SqlType & ~1)
							{
								case GdsCodes.SQL_TEXT:
								case GdsCodes.SQL_VARYING:
									GdsCharset charset = this.connection.Parameters.Charset;

									int byteCount = charset.Encoding.GetByteCount(
										this.parameters[index].Value.ToString());

									if (gdsParams.SqlVar[i].SqlLen != 0			&& 	 
										byteCount > gdsParams.SqlVar[i].SqlLen	&&
										charset.BytesPerCharacter >= 1			&&
										charset.BytesPerCharacter <= 2)
									{
										throw new GdsException(335544321); 	 
									}

									gdsParams.SqlVar[i].SqlData = parameters[index].Value;
									break;

								case GdsCodes.SQL_SHORT:
								case GdsCodes.SQL_LONG:
								case GdsCodes.SQL_QUAD:
								case GdsCodes.SQL_INT64:
								case GdsCodes.SQL_FLOAT:
								case GdsCodes.SQL_DOUBLE:
								case GdsCodes.SQL_D_FLOAT:
								case GdsCodes.SQL_TYPE_DATE:
								case GdsCodes.SQL_TYPE_TIME:
								case GdsCodes.SQL_TIMESTAMP:
									gdsParams.SqlVar[i].SqlData = parameters[index].Value;
									break;

								case GdsCodes.SQL_BLOB:
								{
									if (gdsParams.SqlVar[i].SqlSubType == 1)
									{
										GdsAsciiBlob clob = new GdsAsciiBlob(
											this.statement.DB,
											this.statement.Transaction);
										clob.Write(Convert.ToString(parameters[index].Value));
										gdsParams.SqlVar[i].SqlData = clob.Handle;
										clob.Close();
									}
									else
									{
										GdsBinaryBlob blob = new GdsBinaryBlob(
											this.statement.DB,
											this.statement.Transaction);
										blob.Write((byte[])parameters[index].Value);
										gdsParams.SqlVar[i].SqlData = blob.Handle;
										blob.Close();
									}
								}
									break;

								case GdsCodes.SQL_ARRAY:
								{
									if (gdsParams.SqlVar[i].ArrayHandle == null)
									{
										gdsParams.SqlVar[i].ArrayHandle = new GdsArray(
											this.statement.DB,
											this.statement.Transaction,
											gdsParams.SqlVar[i].RelName,
											gdsParams.SqlVar[i].SqlName);
									}
									else
									{
										gdsParams.SqlVar[i].ArrayHandle.DB			= this.statement.DB;
										gdsParams.SqlVar[i].ArrayHandle.Transaction = this.statement.Transaction;
									}
						
									gdsParams.SqlVar[i].ArrayHandle.Handle = 0;
									gdsParams.SqlVar[i].ArrayHandle.Write((System.Array)parameters[index].Value);
									gdsParams.SqlVar[i].SqlData = gdsParams.SqlVar[i].ArrayHandle.Handle;
								}
									break;

								default:
									throw new NotSupportedException("Unknown data type");
							}							
						}
					}
				}

				this.statement.Parameters = gdsParams;
			}
		}

		private void splitBatchCommands(bool batchAllowed)
		{
			if (this.commands == null)
			{
				if (batchAllowed)
				{
					MatchCollection matches = Regex.Matches(
						this.commandText,
						"([^';]+('.*'))*[^';]*(?=;*)");

					this.commands = new string[matches.Count/2];
					int count = 0;
					for (int i = 0; i < matches.Count; i++)
					{
						if (matches[i].Value != null &&
							matches[i].Value.Trim() != String.Empty)
						{
							this.commands[count] = matches[i].Value.Trim();
							count++;
						}
					}
				}
				else
				{
					this.commands = new string[]{this.commandText};
				}
			}
		}

		#endregion
	}
}
