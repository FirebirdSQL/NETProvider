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
using System.Data.Entity.Core.Metadata.Edm;
#endif

using FirebirdSql.Data.FirebirdClient;

#if (!EF_6)
namespace FirebirdSql.Data.Entity
#else
namespace FirebirdSql.Data.EntityFramework6.SqlGen
#endif
{
	internal sealed class JoinSymbol : Symbol
	{
		#region Fields

		private List<Symbol> columnList;
		private List<Symbol> extentList;
		private List<Symbol> flattenedExtentList;
		private Dictionary<string, Symbol> nameToExtent;
		private bool isNestedJoin;

		#endregion

		#region Properties

		internal List<Symbol> ColumnList
		{
			get
			{
				if (null == columnList)
				{
					columnList = new List<Symbol>();
				}
				return columnList;
			}
			set { columnList = value; }
		}

		internal List<Symbol> ExtentList
		{
			get { return extentList; }
		}

		internal List<Symbol> FlattenedExtentList
		{
			get
			{
				if (null == flattenedExtentList)
				{
					flattenedExtentList = new List<Symbol>();
				}
				return flattenedExtentList;
			}
			set { flattenedExtentList = value; }
		}

		internal Dictionary<string, Symbol> NameToExtent
		{
			get { return nameToExtent; }
		}

		internal bool IsNestedJoin
		{
			get { return isNestedJoin; }
			set { isNestedJoin = value; }
		}

		#endregion

		#region Constructors

		public JoinSymbol(string name, TypeUsage type, List<Symbol> extents)
			: base(name, type)
		{
			extentList = new List<Symbol>(extents.Count);
			nameToExtent = new Dictionary<string, Symbol>(extents.Count, StringComparer.OrdinalIgnoreCase);

			foreach (Symbol symbol in extents)
			{
				this.nameToExtent[symbol.Name] = symbol;
				this.ExtentList.Add(symbol);
			}
		}

		#endregion
	}
}
