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

namespace FirebirdSql.Data.Isql
{
	/// <summary>
	/// StringParser parses a string returnning the (sub)strings between tokens.
	/// </summary>
	/// <example>
	/// An example of how to use this class.
	/// <code>
	/// [STAThread]
	/// static void Main(string[] args) {
	///		int currentIndex = 0;
	///		string s = ".NET Framework doesn't have a string parsing class?!";
	///		StringParser parser = new StringParser(s, false);
	///		while (currentIndex &lt; s.Length) {
	///			Console.WriteLine("Returned Index: {0}", currentIndex = parser.ParseNext());
	///			Console.WriteLine("Chars scanned: {0}", parser.CharsParsed);
	///			Console.WriteLine("Parsing result: {0}", parser.Result);
	///			Console.WriteLine();
	///		}
	/// }
	/// </code>
	/// <para>The output:</para>
	/// <code>
	/// Returned Index: 5
	///	Chars scanned: 5
	///	Parsing	result:	.NET
	///
	///	Returned Index:	15
	///	Chars scanned: 10
	///	Parsing	result:	Framework
	///
	///	Returned Index:	23
	///	Chars scanned: 8
	///	Parsing	result:	doesn't
	///
	///	Returned Index:	28
	///	Chars scanned: 5
	///	Parsing	result:	have
	///
	///	Returned Index:	30
	///	Chars scanned: 2
	///	Parsing	result:	a
	///
	///	Returned Index:	37
	///	Chars scanned: 7
	///	Parsing	result:	string
	///
	///	Returned Index:	45
	///	Chars scanned: 8
	///	Parsing	result:	parsing
	///
	///	Returned Index:	52
	///	Chars scanned: 7
	///	Parsing	result:	class?!
	/// </code>
	/// </example>
	class StringParser
	{
		#region Fields

		private string _source;
		private int _sourceLength;
		private string[] _tokens;
		private int _currentIndex;
		private int _charsParsed;
		private string _result;

		#endregion

		#region Properties

		/// <summary>
		/// Loaded after a parsing operation with the number of chars parsed.
		/// </summary>
		public int CharsParsed
		{
			get { return _charsParsed; }
		}

		/// <summary>
		/// Loaded after a parsing operation with the string that was found between tokens.
		/// </summary>
		public string Result
		{
			get { return _result; }
		}

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
		/// Returns the length of the string that is being parsed.
		/// </summary>
		public int Length
		{
			get { return _sourceLength; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Creates an instance of StringParser.
		/// </summary>
		/// <param name="targetString">Indicates if parser system should be case-sensitive (true) or case-intensitive (false).</param>
		/// <param name="caseSensitive">The string to parse.</param>
		/// <remarks>By defining the string (to parse) in constructor you can call directly the method <see cref="ParseNext"/>
		/// without having to initializate the target string on <see cref="Parse(System.String)"/> method. See the example for further details.
		/// </remarks>
		public StringParser(string targetString)
		{
			_tokens = new[] { " " };
			_source = targetString;
			_sourceLength = targetString.Length;
		}

		#endregion

		#region Methods

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

			int i = _currentIndex;
			bool inLiteral = false;
			string matchedToken = null;

			while (i < _sourceLength)
			{
				if (_source[i] == '\'')
				{
					inLiteral = !inLiteral;
				}

				if (!inLiteral)
				{
					foreach (var token in Tokens)
					{
						if (string.Compare(_source, i, token, 0, token.Length, false, CultureInfo.CurrentUICulture) == 0)
						{
							i += token.Length;
							matchedToken = token;
							goto Break;
						}
					}
				}

				i++;
			}
			// just to get out of the outer loop
			Break:
			{ }

			_charsParsed = i - _currentIndex;
			bool subtractToken = (i != _sourceLength) || (matchedToken != null && _source.EndsWith(matchedToken, false, CultureInfo.CurrentUICulture));
			_result = _source.Substring(_currentIndex, i - _currentIndex - (subtractToken ? matchedToken.Length : 0));

			return _currentIndex = i;
		}

		/// <summary>
		/// Overrided method that returns the string to be parsed.
		/// </summary>
		/// <returns>The string to be parsed.</returns>
		public override string ToString()
		{
			return _source;
		}

		#endregion
	}
}
