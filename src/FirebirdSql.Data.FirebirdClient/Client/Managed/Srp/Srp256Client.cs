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

using System.Linq;
using System.Security.Cryptography;

namespace FirebirdSql.Data.Client.Managed.Srp;

internal sealed class Srp256Client : SrpClientBase
{
	public override string Name => "Srp256";

	protected override byte[] ComputeHash(params byte[][] ba)
	{
		using (var hash = SHA256.Create())
		{
			return hash.ComputeHash(ba.SelectMany(x => x).ToArray());
		}
	}
}
