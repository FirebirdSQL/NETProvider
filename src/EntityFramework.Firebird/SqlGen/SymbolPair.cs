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

using System.Diagnostics;

namespace EntityFramework.Firebird.SqlGen;

internal class SymbolPair : ISqlFragment
{
	#region Fields

	private Symbol _source;
	private Symbol _column;

	#endregion

	#region Properties

	public Symbol Source
	{
		get { return _source; }
		set { _source = value; }
	}

	public Symbol Column
	{
		get { return _column; }
		set { _column = value; }
	}

	#endregion

	#region Constructors

	public SymbolPair(Symbol source, Symbol column)
	{
		Source = source;
		Column = column;
	}

	#endregion

	#region ISqlFragment Members

	public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
	{
		// Symbol pair should never be part of a SqlBuilder.
		Debug.Assert(false);
	}

	#endregion
}
