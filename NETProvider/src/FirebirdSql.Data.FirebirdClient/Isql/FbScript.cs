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
 *  Copyright (c) 2003, 2005 Abel Eduardo Pereira
 *  All Rights Reserved.
 *
 * Contributors:
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.IO;
using System.Text;

namespace FirebirdSql.Data.Isql
{
	/// <summary>
	/// FbScript parses a SQL file and returns its SQL statements.
	/// The class take in consideration that the statement separator can change in code.
	/// For instance, in Firebird databases the statement <c>SET TERM !! ;</c> will change the
	/// statement token terminator <b>;</b> into <b>!!</b>.
	/// </summary>
	public class FbScript
	{
		#region Fields

		private SqlStringParser _parser;
		private FbStatementCollection _results;
		private FbStatementCollection _results2;

		#endregion

		#region Properties

		/// <summary>
		/// Returns a FbStatementCollection containing all the SQL statements (without comments) present on the file.
		/// This property is loaded after the method call <see cref="Parse"/>.
		/// </summary>
		public FbStatementCollection Results
		{
			get { return _results; }
		}

		internal FbStatementCollection Results2
		{
			get { return _results2; }
		}

		#endregion

		#region Static
		/// <summary>
		/// Creates FbScript reading content from file.
		/// </summary>
		public static FbScript LoadFromFile(string fileName)
		{
			return new FbScript(File.ReadAllText(fileName));
		}
		#endregion

		#region Constructors

		public FbScript(string script)
		{
			_results = new FbStatementCollection();
			_results2 = new FbStatementCollection();
			_parser = new SqlStringParser(script);
			_parser.Tokens = new[] { ";" };
		}

		#endregion

		#region Methods

		/// <summary>
		/// Parses the SQL code and loads the SQL statements into the StringCollection <see cref="Results"/>.
		/// </summary>
		/// <returns>The number of statements found.</returns>
		public int Parse()
		{
			_results.Clear();

			while (_parser.ParseNext() != -1)
			{
				var resultClean = _parser.ResultClean;
				if (!string.IsNullOrEmpty(resultClean))
				{
					string newParserToken;
					if (IsSetTermStatement(resultClean, out newParserToken))
					{
						_parser.Tokens = new[] { newParserToken };
						continue;
					}

					_results2.Add(resultClean);
					_results.Add(_parser.Result);
				}
			}

			return _results.Count;
		}

		#endregion

		#region Private Methods

		private bool IsSetTermStatement(string statement, out string newTerm)
		{
			if (statement.StartsWith("SET TERM", StringComparison.OrdinalIgnoreCase))
			{
				newTerm = statement.Substring(8).Trim();
				return true;
			}

			newTerm = default(string);
			return false;
		}

		#endregion
	}
}
