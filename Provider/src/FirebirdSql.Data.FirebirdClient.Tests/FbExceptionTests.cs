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

using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	class FbExceptionTests : FbTestsBase
	{
		#region Constructors

		public FbExceptionTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		#endregion

		#region Unit Tests

		[Test]
		public void SQLSTATETest()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "drop exception nonexisting";
				try
				{
					cmd.ExecuteNonQuery();
				}
				catch (FbException ex)
				{
					Assert.AreEqual("42000", ex.SQLSTATE);
				}
			}
		}

		#endregion
	}
}
