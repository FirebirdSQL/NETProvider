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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;

namespace EntityFramework.Firebird.SqlGen;

internal sealed class SymbolTable
{
	#region Fields

	private List<Dictionary<string, Symbol>> _symbols = new List<Dictionary<string, Symbol>>();

	#endregion

	#region Methods

	internal void EnterScope()
	{
		_symbols.Add(new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase));
	}

	internal void ExitScope()
	{
		_symbols.RemoveAt(_symbols.Count - 1);
	}

	internal void Add(string name, Symbol value)
	{
		_symbols[_symbols.Count - 1][name] = value;
	}

	internal Symbol Lookup(string name)
	{
		for (var i = _symbols.Count - 1; i >= 0; --i)
		{
			if (_symbols[i].ContainsKey(name))
			{
				return _symbols[i][name];
			}
		}

		return null;
	}

	#endregion
}
