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
 *  Copyright (c) 2008-2014 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
#if (!EF_6)
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;
#else
#endif

using FirebirdSql.Data.FirebirdClient;

#if (!EF_6)
namespace FirebirdSql.Data.Entity
#else
namespace FirebirdSql.Data.EntityFramework6.SqlGen
#endif
{
	internal sealed class SymbolTable
	{
		#region Fields

		private List<Dictionary<string, Symbol>> symbols = new List<Dictionary<string, Symbol>>();

		#endregion

		#region Methods

		internal void EnterScope()
		{
			symbols.Add(new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase));
		}

		internal void ExitScope()
		{
			symbols.RemoveAt(symbols.Count - 1);
		}

		internal void Add(string name, Symbol value)
		{
			symbols[symbols.Count - 1][name] = value;
		}

		internal Symbol Lookup(string name)
		{
			for (int i = symbols.Count - 1; i >= 0; --i)
			{
				if (symbols[i].ContainsKey(name))
				{
					return symbols[i][name];
				}
			}

			return null;
		}

		#endregion
	}
}
