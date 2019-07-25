/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System.Collections;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.TestsBase
{
	class FbDefaultServerTypeTestFixtureSource : IEnumerable
	{
		public IEnumerator GetEnumerator()
		{
			yield return new object[] { FbServerType.Default, false, FbWireCrypt.Disabled };
			yield return new object[] { FbServerType.Default, false, FbWireCrypt.Required };
			yield return new object[] { FbServerType.Default, true, FbWireCrypt.Disabled };
			yield return new object[] { FbServerType.Default, true, FbWireCrypt.Required };
		}
	}
}
