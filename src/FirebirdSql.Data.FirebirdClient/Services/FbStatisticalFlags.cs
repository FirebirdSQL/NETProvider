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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.ComponentModel;

namespace FirebirdSql.Data.Services;

/// <summary>
/// Flags used by FbStatistical.Options
/// </summary>
[Flags]
public enum FbStatisticalFlags
{
	/// <summary>
	/// analyze data pages
	/// </summary>
	DataPages = 0x01,

	/// <summary>
	/// DatabaseLog - no longer used by firebird
	/// </summary>
	DatabaseLog = 0x02,

	/// <summary>
	/// analyze header page ONLY
	/// </summary>
	HeaderPages = 0x04,

	/// <summary>
	/// analyze index leaf pages
	/// </summary>
	IndexPages = 0x08,

	/// <summary>
	/// analyze system relations in addition to user tables
	/// </summary>
	SystemTablesRelations = 0x10,

	/// <summary>
	/// analyze average record and version length
	/// </summary>
	RecordVersionStatistics = 0x20,
}
