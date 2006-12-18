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
using System.ComponentModel;
using System.Text.RegularExpressions;

using FirebirdSql.Data.INGDS;
using FirebirdSql.Data.NGDS;

namespace FirebirdSql.Data.Firebird
{		
	/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="T:FbCommand"]/*'/>
	#if (!_MONO)
	[ToolboxBitmap(typeof(FbCommand), "Resources.ToolboxBitmaps.FbCommand.bmp")]
	#endif
	public sealed class FbCommand : Component, IDbCommand, ICloneable
	{				
		#region FIELDS
		
		private FbConnection			connection;
		private FbTransaction			transaction;				
		private FbStatement				statement;
		private FbParameterCollection	parameters;
		private UpdateRowSource			updatedRowSource;
		private CommandBehavior			commandBehavior;
		private bool					disposed;
		private int						actualCommand;
		private int						recordsAffected;
		private string[]				commands;
		private char[]					commandSeparator;
		private bool					splitCommands;
		private string					commandText;
		private CommandType				commandType;
		private bool					designTimeVisible;

		#endregion

		#region PROPERTIES

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="P:CommandText"]/*'/>
		public string CommandText
		{
			get 
			{ 
				// return commands[actualCommand] == null ? "" : commands[actualCommand];
				return commandText;
			}
			set 
			{ 				
				if (statement != null && commandText != value && commandText != String.Empty)
				{
					statement.DropStatement();
				}

				commandText		= value;
				actualCommand	= 0;
			}
		}
		
		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="P:CommandTimeout"]/*'/>
		public int CommandTimeout
		{
			get { return 0; }
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

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="P:CommandType"]/*'/>
		public CommandType CommandType
		{
			get { return commandType; }
			set { commandType = value; }
		}

		IDbConnection IDbCommand.Connection
		{
			get { return Connection; }
			set { Connection = (FbConnection)value; }
		}

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="P:Connection"]/*'/>
		public FbConnection Connection
		{
			get { return connection; }
			set
			{
				if (Transaction != null)
				{
					if (!Transaction.IsUpdated)
					{
						throw new InvalidOperationException("The Connection property was changed while a transaction was in progress.");
					}
				}

				/*
				 * The connection is associated with the transaction
				 * so set the transaction object to return a null reference 
				 * if the connection is reset.
				 */
				if (connection != value)
				{										
					if (Transaction != null)
					{
						Transaction = null;
					}

					if (statement != null)
					{
						statement.Dispose();
						statement = null;
					}
				}

				connection = value;
			}
		}

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="P:DesignTimeVisible"]/*'/>
		public bool DesignTimeVisible
		{
			get { return designTimeVisible; }
			set { designTimeVisible = value; }
		}

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="P:Parameters"]/*'/>
		IDataParameterCollection IDbCommand.Parameters
		{
			get { return Parameters; }
		}

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="P:Parameters"]/*'/>
		public FbParameterCollection Parameters
		{
			get { return parameters; }
		}

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="P:Transaction"]/*'/>
		IDbTransaction IDbCommand.Transaction
		{
			get { return Transaction; }
			set { Transaction = (FbTransaction)value; }
		}

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="P:Transaction"]/*'/>
		public FbTransaction Transaction
		{
			get { return transaction; }
			set
			{
				if (transaction != null)
				{
					if (transaction.IsUpdated)
					{
						transaction = value; 
					}
				}
				else
				{
					transaction = value;
				}
			}
		}
				
		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="P:UpdatedRowSource"]/*'/>
		public UpdateRowSource UpdatedRowSource
		{
			get { return updatedRowSource; }
			set { updatedRowSource = value; }
		}
		
		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="P:CommandBehavior"]/*'/>
		internal CommandBehavior CommandBehavior
		{
			get { return commandBehavior; }
		}

		internal FbStatement Statement
		{
			get { return statement; }
		}

		internal int RecordsAffected
		{
			get { return recordsAffected; }
		}

		internal bool IsDisposed
		{
			get {return disposed;}
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public FbCommand()
		{
			parameters			= new FbParameterCollection();
			updatedRowSource	= UpdateRowSource.Both;
			commandBehavior		= CommandBehavior.Default;
			disposed			= false;
			actualCommand		= -1;
			recordsAffected		= -1;
			splitCommands		= false;
			commandText			= String.Empty;
			commandType			= CommandType.Text;		
			designTimeVisible	= true;
			commandSeparator	= new char[] {';'};
		}

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:#ctor(System.String)"]/*'/>
		public FbCommand(string cmdText) : this()
		{
			this.CommandText = cmdText;
		}
		
		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:#ctor(System.String,FirebirdSql.Data.Firebird.FbConnection)"]/*'/>
		public FbCommand(string cmdText, FbConnection connection) : this()
		{
			this.CommandText = cmdText;
			this.Connection	 = connection;
		}
		
		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:#ctor(System.String,FirebirdSql.Data.Firebird.FbConnection,FirebirdSql.Data.Firebird.FbTransaction)"]/*'/>
		public FbCommand(string cmdText, FbConnection connection, FbTransaction transaction) : this()
		{
			this.CommandText = cmdText;
			this.Connection  = connection;
			this.Transaction = transaction;
		}				 

		#endregion

		#region DESTRUCTORS

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:Dispose(System.Boolean)"]/*'/>
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				try
				{
					if (disposing)
					{
						connection.ActiveCommands.Remove(this);

						// release any managed resources
						if (statement != null)
						{
							statement.Dispose();
							statement = null;
						}

						commandText		= String.Empty;
						actualCommand	= -1;
						commands		= null;
					}
					
					// release any unmanaged resources
					
					disposed = true;
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
			FbCommand command = new FbCommand(CommandText, Connection, Transaction);

			command.CommandType	= this.CommandType;

			return command;
		}

		#endregion

		#region METHODS
						
		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:Cancel"]/*'/>
		public void Cancel()
		{			
			throw new NotSupportedException();
		}
		
		IDbDataParameter IDbCommand.CreateParameter()
		{
			return CreateParameter();
		}

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:CreateParameter"]/*'/>
		public FbParameter CreateParameter()
		{
			return new FbParameter();
		}

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:ExecuteNonQuery"]/*'/>
		public int ExecuteNonQuery()
		{      
			if (Connection == null || Connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must valid and open");

			if (Connection.DataReader != null)
				throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");

			if (Transaction == null)
				throw new InvalidOperationException("Command must have a valid Transaction.");

			if (!Connection.Equals(Transaction.Connection))			
				throw new InvalidOperationException("Command Connection is not equal to Transaction Connection");

			try
			{
				actualCommand	= 0;
				splitCommands	= false;

				Prepare();
				statement.Execute();

				statement.SetOutputParameterValues();
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}
			
			recordsAffected = statement.GetRecordsAffected();

			return recordsAffected;
		}
				
		IDataReader IDbCommand.ExecuteReader()
		{	
			return ExecuteReader();			
		}

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:ExecuteReader"]/*'/>
		public FbDataReader ExecuteReader()
		{	
			return ExecuteReader(CommandBehavior.Default);			
		}
		
		IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
		{
			return ExecuteReader(behavior);
		}

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:ExecuteReader(System.Data.CommandBehavior)"]/*'/>	
		public FbDataReader ExecuteReader(CommandBehavior behavior)
		{
			if (Connection == null || Connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must valid and open");

			if (Connection.DataReader != null)
				throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");

			if (Transaction == null)
				throw new InvalidOperationException("Command must have a valid Transaction.");

			if (!Connection.Equals(Transaction.Connection))
				throw new InvalidOperationException("Command Connection is not equal to Transaction Connection");

			try
			{
				commandBehavior = behavior;				
				splitCommands	= true;

				Prepare();

				if ((commandBehavior & System.Data.CommandBehavior.SequentialAccess) == System.Data.CommandBehavior.SequentialAccess ||
					(commandBehavior & System.Data.CommandBehavior.SingleResult) == System.Data.CommandBehavior.SingleResult ||
					(commandBehavior & System.Data.CommandBehavior.SingleRow) == System.Data.CommandBehavior.SingleRow ||
					(commandBehavior & System.Data.CommandBehavior.CloseConnection) == System.Data.CommandBehavior.CloseConnection ||
					commandBehavior == System.Data.CommandBehavior.Default)				
				{
					statement.Execute();
				}

				recordsAffected = statement.GetRecordsAffected();
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			return new FbDataReader(this);
		}

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:ExecuteScalar"]/*'/>
		public object ExecuteScalar()
		{
			object val = null;

			if (Connection == null || Connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must valid and open");

			if (Connection.DataReader != null)
				throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");

			if (Transaction == null)
				throw new InvalidOperationException("Command must have a valid Transaction.");

			if (!Connection.Equals(Transaction.Connection))
				throw new InvalidOperationException("Command Connection is not equal to Transaction Connection");

			try
			{
				actualCommand	= 0;
				splitCommands	= true;

				Prepare();
				
				statement.Execute();
				recordsAffected = statement.GetRecordsAffected();

				// Gets only the first row
				if (statement.Resultset.Fetch())
				{
					val = statement.Resultset.GetValue(0);
				}
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			return val;
		}

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:Prepare"]/*'/>	
		public void Prepare()
		{
			if (Connection == null || Connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must valid and open.");

			if (Transaction == null)
				throw new InvalidOperationException("Command must have a valid Transaction.");

			if (!Connection.Equals(Transaction.Connection))
				throw new InvalidOperationException("Command Connection is not equal to Transaction Connection.");

			if (Statement != null)
			{
				if (Statement.State == CommandState.Executed || Statement.State == CommandState.Prepared)
				{
					// Close statement for subsequently calls
					statement.Close();
					return;
				}
			}

			try
			{
				if (actualCommand == 0)
				{
					if (splitCommands)
					{						
						commands = commandText.Split(commandSeparator);
					}
					else
					{
						commands	= new string[1];
						commands[0] = commandText;
					}
				}

				statement = new FbStatement(Connection	, 
											Transaction	, 
											Parameters	, 
											commands[actualCommand], 
											CommandType);	

				statement.Prepare();

				connection.ActiveCommands.Add(this);
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}
		
		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:NextResult"]/*'/>
		internal bool NextResult()
		{
			bool returnValue = false;

			actualCommand++;

			if (actualCommand >= commands.Length)
			{
				actualCommand--;
			}
			else
			{
				string commandText = commands[actualCommand].Trim();

				statement.DropStatement();
				statement = null;

				if (commandText.Length > 0)
				{
					Prepare();
				
					statement.Execute();

					recordsAffected = statement.GetRecordsAffected();

					returnValue = true;
				}
			}

			return returnValue;
		}
	
		#endregion
	}
}
