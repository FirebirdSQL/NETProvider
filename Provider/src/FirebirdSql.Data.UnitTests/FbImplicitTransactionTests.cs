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
using System.Text;

using NUnit.Framework;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.UnitTests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	[FbTestFixture(FbServerType.Embedded, default(bool))]
	public class FbImplicitTransactionTests : TestsBase
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
			FbCommand command = new FbCommand("select * from TEST where DATE_FIELD = ?", Connection);
			FbDataAdapter adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@DATE_FIELD", FbDbType.Date, 4, "DATE_FIELD").Value = new DateTime(2003, 1, 5);

			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST");

			TestContext.WriteLine();
			TestContext.WriteLine("Implicit transactions - DataAdapter Fill Method - Test");

			foreach (DataTable table in ds.Tables)
			{
				foreach (DataColumn col in table.Columns)
				{
					TestContext.Write(col.ColumnName + "\t\t");
				}

				TestContext.WriteLine();

				foreach (DataRow row in table.Rows)
				{
					for (int i = 0; i < table.Columns.Count; i++)
					{
						TestContext.Write(row[i] + "\t\t");
					}

					TestContext.WriteLine("");
				}
			}

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void MultipleDataAdapterFillTest()
		{
			FbCommand command = new FbCommand("select * from TEST where DATE_FIELD = ?", Connection);
			FbDataAdapter adapter = new FbDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@DATE_FIELD", FbDbType.Date, 4, "DATE_FIELD").Value = new DateTime(2003, 1, 5);

			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "TEST");

			TestContext.WriteLine();
			TestContext.WriteLine("Implicit transactions - DataAdapter Fill Method - Test");

			foreach (DataTable table in ds.Tables)
			{
				foreach (DataColumn col in table.Columns)
				{
					TestContext.Write(col.ColumnName + "\t\t");
				}

				TestContext.WriteLine();

				foreach (DataRow row in table.Rows)
				{
					for (int i = 0; i < table.Columns.Count; i++)
					{
						TestContext.Write(row[i] + "\t\t");
					}

					TestContext.WriteLine("");
				}
			}

			adapter.SelectCommand.Parameters[0].Value = new DateTime(2003, 1, 6);

			ds = new DataSet();
			adapter.Fill(ds, "TEST");

			TestContext.WriteLine();
			TestContext.WriteLine("Implicit transactions - DataAdapter Fill Method - Test");

			foreach (DataTable table in ds.Tables)
			{
				foreach (DataColumn col in table.Columns)
				{
					TestContext.Write(col.ColumnName + "\t\t");
				}

				TestContext.WriteLine();

				foreach (DataRow row in table.Rows)
				{
					for (int i = 0; i < table.Columns.Count; i++)
					{
						TestContext.Write(row[i] + "\t\t");
					}

					TestContext.WriteLine("");
				}
			}


			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void ExecuteScalarTest()
		{
			FbCommand command = new FbCommand("select sum(int_field) from TEST", Connection);

			Assert.DoesNotThrow(() => command.ExecuteScalar());

			command.Dispose();
		}

		[Test]
		public void UpdatedClobFieldTest()
		{
			FbCommand command = new FbCommand("update TEST set clob_field = @clob_field where int_field = @int_field", Connection);
			command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
			command.Parameters.Add("@clob_field", FbDbType.Text).Value = "Clob field update with implicit transaction";

			int i = command.ExecuteNonQuery();

			Assert.AreEqual(i, 1, "Clob field update with implicit transaction failed");

			// Force the implicit transaction to be committed
			command.Dispose();
		}

		[Test]
		public void UpdatedBlobFieldTest()
		{
			FbCommand command = new FbCommand("update TEST set blob_field = @blob_field where int_field = @int_field", Connection);
			command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
			command.Parameters.Add("@blob_field", FbDbType.Binary).Value = Encoding.UTF8.GetBytes("Blob field update with implicit transaction");

			int i = command.ExecuteNonQuery();

			Assert.AreEqual(i, 1, "Blob field update with implicit transaction failed");

			// Force the implicit transaction to be committed
			command.Dispose();
		}

		[Test]
		public void UpdatedArrayFieldTest()
		{
			int[] values = new int[4];

			values[0] = 10;
			values[1] = 20;
			values[2] = 30;
			values[3] = 40;

			FbCommand command = new FbCommand("update TEST set iarray_field = @iarray_field where int_field = @int_field", Connection);
			command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
			command.Parameters.Add("@iarray_field", FbDbType.Array).Value = values;

			int i = command.ExecuteNonQuery();

			Assert.AreEqual(i, 1, "Array field update with implicit transaction failed");

			// Force the implicit transaction to be committed
			command.Dispose();
		}

		#endregion
	}
}
