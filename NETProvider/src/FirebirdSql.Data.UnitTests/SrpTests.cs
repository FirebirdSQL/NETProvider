/*
 *  Firebird ADO.NET Data provider for .NET and Mono
 *
 *     The contents of this file are subject to the Initial
 *     Developer's Public License Version 1.0 (the "License");
 *     you may not use this file except in compliance with the
 *     License. You may obtain a copy of the License at
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *     express or implied.  See the License for the specific
 *     language governing rights and limitations under the License.
 *
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Numerics;
using System.Data.Common;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Client.Managed.Version10;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture(FbServerType.Default)]
	[TestFixture(FbServerType.Embedded)]
	public class SrpTests
	{
		#region Unit Tests

		[Test]
		public void KeyMatchTest()
		{
            String user = "SYSDBA";
            String password = "masterkey";

            SrpClient srpClient = new SrpClient();
            byte[] salt = srpClient.GetSalt();

			Tuple<BigInteger, BigInteger> serverKeyPair = srpClient.ServerSeed(user, password, salt);
			byte[] serverSessionKey = srpClient.GetServerSessionKey(
					user, password, salt, srpClient.getPublicKey(),
					serverKeyPair.Item1, serverKeyPair.Item2);
			byte[] proof = srpClient.clientProof(user, password, salt, serverKeyPair.Item1);

			Assert.AreEqual(serverSessionKey.ToString(), srpClient.getSessionKey().ToString());
		}

		#endregion
	}
}
