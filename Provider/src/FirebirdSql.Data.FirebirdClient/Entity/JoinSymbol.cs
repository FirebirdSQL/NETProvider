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
#if !EF6
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;
#else
using System.Data.Entity.Core.Metadata.Edm;
#endif

using FirebirdSql.Data.FirebirdClient;

#if !EF6
namespace FirebirdSql.Data.Entity
#else
namespace FirebirdSql.Data.EntityFramework6.SqlGen
#endif
{
	internal sealed class JoinSymbol : Symbol
	{
		#region Fields

		private List<Symbol> _columnList;
		private List<Symbol> _extentList;
		private List<Symbol> _flattenedExtentList;
		private Dictionary<string, Symbol> _nameToExtent;
		private bool _isNestedJoin;

		#endregion

		#region Properties

		internal List<Symbol> ColumnList
		{
			get
			{
				if (null == _columnList)
				{
					_columnList = new List<Symbol>();
				}
				return _columnList;
			}
			set { _columnList = value; }
		}

		internal List<Symbol> ExtentList
		{
			get { return _extentList; }
		}

		internal List<Symbol> FlattenedExtentList
		{
			get
			{
				if (null == _flattenedExtentList)
				{
					_flattenedExtentList = new List<Symbol>();
				}
				return _flattenedExtentList;
			}
			set { _flattenedExtentList = value; }
		}

		internal Dictionary<string, Symbol> NameToExtent
		{
			get { return _nameToExtent; }
		}

		internal bool IsNestedJoin
		{
			get { return _isNestedJoin; }
			set { _isNestedJoin = value; }
		}

		#endregion

		#region Constructors

		public JoinSymbol(string name, TypeUsage type, List<Symbol> extents)
			: base(name, type)
		{
			_extentList = new List<Symbol>(extents.Count);
			_nameToExtent = new Dictionary<string, Symbol>(extents.Count, StringComparer.OrdinalIgnoreCase);

			foreach (Symbol symbol in extents)
			{
				_nameToExtent[symbol.Name] = symbol;
				ExtentList.Add(symbol);
			}
		}

		#endregion
	}
}
