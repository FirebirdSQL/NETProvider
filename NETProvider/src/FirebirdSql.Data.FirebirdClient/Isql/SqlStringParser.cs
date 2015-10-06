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
using System.Globalization;
using System.Linq;
using System.Text;

namespace FirebirdSql.Data.Isql
{
	class SqlStringParser
	{
		string _source;
		int _sourceLength;
		string[] _tokens;
		int _currentIndex;
		string _result;

		/// <summary>
		/// Loaded after a parsing operation with the string that was found between tokens.
		/// </summary>
		public string Result => _result;

		/// <summary>
		/// Returns the length of the string that is being parsed.
		/// </summary>
		public int Length => _sourceLength;

		/// <summary>
		/// The string separator. The default value is a white space: 0x32 ASCII code.
		/// </summary>
		public string[] Tokens
		{
			get { return _tokens; }
			set
			{
				if (value == null)
					throw new ArgumentNullException();
				foreach (var item in value)
				{
					if (value == null)
						throw new ArgumentNullException();
					if (string.IsNullOrEmpty(item))
						throw new ArgumentException();
				}
				_tokens = value;
			}
		}

		/// <summary>
		/// Creates an instance of StringParser.
		/// </summary>
		/// <param name="targetString">Indicates if parser system should be case-sensitive (true) or case-intensitive (false).</param>
		/// <param name="caseSensitive">The string to parse.</param>
		/// <remarks>By defining the string (to parse) in constructor you can call directly the method <see cref="ParseNext"/>
		/// without having to initializate the target string on <see cref="Parse(System.String)"/> method. See the example for further details.
		/// </remarks>
		public SqlStringParser(string targetString)
		{
			_tokens = new[] { " " };
			_source = targetString;
			_sourceLength = targetString.Length;
		}

		/// <summary>
		/// <para>
		/// Repeats the parsing starting on the index returned by <see cref="Parse(System.String)"/> method.</para>
		/// You can also call <b>ParseNext</b> directly (without calling <see cref="Parse(System.String)"/>) if you define the text to be parsed at instance construction.
		/// </summary>
		/// <returns>The index of the char next char after the <see cref="Token"/> end.</returns>
		/// <remarks>If nothing is parsed the method will return -1. Case the <see cref="Token"/> wasn't found until the end of the string the method returns
		/// (in <see cref="Result"/>) the string found between the starting index and the end of the string.</remarks>
		public int ParseNext()
		{
			if (_currentIndex >= _sourceLength)
			{
				return -1;
			}

			var index = _currentIndex;
			while (index < _sourceLength)
			{
				if (GetChar(index) == '\'')
				{
					index++;
					ProcessLiteral(ref index);
					index++;
				}
				else if (GetChar(index) == '-' && GetNextChar(index) == '-')
				{
					index++;
					ProcessSinglelineComment(ref index);
					index++;
				}
				else if (GetChar(index) == '/' && GetNextChar(index) == '*')
				{
					index++;
					ProcessMultilineComment(ref index);
					index++;
				}
				else
				{
					foreach (var token in Tokens)
					{
						if (string.Compare(_source, index, token, 0, token.Length, false, CultureInfo.CurrentUICulture) == 0)
						{
							index += token.Length;
							var matchedToken = token;
							_result = _source.Substring(_currentIndex, index - _currentIndex - token.Length);
							_currentIndex = index;
							return _currentIndex;
						}
					}
					index++;
				}
			}

			if (index > _sourceLength)
			{
				_result = _source.Substring(_currentIndex);
				return _currentIndex = _sourceLength;
			}
			else
			{
				_result = _source.Substring(_currentIndex, index - _currentIndex);
				return _currentIndex = index;
			}
		}

		public override string ToString()
		{
			return _source;
		}

		void ProcessLiteral(ref int index)
		{
			while (index < _sourceLength)
			{
				if (GetChar(index) == '\'')
				{
					if (GetNextChar(index) == '\'')
					{
						index++;
					}
					else
					{
						break;
					}
				}
				index++;
			}
		}

		void ProcessMultilineComment(ref int index)
		{
			while (index < _sourceLength)
			{
				if (GetChar(index) == '*' && GetNextChar(index) == '/')
				{
					index++;
					break;
				}
				index++;
			}
		}

		void ProcessSinglelineComment(ref int index)
		{
			while (index < _sourceLength)
			{
				if (GetChar(index) == '\n')
				{
					break;
				}
				if (GetChar(index) == '\r')
				{
					if (GetNextChar(index) == '\n')
					{
						index++;
					}
					break;
				}
				index++;
			}
		}

		char GetChar(int index)
		{
			return _source[index];
		}

		char? GetNextChar(int index)
		{
			return index + 1 < _sourceLength
				? _source[index + 1]
				: (char?)null;
		}

		internal static string RemoveComments(string source)
		{
			var index = 0;
			var length = source.Length;
			var result = new StringBuilder();
			var insideComment = false;
			var insideLiteral = false;

			while (index < length)
			{
				if (insideLiteral)
				{
					result.Append(source[index]);

					if (source[index] == '\'')
					{
						insideLiteral = false;
					}
				}
				else if (insideComment)
				{
					if (source[index] == '*')
					{
						if ((index < length - 1) && (source[index + 1] == '/'))
						{
							index++;
							insideComment = false;
						}
					}
				}
				else if ((source[index] == '\'') && (index < length - 1))
				{
					result.Append(source[index]);
					insideLiteral = true;
				}
				else if ((source[index] == '/') && (index < length - 1) && (source[index + 1] == '*'))
				{
					index++;
					insideComment = true;
				}
				else if ((source[index] == '-' && (index < length - 1) && source[index + 1] == '-'))
				{
					index++;
					while (index < length && source[index] != '\n')
					{
						index++;
					}
					index--;
				}
				else
				{
					result.Append(source[index]);
				}

				index++;
			}

			return result.ToString();
		}
	}
}
