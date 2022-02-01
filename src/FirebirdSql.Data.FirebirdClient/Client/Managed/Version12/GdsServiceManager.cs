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

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version12;

internal class GdsServiceManager : Version11.GdsServiceManager
{
	public GdsServiceManager(GdsConnection connection)
		: base(connection)
	{ }

	protected override Version10.GdsDatabase CreateDatabase(GdsConnection connection)
	{
		return new GdsDatabase(connection);
	}
}
