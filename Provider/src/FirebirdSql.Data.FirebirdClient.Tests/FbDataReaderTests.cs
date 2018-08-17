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
	public class FbDataReaderTests : FbTestsBase
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
			var transaction = Connection.BeginTransaction();

			var command = new FbCommand("select * from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				for (var i = 0; i < reader.FieldCount; i++)
				{
					reader.GetValue(i);
				}
			}

			reader.Close();
			command.Dispose();
			transaction.Rollback();
		}

		[Test]
		public void ReadClobTest()
		{
			var transaction = Connection.BeginTransaction();

			var command = new FbCommand("select * from TEST", Connection, transaction);

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
			var transaction = Connection.BeginTransaction();

			var command = new FbCommand("select * from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				reader.GetString(reader.GetOrdinal("bigint_field"));
			}

			reader.Close();
			command.Dispose();
			transaction.Rollback();
		}

		[Test]
		public void GetValuesTest()
		{
			var transaction = Connection.BeginTransaction();

			var command = new FbCommand("select * from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				var values = new object[reader.FieldCount];
				reader.GetValues(values);
			}

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		public void IndexerByIndexTest()
		{
			var transaction = Connection.BeginTransaction();

			var command = new FbCommand("select * from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				for (var i = 0; i < reader.FieldCount; i++)
				{
					var dummy = reader[i];
				}
			}

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		public void IndexerByNameTest()
		{
			var transaction = Connection.BeginTransaction();

			var command = new FbCommand("select * from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				for (var i = 0; i < reader.FieldCount; i++)
				{
					var dummy = reader[reader.GetName(i)];
				}
			}

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		public void GetSchemaTableTest()
		{
			var transaction = Connection.BeginTransaction();
			var command = new FbCommand("select * from TEST", Connection, transaction);

			var reader = command.ExecuteReader(CommandBehavior.SchemaOnly);

			var schema = reader.GetSchemaTable();
			var currRows = schema.Select(null, null, DataViewRowState.CurrentRows);

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		public void GetSchemaTableWithExpressionFieldTest()
		{
			var transaction = Connection.BeginTransaction();
			var command = new FbCommand("select TEST.*, 0 AS VALOR from TEST", Connection, transaction);

			var reader = command.ExecuteReader(CommandBehavior.SchemaOnly);

			var schema = reader.GetSchemaTable();
			var currRows = schema.Select(null, null, DataViewRowState.CurrentRows);

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		public void RecordAffectedTest()
		{
			var sql = "insert into test (int_field) values (100000)";

			var command = new FbCommand(sql, Connection);

			var reader = command.ExecuteReader();

			while (reader.Read())
			{
			}

			reader.Close();

			Assert.AreEqual(1, reader.RecordsAffected, "RecordsAffected value is incorrect");
		}

		[Test]
		public void GetBytesLengthTest()
		{
			var sql = "select blob_field from TEST where int_field = @int_field";

			var command = new FbCommand(sql, Connection);
			command.Parameters.Add("@int_field", FbDbType.Integer).Value = 2;

			var reader = command.ExecuteReader();

			reader.Read();

			var length = reader.GetBytes(0, 0, null, 0, 0);

			reader.Close();

			Assert.AreEqual(13, length, "Incorrect blob length");
		}

		[Test]
		public void GetCharsLengthTest()
		{
			var sql = "select clob_field from TEST where int_field = @int_field";

			var command = new FbCommand(sql, Connection);
			command.Parameters.Add("@int_field", FbDbType.Integer).Value = 50;

			var reader = command.ExecuteReader();

			reader.Read();

			var length = reader.GetChars(0, 0, null, 0, 0);

			reader.Close();

			Assert.AreEqual(14, length, "Incorrect clob length");
		}

		[Test]
		public void ValidateDecimalSchema()
		{
			var sql = "select decimal_field from test";

			var test = new FbCommand(sql, Connection);
			var r = test.ExecuteReader(CommandBehavior.SchemaOnly);

			var schema = r.GetSchemaTable();

			r.Close();

			// Check schema values
			Assert.AreEqual(schema.Rows[0]["ColumnSize"], 8, "Invalid length");
			Assert.AreEqual(schema.Rows[0]["NumericPrecision"], 15, "Invalid precision");
			Assert.AreEqual(schema.Rows[0]["NumericScale"], 2, "Invalid scale");
		}

		[Test]
		public void DisposeTest()
		{
			using (var command = new FbCommand("DATAREADERTEST", Connection))
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
			var transaction = Connection.BeginTransaction();

			var command = new FbCommand("select first 1 0 as fOo, 0 as \"BaR\", 0 as BAR from TEST", Connection, transaction);

			IDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				var foo = reader.GetOrdinal("foo");
				var FOO = reader.GetOrdinal("FOO");
				var fOo = reader.GetOrdinal("fOo");
				Assert.AreEqual(0, foo);
				Assert.AreEqual(0, FOO);
				Assert.AreEqual(0, fOo);

				var bar = reader.GetOrdinal("bar");
				var BaR = reader.GetOrdinal("BaR");
				Assert.AreEqual(1, bar);
				Assert.AreEqual(1, BaR);

				var BAR = reader.GetOrdinal("BAR");
				Assert.AreEqual(2, BAR);
			}

			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

		[Test]
		public void ReadBinaryTest()
		{
			var transaction = Connection.BeginTransaction();

			var bytes = new byte[1024];
			var random = new Random();
			for (var i = 0; i < bytes.Length; i++)
			{
				bytes[i] = (byte)random.Next(byte.MinValue, byte.MaxValue);
			}
			var binaryString = $"x'{BitConverter.ToString(bytes).Replace("-", string.Empty)}'";

			var command = new FbCommand($"select {binaryString} from TEST", Connection, transaction);

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
		public void ReadGuidRoundTripTest()
		{
			using (var cmd = Connection.CreateCommand())
			{
				var guid = Guid.NewGuid();
				var commandText = $"select char_to_uuid('{guid}') from rdb$database";
				cmd.CommandText = commandText;
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						Assert.AreEqual(guid, reader.GetGuid(0));
					}
					else
					{
						Assert.Fail();
					}
				}
			}
		}

		[Test]
		public void ReadGuidRoundTrip2Test()
		{
			using (var cmd = Connection.CreateCommand())
			{
				var commandText = @"
execute block
returns (a varchar(16) character set octets, b varchar(36) character set ascii)
as
declare guid varchar(16) character set octets;
begin
	guid = gen_uuid();
	for select :guid, uuid_to_char(:guid) from rdb$database into a, b do
	begin
		suspend;
	end
end";
				cmd.CommandText = commandText;
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						StringAssert.AreEqualIgnoringCase(reader.GetString(1), reader.GetGuid(0).ToString());
					}
					else
					{
						Assert.Fail();
					}
				}
			}
		}

		[Test]
		public void ReadGuidRoundTrip3Test()
		{
			using (var cmd = Connection.CreateCommand())
			{
				var guid = Guid.NewGuid();
				var commandText = $"select cast(@guid as varchar(16) character set octets) from rdb$database";
				cmd.CommandText = commandText;
				cmd.Parameters.Add("guid", guid);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						Assert.AreEqual(guid, reader.GetGuid(0));
					}
					else
					{
						Assert.Fail();
					}
				}
			}
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
