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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FirebirdSql.Data.Isql;

class SqlStringParser
{
	string _source;
	int _sourceLength;
	string[] _tokens;

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

	public SqlStringParser(string targetString)
	{
		_tokens = new[] { " " };
		_source = targetString;
		_sourceLength = targetString.Length;
	}

	public IEnumerable<FbStatement> Parse()
	{
		var lastYield = 0;
		var index = 0;
		var rawResult = new StringBuilder();
		while (true)
		{
			Continue:
			{ }
			if (index >= _sourceLength)
			{
				break;
			}
			if (GetChar(index) == '\'')
			{
				rawResult.Append(GetChar(index));
				index++;
				rawResult.Append(ProcessLiteral(ref index));
				rawResult.Append(GetChar(index));
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
					if (string.Compare(_source, index, token, 0, token.Length, StringComparison.Ordinal) == 0)
					{
						index += token.Length;
						yield return new FbStatement(_source.Substring(lastYield, index - lastYield - token.Length), rawResult.ToString());
						lastYield = index;
						rawResult.Clear();
						goto Continue;
					}
				}
				if (!(rawResult.Length == 0 && char.IsWhiteSpace(GetChar(index))))
				{
					rawResult.Append(GetChar(index));
				}
				index++;
			}
		}

		if (index >= _sourceLength)
		{
			var parsed = _source.Substring(lastYield);
			if (parsed.Trim() == string.Empty)
			{
				yield break;
			}
			yield return new FbStatement(parsed, rawResult.ToString());
			lastYield = _sourceLength;
			rawResult.Clear();
		}
		else
		{
			yield return new FbStatement(_source.Substring(lastYield, index - lastYield), rawResult.ToString());
			lastYield = index;
			rawResult.Clear();
		}
	}

	string ProcessLiteral(ref int index)
	{
		var sb = new StringBuilder();
		while (index < _sourceLength)
		{
			if (GetChar(index) == '\'')
			{
				if (GetNextChar(index) == '\'')
				{
					sb.Append(GetChar(index));
					index++;
				}
				else
				{
					break;
				}
			}
			sb.Append(GetChar(index));
			index++;
		}
		return sb.ToString();
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
}
