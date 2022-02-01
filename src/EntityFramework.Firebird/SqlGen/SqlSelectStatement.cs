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

using System.Collections.Generic;
using System.Diagnostics;

namespace EntityFramework.Firebird.SqlGen;

internal sealed class SqlSelectStatement : ISqlFragment
{
	#region Fields

	private bool _isDistinct;
	private List<Symbol> _allJoinExtents;
	private List<Symbol> _fromExtents;
	private Dictionary<Symbol, bool> _outerExtents;
	private FirstClause _first;
	private SkipClause _skip;
	private SqlBuilder _select = new SqlBuilder();
	private SqlBuilder _from = new SqlBuilder();
	private SqlBuilder _where;
	private SqlBuilder _groupBy;
	private SqlBuilder _orderBy;
	//indicates whether it is the top most select statement,
	// if not Order By should be omitted unless there is a corresponding TOP
	private bool _isTopMost;

	#endregion

	#region Properties

	public SqlBuilder OrderBy
	{
		get
		{
			if (null == _orderBy)
			{
				_orderBy = new SqlBuilder();
			}
			return _orderBy;
		}
	}

	#endregion

	#region Internal Properties

	/// <summary>
	/// Do we need to add a DISTINCT at the beginning of the SELECT
	/// </summary>
	internal bool IsDistinct
	{
		get { return _isDistinct; }
		set { _isDistinct = value; }
	}

	internal List<Symbol> AllJoinExtents
	{
		get { return _allJoinExtents; }
		// We have a setter as well, even though this is a list,
		// since we use this field only in special cases.
		set { _allJoinExtents = value; }
	}

	internal List<Symbol> FromExtents
	{
		get
		{
			if (null == _fromExtents)
			{
				_fromExtents = new List<Symbol>();
			}
			return _fromExtents;
		}
	}

	internal Dictionary<Symbol, bool> OuterExtents
	{
		get
		{
			if (null == _outerExtents)
			{
				_outerExtents = new Dictionary<Symbol, bool>();
			}
			return _outerExtents;
		}
	}

	internal FirstClause First
	{
		get { return _first; }
		set
		{
			Debug.Assert(_first == null, "SqlSelectStatement.Top has already been set");
			_first = value;
		}
	}

	internal SkipClause Skip
	{
		get { return _skip; }
		set
		{
			Debug.Assert(_skip == null, "SqlSelectStatement.Skip has already been set");
			_skip = value;
		}
	}

	internal SqlBuilder Select
	{
		get { return _select; }
	}

	internal SqlBuilder From
	{
		get { return _from; }
	}

	internal SqlBuilder Where
	{
		get
		{
			if (null == _where)
			{
				_where = new SqlBuilder();
			}
			return _where;
		}
	}

	internal SqlBuilder GroupBy
	{
		get
		{
			if (null == _groupBy)
			{
				_groupBy = new SqlBuilder();
			}
			return _groupBy;
		}
	}

	internal bool IsTopMost
	{
		get { return _isTopMost; }
		set { _isTopMost = value; }
	}

	#endregion

	#region ISqlFragment Members

	/// <summary>
	/// Write out a SQL select statement as a string.
	/// We have to
	/// <list type="number">
	/// <item>Check whether the aliases extents we use in this statement have
	/// to be renamed.
	/// We first create a list of all the aliases used by the outer extents.
	/// For each of the FromExtents( or AllJoinExtents if it is non-null),
	/// rename it if it collides with the previous list.
	/// </item>
	/// <item>Write each of the clauses (if it exists) as a string</item>
	/// </list>
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="sqlGenerator"></param>
	public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
	{
		#region Check if FROM aliases need to be renamed

		// Create a list of the aliases used by the outer extents
		// JoinSymbols have to be treated specially.
		List<string> outerExtentAliases = null;

		if ((null != _outerExtents) && (0 < _outerExtents.Count))
		{
			foreach (var outerExtent in _outerExtents.Keys)
			{
				if (outerExtent is JoinSymbol joinSymbol)
				{
					foreach (var symbol in joinSymbol.FlattenedExtentList)
					{
						if (null == outerExtentAliases)
						{
							outerExtentAliases = new List<string>();
						}
						outerExtentAliases.Add(symbol.NewName);
					}
				}
				else
				{
					if (null == outerExtentAliases)
					{
						outerExtentAliases = new List<string>();
					}
					outerExtentAliases.Add(outerExtent.NewName);
				}
			}
		}

		// An then rename each of the FromExtents we have
		// If AllJoinExtents is non-null - it has precedence.
		// The new name is derived from the old name - we append an increasing int.
		var extentList = AllJoinExtents ?? _fromExtents;
		if (null != extentList)
		{
			foreach (var fromAlias in extentList)
			{
				if ((null != outerExtentAliases) && outerExtentAliases.Contains(fromAlias.Name))
				{
					var i = sqlGenerator.AllExtentNames[fromAlias.Name];
					string newName;

					do
					{
						++i;
						newName = fromAlias.Name + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
					}
					while (sqlGenerator.AllExtentNames.ContainsKey(newName));

					sqlGenerator.AllExtentNames[fromAlias.Name] = i;
					fromAlias.NewName = newName;

					// Add extent to list of known names (although i is always incrementing, "prefix11" can
					// eventually collide with "prefix1" when it is extended)
					sqlGenerator.AllExtentNames[newName] = 0;
				}

				// Add the current alias to the list, so that the extents
				// that follow do not collide with me.
				if (null == outerExtentAliases)
				{
					outerExtentAliases = new List<string>();
				}
				outerExtentAliases.Add(fromAlias.NewName);
			}
		}

		#endregion

		// Increase the indent, so that the Sql statement is nested by one tab.
		writer.Indent += 1; // ++ can be confusing in this context

		writer.Write("SELECT ");
		if (IsDistinct)
		{
			writer.Write("DISTINCT ");
		}

		if (First != null)
		{
			First.WriteSql(writer, sqlGenerator);
		}

		if (Skip != null)
		{
			Skip.WriteSql(writer, sqlGenerator);
		}

		if ((_select == null) || Select.IsEmpty)
		{
			Debug.Assert(false);  // we have removed all possibilities of SELECT *.
			writer.Write("*");
		}
		else
		{
			Select.WriteSql(writer, sqlGenerator);
		}

		writer.WriteLine();
		writer.Write("FROM ");
		From.WriteSql(writer, sqlGenerator);

		if ((_where != null) && !Where.IsEmpty)
		{
			writer.WriteLine();
			writer.Write("WHERE ");
			Where.WriteSql(writer, sqlGenerator);
		}

		if ((_groupBy != null) && !GroupBy.IsEmpty)
		{
			writer.WriteLine();
			writer.Write("GROUP BY ");
			GroupBy.WriteSql(writer, sqlGenerator);
		}

		if ((_orderBy != null) && !OrderBy.IsEmpty && (IsTopMost || First != null || Skip != null))
		{
			writer.WriteLine();
			writer.Write("ORDER BY ");
			OrderBy.WriteSql(writer, sqlGenerator);
		}

		--writer.Indent;
	}

	#endregion
}
