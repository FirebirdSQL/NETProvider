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
 *  Copyright (c) 2008-2010 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

#if (!(NET_35 && !ENTITY_FRAMEWORK))

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Data.SqlClient;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;

using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Entity
{
	/// <summary>
	/// A Join symbol is a special kind of Symbol.
	/// It has to carry additional information
	/// <list type="bullet">
	/// <item>ColumnList for the list of columns in the select clause if this
	/// symbol represents a sql select statement.  This is set by <see cref="SqlGenerator.AddDefaultColumns"/>. </item>
	/// <item>ExtentList is the list of extents in the select clause.</item>
	/// <item>FlattenedExtentList - if the Join has multiple extents flattened at the 
	/// top level, we need this information to ensure that extent aliases are renamed
	/// correctly in <see cref="SqlSelectStatement.WriteSql"/></item>
	/// <item>NameToExtent has all the extents in ExtentList as a dictionary.
	/// This is used by <see cref="SqlGenerator.Visit(DbPropertyExpression)"/> to flatten
	/// record accesses.</item>
	/// <item>IsNestedJoin - is used to determine whether a JoinSymbol is an 
	/// ordinary join symbol, or one that has a corresponding SqlSelectStatement.</item>
	/// </list>
	/// 
	/// All the lists are set exactly once, and then used for lookups/enumerated.
	/// </summary>
	internal sealed class JoinSymbol : Symbol
	{
		#region · Fields ·

		private List<Symbol> columnList;
		private List<Symbol> extentList;
		private List<Symbol> flattenedExtentList;
		private Dictionary<string, Symbol> nameToExtent;
		private bool isNestedJoin;

		#endregion

		#region · Properties ·

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

		#region · Constructors ·

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
#endif
