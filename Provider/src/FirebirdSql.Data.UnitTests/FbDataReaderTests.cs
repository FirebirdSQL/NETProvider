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

using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	[FbTestFixture(FbServerType.Embedded, default(bool))]
	public class FbDataReaderTests : TestsBase
	{
		#region Constructors

		public FbDataReaderTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{ }

		#endregion

		#region Unit Tests

		[Test]
		public void ReadTest()
		{
			FbTransaction transaction = Connection.BeginTransaction();

			FbCommand command = new FbCommand("select * from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				for (int i = 0; i < reader.FieldCount; i++)
				{
					TestContext.Write(reader.GetValue(i) + "\t");
				}

				TestContext.WriteLine();
			}

			reader.Close();
			command.Dispose();
			transaction.Rollback();
		}

		[Test]
		public void ReadClobTest()
		{
			FbTransaction transaction = Connection.BeginTransaction();

			FbCommand command = new FbCommand("select * from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				reader.GetValue(reader.GetOrdinal("clob_field"));
			}

			reader.Close();
			command.Dispose();
			transaction.Rollback();
		}

		[Test]
		public void BigIntGetStringTest()
		{
			FbTransaction transaction = Connection.BeginTransaction();

			FbCommand command = new FbCommand("select * from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				TestContext.WriteLine(reader.GetString(reader.GetOrdinal("bigint_field")) + "\t");
			}

			reader.Close();
			command.Dispose();
			transaction.Rollback();
		}

		[Test]
		public void GetValuesTest()
		{
			FbTransaction transaction = Connection.BeginTransaction();

			FbCommand command = new FbCommand("select * from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				object[] values = new object[reader.FieldCount];
				reader.GetValues(values);

				for (int i = 0; i < values.Length; i++)
				{
					TestContext.Write(values[i] + "\t");
				}
				TestContext.WriteLine();
			}

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		public void IndexerByIndexTest()
		{
			FbTransaction transaction = Connection.BeginTransaction();

			FbCommand command = new FbCommand("select * from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				for (int i = 0; i < reader.FieldCount; i++)
				{
					TestContext.Write(reader[i] + "\t");
				}
				TestContext.WriteLine();
			}

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		public void IndexerByNameTest()
		{
			FbTransaction transaction = Connection.BeginTransaction();

			FbCommand command = new FbCommand("select * from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				for (int i = 0; i < reader.FieldCount; i++)
				{
					TestContext.Write(reader[reader.GetName(i)] + "\t");
				}
				TestContext.WriteLine();
			}

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		public void GetSchemaTableTest()
		{
			FbTransaction transaction = Connection.BeginTransaction();
			FbCommand command = new FbCommand("select * from TEST", Connection, transaction);

			FbDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);

			DataTable schema = reader.GetSchemaTable();
			DataRow[] currRows = schema.Select(null, null, DataViewRowState.CurrentRows);

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		public void GetSchemaTableWithExpressionFieldTest()
		{
			FbTransaction transaction = Connection.BeginTransaction();
			FbCommand command = new FbCommand("select TEST.*, 0 AS VALOR from TEST", Connection, transaction);

			FbDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);

			DataTable schema = reader.GetSchemaTable();
			DataRow[] currRows = schema.Select(null, null, DataViewRowState.CurrentRows);

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		[Ignore("Not supported")]
		public void NextResultTest()
		{
			string querys = "select * from TEST order by INT_FIELD asc;" +
							"select * from TEST order by INT_FIELD desc;";

			FbTransaction transaction = Connection.BeginTransaction();
			FbCommand command = new FbCommand(querys, Connection, transaction);

			FbDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				for (int i = 0; i < reader.FieldCount; i++)
				{
					TestContext.Write(reader.GetValue(i) + "\t");
				}
				TestContext.WriteLine();
			}

			TestContext.WriteLine("===");

			if (reader.NextResult())
			{
				while (reader.Read())
				{
					for (int i = 0; i < reader.FieldCount; i++)
					{
						TestContext.Write(reader.GetValue(i) + "\t");
					}
					TestContext.WriteLine();
				}
			}

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		public void RecordAffectedTest()
		{
			string sql = "insert into test (int_field) values (100000)";

			FbCommand command = new FbCommand(sql, Connection);

			FbDataReader reader = command.ExecuteReader();

			bool nextResult = true;

			while (nextResult)
			{
				while (reader.Read())
				{
				}

				nextResult = reader.NextResult();
			}

			reader.Close();

			Assert.AreEqual(1, reader.RecordsAffected, "RecordsAffected value is incorrect");
		}

		[Test]
		public void GetBytesLengthTest()
		{
			string sql = "select blob_field from TEST where int_field = @int_field";

			FbCommand command = new FbCommand(sql, Connection);
			command.Parameters.Add("@int_field", FbDbType.Integer).Value = 2;

			FbDataReader reader = command.ExecuteReader();

			reader.Read();

			long length = reader.GetBytes(0, 0, null, 0, 0);

			reader.Close();

			Assert.AreEqual(13, length, "Incorrect blob length");
		}

		[Test]
		public void GetCharsLengthTest()
		{
			string sql = "select clob_field from TEST where int_field = @int_field";

			FbCommand command = new FbCommand(sql, Connection);
			command.Parameters.Add("@int_field", FbDbType.Integer).Value = 50;

			FbDataReader reader = command.ExecuteReader();

			reader.Read();

			long length = reader.GetChars(0, 0, null, 0, 0);

			reader.Close();

			Assert.AreEqual(14, length, "Incorrect clob length");
		}

		[Test]
		public void ValidateDecimalSchema()
		{
			string sql = "select decimal_field from test";

			FbCommand test = new FbCommand(sql, Connection);
			FbDataReader r = test.ExecuteReader(CommandBehavior.SchemaOnly);

			DataTable schema = r.GetSchemaTable();

			r.Close();

			// Check schema values
			Assert.AreEqual(schema.Rows[0]["ColumnSize"], 8, "Invalid length");
			Assert.AreEqual(schema.Rows[0]["NumericPrecision"], 15, "Invalid precision");
			Assert.AreEqual(schema.Rows[0]["NumericScale"], 2, "Invalid scale");
		}

		[Test]
		public void DisposeTest()
		{
			using (FbCommand command = new FbCommand("DATAREADERTEST", Connection))
			{
				command.CommandType = CommandType.StoredProcedure;

				FbCommandBuilder.DeriveParameters(command);

				using (IDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
					}
				}
			}
		}

		[Test]
		public void GetOrdinalTest()
		{
			FbTransaction transaction = Connection.BeginTransaction();

			FbCommand command = new FbCommand("select first 1 0 as fOo, 0 as \"BaR\", 0 as BAR from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				int foo = reader.GetOrdinal("foo");
				int FOO = reader.GetOrdinal("FOO");
				int fOo = reader.GetOrdinal("fOo");
				Assert.AreEqual(0, foo);
				Assert.AreEqual(0, FOO);
				Assert.AreEqual(0, fOo);

				int bar = reader.GetOrdinal("bar");
				int BaR = reader.GetOrdinal("BaR");
				Assert.AreEqual(1, bar);
				Assert.AreEqual(1, BaR);

				int BAR = reader.GetOrdinal("BAR");
				Assert.AreEqual(2, BAR);
			}

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		public void ReadBinaryTest()
		{
			FbTransaction transaction = Connection.BeginTransaction();

			byte[] bytes = new byte[1024];
			var random = new Random();
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = (byte)random.Next(byte.MinValue, byte.MaxValue);
			}
			var binaryString = $"x'{BitConverter.ToString(bytes).Replace("-", string.Empty)}'";

			FbCommand command = new FbCommand($"select {binaryString} from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			if (reader.Read())
			{
				reader.GetValue(0);
			}

			reader.Close();
			command.Dispose();
			transaction.Rollback();
		}

		[Test]
		public void DNET60_EmptyFieldReadingError()
		{
			using (var command = Connection.CreateCommand())
			{
				command.CommandText = "select '' AS EmptyColumn from rdb$database";

				using (var r = command.ExecuteReader())
				{
					while (r.Read())
					{
					}
				}
			}
		}

		[Test]
		public void DNET183_VarcharSpacesShouldNotBeTrimmed()
		{
			const string value = "foo  ";

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select cast(@foo as varchar(5)) from rdb$database";
				cmd.Parameters.Add(new FbParameter() { ParameterName = "@foo", FbDbType = FbDbType.VarChar, Size = 5, Value = value });
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						Assert.AreEqual(value, (string)reader[0]);
					}
				}
			}
		}

		[Test]
		public void DNET749_CommandBehaviorCloseConnectionStackOverflow()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "select * from rdb$database";
				var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
				{
					while (reader.Read())
					{

					}
				}
			}
		}

		#endregion
	}
}
