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
using System.Text;

namespace FirebirdSql.Data.Services;

[Flags]
public enum FbBackupRestoreStatistics
{
	TotalTime = 0b0001,
	TimeDelta = 0b0010,
	PageReads = 0b0100,
	PageWrites = 0b1000,
}

internal static class FbBackupRestoreStatisticsExtensions
{
	public static string BuildConfiguration(this FbBackupRestoreStatistics statistics)
	{
		var sb = new StringBuilder();
		if (statistics.HasFlag(FbBackupRestoreStatistics.TotalTime))
			sb.Append("T");
		if (statistics.HasFlag(FbBackupRestoreStatistics.TimeDelta))
			sb.Append("D");
		if (statistics.HasFlag(FbBackupRestoreStatistics.PageReads))
			sb.Append("R");
		if (statistics.HasFlag(FbBackupRestoreStatistics.PageWrites))
			sb.Append("W");
		return sb.ToString();
	}
}
