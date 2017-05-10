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
 *  Copyright (c) 2015 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 *
 * Contributors:
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
		/// <summary>
		/// The event trigged before a SQL statement goes for execution.
		/// </summary>
		public event EventHandler<CommandExecutingEventArgs> CommandExecuting;

		/// <summary>
		/// The event trigged after a SQL statement execution.
		/// </summary>
		public event EventHandler<CommandExecutedEventArgs> CommandExecuted;

		FbStatementCollection _statements;
		FbConnection _sqlConnection;
		FbTransaction _sqlTransaction;
		FbConnectionStringBuilder _connectionString;
		FbCommand _sqlCommand;

		// control fields
		bool _requiresNewConnection;
		bool _shouldClose;

		/// <summary>
		/// Represents the list of SQL statements for batch execution.
		/// </summary>
		public FbStatementCollection Statements
		{
			get { return _statements; }
		}

		/// <summary>
		/// Creates an instance of FbBatchExecution engine with the given
		/// connection.
		/// </summary>
		/// <param name="sqlConnection">A <see cref="FbConnection"/> object.</param>
		public FbBatchExecution(FbConnection sqlConnection = null)
		{
			_statements = new FbStatementCollection();
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
		/// Appends SQL statements from <see cref="FbScript"/> instance. <see cref="FbScript.Parse"/> should be already called.
		/// </summary>
		/// <param name="isqlScript">A <see cref="FbScript"/> object.</param>
		public void AppendSqlStatements(FbScript isqlScript)
		{
			_statements.AddRange(isqlScript.Results);
		}

		/// <summary>
		/// Starts the ordered execution of the SQL statements that are in <see cref="SqlStatements"/> collection.
		/// </summary>
		/// <param name="autoCommit">Specifies if the transaction should be committed after a DDL command execution</param>
		public void Execute(bool autoCommit = true)
		{
			if ((_statements?.Count ?? 0) == 0)
			{
				throw new InvalidOperationException("There are no commands for execution.");
			}

			_shouldClose = false;

			foreach (var statement in Statements)
			{
				if (!(statement.StatementType == SqlStatementType.Connect ||
					statement.StatementType == SqlStatementType.CreateDatabase ||
					statement.StatementType == SqlStatementType.Disconnect ||
					statement.StatementType == SqlStatementType.DropDatabase ||
					statement.StatementType == SqlStatementType.SetAutoDDL ||
					statement.StatementType == SqlStatementType.SetDatabase ||
					statement.StatementType == SqlStatementType.SetNames ||
					statement.StatementType == SqlStatementType.SetSQLDialect))
				{
					ProvideCommand();
					_sqlCommand.CommandText = statement.Text;
					if (_sqlTransaction == null && !(statement.StatementType == SqlStatementType.Commit || statement.StatementType == SqlStatementType.Rollback))
					{
						_sqlTransaction = _sqlConnection.BeginTransaction();
					}
					_sqlCommand.Transaction = _sqlTransaction;
				}

				try
				{
					switch (statement.StatementType)
					{
						case SqlStatementType.AlterCharacterSet:
						case SqlStatementType.AlterDatabase:
						case SqlStatementType.AlterDomain:
						case SqlStatementType.AlterException:
						case SqlStatementType.AlterFunction:
						case SqlStatementType.AlterIndex:
						case SqlStatementType.AlterProcedure:
						case SqlStatementType.AlterRole:
						case SqlStatementType.AlterSequence:
						case SqlStatementType.AlterTable:
						case SqlStatementType.AlterTrigger:
						case SqlStatementType.AlterView:
						case SqlStatementType.CommentOn:
						case SqlStatementType.CreateCollation:
						case SqlStatementType.CreateDomain:
						case SqlStatementType.CreateException:
						case SqlStatementType.CreateFunction:
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
						case SqlStatementType.DropCollation:
						case SqlStatementType.DropDomain:
						case SqlStatementType.DropException:
						case SqlStatementType.DropExternalFunction:
						case SqlStatementType.DropFunction:
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
						case SqlStatementType.Grant:
						case SqlStatementType.Insert:
						case SqlStatementType.InsertCursor:
						case SqlStatementType.Open:
						case SqlStatementType.Prepare:
						case SqlStatementType.Revoke:
						case SqlStatementType.RecreateFunction:
						case SqlStatementType.RecreateProcedure:
						case SqlStatementType.RecreateTable:
						case SqlStatementType.RecreateTrigger:
						case SqlStatementType.RecreateView:
						case SqlStatementType.SetGenerator:
						case SqlStatementType.Update:
						case SqlStatementType.Whenever:
							OnCommandExecuting(_sqlCommand, statement.StatementType);

							var rowsAffected = ExecuteCommand(autoCommit);
							_requiresNewConnection = false;

							OnCommandExecuted(null, statement.Text, statement.StatementType, rowsAffected);
							break;

						case SqlStatementType.ExecuteBlock:
						case SqlStatementType.Select:
#warning Who's disposing this?
							ProvideCommand().CommandText = statement.Text;

							OnCommandExecuting(_sqlCommand, statement.StatementType);

							using (var dataReader = _sqlCommand.ExecuteReader())
							{
								_requiresNewConnection = false;

								OnCommandExecuted(dataReader, statement.Text, statement.StatementType, -1);
							}
							break;

						case SqlStatementType.Commit:
							OnCommandExecuting(null, statement.StatementType);

							CommitTransaction();

							OnCommandExecuted(null, statement.Text, statement.StatementType, -1);
							break;

						case SqlStatementType.Rollback:
							OnCommandExecuting(null, statement.StatementType);

							RollbackTransaction();

							OnCommandExecuted(null, statement.Text, statement.StatementType, -1);
							break;

						case SqlStatementType.CreateDatabase:
							OnCommandExecuting(null, statement.StatementType);

							CreateDatabase(statement.CleanText);
							_requiresNewConnection = false;

							OnCommandExecuted(null, statement.Text, statement.StatementType, -1);
							break;

						case SqlStatementType.DropDatabase:
							OnCommandExecuting(null, statement.StatementType);

							FbConnection.DropDatabase(_connectionString.ToString());
							_requiresNewConnection = true;

							OnCommandExecuted(null, statement.Text, statement.StatementType, -1);
							break;

						case SqlStatementType.Connect:
							OnCommandExecuting(null, statement.StatementType);

							ConnectToDatabase(statement.CleanText);
							_requiresNewConnection = false;

							OnCommandExecuted(null, statement.Text, statement.StatementType, -1);
							break;

						case SqlStatementType.Disconnect:
							OnCommandExecuting(null, statement.StatementType);

							_sqlConnection.Close();
							FbConnection.ClearPool(_sqlConnection);
							_requiresNewConnection = false;

							OnCommandExecuted(null, statement.Text, statement.StatementType, -1);
							break;

						case SqlStatementType.SetAutoDDL:
							OnCommandExecuting(null, statement.StatementType);

							SetAutoDdl(statement.CleanText, ref autoCommit);
							_requiresNewConnection = false;

							OnCommandExecuted(null, statement.Text, statement.StatementType, -1);
							break;

						case SqlStatementType.SetNames:
							OnCommandExecuting(null, statement.StatementType);

							SetNames(statement.CleanText);
							_requiresNewConnection = true;

							OnCommandExecuted(null, statement.Text, statement.StatementType, -1);
							break;

						case SqlStatementType.SetSQLDialect:
							OnCommandExecuting(null, statement.StatementType);

							SetSqlDialect(statement.CleanText);
							_requiresNewConnection = true;

							OnCommandExecuted(null, statement.Text, statement.StatementType, -1);
							break;

						case SqlStatementType.Fetch:
						case SqlStatementType.Describe:
							break;

						case SqlStatementType.SetDatabase:
						case SqlStatementType.SetStatistics:
						case SqlStatementType.SetTransaction:
						case SqlStatementType.ShowSQLDialect:
							throw new NotImplementedException();
					}
				}
				catch (Exception ex)
				{
					RollbackTransaction();
					CloseConnection();

					throw new FbException(string.Format("An exception was thrown when executing command: {1}.{0}Batch execution aborted.{0}The returned message was: {2}.",
							Environment.NewLine,
							statement.Text,
							ex.Message),
						ex);
				}
			}

			CommitTransaction();
			CloseConnection();
		}

		/// <summary>
		/// Updates the connection string with the data parsed from the parameter and opens a connection
		/// to the database.
		/// </summary>
		/// <param name="connectDbStatement"></param>
		protected void ConnectToDatabase(string connectDbStatement)
		{
			// CONNECT 'filespec'
			// [USER 'username']
			// [PASSWORD 'password']
			// [CACHE int]
			// [ROLE 'rolename']
			SqlStringParser parser = new SqlStringParser(connectDbStatement);
			parser.Tokens = new[] { " ", "\r\n", "\n", "\r" };
			using (var enumerator = parser.Parse().GetEnumerator())
			{
				enumerator.MoveNext();
				if (enumerator.Current.Text.ToUpperInvariant() != "CONNECT")
				{
					throw new ArgumentException("Malformed isql CONNECT statement. Expected keyword CONNECT but something else was found.");
				}
				enumerator.MoveNext();
				_connectionString.Database = enumerator.Current.Text.Replace("'", string.Empty);
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current.Text.ToUpperInvariant())
					{
						case "USER":
							enumerator.MoveNext();
							_connectionString.UserID = enumerator.Current.Text.Replace("'", string.Empty);
							break;

						case "PASSWORD":
							enumerator.MoveNext();
							_connectionString.Password = enumerator.Current.Text.Replace("'", string.Empty);
							break;

						case "CACHE":
							enumerator.MoveNext();
							break;

						case "ROLE":
							enumerator.MoveNext();
							_connectionString.Role = enumerator.Current.Text.Replace("'", string.Empty);
							break;

						default:
							throw new ArgumentException("Unexpected token '" + enumerator.Current.Text + "' on isql CONNECT statement.");

					}
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
			SqlStringParser parser = new SqlStringParser(createDatabaseStatement);
			parser.Tokens = new[] { " ", "\r\n", "\n", "\r" };
			using (var enumerator = parser.Parse().GetEnumerator())
			{
				enumerator.MoveNext();
				if (enumerator.Current.Text.ToUpperInvariant() != "CREATE")
				{
					throw new ArgumentException("Malformed isql CREATE statement. Expected keyword CREATE but something else was found.");
				}
				enumerator.MoveNext(); // {DATABASE | SCHEMA}
				enumerator.MoveNext();
				_connectionString.Database = enumerator.Current.Text.Replace("'", string.Empty);
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current.Text.ToUpperInvariant())
					{
						case "USER":
							enumerator.MoveNext();
							_connectionString.UserID = enumerator.Current.Text.Replace("'", string.Empty);
							break;

						case "PASSWORD":
							enumerator.MoveNext();
							_connectionString.Password = enumerator.Current.Text.Replace("'", string.Empty);
							break;

						case "PAGE_SIZE":
							enumerator.MoveNext();
							if (enumerator.Current.Text == "=")
								enumerator.MoveNext();
							int.TryParse(enumerator.Current.Text, out pageSize);
							break;

						case "DEFAULT":
							enumerator.MoveNext();
							if (enumerator.Current.Text.ToUpperInvariant() != "CHARACTER")
								throw new ArgumentException("Expected the keyword CHARACTER but something else was found.");

							enumerator.MoveNext();
							if (enumerator.Current.Text.ToUpperInvariant() != "SET")
								throw new ArgumentException("Expected the keyword SET but something else was found.");

							enumerator.MoveNext();
							_connectionString.Charset = enumerator.Current.Text;
							break;
					}
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
			SqlStringParser parser = new SqlStringParser(setAutoDdlStatement);
			parser.Tokens = new[] { " ", "\r\n", "\n", "\r" };
			using (var enumerator = parser.Parse().GetEnumerator())
			{
				enumerator.MoveNext();
				if (enumerator.Current.Text.ToUpperInvariant() != "SET")
				{
					throw new ArgumentException("Malformed isql SET statement. Expected keyword SET but something else was found.");
				}
				enumerator.MoveNext(); // AUTO
				if (enumerator.MoveNext())
				{
					string onOff = enumerator.Current.Text.ToUpperInvariant();
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
		}

		/// <summary>
		/// Parses the isql statement SET NAMES and sets the character set to current connection string.
		/// </summary>
		/// <param name="setNamesStatement">The set names statement.</param>
		protected void SetNames(string setNamesStatement)
		{
			// SET NAMES charset
			SqlStringParser parser = new SqlStringParser(setNamesStatement);
			parser.Tokens = new[] { " ", "\r\n", "\n", "\r" };
			using (var enumerator = parser.Parse().GetEnumerator())
			{
				enumerator.MoveNext();
				if (enumerator.Current.Text.ToUpperInvariant() != "SET")
				{
					throw new ArgumentException("Malformed isql SET statement. Expected keyword SET but something else was found.");
				}
				enumerator.MoveNext(); // NAMES
				enumerator.MoveNext();
				_connectionString.Charset = enumerator.Current.Text;
			}
		}

		/// <summary>
		/// Parses the isql statement SET SQL DIALECT and sets the dialect set to current connection string.
		/// </summary>
		/// <param name="setSqlDialectStatement">The set sql dialect statement.</param>
		protected void SetSqlDialect(string setSqlDialectStatement)
		{
			// SET SQL DIALECT dialect
			SqlStringParser parser = new SqlStringParser(setSqlDialectStatement);
			parser.Tokens = new[] { " ", "\r\n", "\n", "\r" };
			using (var enumerator = parser.Parse().GetEnumerator())
			{
				enumerator.MoveNext();
				if (enumerator.Current.Text.ToUpperInvariant() != "SET")
				{
					throw new ArgumentException("Malformed isql SET statement. Expected keyword SET but something else was found.");
				}
				enumerator.MoveNext(); // SQL
				enumerator.MoveNext(); // DIALECT
				enumerator.MoveNext();
				int.TryParse(enumerator.Current.Text, out var dialect);
				_connectionString.Dialect = dialect;
			}
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
				if (_sqlConnection != null && _sqlConnection.State != ConnectionState.Closed)
				{
					CloseConnection();
				}
				_sqlConnection = new FbConnection(_connectionString.ToString());
			}

			if (_sqlConnection.State == ConnectionState.Closed)
			{
				_sqlConnection.Open();
				_shouldClose = true;
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

		protected void CloseConnection()
		{
			if (_shouldClose)
			{
				_sqlConnection.Close();
			}
		}

		/// <summary>
		/// The trigger function for <see cref="CommandExecuting"/>	event.
		/// </summary>
		/// <param name="sqlCommand">The SQL command that is going for execution.</param>
		protected void OnCommandExecuting(FbCommand sqlCommand, SqlStatementType statementType)
		{
			CommandExecuting?.Invoke(this, new CommandExecutingEventArgs(sqlCommand, statementType));
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
		protected void OnCommandExecuted(FbDataReader dataReader, string commandText, SqlStatementType statementType, int rowsAffected)
		{
			CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(dataReader, commandText, statementType, rowsAffected));
		}
	}
}
