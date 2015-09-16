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
using System.Linq;
using System.Globalization;

using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Isql
{
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

		private FbStatementCollection _sqlStatements;
		private FbConnection _sqlConnection;
		private FbTransaction _sqlTransaction;
		private FbConnectionStringBuilder _connectionString;
		private FbCommand _sqlCommand;

		// control fields
		private bool _requiresNewConnection;

		#endregion

		#region Properties

		/// <summary>
		/// Represents the list of SQL statements for batch execution.
		/// </summary>
		public FbStatementCollection SqlStatements
		{
			get { return _sqlStatements; }
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
			_sqlStatements = new FbStatementCollection();
			if (sqlConnection == null)
			{
				_sqlConnection = new FbConnection(); // do not specify the connection string
				_connectionString = new FbConnectionStringBuilder();
			}
			else
			{
				_sqlConnection = sqlConnection;
				_connectionString = new FbConnectionStringBuilder(sqlConnection.ConnectionString);
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
			_sqlStatements.AddRange(isqlScript.Results);
		}

		/// <summary>
		/// Starts the ordered execution of the SQL statements that are in <see cref="SqlStatements"/> collection.
		/// </summary>
		/// <param name="autoCommit">Specifies if the transaction should be committed after a DDL command execution</param>
		public void Execute(bool autoCommit = true)
		{
			if (SqlStatements == null || SqlStatements.Count == 0)
			{
				throw new InvalidOperationException("There are no commands for execution.");
			}

			foreach (string sqlStatement in SqlStatements.Where(x => !string.IsNullOrEmpty(x)))
			{
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
					ProvideCommand();
					_sqlCommand.CommandText = sqlStatement;
					if (_sqlTransaction == null && !(statementType == SqlStatementType.Commit || statementType == SqlStatementType.Rollback))
					{
						_sqlTransaction = _sqlConnection.BeginTransaction();
					}
					_sqlCommand.Transaction = _sqlTransaction;
				}

				try
				{
					switch (statementType)
					{
						case SqlStatementType.AlterCharacterSet:
						case SqlStatementType.AlterDatabase:
						case SqlStatementType.AlterDomain:
						case SqlStatementType.AlterException:
						case SqlStatementType.AlterIndex:
						case SqlStatementType.AlterProcedure:
						case SqlStatementType.AlterRole:
						case SqlStatementType.AlterTable:
						case SqlStatementType.AlterTrigger:
						case SqlStatementType.AlterView:
							OnCommandExecuting(_sqlCommand);

							rowsAffected = ExecuteCommand(autoCommit);
							_requiresNewConnection = false;

							OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.Commit:
							OnCommandExecuting(null);

							CommitTransaction();

							OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.Connect:
							OnCommandExecuting(null);

							ConnectToDatabase(sqlStatement);

							_requiresNewConnection = false;

							OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.CreateDatabase:
							OnCommandExecuting(null);

							CreateDatabase(sqlStatement);
							_requiresNewConnection = false;

							OnCommandExecuted(sqlStatement, null, -1);
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
							OnCommandExecuting(_sqlCommand);

							rowsAffected = ExecuteCommand(autoCommit);
							_requiresNewConnection = false;

							OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.Describe:
							break;

						case SqlStatementType.Disconnect:
							OnCommandExecuting(null);

							_sqlConnection.Close();
							FbConnection.ClearPool(_sqlConnection);
							_requiresNewConnection = false;

							OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.DropDatabase:
							OnCommandExecuting(null);

							FbConnection.DropDatabase(_connectionString.ToString());
							_requiresNewConnection = true;

							OnCommandExecuted(sqlStatement, null, -1);
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
							ProvideCommand().CommandText = sqlStatement;

							OnCommandExecuting(_sqlCommand);

							rowsAffected = ExecuteCommand(autoCommit);
							_requiresNewConnection = false;

							OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.ExecuteBlock:
							ProvideCommand().CommandText = sqlStatement;

							OnCommandExecuting(_sqlCommand);

							dataReader = _sqlCommand.ExecuteReader();
							_requiresNewConnection = false;

							OnCommandExecuted(sqlStatement, dataReader, -1);
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
							OnCommandExecuting(_sqlCommand);

							rowsAffected = ExecuteCommand(autoCommit);
							_requiresNewConnection = false;

							OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.RecreateProcedure:
						case SqlStatementType.RecreateTable:
						case SqlStatementType.RecreateTrigger:
						case SqlStatementType.RecreateView:
							OnCommandExecuting(_sqlCommand);

							rowsAffected = ExecuteCommand(autoCommit);
							_requiresNewConnection = false;

							OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.Rollback:
							OnCommandExecuting(null);

							RollbackTransaction();

							OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.Select:
							ProvideCommand().CommandText = sqlStatement;

							OnCommandExecuting(_sqlCommand);

							dataReader = _sqlCommand.ExecuteReader();
							_requiresNewConnection = false;

							OnCommandExecuted(sqlStatement, dataReader, -1);
							if (!dataReader.IsClosed)
							{
								dataReader.Close();
							}
							break;

						case SqlStatementType.SetAutoDDL:
							OnCommandExecuting(null);

							SetAutoDdl(sqlStatement, ref autoCommit);
							_requiresNewConnection = false;

							OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.SetGenerator:
						case SqlStatementType.AlterSequence:
							OnCommandExecuting(_sqlCommand);

							rowsAffected = ExecuteCommand(autoCommit);
							_requiresNewConnection = false;

							OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.SetNames:
							OnCommandExecuting(null);

							SetNames(sqlStatement);
							_requiresNewConnection = true;

							OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.SetSQLDialect:
							OnCommandExecuting(null);

							SetSqlDialect(sqlStatement);
							_requiresNewConnection = true;

							OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.SetDatabase:
						case SqlStatementType.SetStatistics:
						case SqlStatementType.SetTransaction:
						case SqlStatementType.ShowSQLDialect:
							throw new NotImplementedException();

						case SqlStatementType.Update:
						case SqlStatementType.Whenever:
							OnCommandExecuting(_sqlCommand);

							rowsAffected = ExecuteCommand(autoCommit);
							_requiresNewConnection = false;

							OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;
					}
				}
				catch (Exception ex)
				{
					RollbackTransaction();

					throw new FbException(string.Format("An exception was thrown when executing command: {1}.{0}Batch execution aborted.{0}The returned message was: {2}.",
						Environment.NewLine,
						sqlStatement,
						ex.Message));
				}
			}

			CommitTransaction();

			_sqlConnection.Close();
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
			StringParser parser = new StringParser(connectDbStatement);
			parser.Tokens = new[] { " ", "\r\n", "\n", "\r" };
			parser.ParseNext();
			if (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture) != "CONNECT")
			{
				throw new ArgumentException("Malformed isql CONNECT statement. Expected keyword CONNECT but something else was found.");
			}
			parser.ParseNext();
			_connectionString.Database = parser.Result.Replace("'", string.Empty);
			while (parser.ParseNext() != -1)
			{
				switch (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture))
				{
					case "USER":
						parser.ParseNext();
						_connectionString.UserID = parser.Result.Replace("'", string.Empty);
						break;

					case "PASSWORD":
						parser.ParseNext();
						_connectionString.Password = parser.Result.Replace("'", string.Empty);
						break;

					case "CACHE":
						parser.ParseNext();
						break;

					case "ROLE":
						parser.ParseNext();
						_connectionString.Role = parser.Result.Replace("'", string.Empty);
						break;

					default:
						throw new ArgumentException("Unexpected token '" + parser.Result.Trim() + "' on isql CONNECT statement.");

				}
			}
			_requiresNewConnection = true;
			ProvideConnection();
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
			StringParser parser = new StringParser(createDatabaseStatement);
			parser.Tokens = new[] { " ", "\r\n", "\n", "\r" };
			parser.ParseNext();
			if (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture) != "CREATE")
			{
				throw new ArgumentException("Malformed isql CREATE statement. Expected keyword CREATE but something else was found.");
			}
			parser.ParseNext(); // {DATABASE | SCHEMA}
			parser.ParseNext();
			_connectionString.Database = parser.Result.Replace("'", string.Empty);
			while (parser.ParseNext() != -1)
			{
				switch (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture))
				{
					case "USER":
						parser.ParseNext();
						_connectionString.UserID = parser.Result.Replace("'", string.Empty);
						break;

					case "PASSWORD":
						parser.ParseNext();
						_connectionString.Password = parser.Result.Replace("'", string.Empty);
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
						_connectionString.Charset = parser.Result;
						break;
				}
			}
			FbConnection.CreateDatabase(_connectionString.ToString(), pageSize, true, false);
			_requiresNewConnection = true;
			ProvideConnection();
		}

		/// <summary>
		/// Parses the isql statement SET AUTODDL and sets the character set to current connection string.
		/// </summary>
		/// <param name="setAutoDdlStatement">The set names statement.</param>
		protected void SetAutoDdl(string setAutoDdlStatement, ref bool autoCommit)
		{
			// SET AUTODDL [ON | OFF]
			StringParser parser = new StringParser(setAutoDdlStatement);
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
			StringParser parser = new StringParser(setNamesStatement);
			parser.Tokens = new[] { " ", "\r\n", "\n", "\r" };
			parser.ParseNext();
			if (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture) != "SET")
			{
				throw new ArgumentException("Malformed isql SET statement. Expected keyword SET but something else was found.");
			}
			parser.ParseNext(); // NAMES
			parser.ParseNext();
			_connectionString.Charset = parser.Result;
		}

		/// <summary>
		/// Parses the isql statement SET SQL DIALECT and sets the dialect set to current connection string.
		/// </summary>
		/// <param name="setSqlDialectStatement">The set sql dialect statement.</param>
		protected void SetSqlDialect(string setSqlDialectStatement)
		{
			// SET SQL DIALECT dialect
			StringParser parser = new StringParser(setSqlDialectStatement);
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
			_connectionString.Dialect = dialect;
		}

		protected FbCommand ProvideCommand()
		{
			if (_sqlCommand == null)
			{
				_sqlCommand = new FbCommand();
			}

			_sqlCommand.Connection = ProvideConnection();

			return _sqlCommand;
		}

		protected FbConnection ProvideConnection()
		{
			if (_requiresNewConnection)
			{
				if ((_sqlConnection != null) &&
					((_sqlConnection.State != ConnectionState.Closed) ||
					(_sqlConnection.State != ConnectionState.Broken)))
				{
					_sqlConnection.Close();
				}
				_sqlConnection = new FbConnection(_connectionString.ToString());
			}

			if (_sqlConnection.State == ConnectionState.Closed)
			{
				_sqlConnection.Open();
			}

			return _sqlConnection;
		}

		/// <summary>
		/// Executes a command and optionally commits the transaction.
		/// </summary>
		/// <param name="command">Command to execute.</param>
		/// <param name="autocommit">true to commit the transaction after execution; or false if not.</param>
		/// <returns>The number of rows affected by the query execution.</returns>
		protected int ExecuteCommand(bool autoCommit)
		{
			int rowsAffected = _sqlCommand.ExecuteNonQuery();
			if (autoCommit && _sqlCommand.IsDDLCommand)
			{
				CommitTransaction();
			}

			return rowsAffected;
		}

		protected void CommitTransaction()
		{
			if (_sqlTransaction != null)
			{
				_sqlTransaction.Commit();
				_sqlTransaction = null;
			}
		}

		protected void RollbackTransaction()
		{
			if (_sqlTransaction != null)
			{
				_sqlTransaction.Rollback();
				_sqlTransaction = null;
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
			CommandExecuting?.Invoke(this, new CommandExecutingEventArgs(sqlCommand));
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
			CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(dataReader, commandText, rowsAffected));
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
			sqlStatement = sqlStatement.TrimStart();
			switch (sqlStatement.FirstOrDefault())
			{
				case 'A':
				case 'a':
					if (sqlStatement.StartsWith("ALTER CHARACTER SET", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.AlterCharacterSet;
					}
					if (sqlStatement.StartsWith("ALTER DATABASE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.AlterDatabase;
					}
					if (sqlStatement.StartsWith("ALTER DOMAIN", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.AlterDomain;
					}
					if (sqlStatement.StartsWith("ALTER EXCEPTION", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.AlterException;
					}
					if (sqlStatement.StartsWith("ALTER INDEX", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.AlterIndex;
					}
					if (sqlStatement.StartsWith("ALTER PROCEDURE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.AlterProcedure;
					}
					if (sqlStatement.StartsWith("ALTER ROLE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.AlterRole;
					}
					if (sqlStatement.StartsWith("ALTER SEQUENCE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.AlterSequence;
					}
					if (sqlStatement.StartsWith("ALTER TABLE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.AlterTable;
					}
					if (sqlStatement.StartsWith("ALTER TRIGGER", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.AlterTrigger;
					}
					if (sqlStatement.StartsWith("ALTER VIEW", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.AlterView;
					}
					break;

				case 'C':
				case 'c':
					if (sqlStatement.StartsWith("CLOSE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Close;
					}
					if (sqlStatement.StartsWith("COMMENT ON", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CommentOn;
					}
					if (sqlStatement.StartsWith("COMMIT", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Commit;
					}
					if (sqlStatement.StartsWith("CONNECT", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Connect;
					}
					if (sqlStatement.StartsWith("CREATE COLLATION", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateCollation;
					}
					if (sqlStatement.StartsWith("CREATE DATABASE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateDatabase;
					}
					if (sqlStatement.StartsWith("CREATE DOMAIN", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateDomain;
					}
					if (sqlStatement.StartsWith("CREATE EXCEPTION", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("CREATE OR ALTER EXCEPTION", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateException;
					}
					if (sqlStatement.StartsWith("CREATE GENERATOR", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateGenerator;
					}
					if (sqlStatement.StartsWith("CREATE INDEX", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("CREATE ASC INDEX", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("CREATE ASCENDING INDEX", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("CREATE DESC INDEX", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("CREATE DESCENDING INDEX", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateIndex;
					}
					if (sqlStatement.StartsWith("CREATE PROCEDURE", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("CREATE OR ALTER PROCEDURE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateProcedure;
					}
					if (sqlStatement.StartsWith("CREATE ROLE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateRole;
					}
					if (sqlStatement.StartsWith("CREATE SEQUENCE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateSequence;
					}
					if (sqlStatement.StartsWith("CREATE SHADOW", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateShadow;
					}
					if (sqlStatement.StartsWith("CREATE TABLE", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("CREATE GLOBAL TEMPORARY TABLE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateTable;
					}
					if (sqlStatement.StartsWith("CREATE TRIGGER", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("CREATE OR ALTER TRIGGER", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateTrigger;
					}
					if (sqlStatement.StartsWith("CREATE UNIQUE INDEX", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("CREATE UNIQUE ASC INDEX", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("CREATE UNIQUE ASCENDING INDEX", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("CREATE UNIQUE DESC INDEX", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("CREATE UNIQUE DESCENDING INDEX", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateIndex;
					}
					if (sqlStatement.StartsWith("CREATE VIEW", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("CREATE OR ALTER VIEW", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.CreateView;
					}
					break;

				case 'D':
				case 'd':
					if (sqlStatement.StartsWith("DECLARE CURSOR", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DeclareCursor;
					}
					if (sqlStatement.StartsWith("DECLARE EXTERNAL FUNCTION", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DeclareExternalFunction;
					}
					if (sqlStatement.StartsWith("DECLARE FILTER", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DeclareFilter;
					}
					if (sqlStatement.StartsWith("DECLARE STATEMENT", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DeclareStatement;
					}
					if (sqlStatement.StartsWith("DECLARE TABLE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DeclareTable;
					}
					if (sqlStatement.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Delete;
					}
					if (sqlStatement.StartsWith("DESCRIBE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Describe;
					}
					if (sqlStatement.StartsWith("DISCONNECT", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Disconnect;
					}
					if (sqlStatement.StartsWith("DROP COLLATION", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropCollation;
					}
					if (sqlStatement.StartsWith("DROP DATABASE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropDatabase;
					}
					if (sqlStatement.StartsWith("DROP DOMAIN", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropDomain;
					}
					if (sqlStatement.StartsWith("DROP EXCEPTION", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropException;
					}
					if (sqlStatement.StartsWith("DROP EXTERNAL FUNCTION", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropExternalFunction;
					}
					if (sqlStatement.StartsWith("DROP FILTER", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropFilter;
					}
					if (sqlStatement.StartsWith("DROP GENERATOR", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropGenerator;
					}
					if (sqlStatement.StartsWith("DROP INDEX", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropIndex;
					}
					if (sqlStatement.StartsWith("DROP PROCEDURE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropProcedure;
					}
					if (sqlStatement.StartsWith("DROP SEQUENCE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropSequence;
					}
					if (sqlStatement.StartsWith("DROP ROLE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropRole;
					}
					if (sqlStatement.StartsWith("DROP SHADOW", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropShadow;
					}
					if (sqlStatement.StartsWith("DROP TABLE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropTable;
					}
					if (sqlStatement.StartsWith("DROP TRIGGER", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropTrigger;
					}
					if (sqlStatement.StartsWith("DROP VIEW", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.DropView;
					}
					break;

				case 'E':
				case 'e':
					if (sqlStatement.StartsWith("EXECUTE BLOCK", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.ExecuteBlock;
					}
					if (sqlStatement.StartsWith("EXECUTE IMMEDIATE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.ExecuteImmediate;
					}
					if (sqlStatement.StartsWith("EXECUTE PROCEDURE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.ExecuteProcedure;
					}
					if (sqlStatement.StartsWith("EXECUTE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Execute;
					}
					if (sqlStatement.StartsWith("EVENT WAIT", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.EventWait;
					}
					if (sqlStatement.StartsWith("EVENT INIT", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.EventInit;
					}
					if (sqlStatement.StartsWith("END DECLARE SECTION", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.EndDeclareSection;
					}
					break;

				case 'F':
				case 'f':
					if (sqlStatement.StartsWith("FETCH", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Fetch;
					}
					break;

				case 'G':
				case 'g':
					if (sqlStatement.StartsWith("GRANT", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Grant;
					}
					break;

				case 'I':
				case 'i':
					if (sqlStatement.StartsWith("INSERT CURSOR", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.InsertCursor;
					}
					if (sqlStatement.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Insert;
					}
					break;

				case 'O':
				case 'o':
					if (sqlStatement.StartsWith("OPEN", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Open;
					}
					break;

				case 'P':
				case 'p':
					if (sqlStatement.StartsWith("PREPARE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Prepare;
					}
					break;

				case 'R':
				case 'r':
					if (sqlStatement.StartsWith("REVOKE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Revoke;
					}
					if (sqlStatement.StartsWith("RECREATE PROCEDURE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.RecreateProcedure;
					}
					if (sqlStatement.StartsWith("RECREATE TABLE", StringComparison.OrdinalIgnoreCase) ||
						sqlStatement.StartsWith("RECREATE GLOBAL TEMPORARY TABLE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.RecreateTable;
					}
					if (sqlStatement.StartsWith("RECREATE TRIGGER", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.RecreateTrigger;
					}
					if (sqlStatement.StartsWith("RECREATE VIEW", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.RecreateView;
					}
					if (sqlStatement.StartsWith("ROLLBACK", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Rollback;
					}
					break;

				case 'S':
				case 's':
					if (sqlStatement.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Select;
					}
					if (sqlStatement.StartsWith("SET AUTODDL", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.SetAutoDDL;
					}
					if (sqlStatement.StartsWith("SET DATABASE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.SetDatabase;
					}
					if (sqlStatement.StartsWith("SET GENERATOR", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.SetGenerator;
					}
					if (sqlStatement.StartsWith("SET NAMES", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.SetNames;
					}
					if (sqlStatement.StartsWith("SET SQL DIALECT", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.SetSQLDialect;
					}
					if (sqlStatement.StartsWith("SET STATISTICS", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.SetStatistics;
					}
					if (sqlStatement.StartsWith("SET TRANSACTION", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.SetTransaction;
					}
					if (sqlStatement.StartsWith("SHOW SQL DIALECT", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.ShowSQLDialect;
					}
					break;

				case 'U':
				case 'u':
					if (sqlStatement.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
					{
						return SqlStatementType.Update;
					}
					break;

				case 'W':
				case 'w':
					if (sqlStatement.StartsWith("WHENEVER", StringComparison.OrdinalIgnoreCase))
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
