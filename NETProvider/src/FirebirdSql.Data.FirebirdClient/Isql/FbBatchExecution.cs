/*
 *  Firebird ADO.NET Data provider for .NET and Mono
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
 *  Copyright (c) 2003-2007 Abel Eduardo Pereira
 *  All Rights Reserved.
 *
 * Contributors:
 *   Jiri Cincura (jiri@cincura.net)
 *   Olivier Metod
 */

using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Isql
{
	/// <summary>
	/// Summary description for BatchExecution.
	/// </summary>
	public class FbBatchExecution
	{
		#region Events

		/// <summary>
		/// The event trigged before a SQL statement goes for execution.
		/// </summary>
		public event CommandExecutingEventHandler CommandExecuting;

		/// <summary>
		/// The event trigged after a SQL statement execution.
		/// </summary>
		public event CommandExecutedEventHandler CommandExecuted;

		#endregion

		#region Fields

		private FbStatementCollection sqlStatements;
		private FbConnection sqlConnection;
		private FbTransaction sqlTransaction;
		private FbConnectionStringBuilder connectionString;
		private FbCommand sqlCommand;

		// control fields
		private bool requiresNewConnection;

		#endregion

		#region Properties

		/// <summary>
		/// Represents the list of SQL statements for batch execution.
		/// </summary>
		public FbStatementCollection SqlStatements
		{
			get { return this.sqlStatements; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Creates an instance of FbBatchExecution engine with the given
		/// connection.
		/// </summary>
		/// <param name="sqlConnection">A <see cref="FbConnection"/> object.</param>
		public FbBatchExecution(FbConnection sqlConnection = null)
		{
			this.sqlStatements = new FbStatementCollection();
			if (sqlConnection == null)
			{
				this.sqlConnection = new FbConnection(); // do not specify the connection string
				this.connectionString = new FbConnectionStringBuilder();
			}
			else
			{
				this.sqlConnection = sqlConnection;
				this.connectionString = new FbConnectionStringBuilder(sqlConnection.ConnectionString);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <b>FbBatchExecution</b> class with the given
		/// <see cref="FbConnection" /> and <see cref="FbScript"/> instances.
		/// </summary>
		/// <param name="sqlConnection">A <see cref="FbConnection"/> object.</param>
		/// <param name="isqlScript">A <see cref="FbScript"/> object.</param>
		[Obsolete("Use other ctor together with AppendSqlStatements mehod.")]
		public FbBatchExecution(FbConnection sqlConnection, FbScript isqlScript)
			: this(sqlConnection)
		{
			AppendSqlStatements(isqlScript);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Appends SQL statements from <see cref="FbScript"/> instance. <see cref="FbScript.Parse"/> should be already called.
		/// </summary>
		/// <param name="isqlScript">A <see cref="FbScript"/> object.</param>
		public void AppendSqlStatements(FbScript isqlScript)
		{
			this.sqlStatements.AddRange(isqlScript.Results);
		}

		/// <summary>
		/// Starts the ordered execution of the SQL statements that are in <see cref="SqlStatements"/> collection.
		/// </summary>
		/// <param name="autoCommit">Specifies if the transaction should be committed after a DDL command execution</param>
		public void Execute(bool autoCommit = true)
		{
			if (this.SqlStatements == null || this.SqlStatements.Count == 0)
			{
				throw new InvalidOperationException("There are no commands for execution.");
			}

			foreach (string sqlStatement in this.SqlStatements)
			{
				if (string.IsNullOrEmpty(sqlStatement))
				{
					continue;
				}

				// initializate outputs to default
				int rowsAffected = -1;
				FbDataReader dataReader = null;
				SqlStatementType statementType = FbBatchExecution.GetStatementType(sqlStatement);

				if (!(statementType == SqlStatementType.Connect ||
					statementType == SqlStatementType.CreateDatabase ||
					statementType == SqlStatementType.Disconnect ||
					statementType == SqlStatementType.DropDatabase ||
					statementType == SqlStatementType.SetAutoDDL ||
					statementType == SqlStatementType.SetDatabase ||
					statementType == SqlStatementType.SetNames ||
					statementType == SqlStatementType.SetSQLDialect))
				{
					this.ProvideCommand();
					this.sqlCommand.CommandText = sqlStatement;
					if (this.sqlTransaction == null && !(statementType == SqlStatementType.Commit || statementType == SqlStatementType.Rollback))
					{
						this.sqlTransaction = this.sqlConnection.BeginTransaction();
					}
					this.sqlCommand.Transaction = this.sqlTransaction;
				}

				try
				{
					switch (statementType)
					{
						case SqlStatementType.AlterDatabase:
						case SqlStatementType.AlterDomain:
						case SqlStatementType.AlterException:
						case SqlStatementType.AlterIndex:
						case SqlStatementType.AlterProcedure:
						case SqlStatementType.AlterRole:
						case SqlStatementType.AlterTable:
						case SqlStatementType.AlterTrigger:
						case SqlStatementType.AlterView:
							this.OnCommandExecuting(this.sqlCommand);

							rowsAffected = this.ExecuteCommand(autoCommit);
							this.requiresNewConnection = false;

							this.OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.Commit:
							this.OnCommandExecuting(null);

							this.CommitTransaction();

							this.OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.Connect:
							this.OnCommandExecuting(null);

							this.ConnectToDatabase(sqlStatement);

							this.requiresNewConnection = false;

							this.OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.CreateDatabase:
							this.OnCommandExecuting(null);

							this.CreateDatabase(sqlStatement);
							this.requiresNewConnection = false;

							this.OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.CommentOn:
						case SqlStatementType.CreateCollation:
						case SqlStatementType.CreateDomain:
						case SqlStatementType.CreateException:
						case SqlStatementType.CreateGenerator:
						case SqlStatementType.CreateIndex:
						case SqlStatementType.CreateProcedure:
						case SqlStatementType.CreateRole:
						case SqlStatementType.CreateSequence:
						case SqlStatementType.CreateShadow:
						case SqlStatementType.CreateTable:
						case SqlStatementType.CreateTrigger:
						case SqlStatementType.CreateView:
						case SqlStatementType.DeclareCursor:
						case SqlStatementType.DeclareExternalFunction:
						case SqlStatementType.DeclareFilter:
						case SqlStatementType.DeclareStatement:
						case SqlStatementType.DeclareTable:
						case SqlStatementType.Delete:
							this.OnCommandExecuting(this.sqlCommand);

							rowsAffected = this.ExecuteCommand(autoCommit);
							this.requiresNewConnection = false;

							this.OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.Describe:
							break;

						case SqlStatementType.Disconnect:
							this.OnCommandExecuting(null);

							this.sqlConnection.Close();
							FbConnection.ClearPool(this.sqlConnection);
							this.requiresNewConnection = false;

							this.OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.DropDatabase:
							this.OnCommandExecuting(null);

							FbConnection.DropDatabase(this.connectionString.ToString());
							this.requiresNewConnection = true;

							this.OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.DropCollation:
						case SqlStatementType.DropDomain:
						case SqlStatementType.DropException:
						case SqlStatementType.DropExternalFunction:
						case SqlStatementType.DropFilter:
						case SqlStatementType.DropGenerator:
						case SqlStatementType.DropIndex:
						case SqlStatementType.DropProcedure:
						case SqlStatementType.DropSequence:
						case SqlStatementType.DropRole:
						case SqlStatementType.DropShadow:
						case SqlStatementType.DropTable:
						case SqlStatementType.DropTrigger:
						case SqlStatementType.DropView:
						case SqlStatementType.EventInit:
						case SqlStatementType.EventWait:
						case SqlStatementType.Execute:
						case SqlStatementType.ExecuteImmediate:
						case SqlStatementType.ExecuteProcedure:
							this.ProvideCommand().CommandText = sqlStatement;

							this.OnCommandExecuting(this.sqlCommand);

							rowsAffected = this.ExecuteCommand(autoCommit);
							this.requiresNewConnection = false;

							this.OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.ExecuteBlock:
							this.ProvideCommand().CommandText = sqlStatement;

							this.OnCommandExecuting(this.sqlCommand);

							dataReader = this.sqlCommand.ExecuteReader();
							this.requiresNewConnection = false;

							this.OnCommandExecuted(sqlStatement, dataReader, -1);
							if (!dataReader.IsClosed)
							{
								dataReader.Close();
							}
							break;

						case SqlStatementType.Fetch:
							break;

						case SqlStatementType.Grant:
						case SqlStatementType.Insert:
						case SqlStatementType.InsertCursor:
						case SqlStatementType.Open:
						case SqlStatementType.Prepare:
						case SqlStatementType.Revoke:
							this.OnCommandExecuting(this.sqlCommand);

							rowsAffected = this.ExecuteCommand(autoCommit);
							this.requiresNewConnection = false;

							this.OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.RecreateProcedure:
						case SqlStatementType.RecreateTable:
						case SqlStatementType.RecreateTrigger:
						case SqlStatementType.RecreateView:
							this.OnCommandExecuting(this.sqlCommand);

							rowsAffected = this.ExecuteCommand(autoCommit);
							this.requiresNewConnection = false;

							this.OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.Rollback:
							this.OnCommandExecuting(null);

							this.RollbackTransaction();

							this.OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.Select:
							this.ProvideCommand().CommandText = sqlStatement;

							this.OnCommandExecuting(this.sqlCommand);

							dataReader = this.sqlCommand.ExecuteReader();
							this.requiresNewConnection = false;

							this.OnCommandExecuted(sqlStatement, dataReader, -1);
							if (!dataReader.IsClosed)
							{
								dataReader.Close();
							}
							break;

						case SqlStatementType.SetAutoDDL:
							this.OnCommandExecuting(null);

							this.SetAutoDdl(sqlStatement, ref autoCommit);
							this.requiresNewConnection = false;

							this.OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.SetGenerator:
						case SqlStatementType.AlterSequence:
							this.OnCommandExecuting(this.sqlCommand);

							rowsAffected = this.ExecuteCommand(autoCommit);
							this.requiresNewConnection = false;

							this.OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.SetNames:
							this.OnCommandExecuting(null);

							this.SetNames(sqlStatement);
							this.requiresNewConnection = true;

							this.OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.SetSQLDialect:
							this.OnCommandExecuting(null);

							this.SetSqlDialect(sqlStatement);
							this.requiresNewConnection = true;

							this.OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.SetDatabase:
						case SqlStatementType.SetStatistics:
						case SqlStatementType.SetTransaction:
						case SqlStatementType.ShowSQLDialect:
							throw new NotImplementedException();

						case SqlStatementType.Update:
						case SqlStatementType.Whenever:
							this.OnCommandExecuting(this.sqlCommand);

							rowsAffected = this.ExecuteCommand(autoCommit);
							this.requiresNewConnection = false;

							this.OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;
					}
				}
				catch (Exception ex)
				{
					this.RollbackTransaction();

					throw new FbException(string.Format("An exception was thrown when executing command: {1}.{0}Batch execution aborted.{0}The returned message was: {2}.",
						Environment.NewLine,
						sqlStatement,
						ex.Message));
				}
			}

			this.CommitTransaction();

			this.sqlConnection.Close();
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Updates the connection string with the data parsed from the parameter and opens a connection
		/// to the database.
		/// </summary>
		/// <param name="connectDbStatement"></param>
		protected internal void ConnectToDatabase(string connectDbStatement)
		{
			// CONNECT 'filespec'
			// [USER 'username']
			// [PASSWORD 'password']
			// [CACHE int]
			// [ROLE 'rolename']
			StringParser parser = new StringParser(connectDbStatement, false);
			parser.Tokens = new[] { " ", "\r\n", "\n", "\r" };
			parser.ParseNext();
			if (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture) != "CONNECT")
			{
				throw new ArgumentException("Malformed isql CONNECT statement. Expected keyword CONNECT but something else was found.");
			}
			parser.ParseNext();
			this.connectionString.Database = parser.Result.Replace("'", string.Empty);
			while (parser.ParseNext() != -1)
			{
				switch (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture))
				{
					case "USER":
						parser.ParseNext();
						this.connectionString.UserID = parser.Result.Replace("'", string.Empty);
						break;

					case "PASSWORD":
						parser.ParseNext();
						this.connectionString.Password = parser.Result.Replace("'", string.Empty);
						break;

					case "CACHE":
						parser.ParseNext();
						break;

					case "ROLE":
						parser.ParseNext();
						this.connectionString.Role = parser.Result.Replace("'", string.Empty);
						break;

					default:
						throw new ArgumentException("Unexpected token '" + parser.Result.Trim() + "' on isql CONNECT statement.");

				}
			}
			this.requiresNewConnection = true;
			this.ProvideConnection();
		}

		/// <summary>
		/// Parses the isql statement CREATE DATABASE and creates the database and opens a connection to the recently created database.
		/// </summary>
		/// <param name="createDatabaseStatement">The create database statement.</param>
		protected void CreateDatabase(string createDatabaseStatement)
		{
			// CREATE {DATABASE | SCHEMA} 'filespec'
			// [USER 'username' [PASSWORD 'password']]
			// [PAGE_SIZE [=] int]
			// [LENGTH [=] int [PAGE[S]]]
			// [DEFAULT CHARACTER SET charset]
			// [<secondary_file>];
			int pageSize = 0;
			StringParser parser = new StringParser(createDatabaseStatement, false);
			parser.Tokens = new[] { " ", "\r\n", "\n", "\r" };
			parser.ParseNext();
			if (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture) != "CREATE")
			{
				throw new ArgumentException("Malformed isql CREATE statement. Expected keyword CREATE but something else was found.");
			}
			parser.ParseNext(); // {DATABASE | SCHEMA}
			parser.ParseNext();
			this.connectionString.Database = parser.Result.Replace("'", string.Empty);
			while (parser.ParseNext() != -1)
			{
				switch (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture))
				{
					case "USER":
						parser.ParseNext();
						this.connectionString.UserID = parser.Result.Replace("'", string.Empty);
						break;

					case "PASSWORD":
						parser.ParseNext();
						this.connectionString.Password = parser.Result.Replace("'", string.Empty);
						break;

					case "PAGE_SIZE":
						parser.ParseNext();
						if (parser.Result.Trim() == "=")
							parser.ParseNext();
						int.TryParse(parser.Result, out pageSize);
						break;

					case "DEFAULT":
						parser.ParseNext();
						if (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture) != "CHARACTER")
							throw new ArgumentException("Expected the keyword CHARACTER but something else was found.");

						parser.ParseNext();
						if (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture) != "SET")
							throw new ArgumentException("Expected the keyword SET but something else was found.");

						parser.ParseNext();
						this.connectionString.Charset = parser.Result;
						break;
				}
			}
			FbConnection.CreateDatabase(this.connectionString.ToString(), pageSize, true, false);
			this.requiresNewConnection = true;
			this.ProvideConnection();
		}

		/// <summary>
		/// Parses the isql statement SET AUTODDL and sets the character set to current connection string.
		/// </summary>
		/// <param name="setAutoDdlStatement">The set names statement.</param>
		protected void SetAutoDdl(string setAutoDdlStatement, ref bool autoCommit)
		{
			// SET AUTODDL [ON | OFF]
			StringParser parser = new StringParser(setAutoDdlStatement, false);
			parser.Tokens = new[] { " ", "\r\n", "\n", "\r" };
			parser.ParseNext();
			if (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture) != "SET")
			{
				throw new ArgumentException("Malformed isql SET statement. Expected keyword SET but something else was found.");
			}
			parser.ParseNext(); // AUTO
			if (parser.ParseNext() != -1)
			{
				string onOff = parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture);
				if (onOff == "ON")
				{
					autoCommit = true;
				}
				else if (onOff == "OFF")
				{
					autoCommit = false;
				}
				else
				{
					throw new ArgumentException("Expected the ON or OFF but something else was found.");
				}
			}
			else
			{
				autoCommit = !autoCommit;
			}
		}

		/// <summary>
		/// Parses the isql statement SET NAMES and sets the character set to current connection string.
		/// </summary>
		/// <param name="setNamesStatement">The set names statement.</param>
		protected void SetNames(string setNamesStatement)
		{
			// SET NAMES charset
			StringParser parser = new StringParser(setNamesStatement, false);
			parser.Tokens = new[] { " ", "\r\n", "\n", "\r" };
			parser.ParseNext();
			if (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture) != "SET")
			{
				throw new ArgumentException("Malformed isql SET statement. Expected keyword SET but something else was found.");
			}
			parser.ParseNext(); // NAMES
			parser.ParseNext();
			this.connectionString.Charset = parser.Result;
		}

		/// <summary>
		/// Parses the isql statement SET SQL DIALECT and sets the dialect set to current connection string.
		/// </summary>
		/// <param name="setSqlDialectStatement">The set sql dialect statement.</param>
		protected void SetSqlDialect(string setSqlDialectStatement)
		{
			// SET SQL DIALECT dialect
			StringParser parser = new StringParser(setSqlDialectStatement, false);
			parser.Tokens = new[] { " ", "\r\n", "\n", "\r" };
			parser.ParseNext();
			if (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture) != "SET")
			{
				throw new ArgumentException("Malformed isql SET statement. Expected keyword SET but something else was found.");
			}
			parser.ParseNext(); // SQL
			parser.ParseNext(); // DIALECT
			parser.ParseNext();
			int dialect = 3;
			int.TryParse(parser.Result, out dialect);
			this.connectionString.Dialect = dialect;
		}

		protected FbCommand ProvideCommand()
		{
			if (this.sqlCommand == null)
			{
				this.sqlCommand = new FbCommand();
			}

			this.sqlCommand.Connection = this.ProvideConnection();

			return this.sqlCommand;
		}

		protected FbConnection ProvideConnection()
		{
			if (requiresNewConnection)
			{
				if ((this.sqlConnection != null) &&
					((this.sqlConnection.State != ConnectionState.Closed) ||
					(this.sqlConnection.State != ConnectionState.Broken)))
				{
					this.sqlConnection.Close();
				}
				this.sqlConnection = new FbConnection(this.connectionString.ToString());
			}

			if (this.sqlConnection.State == ConnectionState.Closed)
			{
				this.sqlConnection.Open();
			}

			return this.sqlConnection;
		}

		/// <summary>
		/// Executes a command and optionally commits the transaction.
		/// </summary>
		/// <param name="command">Command to execute.</param>
		/// <param name="autocommit">true to commit the transaction after execution; or false if not.</param>
		/// <returns>The number of rows affected by the query execution.</returns>
		protected int ExecuteCommand(bool autoCommit)
		{
			int rowsAffected = this.sqlCommand.ExecuteNonQuery();
			if (autoCommit && this.sqlCommand.IsDDLCommand)
			{
				this.CommitTransaction();
			}

			return rowsAffected;
		}

		protected void CommitTransaction()
		{
			if (this.sqlTransaction != null)
			{
				this.sqlTransaction.Commit();
				this.sqlTransaction = null;
			}
		}

		protected void RollbackTransaction()
		{
			if (this.sqlTransaction != null)
			{
				this.sqlTransaction.Rollback();
				this.sqlTransaction = null;
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// The trigger function for <see cref="CommandExecuting"/>	event.
		/// </summary>
		/// <param name="sqlCommand">The SQL command that is going for execution.</param>
		protected void OnCommandExecuting(FbCommand sqlCommand)
		{
			if (CommandExecuting != null)
			{
				CommandExecutingEventArgs e = new CommandExecutingEventArgs(sqlCommand);
				CommandExecuting(this, e);
			}
		}

		/// <summary>
		/// The trigger function for <see cref="CommandExecuted"/> event.
		/// </summary>
		/// <param name="commandText">The <see cref="FbCommand.CommandText"/> of the executed SQL command.</param>
		/// <param name="dataReader">The <see cref="FbDataReader"/> instance with the returned data. If the
		/// command executed is not meant to return data (ex: UPDATE, INSERT...) this parameter must be
		/// setled to <b>null</b>.</param>
		/// <param name="rowsAffected">The rows that were affected by the executed SQL command. If the executed
		/// command is not meant to return this kind of information (ex: SELECT) this parameter must
		/// be setled to <b>-1</b>.</param>
		protected void OnCommandExecuted(string commandText, FbDataReader dataReader, int rowsAffected)
		{
			if (CommandExecuted != null)
			{
				CommandExecutedEventArgs e = new CommandExecutedEventArgs(dataReader, commandText, rowsAffected);
				CommandExecuted(this, e);
			}
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Determines the <see cref="SqlStatementType"/> of the provided SQL statement.
		/// </summary>
		/// <param name="sqlStatement">The string containing the SQL statement.</param>
		/// <returns>The <see cref="SqlStatementType"/> of the <b>sqlStatement</b>.</returns>
		/// <remarks>If the type of <b>sqlStatement</b> could not be determined this
		/// method will throw an exception.</remarks>
		public static SqlStatementType GetStatementType(string sqlStatement)
		{
			char type = sqlStatement == null ? ' ' : sqlStatement.Trim().ToUpper(CultureInfo.CurrentUICulture)[0];

			switch (type)
			{
				case 'A':
					if (StringParser.StartsWith(sqlStatement, "ALTER DATABASE", true))
					{
						return SqlStatementType.AlterDatabase;
					}
					if (StringParser.StartsWith(sqlStatement, "ALTER DOMAIN", true))
					{
						return SqlStatementType.AlterDomain;
					}
					if (StringParser.StartsWith(sqlStatement, "ALTER EXCEPTION", true))
					{
						return SqlStatementType.AlterException;
					}
					if (StringParser.StartsWith(sqlStatement, "ALTER INDEX", true))
					{
						return SqlStatementType.AlterIndex;
					}
					if (StringParser.StartsWith(sqlStatement, "ALTER PROCEDURE", true))
					{
						return SqlStatementType.AlterProcedure;
					}
					if (StringParser.StartsWith(sqlStatement, "ALTER ROLE", true))
					{
						return SqlStatementType.AlterRole;
					}
					if (StringParser.StartsWith(sqlStatement, "ALTER SEQUENCE", true))
					{
						return SqlStatementType.AlterSequence;
					}
					if (StringParser.StartsWith(sqlStatement, "ALTER TABLE", true))
					{
						return SqlStatementType.AlterTable;
					}
					if (StringParser.StartsWith(sqlStatement, "ALTER TRIGGER", true))
					{
						return SqlStatementType.AlterTrigger;
					}
					if (StringParser.StartsWith(sqlStatement, "ALTER VIEW", true))
					{
						return SqlStatementType.AlterView;
					}
					break;

				case 'C':
					switch (char.ToUpper(sqlStatement[1], CultureInfo.CurrentUICulture))
					{
						case 'L':
							if (StringParser.StartsWith(sqlStatement, "CLOSE", true))
							{
								return SqlStatementType.Close;
							}
							break;

						case 'O':
							if (StringParser.StartsWith(sqlStatement, "COMMENT ON", true))
							{
								return SqlStatementType.CommentOn;
							}
							if (StringParser.StartsWith(sqlStatement, "COMMIT", true))
							{
								return SqlStatementType.Commit;
							}
							if (StringParser.StartsWith(sqlStatement, "CONNECT", true))
							{
								return SqlStatementType.Connect;
							}
							break;

						case 'R':
							if (StringParser.StartsWith(sqlStatement, "CREATE COLLATION", true))
							{
								return SqlStatementType.CreateCollation;
							}
							if (StringParser.StartsWith(sqlStatement, "CREATE DATABASE", true))
							{
								return SqlStatementType.CreateDatabase;
							}
							if (StringParser.StartsWith(sqlStatement, "CREATE DOMAIN", true))
							{
								return SqlStatementType.CreateDomain;
							}
							if (StringParser.StartsWith(sqlStatement, "CREATE EXCEPTION", true) ||
								StringParser.StartsWith(sqlStatement, "CREATE OR ALTER EXCEPTION", true))
							{
								return SqlStatementType.CreateException;
							}
							if (StringParser.StartsWith(sqlStatement, "CREATE GENERATOR", true))
							{
								return SqlStatementType.CreateGenerator;
							}
							if (StringParser.StartsWith(sqlStatement, "CREATE INDEX", true) ||
								StringParser.StartsWith(sqlStatement, "CREATE ASC INDEX", true) ||
								StringParser.StartsWith(sqlStatement, "CREATE ASCENDING INDEX", true) ||
								StringParser.StartsWith(sqlStatement, "CREATE DESC INDEX", true) ||
								StringParser.StartsWith(sqlStatement, "CREATE DESCENDING INDEX", true))
							{
								return SqlStatementType.CreateIndex;
							}
							if (StringParser.StartsWith(sqlStatement, "CREATE PROCEDURE", true) ||
								StringParser.StartsWith(sqlStatement, "CREATE OR ALTER PROCEDURE", true))
							{
								return SqlStatementType.CreateProcedure;
							}
							if (StringParser.StartsWith(sqlStatement, "CREATE ROLE", true))
							{
								return SqlStatementType.CreateRole;
							}
							if (StringParser.StartsWith(sqlStatement, "CREATE SEQUENCE", true))
							{
								return SqlStatementType.CreateSequence;
							}
							if (StringParser.StartsWith(sqlStatement, "CREATE SHADOW", true))
							{
								return SqlStatementType.CreateShadow;
							}
							if (StringParser.StartsWith(sqlStatement, "CREATE TABLE", true) ||
								StringParser.StartsWith(sqlStatement, "CREATE GLOBAL TEMPORARY TABLE", true))
							{
								return SqlStatementType.CreateTable;
							}
							if (StringParser.StartsWith(sqlStatement, "CREATE TRIGGER", true) ||
								StringParser.StartsWith(sqlStatement, "CREATE OR ALTER TRIGGER", true))
							{
								return SqlStatementType.CreateTrigger;
							}
							if (StringParser.StartsWith(sqlStatement, "CREATE UNIQUE INDEX", true) ||
								StringParser.StartsWith(sqlStatement, "CREATE UNIQUE ASC INDEX", true) ||
								StringParser.StartsWith(sqlStatement, "CREATE UNIQUE ASCENDING INDEX", true) ||
								StringParser.StartsWith(sqlStatement, "CREATE UNIQUE DESC INDEX", true) ||
								StringParser.StartsWith(sqlStatement, "CREATE UNIQUE DESCENDING INDEX", true))
							{
								return SqlStatementType.CreateIndex;
							}
							if (StringParser.StartsWith(sqlStatement, "CREATE VIEW", true) ||
								StringParser.StartsWith(sqlStatement, "CREATE OR ALTER VIEW", true))
							{
								return SqlStatementType.CreateView;
							}
							break;
					}
					break;

				case 'D':
					switch (char.ToUpper(sqlStatement[1], CultureInfo.CurrentUICulture))
					{
						case 'E':
							if (StringParser.StartsWith(sqlStatement, "DECLARE CURSOR", true))
							{
								return SqlStatementType.DeclareCursor;
							}
							if (StringParser.StartsWith(sqlStatement, "DECLARE EXTERNAL FUNCTION", true))
							{
								return SqlStatementType.DeclareExternalFunction;
							}
							if (StringParser.StartsWith(sqlStatement, "DECLARE FILTER", true))
							{
								return SqlStatementType.DeclareFilter;
							}
							if (StringParser.StartsWith(sqlStatement, "DECLARE STATEMENT", true))
							{
								return SqlStatementType.DeclareStatement;
							}
							if (StringParser.StartsWith(sqlStatement, "DECLARE TABLE", true))
							{
								return SqlStatementType.DeclareTable;
							}
							if (StringParser.StartsWith(sqlStatement, "DELETE", true))
							{
								return SqlStatementType.Delete;
							}
							if (StringParser.StartsWith(sqlStatement, "DESCRIBE", true))
							{
								return SqlStatementType.Describe;
							}
							break;

						case 'I':
							if (StringParser.StartsWith(sqlStatement, "DISCONNECT", true))
							{
								return SqlStatementType.Disconnect;
							}
							break;

						case 'R':
							if (StringParser.StartsWith(sqlStatement, "DROP COLLATION", true))
							{
								return SqlStatementType.DropCollation;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP DATABASE", true))
							{
								return SqlStatementType.DropDatabase;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP DOMAIN", true))
							{
								return SqlStatementType.DropDomain;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP EXCEPTION", true))
							{
								return SqlStatementType.DropException;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP EXTERNAL FUNCTION", true))
							{
								return SqlStatementType.DropExternalFunction;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP FILTER", true))
							{
								return SqlStatementType.DropFilter;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP GENERATOR", true))
							{
								return SqlStatementType.DropGenerator;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP INDEX", true))
							{
								return SqlStatementType.DropIndex;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP PROCEDURE", true))
							{
								return SqlStatementType.DropProcedure;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP SEQUENCE", true))
							{
								return SqlStatementType.DropSequence;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP ROLE", true))
							{
								return SqlStatementType.DropRole;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP SHADOW", true))
							{
								return SqlStatementType.DropShadow;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP TABLE", true))
							{
								return SqlStatementType.DropTable;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP TRIGGER", true))
							{
								return SqlStatementType.DropTrigger;
							}
							if (StringParser.StartsWith(sqlStatement, "DROP VIEW", true))
							{
								return SqlStatementType.DropView;
							}
							break;
					}
					break;

				case 'E':
					if (StringParser.StartsWith(sqlStatement, "EXECUTE BLOCK", true))
					{
						return SqlStatementType.ExecuteBlock;
					}
					if (StringParser.StartsWith(sqlStatement, "EXECUTE IMMEDIATE", true))
					{
						return SqlStatementType.ExecuteImmediate;
					}
					if (StringParser.StartsWith(sqlStatement, "EXECUTE PROCEDURE", true))
					{
						return SqlStatementType.ExecuteProcedure;
					}
					if (StringParser.StartsWith(sqlStatement, "EXECUTE", true))
					{
						return SqlStatementType.Execute;
					}
					if (StringParser.StartsWith(sqlStatement, "EVENT WAIT", true))
					{
						return SqlStatementType.EventWait;
					}
					if (StringParser.StartsWith(sqlStatement, "EVENT INIT", true))
					{
						return SqlStatementType.EventInit;
					}
					if (StringParser.StartsWith(sqlStatement, "END DECLARE SECTION", true))
					{
						return SqlStatementType.EndDeclareSection;
					}
					break;

				case 'F':
					if (StringParser.StartsWith(sqlStatement, "FETCH", true))
					{
						return SqlStatementType.Fetch;
					}
					break;

				case 'G':
					if (StringParser.StartsWith(sqlStatement, "GRANT", true))
					{
						return SqlStatementType.Grant;
					}
					break;

				case 'I':
					if (StringParser.StartsWith(sqlStatement, "INSERT CURSOR", true))
					{
						return SqlStatementType.InsertCursor;
					}
					if (StringParser.StartsWith(sqlStatement, "INSERT", true))
					{
						return SqlStatementType.Insert;
					}
					break;

				case 'O':
					if (StringParser.StartsWith(sqlStatement, "OPEN", true))
					{
						return SqlStatementType.Open;
					}
					break;

				case 'P':
					if (StringParser.StartsWith(sqlStatement, "PREPARE", true))
					{
						return SqlStatementType.Prepare;
					}
					break;

				case 'R':
					if (StringParser.StartsWith(sqlStatement, "REVOKE", true))
					{
						return SqlStatementType.Revoke;
					}
					if (StringParser.StartsWith(sqlStatement, "RECREATE PROCEDURE", true))
					{
						return SqlStatementType.RecreateProcedure;
					}
					if (StringParser.StartsWith(sqlStatement, "RECREATE TABLE", true) ||
						StringParser.StartsWith(sqlStatement, "RECREATE GLOBAL TEMPORARY TABLE", true))
					{
						return SqlStatementType.RecreateTable;
					}
					if (StringParser.StartsWith(sqlStatement, "RECREATE TRIGGER", true))
					{
						return SqlStatementType.RecreateTrigger;
					}
					if (StringParser.StartsWith(sqlStatement, "RECREATE VIEW", true))
					{
						return SqlStatementType.RecreateView;
					}
					if (StringParser.StartsWith(sqlStatement, "ROLLBACK", true))
					{
						return SqlStatementType.Rollback;
					}
					break;

				case 'S':
					if (StringParser.StartsWith(sqlStatement, "SELECT", true))
					{
						return SqlStatementType.Select;
					}
					if (StringParser.StartsWith(sqlStatement, "SET AUTODDL", true))
					{
						return SqlStatementType.SetAutoDDL;
					}
					if (StringParser.StartsWith(sqlStatement, "SET DATABASE", true))
					{
						return SqlStatementType.SetDatabase;
					}
					if (StringParser.StartsWith(sqlStatement, "SET GENERATOR", true))
					{
						return SqlStatementType.SetGenerator;
					}
					if (StringParser.StartsWith(sqlStatement, "SET NAMES", true))
					{
						return SqlStatementType.SetNames;
					}
					if (StringParser.StartsWith(sqlStatement, "SET SQL DIALECT", true))
					{
						return SqlStatementType.SetSQLDialect;
					}
					if (StringParser.StartsWith(sqlStatement, "SET STATISTICS", true))
					{
						return SqlStatementType.SetStatistics;
					}
					if (StringParser.StartsWith(sqlStatement, "SET TRANSACTION", true))
					{
						return SqlStatementType.SetTransaction;
					}
					if (StringParser.StartsWith(sqlStatement, "SHOW SQL DIALECT", true))
					{
						return SqlStatementType.ShowSQLDialect;
					}
					break;

				case 'U':
					if (StringParser.StartsWith(sqlStatement, "UPDATE", true))
					{
						return SqlStatementType.Update;
					}
					break;

				case 'W':
					if (StringParser.StartsWith(sqlStatement, "WHENEVER", true))
					{
						return SqlStatementType.Whenever;
					}
					break;
			}
			throw new ArgumentException(string.Format("The type of the SQL statement could not be determined.{0}Statement: {1}.",
				Environment.NewLine,
				sqlStatement));
		}

		#endregion
	}
}
