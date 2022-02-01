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
using System.Diagnostics;

namespace EntityFramework.Firebird.SqlGen;

internal sealed class SqlBuilder : ISqlFragment
{
	#region Fields

	private List<object> _sqlFragments;

	#endregion

	#region Properties

	private List<object> SqlFragments
	{
		get
		{
			if (null == _sqlFragments)
			{
				_sqlFragments = new List<object>();
			}
			return _sqlFragments;
		}
	}

	#endregion

	#region Methods

	/// <summary>
	/// Add an object to the list - we do not verify that it is a proper sql fragment
	/// since this is an internal method.
	/// </summary>
	/// <param name="s"></param>
	public void Append(object s)
	{
		Debug.Assert(s != null);
		SqlFragments.Add(s);
	}

	/// <summary>
	/// This is to pretty print the SQL.  The writer <see cref="SqlWriter.Write"/>
	/// needs to know about new lines so that it can add the right amount of
	/// indentation at the beginning of lines.
	/// </summary>
	public void AppendLine()
	{
		SqlFragments.Add(Environment.NewLine);
	}

	/// <summary>
	/// Whether the builder is empty.  This is used by the <see cref="SqlGenerator.Visit(ProjectExpression)"/>
	/// to determine whether a sql statement can be reused.
	/// </summary>
	public bool IsEmpty
	{
		get { return ((null == _sqlFragments) || (0 == _sqlFragments.Count)); }
	}

	#endregion

	#region ISqlFragment Members

	/// <summary>
	/// We delegate the writing of the fragment to the appropriate type.
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="sqlGenerator"></param>
	public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
	{
		if (null != _sqlFragments)
		{
			foreach (var o in _sqlFragments)
			{
				var str = (o as string);
				if (null != str)
				{
					writer.Write(str);
				}
				else
				{
					var sqlFragment = (o as ISqlFragment);
					if (null != sqlFragment)
					{
						sqlFragment.WriteSql(writer, sqlGenerator);
					}
					else
					{
						throw new InvalidOperationException();
					}
				}
			}
		}
	}

	#endregion
}
