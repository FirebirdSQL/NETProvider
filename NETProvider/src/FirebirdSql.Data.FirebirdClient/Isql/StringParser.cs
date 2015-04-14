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

		private string source;
		private int sourceLength;
		private bool caseSensitive;
		private string[] tokens;
		private int currentIndex;
		private int charsParsed;
		private string result;

		#endregion

		#region Properties

		/// <summary>
		/// Loaded after a parsing operation with the number of chars parsed.
		/// </summary>
		public int CharsParsed
		{
			get { return this.charsParsed; }
		}

		/// <summary>
		/// Loaded after a parsing operation with the string that was found between tokens.
		/// </summary>
		public string Result
		{
			get { return this.result; }
		}

		/// <summary>
		/// The string separator. The default value is a white space: 0x32 ASCII code.
		/// </summary>
		public string[] Tokens
		{
			get { return this.tokens; }
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
				this.tokens = value;
			}
		}

		/// <summary>
		/// Returns the length of the string that is being parsed.
		/// </summary>
		public int Length
		{
			get { return this.sourceLength; }
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
		public StringParser(string targetString, bool caseSensitive)
		{
			this.caseSensitive = caseSensitive;
			this.tokens = new[] { " " };
			this.source = targetString;
			this.sourceLength = targetString.Length;
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
			if (this.currentIndex >= this.sourceLength)
			{
				return -1;
			}

			int i = this.currentIndex;
			bool inLiteral = false;
			string matchedToken = null;

			while (i < this.sourceLength)
			{
				if (this.source[i] == '\'')
				{
					inLiteral = !inLiteral;
				}

				if (!inLiteral)
				{
					foreach (var token in this.Tokens)
					{
						if (string.Compare(this.source[i].ToString(), token[0].ToString(), !this.caseSensitive, CultureInfo.CurrentUICulture) == 0)
						{
							if (string.Compare(this.source.Substring(i, token.Length), token, !this.caseSensitive, CultureInfo.CurrentUICulture) == 0)
							{
								i += token.Length;
								matchedToken = token;
								goto Break;
							}
						}
					}
				}

				i++;
			}
		// just to get out of the outer loop
		Break:
			{ }

			this.charsParsed = i - this.currentIndex;
			bool subtractToken = (i != this.sourceLength) || (matchedToken != null && this.source.EndsWith(matchedToken, !this.caseSensitive, CultureInfo.CurrentUICulture));
			this.result = this.source.Substring(this.currentIndex, i - this.currentIndex - (subtractToken ? matchedToken.Length : 0));

			return this.currentIndex = i;
		}

		/// <summary>
		/// Overrided method that returns the string to be parsed.
		/// </summary>
		/// <returns>The string to be parsed.</returns>
		public override string ToString()
		{
			return this.source;
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Indicates if the string specified as <b>source</b> starts with the <b>token</b> string.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <param name="token">The token that is intended to find.</param>
		/// <param name="ignoreCase">Indicated is char case should be ignored.</param>
		/// <returns>Returns <b>true</b> if the <b>token</b> precedes the <b>source</b>.</returns>
		public static bool StartsWith(string source, string token, bool ignoreCase)
		{
			return source.StartsWith(token, ignoreCase, CultureInfo.CurrentUICulture);
		}

		#endregion
	}
}
