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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class TrackerIssuesTests : FbTestsBase
	{
		public TrackerIssuesTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[Test]
		public async Task DNET217_ReadingALotOfFields()
		{
			var timestampExpression = await GetServerVersion() >= new Version(4, 0, 0, 0) ? "localtimestamp" : "current_timestamp";

			var cols = new StringBuilder();
			var separator = string.Empty;
			for (var i = 0; i < 1235; i++)
			{
				if (i % 2 == 0)
					cols.AppendFormat("{0}'r' as col{1}", separator, i);
				else
					cols.AppendFormat("{0}24 as col{1}", separator, i);

				separator = ",";
			}
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = $"select {cols} from rdb$database where 'x' = @x or 'x' = @x and {timestampExpression} = @y and {timestampExpression} = @y and {timestampExpression} = @y and {timestampExpression} = @y and {timestampExpression} = @y and {timestampExpression} = @y and {timestampExpression} = @y and {timestampExpression} = @y and {timestampExpression} = @y and {timestampExpression} = @y and {timestampExpression} = @y";
				cmd.Parameters.Add(new FbParameter() { ParameterName = "@x", Value = "z" });
				cmd.Parameters.Add(new FbParameter() { ParameterName = "@y", Value = DateTime.Now });
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{ }
				}
			}
		}

		[Test]
		public async Task DNET260_ProcedureWithALotOfParameters()
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
RECREATE PROCEDURE TEST_SP (
  P01 SMALLINT,
  P02 INTEGER,
  P03 INTEGER,
  P04 FLOAT,
  P05 INTEGER,
  P06 INTEGER,
  P07 DATE,
  P08 DATE )
RETURNS (
  R01 FLOAT,
  R02 FLOAT,
  R03 FLOAT,
  R04 FLOAT,
  R05 FLOAT,
  R06 FLOAT,
  R07 FLOAT,
  R08 FLOAT,
  R09 FLOAT,
  R10 FLOAT,
  R11 FLOAT,
  R12 FLOAT,
  R13 FLOAT,
  R14 FLOAT,
  R15 FLOAT,
  R16 FLOAT,
  R17 FLOAT,
  R18 FLOAT,
  R19 FLOAT,
  R20 FLOAT,
  R21 FLOAT,
  R22 FLOAT,
  R23 FLOAT,
  R24 FLOAT,
  R25 FLOAT,
  R26 FLOAT,
  R27 FLOAT,
  R28 FLOAT,
  R29 FLOAT,
  R30 FLOAT,
  R31 FLOAT,
  R32 FLOAT,
  R33 FLOAT,
  R34 FLOAT,
  R35 FLOAT,
  R36 FLOAT,
  R37 FLOAT,
  R38 FLOAT,
  R39 FLOAT,
  R40 FLOAT,
  R41 FLOAT,
  R42 FLOAT,
  R43 FLOAT,
  R44 FLOAT,
  R45 FLOAT,
  R46 FLOAT,
  R47 FLOAT,
  R48 FLOAT,
  R49 FLOAT,
  R50 FLOAT,
  R51 FLOAT,
  R52 FLOAT,
  R53 FLOAT,
  R54 FLOAT,
  R55 FLOAT,
  R56 FLOAT,
  R57 FLOAT,
  R58 FLOAT,
  R59 FLOAT,
  R60 FLOAT,
  R61 FLOAT,
  R62 FLOAT,
  R63 FLOAT,
  R64 FLOAT,
  R65 FLOAT,
  R66 FLOAT,
  R67 FLOAT,
  R68 FLOAT,
  R69 FLOAT,
  R70 FLOAT,
  R71 FLOAT,
  R72 FLOAT,
  R73 FLOAT,
  R74 FLOAT,
  R75 FLOAT,
  R76 FLOAT,
  R77 FLOAT,
  R78 FLOAT,
  R79 FLOAT,
  R80 FLOAT,
  R81 FLOAT,
  R82 FLOAT,
  R83 FLOAT,
  R84 FLOAT,
  R85 FLOAT,
  R86 FLOAT,
  R87 FLOAT,
  R88 FLOAT,
  R89 FLOAT,
  R90 FLOAT,
  R91 FLOAT,
  R92 FLOAT,
  R93 FLOAT,
  R94 FLOAT,
  R95 FLOAT )
AS
BEGIN
  SUSPEND;
END
";
				await cmd.ExecuteNonQueryAsync();
			}

			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "TEST_SP";
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.Add(new FbParameter() { Value = 1 });
				cmd.Parameters.Add(new FbParameter() { Value = 1 });
				cmd.Parameters.Add(new FbParameter() { Value = 1 });
				cmd.Parameters.Add(new FbParameter() { Value = 1 });
				cmd.Parameters.Add(new FbParameter() { Value = 1 });
				cmd.Parameters.Add(new FbParameter() { Value = 1 });
				cmd.Parameters.Add(new FbParameter() { Value = DateTime.Today });
				cmd.Parameters.Add(new FbParameter() { Value = DateTime.Today });

				await cmd.ExecuteNonQueryAsync();
			}
		}

		[Test]
		public async Task DNET273_WritingClobAsBinary()
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "insert into test (INT_FIELD, CLOB_FIELD) values (@INT_FIELD, @CLOB_FIELD)";
				cmd.Parameters.Add("@INT_FIELD", FbDbType.Integer).Value = 100;
				cmd.Parameters.Add("@CLOB_FIELD", FbDbType.Binary).Value = new byte[] { 0x00, 0x001 };
				await cmd.ExecuteNonQueryAsync();
			}
		}

		[Test]
		public async Task DNET595_ProperConnectionPoolConnectionsClosing()
		{
			FbConnection.ClearAllPools();
			const int NumberOfThreads = 15;

			var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
			csb.Pooling = true;
			csb.ConnectionLifeTime = 5;
			csb.MinPoolSize = 0;
			var cs = csb.ToString();

			var active = await GetActiveConnections();

			var tasks = new List<Task>();
			for (var i = 0; i < NumberOfThreads; i++)
			{
				tasks.Add(GetSomethingLoopHelper(cs, 50));
			}
			await Task.WhenAll(tasks);

			Assert.Greater(await GetActiveConnections(), active);

			var sw = new Stopwatch();
			sw.Start();
			while (sw.Elapsed.TotalSeconds < 60)
			{
				await GetSomethingHelper(cs);
			}

			Thread.Sleep(TimeSpan.FromSeconds(csb.ConnectionLifeTime * 2));
			Assert.AreEqual(active, await GetActiveConnections());

			static async Task GetSomethingHelper(string connectionString)
			{
				await using (var conn = new FbConnection(connectionString))
				{
					await conn.OpenAsync();
					await using (var command = new FbCommand("select current_timestamp from mon$database", conn))
					{
						await command.ExecuteScalarAsync();
					}
				}
			}

			static async Task GetSomethingLoopHelper(string connectionString, int loop)
			{
				for (var i = 0; i < loop; i++)
				{
					await GetSomethingHelper(connectionString);
				}
			}
		}

		[Test]
		public async Task DNET313_MultiDimensionalArray()
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
CREATE TABLE TABMAT (
    ID INTEGER NOT NULL,
	MATRIX INTEGER[1:3, 1:4]
)";
				await cmd.ExecuteNonQueryAsync();
			}
			try
			{
				var sql = "INSERT INTO TabMat (Id,Matrix) Values(@ValId,@ValMat)";
				int[,] mat = { { 1, 2, 3, 4 }, { 10, 20, 30, 40 }, { 101, 102, 103, 104 } };
				var random = new Random();
				await using (var tx = await Connection.BeginTransactionAsync())
				{
					await using (var cmd = new FbCommand(sql, Connection, tx))
					{
						cmd.Parameters.Add("@ValId", FbDbType.Integer).Value = random.Next();
						cmd.Parameters.Add("@ValMat", FbDbType.Array).Value = mat;
						await cmd.ExecuteNonQueryAsync();
					}
					await tx.CommitAsync();
				}
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = @"select matrix from tabmat";
					await using (var reader = await cmd.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							Assert.AreEqual(mat, reader[0]);
						}
						else
						{
							Assert.Fail();
						}
					}
				}
			}
			finally
			{
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "drop table tabmat";
					await cmd.ExecuteNonQueryAsync();
				}
			}
		}

		[Test]
		public async Task DNET304_VarcharOctetsParameterRoundtrip()
		{
			var data = new byte[] { 10, 20 };
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.Parameters.Add(new FbParameter() { ParameterName = "@x", Value = data });
				cmd.CommandText = "select cast(@x as varchar(10) character set octets) from rdb$database";
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						Assert.AreEqual(data, reader[0]);
					}
				}
			}
		}

		[Test]
		public async Task DNET304_CharOctetsParameterRoundtrip()
		{
			var data = new byte[] { 10, 20 };
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.Parameters.Add(new FbParameter() { ParameterName = "@x", Value = data });
				cmd.CommandText = "select cast(@x as char(10) character set octets) from rdb$database";
				await using (var reader = await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						Assert.AreEqual(new byte[] { data[0], data[1], 32, 32, 32, 32, 32, 32, 32, 32 }, reader[0]);
					}
				}
			}
		}

		[Test]
		public async Task DNET1036_ReadNumericScaleZero()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select cast(3 as numeric(18,0)) from rdb$database", Connection, transaction))
				{
					await using (var reader = await command.ExecuteReaderAsync())
					{
						await reader.ReadAsync();
						Assert.AreEqual(3m, reader[0]);
					}
				}
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task DNET1036_ReadDecimalScaleZero()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select cast(3 as decimal(18,0)) from rdb$database", Connection, transaction))
				{
					await using (var reader = await command.ExecuteReaderAsync())
					{
						await reader.ReadAsync();
						Assert.AreEqual(3m, reader[0]);
					}
				}
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task DNET1036_WriteNumericScaleZero()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select cast(@value as numeric(18,0)) from rdb$database", Connection, transaction))
				{
					command.Parameters.AddWithValue("value", 3m);
					await using (var reader = await command.ExecuteReaderAsync())
					{
						await reader.ReadAsync();
						Assert.AreEqual(3m, reader[0]);
					}
				}
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task DNET1036_WriteDecimalScaleZero()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select cast(@value as decimal(18,0)) from rdb$database", Connection, transaction))
				{
					command.Parameters.AddWithValue("value", 3m);
					await using (var reader = await command.ExecuteReaderAsync())
					{
						await reader.ReadAsync();
						Assert.AreEqual(3m, reader[0]);
					}
				}
				await transaction.RollbackAsync();
			}
		}
	}
}
