/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Abel Eduardo Pereira, Jiri Cincura (jiri@cincura.net)

using System;
using System.IO;
using System.Linq;

namespace FirebirdSql.Data.Isql;

/// <summary>
/// FbScript parses a SQL file and returns its SQL statements.
/// The class take in consideration that the statement separator can change in code.
/// For instance, in Firebird databases the statement <c>SET TERM !! ;</c> will change the
/// statement token terminator <b>;</b> into <b>!!</b>.
/// </summary>
public class FbScript
{
	public event EventHandler<UnknownStatementEventArgs> UnknownStatement;

	SqlStringParser _parser;
	FbStatementCollection _results;

	/// <summary>
	/// Returns a FbStatementCollection containing all the SQL statements (without comments) present on the file.
	/// This property is loaded after the method call <see cref="Parse"/>.
	/// </summary>
	public FbStatementCollection Results
	{
		get { return _results; }
	}

	/// <summary>
	/// Creates FbScript reading content from file.
	/// </summary>
	public static FbScript LoadFromFile(string fileName)
	{
		return new FbScript(File.ReadAllText(fileName));
	}

	public FbScript(string script)
	{
		if (script == null)
			throw new ArgumentNullException();

		_results = new FbStatementCollection();
		_parser = new SqlStringParser(script);
		_parser.Tokens = new[] { ";" };
	}

	/// <summary>
	/// Parses the SQL code and loads the SQL statements into the StringCollection <see cref="Results"/>.
	/// </summary>
	/// <returns>The number of statements found.</returns>
	public int Parse()
	{
		_results.Clear();
		foreach (var statement in _parser.Parse())
		{
			if (IsSetTermStatement(statement.CleanText, out var newParserToken))
			{
				_parser.Tokens = new[] { newParserToken };
				continue;
			}

			if (statement.CleanText != string.Empty)
			{
				var type = GetStatementType(statement.CleanText);
				if (type != null)
				{
					statement.SetStatementType((SqlStatementType)type);
					_results.Add(statement);
					continue;
				}
			}

			if (statement.Text.Trim() != string.Empty)
			{
				var unknownStatementEventArgs = new UnknownStatementEventArgs(statement);
				UnknownStatement?.Invoke(this, unknownStatementEventArgs);
				if (unknownStatementEventArgs.Handled && !unknownStatementEventArgs.Ignore)
				{
					statement.SetStatementType(unknownStatementEventArgs.NewStatementType);
					_results.Add(statement);
					continue;
				}
				else if (!unknownStatementEventArgs.Handled && unknownStatementEventArgs.Ignore)
				{
					continue;
				}
				else if (unknownStatementEventArgs.Handled && unknownStatementEventArgs.Ignore)
				{
					throw new InvalidOperationException($"Both {nameof(UnknownStatementEventArgs.Handled)} and {nameof(UnknownStatementEventArgs.Ignore)} should not be set.");
				}
				else
				{
					throw new ArgumentException($"The type of the SQL statement could not be determined. See also {nameof(UnknownStatement)} event.{Environment.NewLine}Statement: {statement.Text}.");
				}
			}
		}
		return _results.Count;
	}

	static bool IsSetTermStatement(string statement, out string newTerm)
	{
		if (statement.StartsWith("SET TERM", StringComparison.OrdinalIgnoreCase))
		{
			newTerm = statement.Substring(8).Trim();
			return true;
		}

		newTerm = default;
		return false;
	}

	static SqlStatementType? GetStatementType(string sqlStatement)
	{
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
				if (sqlStatement.StartsWith("ALTER EXTERNAL FUNCTION", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.AlterExternalFunction;
				}
				if (sqlStatement.StartsWith("ALTER FUNCTION", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.AlterFunction;
				}
				if (sqlStatement.StartsWith("ALTER INDEX", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.AlterIndex;
				}
				if (sqlStatement.StartsWith("ALTER PACKAGE", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.AlterPackage;
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
				if (sqlStatement.StartsWith("CREATE FUNCTION", StringComparison.OrdinalIgnoreCase) ||
					sqlStatement.StartsWith("CREATE OR ALTER FUNCTION", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.CreateFunction;
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
				if (sqlStatement.StartsWith("CREATE PACKAGE BODY", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.CreatePackageBody;
				}
				if (sqlStatement.StartsWith("CREATE PACKAGE", StringComparison.OrdinalIgnoreCase) ||
					sqlStatement.StartsWith("CREATE OR ALTER PACKAGE", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.CreatePackage;
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
				if (sqlStatement.StartsWith("DROP FUNCTION", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.DropFunction;
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
				if (sqlStatement.StartsWith("DROP PACKAGE BODY", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.DropPackageBody;
				}
				if (sqlStatement.StartsWith("DROP PACKAGE", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.DropPackage;
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

			case 'M':
			case 'm':
				if (sqlStatement.StartsWith("MERGE", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.Merge;
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
				if (sqlStatement.StartsWith("RECREATE FUNCTION", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.RecreateFunction;
				}
				if (sqlStatement.StartsWith("RECREATE PACKAGE BODY", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.RecreatePackageBody;
				}
				if (sqlStatement.StartsWith("RECREATE PACKAGE", StringComparison.OrdinalIgnoreCase))
				{
					return SqlStatementType.RecreatePackage;
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
		return null;
	}
}
