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
using System.Text;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	[FbTestFixture(FbServerType.Embedded, default)]
	public class FbImplicitTransactionTests : FbTestsBase
	{
		#region Constructors

		public FbImplicitTransactionTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{ }

		#endregion

		#region Unit Tests

		[Test]
		public void DataAdapterFillTest()
		{
			var command = new FbCommand("select * from TEST where DATE_FIELD <> ?", Connection);
			var adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@DATE_FIELD", FbDbType.Date, 4, "DATE_FIELD").Value = new DateTime(2003, 1, 5);

			var builder = new FbCommandBuilder(adapter);

			var ds = new DataSet();
			adapter.Fill(ds, "TEST");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();

			Assert.AreEqual(1, ds.Tables.Count);
			Assert.Greater(ds.Tables[0].Rows.Count, 0);
			Assert.Greater(ds.Tables[0].Columns.Count, 0);
		}

		[Test]
		public void ExecuteScalarTest()
		{
			var command = new FbCommand("select sum(int_field) from TEST", Connection);

			Assert.DoesNotThrow(() => command.ExecuteScalar());

			command.Dispose();
		}

		[Test]
		public void UpdatedClobFieldTest()
		{
			var command = new FbCommand("update TEST set clob_field = @clob_field where int_field = @int_field", Connection);
			command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
			command.Parameters.Add("@clob_field", FbDbType.Text).Value = "Clob field update with implicit transaction";

			var i = command.ExecuteNonQuery();

			Assert.AreEqual(i, 1, "Clob field update with implicit transaction failed");

			command.Dispose();
		}

		[Test]
		public void UpdatedBlobFieldTest()
		{
			var command = new FbCommand("update TEST set blob_field = @blob_field where int_field = @int_field", Connection);
			command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
			command.Parameters.Add("@blob_field", FbDbType.Binary).Value = Encoding.UTF8.GetBytes("Blob field update with implicit transaction");

			var i = command.ExecuteNonQuery();

			Assert.AreEqual(i, 1, "Blob field update with implicit transaction failed");

			command.Dispose();
		}

		[Test]
		public void UpdatedArrayFieldTest()
		{
			var values = new int[4];

			values[0] = 10;
			values[1] = 20;
			values[2] = 30;
			values[3] = 40;

			var command = new FbCommand("update TEST set iarray_field = @iarray_field where int_field = @int_field", Connection);
			command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
			command.Parameters.Add("@iarray_field", FbDbType.Array).Value = values;

			var i = command.ExecuteNonQuery();

			Assert.AreEqual(i, 1, "Array field update with implicit transaction failed");

			command.Dispose();
		}

		#endregion
	}
}
