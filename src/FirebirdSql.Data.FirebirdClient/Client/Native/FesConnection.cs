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
using System.Text.RegularExpressions;

namespace FirebirdSql.Data.Client.Native;

internal sealed class FesConnection
{
	private FesConnection()
	{ }

	public static Version GetClientVersion(IFbClient fbClient)
	{
		var sb = new StringBuilder(64);
		fbClient.isc_get_client_version(sb);
		var version = sb.ToString();
		var m = Regex.Match(version, @"Firebird (\d+.\d+)");
		if (!m.Success)
			return null;
		return new Version(m.Groups[1].Value);
	}
}
