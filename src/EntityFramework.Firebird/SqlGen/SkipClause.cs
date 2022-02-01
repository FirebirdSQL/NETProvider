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

using System.Globalization;

namespace EntityFramework.Firebird.SqlGen;

internal class SkipClause : ISqlFragment
{
	#region Fields

	private ISqlFragment _skipCount;

	#endregion

	#region Internal Properties

	/// <summary>
	/// How many rows should be skipped.
	/// </summary>
	internal ISqlFragment SkipCount
	{
		get { return _skipCount; }
	}

	#endregion

	#region Constructors

	/// <summary>
	/// Creates a SkipClause with the given skipCount.
	/// </summary>
	/// <param name="topCount"></param>
	internal SkipClause(ISqlFragment skipCount)
	{
		_skipCount = skipCount;
	}

	/// <summary>
	/// Creates a SkipClause with the given skipCount.
	/// </summary>
	/// <param name="topCount"></param>
	internal SkipClause(int skipCount)
	{
		var sqlBuilder = new SqlBuilder();
		sqlBuilder.Append(skipCount.ToString(CultureInfo.InvariantCulture));
		_skipCount = sqlBuilder;
	}

	#endregion

	#region ISqlFragment Members

	/// <summary>
	/// Write out the SKIP part of sql select statement
	/// It basically writes SKIP (X).
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="sqlGenerator"></param>
	public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
	{
		writer.Write("SKIP (");
		SkipCount.WriteSql(writer, sqlGenerator);
		writer.Write(")");

		writer.Write(" ");
	}

	#endregion
}
