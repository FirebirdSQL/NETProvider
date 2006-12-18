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
using System.Globalization;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird
{		
	/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/overview/*'/>
	[ToolboxItem(true),
	ToolboxBitmap(typeof(FbCommand), "Resources.FbCommand.bmp")]
	public sealed class FbCommand : Component, IDbCommand, ICloneable
	{	
		#region Private static fields

		private static Regex namedRegex = new Regex(
			@"(('[^']*?\@[^']*')*[^'@]*?)*(?<param>@\w+)*([^'@]*?('[^']*?\@*[^']*'))*",
			RegexOptions.Compiled|RegexOptions.ExplicitCapture);

		private static Regex splitRegex = new Regex(
			@"([^';]+('.*'))*[^';]*(?=;*)",
			RegexOptions.Compiled|RegexOptions.ExplicitCapture);


		#endregion
		
		#region Fields
		
		private CommandType				commandType;
		private UpdateRowSource			updatedRowSource;
		private FbConnection			connection;
		private FbTransaction			transaction;
        private FbDataReader            activeReader;
        private FbParameterCollection	parameters;
		private StatementBase			statement;
		private string					commandText;
		private bool					disposed;
		private bool					designTimeVisible;		
		private bool					returnsSet;
		private bool					implicitTransaction;
		private int						commandTimeout;
		private int						actualCommand;
		private StringCollection		commands;
		private StringCollection		namedParameters;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/property[@name="CommandText"]/*'/>
		[Category("Data"),
		DefaultValue(""),
		RefreshProperties(RefreshProperties.All)]
		#if (!MONO)
		[Editor(typeof(Design.FbCommandTextUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
		#endif
		public string CommandText
		{
			get { return this.commandText; }
			set 
			{
				lock (this)
				{
					if (this.statement != null		&& 
						this.commandText != null	&&
						this.commandText != value	&& 
						this.commandText.Length != 0)
					{
						this.Release();
					}
			
					this.commandText	= value;
					this.actualCommand	= 0;
					this.commands.Clear();
				}
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

		int IDbCommand.CommandTimeout
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
                    this.commandTimeout = value;
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
				lock (this)
				{
					if (this.transaction != null && this.transaction.IsUpdated)
					{
						this.transaction = null;
					}

                    if (this.activeReader != null)
                    {
                        throw new InvalidOperationException("There is already an open DataReader associated with this Command which must be closed first.");
                    }

                    if (this.connection != null)
					{
						if (this.connection.ActiveCommands != null)
						{
							this.connection.ActiveCommands.Remove(this);
						}
					}

					this.connection = value;
				}
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
			get { return this.implicitTransaction ? null : this.transaction; }
			set
			{
				lock (this)
				{
					if (this.activeReader != null)
					{
						throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
					}

					this.RollbackImplicitTransaction();
					
					this.transaction = value;

					if (this.statement != null)
					{
						if (this.transaction != null)
						{
							this.statement.Transaction = this.transaction.Transaction;
						}
						else
						{
							this.statement.Transaction = null;
						}
					}					
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

		#region Internal Properties
		
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

        internal FbDataReader ActiveReader
        {
            get { return this.activeReader; }
            set { this.activeReader = value; }
        }

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/constructor[@name="ctor"]/*'/>
		public FbCommand() : base()
		{
			this.parameters			= new FbParameterCollection();
			this.commandText		= String.Empty;
			this.updatedRowSource	= UpdateRowSource.Both;
			this.commandType		= CommandType.Text;
			this.designTimeVisible	= true;
			this.commandTimeout		= 30;
			this.namedParameters	= new StringCollection();
			this.commands			= new StringCollection();
			this.actualCommand		= -1;

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
			this.CommandText = cmdText;
			this.Connection	 = connection;
		}
		
		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/constructor[@name="ctor(System.String,FbConnection,Transaction)"]/*'/>
		public FbCommand(string cmdText, FbConnection connection, FbTransaction transaction) : this()
		{
			this.CommandText = cmdText;
			this.Connection  = connection;
			this.transaction = transaction;
		}				 

		#endregion

		#region IDisposable Methods

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/method[@name="Dispose(System.Boolean)"]/*'/>
		protected override void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!this.disposed)
				{
					try
					{
						if (disposing)
						{
                            // Close the active data reader
                            if (this.activeReader != null)
                            {
                                this.activeReader.Close();
                                this.activeReader = null;
                            }

                            // Clear active commands
							if (this.connection != null &&
								this.connection.ActiveCommands != null)
							{
								this.connection.ActiveCommands.Remove(this);
							}

							// release any managed resources
							this.Release();

							// Commit transaction if needed
							if (this.implicitTransaction &&
								this.transaction != null &&
								!this.transaction.IsUpdated)
							{
								this.RollbackImplicitTransaction();
							}

                            this.commandText			= String.Empty;
							this.actualCommand			= -1;
							this.implicitTransaction	= false;
							this.connection				= null;
							this.transaction			= null;
							this.parameters				= null;
							this.namedParameters.Clear();
							this.namedParameters		= null;
							this.commands.Clear();
							this.commands				= null;
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
		}

		#endregion

		#region ICloneable Methods

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

		#region Methods

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
			lock (this)
			{
				this.checkCommand();

				try
				{
					this.actualCommand	= 0;
					this.returnsSet		= false;
				
					this.executeCommand(CommandBehavior.Default, false);

					this.InternalSetOutputParameters();

					this.CommitImplicitTransaction();
				}
				catch (IscException ex)
				{
					this.RollbackImplicitTransaction();

					throw new FbException(ex.Message, ex);
				}
				catch (Exception)
				{
					this.RollbackImplicitTransaction();

					throw;
				}
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
			lock (this)
			{
				this.checkCommand();

				try
				{
					this.returnsSet	= true;
					
					this.executeCommand(behavior, true);	
				}
				catch (IscException ex)
				{
					this.RollbackImplicitTransaction();

					throw new FbException(ex.Message, ex);
				}
				catch (Exception)
				{
					this.RollbackImplicitTransaction();

					throw;
				}
			}

			return new FbDataReader(this, this.connection, behavior);
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/method[@name="ExecuteScalar"]/*'/>
		public object ExecuteScalar()
		{
			object val = null;

			lock (this)
			{
				this.checkCommand();

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

					this.executeCommand(CommandBehavior.Default, false);

					// Gets only the values of the first row
					DbValue[] values = this.statement.Fetch();
					if (values != null && values.Length > 0)
					{
						val = values[0].Value;
					}

					this.InternalSetOutputParameters();

					this.CommitImplicitTransaction();
				}
				catch (IscException ex)
				{
					this.RollbackImplicitTransaction();

					throw new FbException(ex.Message, ex);
				}
				catch (Exception)
				{
					this.RollbackImplicitTransaction();

					throw;
				}
			}

			return val;
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbCommand"]/method[@name="Prepare"]/*'/>
		public void Prepare()
		{
			lock (this)
			{
				this.checkCommand();

				try
				{
					this.returnsSet		= false;
					this.actualCommand	= 0;
					this.commands.Clear();

					this.InternalPrepare();
				}
				catch (IscException ex)
				{
					this.RollbackImplicitTransaction();

					throw new FbException(ex.Message, ex);
				}
				catch (Exception)
				{
					this.RollbackImplicitTransaction();

					throw;
				}
			}
		}

		#endregion

		#region Internal Methods

		internal void InternalPrepare()
		{
			// Add this command to the active command list
			if (this.connection != null &&
				this.connection.ActiveCommands != null &&
				!this.connection.ActiveCommands.Contains(this))
			{
				this.connection.ActiveCommands.Add(this);
			}

			// Check if we have a valid transaction
			if (this.transaction == null)
			{
				this.implicitTransaction = true;

				this.transaction = new FbTransaction(this.connection);
				this.transaction.BeginTransaction();
			}

			// Split the command text
			this.splitCommandText(false);

			// Check if we need a new statement handle
			if (this.statement == null)
			{
				this.statement = this.connection.IscDb.CreateStatement(this.transaction.Transaction);
			}
			else
			{
				if (this.implicitTransaction)
				{
					this.statement.Transaction = this.transaction.Transaction;
				}
				this.statement.DB = this.connection.DbConnection.DB;
			}

			// Prepare the command if needed
			if (!this.statement.IsPrepared)
			{
				if (this.commandType == CommandType.StoredProcedure)
				{
					this.commands[actualCommand] = this.buildStoredProcedureSql();
				}
				
				this.statement.Prepare(this.parseNamedParameters());

				this.describeInput();
			}
			else
			{
				// Close statement for subsequently executions
				this.Close();
			}
		}

		internal void InternalExecute()
		{
			if (this.parameters.Count > 0)
			{
				if (this.statement.Parameters == null)
				{
					this.describeInput();
				}
				this.updateParameterValues();
			}

			this.statement.Execute();
		}

		internal DbValue[] Fetch()
		{
			try
			{
				if (this.statement != null)
				{
					if (this.statement.StatementType == DbStatementType.Select ||
						this.statement.StatementType == DbStatementType.SelectForUpdate ||
						this.statement.StatementType == DbStatementType.StoredProcedure)
					{
						return this.statement.Fetch();
					}
				}
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			return null;
		}

		internal RowDescriptor GetFieldsDescriptor()
		{
			if (this.statement != null)
			{
				return this.statement.Fields;
			}

			return null;
		}

		internal bool NextResult(CommandBehavior behavior)
		{
			bool returnValue = false;

			this.Release();

			if ((behavior & CommandBehavior.SingleResult) != CommandBehavior.SingleResult)
			{
				this.actualCommand++;

				if (this.actualCommand >= commands.Count)
				{
					this.actualCommand--;
				}
				else
				{
					string commandText = this.commands[actualCommand];
					if (commandText != null && commandText.Length > 0)
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
					DbValue[] values = (DbValue[])this.statement.Rows[0];

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
			if (this.implicitTransaction && this.transaction != null)
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
			if (this.implicitTransaction && this.transaction != null)
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

		internal void Close()
		{
			if (this.statement != null)
			{
				this.statement.Close();
			}
		}

		internal void Release()
		{
			if (this.statement != null)
			{
				this.statement.Release();
				this.statement = null;
			}
		}

		#endregion

		#region Input parameter descriptor generation methods

		private RowDescriptor buildParametersDescriptor()
		{
			short count	= this.validateInputParameters();

			if (count > 0)
			{
				if (this.namedParameters.Count > 0)
				{
					count = (short)this.namedParameters.Count;
					return this.buildNamedParametersDescriptor(count);
				}
				else
				{
					return this.buildPlaceHoldersDescriptor(count);
				}
			}

			return null;
		}

		private RowDescriptor buildNamedParametersDescriptor(short count)
		{
			RowDescriptor	descriptor	= new RowDescriptor(count);
			int				index		= 0;

			for (int i = 0; i < this.namedParameters.Count; i++)
			{	
				FbParameter parameter = this.parameters[this.namedParameters[i]];
		
				if (parameter.Direction == ParameterDirection.Input ||
					parameter.Direction == ParameterDirection.InputOutput)
				{
					if (!this.buildParameterDescriptor(descriptor, parameter, index++))
					{
						return null;
					}
				}
			}

			return descriptor;
		}

		private RowDescriptor buildPlaceHoldersDescriptor(short count)
		{
			RowDescriptor	descriptor	= new RowDescriptor(count);
			int				index		= 0;

			for (int i = 0; i < this.parameters.Count; i++)
			{
				FbParameter parameter = this.parameters[i];

				if (parameter.Direction == ParameterDirection.Input ||
					parameter.Direction == ParameterDirection.InputOutput)
				{
					if (!this.buildParameterDescriptor(descriptor, parameter, index++))
					{
						return null;
					}					
				}
			}

			return descriptor;
		}

		private bool buildParameterDescriptor(
			RowDescriptor	descriptor,
			FbParameter		parameter,
			int				index)
		{
			Charset	 charset = this.connection.Parameters.Charset;
			FbDbType type	 = parameter.FbDbType;

			// Check the parameter character set
			if (parameter.Charset != FbCharset.Default)
			{
				int idx = Charset.SupportedCharsets.IndexOf((int)parameter.Charset);
				charset = Charset.SupportedCharsets[idx];
			}

			// Set parameter Data Type
			descriptor[index].DataType = (short)TypeHelper.GetFbType(
				(DbDataType)type, 
				parameter.IsNullable);

			// Set parameter Sub Type
			switch (type)
			{
				case FbDbType.Binary:
					descriptor[index].SubType = 0;
					break;

				case FbDbType.Text:
					descriptor[index].SubType = 1;
					break;

				case FbDbType.Char:
				case FbDbType.VarChar:
					descriptor[index].SubType = (short)charset.ID;
                    if (parameter.Size > 0)
                    {
                        short len = (short)(parameter.Size * charset.BytesPerCharacter);
                        descriptor[index].Length = len;
                    }
                    break;
			}

			// Set parameter length
			if (descriptor[index].Length == 0)
			{
				descriptor[index].Length = TypeHelper.GetSize((DbDataType)type);
			}

			// Verify parameter
			if (descriptor[index].SqlType == 0 || descriptor[index].Length == 0)
			{
				return false;
			}

			return true;
		}

		private short validateInputParameters()
		{
			short count = 0;

			for (int i = 0; i < this.parameters.Count; i++)
			{
				if (this.parameters[i].Direction == ParameterDirection.Input ||
					this.parameters[i].Direction == ParameterDirection.InputOutput)
				{
					FbDbType type = this.parameters[i].FbDbType;
					
					if (type == FbDbType.Array		||
						type == FbDbType.Decimal	||
						type == FbDbType.Numeric)
					{
						return -1;
					}
					else
					{
						count++;
					}
				}
			}

			return count;
		}

		#endregion

		#region Private Methods

		private void describeInput()
		{
			if (this.parameters.Count > 0)
			{
				RowDescriptor descriptor = this.buildParametersDescriptor();
				if (descriptor == null)
				{
					this.statement.DescribeParameters();
				}
				else
				{
					this.statement.Parameters = descriptor;
				}
			}
		}

		private void executeCommand(CommandBehavior behavior, bool split)
		{
			this.splitCommandText(split);
			this.InternalPrepare();

			if ((behavior & CommandBehavior.SequentialAccess) == CommandBehavior.SequentialAccess ||
				(behavior & CommandBehavior.SingleResult) == CommandBehavior.SingleResult ||
				(behavior & CommandBehavior.SingleRow) == CommandBehavior.SingleRow ||
				(behavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection ||
				behavior == CommandBehavior.Default)				
			{
				this.InternalExecute();
			}
		}

		private void splitCommandText(bool batchAllowed)
		{
			if (this.commands.Count == 0)
			{
				this.commands.Clear();

				if (batchAllowed)
				{
					MatchCollection matches = splitRegex.Matches(this.commandText);

					for (int i = 0; i < matches.Count; i++)
					{
						if (matches[i].Value != null && matches[i].Value.Length > 0)
						{
							this.commands.Add(matches[i].Value.Trim());
						}
					}
				}
				else
				{
					this.commands.Add(this.CommandText);
				}
			}
		}

		private string buildStoredProcedureSql()
		{
			string commandText = this.commands[actualCommand];

			if (commandText != null &&
				!commandText.ToLower(CultureInfo.CurrentCulture).StartsWith("execute procedure ") &&
				!commandText.ToLower(CultureInfo.CurrentCulture).StartsWith("select "))
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
				
				if (this.returnsSet)
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

		private string parseNamedParameters()
		{
			string sql = this.commands[actualCommand];

			this.namedParameters.Clear();
			
			if (sql.IndexOf("@") != -1)
			{
				MatchEvaluator me = new MatchEvaluator(matchEvaluator);

				sql = namedRegex.Replace(sql, me);
			}

			return sql;
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

		private void updateParameterValues()
		{
			int	 index = -1;
			
			for (int i = 0; i < this.statement.Parameters.Count; i++)
			{
				index = i;

				if (this.namedParameters.Count > 0)
				{
					index = this.parameters.IndexOf(this.namedParameters[i]);
				}

				if (index != -1)
				{
					if (this.parameters[index].Value == DBNull.Value ||
                        this.parameters[index].Value == null)
                    {
						this.statement.Parameters[i].NullFlag	= -1;
						this.statement.Parameters[i].Value		= DBNull.Value;
						if (!this.statement.Parameters[i].AllowDBNull())
						{
							this.statement.Parameters[i].DataType++;
						}
					}
					else
					{
						// Parameter value is not null
						this.statement.Parameters[i].NullFlag = 0;

						switch (this.statement.Parameters[i].DbDataType)
						{
							case DbDataType.Binary:
							{
								BlobBase blob = this.statement.CreateBlob();
								blob.Write((byte[])this.parameters[index].Value);
								this.statement.Parameters[i].Value = blob.Id;
							}
							break;

							case DbDataType.Text:
							{
								BlobBase blob = this.statement.CreateBlob();
								blob.Write((string)this.parameters[index].Value);
								this.statement.Parameters[i].Value = blob.Id;
							}
							break;

							case DbDataType.Array:
							{
								if (this.statement.Parameters[i].ArrayHandle == null)
								{
									this.statement.Parameters[i].ArrayHandle = 
										this.statement.CreateArray(
											this.statement.Parameters[i].Relation,
											this.statement.Parameters[i].Name);
								}
								else
								{
									this.statement.Parameters[i].ArrayHandle.DB			= this.statement.DB;
									this.statement.Parameters[i].ArrayHandle.Transaction = this.statement.Transaction;
								}
					
								this.statement.Parameters[i].ArrayHandle.Handle = 0;
								this.statement.Parameters[i].ArrayHandle.Write((System.Array)this.parameters[index].Value);
								this.statement.Parameters[i].Value = this.statement.Parameters[i].ArrayHandle.Handle;
							}
							break;

							default:
								this.statement.Parameters[i].Value = this.parameters[index].Value;
								break;
						}							
					}
				}
			}
		}

		private void checkCommand()
		{
			if (this.transaction != null && this.transaction.IsUpdated)
			{
				this.transaction = null;
			}

			if (this.connection == null || this.connection.State != ConnectionState.Open)
			{
				throw new InvalidOperationException("Connection must valid and open");
			}

			if (this.activeReader != null)
			{
				throw new InvalidOperationException("There is already an open DataReader associated with this Command which must be closed first.");
			}

			if (this.transaction == null && this.connection.ActiveTransaction != null)
			{
				throw new InvalidOperationException("Execute requires the Command object to have a Transaction object when the Connection object assigned to the command is in a pending local transaction.  The Transaction property of the Command has not been initialized.");
			}

			if (this.transaction != null	&&
				!this.transaction.IsUpdated &&
				!this.connection.Equals(transaction.Connection))
			{
				throw new InvalidOperationException("Command Connection is not equal to Transaction Connection.");
			}

			if (this.commandText == null || this.commandText.Length == 0)
			{
				throw new InvalidOperationException ("The command text for this Command has not been set.");
			}
		}

		#endregion
	}
}