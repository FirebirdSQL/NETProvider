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
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class FbCommandBuilderTests : FbTestsBase
	{
		private FbDataAdapter _adapter;

		public FbCommandBuilderTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[SetUp]
		public override async Task SetUp()
		{
			await base.SetUp();
			_adapter = new FbDataAdapter(new FbCommand("select * from TEST where VARCHAR_FIELD = ?", Connection));
		}

		[TearDown]
		public override async Task TearDown()
		{
			_adapter.Dispose();
			await base.TearDown();
		}

		[Test]
		public void GetInsertCommandTest()
		{
			using (var builder = new FbCommandBuilder(_adapter))
			{
				StringAssert.StartsWith("INSERT", builder.GetInsertCommand().CommandText);
			}
		}

		[Test]
		public void GetUpdateCommandTest()
		{
			using (var builder = new FbCommandBuilder(_adapter))
			{
				StringAssert.StartsWith("UPDATE", builder.GetUpdateCommand().CommandText);
			}
		}

		[Test]
		public void GetDeleteCommandTest()
		{
			using (var builder = new FbCommandBuilder(_adapter))
			{
				StringAssert.StartsWith("DELETE", builder.GetDeleteCommand().CommandText);
			}
		}

		[Test]
		public void RefreshSchemaTest()
		{
			using (var builder = new FbCommandBuilder(_adapter))
			{
				Assert.DoesNotThrow(() => builder.GetInsertCommand());
				Assert.DoesNotThrow(() => builder.GetUpdateCommand());
				Assert.DoesNotThrow(() => builder.GetDeleteCommand());

				_adapter.SelectCommand.CommandText = "select * from TEST where BIGINT_FIELD = ?";

				builder.RefreshSchema();

				Assert.DoesNotThrow(() => builder.GetInsertCommand());
				Assert.DoesNotThrow(() => builder.GetUpdateCommand());
				Assert.DoesNotThrow(() => builder.GetDeleteCommand());
			}
		}

		[Test]
		public void CommandBuilderWithExpressionFieldTest()
		{
			_adapter.SelectCommand.CommandText = "select TEST.*, 0 AS VALOR from TEST";

			using (var builder = new FbCommandBuilder(_adapter))
			{
				StringAssert.DoesNotContain("VALOR", builder.GetUpdateCommand().CommandText);
			}
		}

		[Test]
		public async Task DeriveParameters()
		{
			await using (var command = new FbCommand("GETVARCHARFIELD", Connection))
			{
				command.CommandType = CommandType.StoredProcedure;
				FbCommandBuilder.DeriveParameters(command);
				Assert.AreEqual(2, command.Parameters.Count);
			}
		}

		[Test]
		public async Task DeriveParameters2()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("GETVARCHARFIELD", Connection, transaction))
				{
					command.CommandType = CommandType.StoredProcedure;
					FbCommandBuilder.DeriveParameters(command);
					Assert.AreEqual(2, command.Parameters.Count);
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task DeriveParametersNonExistingSP()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("BlaBlaBla", Connection, transaction))
				{
					command.CommandType = CommandType.StoredProcedure;
					Assert.Throws<InvalidOperationException>(() => FbCommandBuilder.DeriveParameters(command));
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task TestWithClosedConnection()
		{
			await Connection.CloseAsync();

			using (var builder = new FbCommandBuilder(_adapter))
			{
				Assert.DoesNotThrow(() => builder.GetInsertCommand());
				Assert.DoesNotThrow(() => builder.GetUpdateCommand());
				Assert.DoesNotThrow(() => builder.GetDeleteCommand());

				_adapter.SelectCommand.CommandText = "select * from TEST where BIGINT_FIELD = ?";

				builder.RefreshSchema();

				Assert.DoesNotThrow(() => builder.GetInsertCommand());
				Assert.DoesNotThrow(() => builder.GetUpdateCommand());
				Assert.DoesNotThrow(() => builder.GetDeleteCommand());
			}
		}
	}
}
