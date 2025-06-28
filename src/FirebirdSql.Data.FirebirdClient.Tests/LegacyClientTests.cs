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

//$Authors = Osincev Daniil

using System.Text;
using FirebirdSql.Data.Client.Managed.Legacy;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[NoServerCategory]
public class LegacyClientTests
{
	[Test]
	public void ClientProofProducesExpectedHash()
	{
		var client = new LegacyClient();
		var result = client.ClientProof("masterkey");
		var hash = Encoding.ASCII.GetString(result);
		Assert.AreEqual("QP3LMZ/MJh.", hash);
	}

	[Test]
	public void ClientProofNullReturnsStar()
	{
		var client = new LegacyClient();
		var result = client.ClientProof(null);
		var hash = Encoding.ASCII.GetString(result);
		Assert.AreEqual("*", hash);
	}
}