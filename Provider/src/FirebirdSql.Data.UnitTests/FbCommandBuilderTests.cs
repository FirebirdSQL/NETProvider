/*
 *	Firebird ADO.NET Data provider for .NET	and	Mono
 *
 *	   The contents	of this	file are subject to	the	Initial
 *	   Developer's Public License Version 1.0 (the "License");
 *	   you may not use this	file except	in compliance with the
 *	   License.	You	may	obtain a copy of the License at
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software	distributed	under the License is distributed on
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *	   express or implied.	See	the	License	for	the	specific
 *	   language	governing rights and limitations under the License.
 *
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All	Rights Reserved.
 *
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
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
	public class FbCommandBuilderTests : TestsBase
	{
		#region Fields

		private FbDataAdapter _adapter;

		#endregion

		#region Constructors

		public FbCommandBuilderTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{ }

		#endregion

		#region SetUp and TearDown methods

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			_adapter = new FbDataAdapter(new FbCommand("select * from TEST where	VARCHAR_FIELD =	?", Connection));
		}

		[TearDown]
		public override void TearDown()
		{
			_adapter.Dispose();
			base.TearDown();
		}

		#endregion

		#region Unit Tests

		[Test]
		public void GetInsertCommandTest()
		{
			FbCommandBuilder builder = new FbCommandBuilder(_adapter);

			TestContext.WriteLine(builder.GetInsertCommand().CommandText);

			builder.Dispose();
		}

		[Test]
		public void GetUpdateCommandTest()
		{
			FbCommandBuilder builder = new FbCommandBuilder(_adapter);

			TestContext.WriteLine(builder.GetUpdateCommand().CommandText);

			builder.Dispose();
		}

		[Test]
		public void GetDeleteCommandTest()
		{
			FbCommandBuilder builder = new FbCommandBuilder(_adapter);

			TestContext.WriteLine(builder.GetDeleteCommand().CommandText);

			builder.Dispose();
		}

		[Test]
		public void RefreshSchemaTest()
		{
			FbCommandBuilder builder = new FbCommandBuilder(_adapter);

			TestContext.WriteLine(builder.GetInsertCommand().CommandText);
			TestContext.WriteLine(builder.GetUpdateCommand().CommandText);
			TestContext.WriteLine(builder.GetDeleteCommand().CommandText);

			_adapter.SelectCommand.CommandText = "select	* from TEST	where BIGINT_FIELD = ?";

			builder.RefreshSchema();

			TestContext.WriteLine(builder.GetInsertCommand().CommandText);
			TestContext.WriteLine(builder.GetUpdateCommand().CommandText);
			TestContext.WriteLine(builder.GetDeleteCommand().CommandText);

			builder.Dispose();
		}

		[Test]
		public void CommandBuilderWithExpressionFieldTest()
		{
			_adapter.SelectCommand.CommandText = "select	TEST.*,	0 AS VALOR from	TEST";

			FbCommandBuilder builder = new FbCommandBuilder(_adapter);

			TestContext.WriteLine(builder.GetUpdateCommand().CommandText);

			builder.Dispose();
		}

		[Test]
		public void DeriveParameters()
		{
			FbCommand command = new FbCommand("GETVARCHARFIELD", Connection);

			command.CommandType = CommandType.StoredProcedure;

			FbCommandBuilder.DeriveParameters(command);

			Assert.AreEqual(2, command.Parameters.Count);
		}

		[Test]
		public void DeriveParameters2()
		{
			FbTransaction transaction = Connection.BeginTransaction();

			FbCommand command = new FbCommand("GETVARCHARFIELD", Connection, transaction);

			command.CommandType = CommandType.StoredProcedure;

			FbCommandBuilder.DeriveParameters(command);

			Assert.AreEqual(2, command.Parameters.Count);

			transaction.Commit();
		}

		[Test]
		public void DeriveParametersNonExistingSP()
		{
			Assert.Throws<InvalidOperationException>(() =>
			{
				FbTransaction transaction = Connection.BeginTransaction();

				FbCommand command = new FbCommand("BlaBlaBla", Connection, transaction);

				command.CommandType = CommandType.StoredProcedure;

				FbCommandBuilder.DeriveParameters(command);

				transaction.Commit();
			});
		}

		[Test]
		public void TestWithClosedConnection()
		{
			Connection.Close();

			FbCommandBuilder builder = new FbCommandBuilder(_adapter);

			TestContext.WriteLine(builder.GetInsertCommand().CommandText);
			TestContext.WriteLine(builder.GetUpdateCommand().CommandText);
			TestContext.WriteLine(builder.GetDeleteCommand().CommandText);

			_adapter.SelectCommand.CommandText = "select	* from TEST	where BIGINT_FIELD = ?";

			builder.RefreshSchema();

			TestContext.WriteLine(builder.GetInsertCommand().CommandText);
			TestContext.WriteLine(builder.GetUpdateCommand().CommandText);
			TestContext.WriteLine(builder.GetDeleteCommand().CommandText);

			builder.Dispose();
		}

		#endregion
	}
}
