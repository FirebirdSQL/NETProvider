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

		private StringParser parser;
		private FbStatementCollection results;

		#endregion

		#region Properties

		/// <summary>
		/// Returns a FbStatementCollection containing all the SQL statements (without comments) present on the file.
		/// This property is loaded after the method call <see cref="Parse"/>.
		/// </summary>
		public FbStatementCollection Results
		{
			get { return this.results; }
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
			this.results = new FbStatementCollection();
			this.parser = new StringParser(RemoveComments(script), false);
			this.parser.Tokens = new[] { ";" };
		}

		#endregion

		#region Methods

		/// <summary>
		/// Parses the SQL code and loads the SQL statements into the StringCollection <see cref="Results"/>.
		/// </summary>
		/// <returns>The number of statements found.</returns>
		public int Parse()
		{
			int index = 0;
			string atomicResult;
			string newParserToken;

			this.results.Clear();

			while (index < this.parser.Length)
			{
				index = this.parser.ParseNext();
				atomicResult = this.parser.Result.Trim();

				if (this.IsSetTermStatement(atomicResult, out newParserToken))
				{
					this.parser.Tokens = new[] { newParserToken };
					continue;
				}

				if (atomicResult != null && atomicResult.Length > 0)
				{
					this.results.Add(atomicResult);
				}
			}

			return this.results.Count;
		}

		/// <summary>
		/// Overrided method, returns the the SQL code to be parsed (with comments removed).
		/// </summary>
		/// <returns>The SQL code to be parsed (without comments).</returns>
		public override string ToString()
		{
			return this.parser.ToString();
		}

		#endregion

		#region Internal Static Methods

		/// <summary>
		/// Removes from the SQL code all comments of the type: /*...*/ or --
		/// </summary>
		/// <param name="source">The string containing the original SQL code.</param>
		/// <returns>A string containing the SQL code without comments.</returns>
		internal static string RemoveComments(string source)
		{
			int i = 0;
			int length = source.Length;
			StringBuilder result = new StringBuilder();
			bool insideComment = false;
			bool insideLiteral = false;

			while (i < length)
			{
				if (insideLiteral)
				{
					result.Append(source[i]);

					if (source[i] == '\'')
					{
						insideLiteral = false;
					}
				}
				else if (insideComment)
				{
					if (source[i] == '*')
					{
						if ((i < length - 1) && (source[i + 1] == '/'))
						{
							i++;
							insideComment = false;
						}
					}
				}
				else if ((source[i] == '\'') && (i < length - 1))
				{
					result.Append(source[i]);
					insideLiteral = true;
				}
				else if ((source[i] == '/') && (i < length - 1) && (source[i + 1] == '*'))
				{
					i++;
					insideComment = true;
				}
				else if ((source[i] == '-' && (i < length - 1) && source[i + 1] == '-'))
				{
					i++;
					while (i < length && source[i] != '\n')
					{
						i++;
					}
					i--;
				}
				else
				{
					result.Append(source[i]);
				}

				i++;
			}

			return result.ToString();
		}

		#endregion

		#region Private Methods

		// method assumes that statement is trimmed 
		private bool IsSetTermStatement(string statement, out string newTerm)
		{
			bool result = false;

			newTerm = string.Empty;

			if (StringParser.StartsWith(statement, "SET TERM", true))
			{
				newTerm = statement.Substring(8).Trim();
				result = true;
			}

			return result;
		}

		#endregion
	}
}
