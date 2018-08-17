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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	[FbTestFixture(FbServerType.Embedded, default)]
	public class FbStoredProcCallsTests : FbTestsBase
	{
		#region Constructors

		public FbStoredProcCallsTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{ }

		#endregion

		#region Unit Tests

		[Test]
		public void FirebirdLikeTest00()
		{
			var command = new FbCommand("EXECUTE PROCEDURE GETVARCHARFIELD(?)", Connection);

			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters.Add("@VARCHAR_FIELD", FbDbType.VarChar).Direction = ParameterDirection.Output;

			command.Parameters[0].Value = 1;

			command.ExecuteNonQuery();

			var value = command.Parameters[1].Value;

			command.Dispose();

			Assert.AreEqual("IRow Number 1", value);
		}

		[Test]
		public void FirebirdLikeTest01()
		{
			var command = new FbCommand("SELECT * FROM GETVARCHARFIELD(?)", Connection);
			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters[0].Value = 1;

			var reader = command.ExecuteReader();
			reader.Read();
			var value = reader[0];
			reader.Close();

			command.Dispose();

			Assert.AreEqual("IRow Number 1", value);
		}

		[Test]
		public void SqlServerLikeTest00()
		{
			var command = new FbCommand("GETVARCHARFIELD", Connection);

			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", FbDbType.VarChar).Direction = ParameterDirection.Input;
			command.Parameters.Add("@VARCHAR_FIELD", FbDbType.VarChar).Direction = ParameterDirection.Output;

			command.Parameters[0].Value = 1;

			command.ExecuteNonQuery();

			var value = command.Parameters[1].Value;

			command.Dispose();

			Assert.AreEqual("IRow Number 1", value);
		}

		[Test]
		public void SqlServerLikeTest01()
		{
			var command = new FbCommand("GETRECORDCOUNT", Connection);
			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@RECORDCOUNT", FbDbType.Integer).Direction = ParameterDirection.Output;

			command.ExecuteNonQuery();

			var value = command.Parameters[0].Value;

			command.Dispose();

			Assert.Greater(Convert.ToInt32(value), 0);
		}

		[Test]
		public void SqlServerLikeTest02()
		{
			var command = new FbCommand("GETVARCHARFIELD", Connection);

			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@ID", FbDbType.VarChar).Value = 1;

			var r = command.ExecuteReader();

			var count = 0;

			while (r.Read())
			{
				count++;
			}

			r.Close();

			command.Dispose();

			Assert.AreEqual(1, count);
		}

		#endregion
	}
}
