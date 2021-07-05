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

using FirebirdSql.Data.Client.Managed;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[NoServerCategory]
	public class Srp256ClientTests
	{
		[Test]
		public void KeyMatchTest()
		{
			var user = "SYSDBA";
			var password = "masterkey";
			var client = new Srp256Client();
			var salt = client.GetSalt();
			var serverKeyPair = client.ServerSeed(user, password, salt);
			var serverSessionKey = client.GetServerSessionKey(user, password, salt, client.PublicKey, serverKeyPair.Item1, serverKeyPair.Item2);
			client.ClientProof(user, password, salt, serverKeyPair.Item1);
			Assert.AreEqual(serverSessionKey.ToString(), client.SessionKey.ToString());
		}
	}
}
