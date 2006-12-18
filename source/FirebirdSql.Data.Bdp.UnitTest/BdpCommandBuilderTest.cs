/*
 *  Firebird BDP - Borland Data provider Firebird
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
 *  Copyright (c) 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;

using NUnit.Framework;
using Borland.Data.Common;
using Borland.Data.Provider;

namespace FirebirdSql.Data.Bdp.Tests
{
	[TestFixture]
	public class BdpCommandBuilderTest : BaseTest
	{
		BdpDataAdapter	adapter;
		
		public BdpCommandBuilderTest() : base(false)
		{
		}
		
		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			adapter = new BdpDataAdapter(new BdpCommand("select * from TEST where VARCHAR_FIELD = ?", Connection));
            adapter.SelectCommand.Parameters.Add("@varchar_field", BdpType.String);
        }
		
		[TearDown]
		public override void TearDown()
		{
			adapter.Dispose();
			base.TearDown();
		}
		
		[Test]
		public void GetInsertCommandTest()
		{
			BdpCommandBuilder builder = new BdpCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("CommandBuilder - GetInsertCommand Method Test");
			
			Console.WriteLine( builder.GetInsertCommand().CommandText );

			builder.Dispose();
		}
		
		[Test]
		public void GetUpdateCommandTest()
		{
			BdpCommandBuilder builder = new BdpCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("CommandBuilder - GetUpdateCommand Method Test");
			
			Console.WriteLine( builder.GetUpdateCommand().CommandText );

			builder.Dispose();
		}
		
		[Test]
		public void GetDeleteCommandTest()
		{
			BdpCommandBuilder builder = new BdpCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("CommandBuilder - GetDeleteCommand Method Test");
			
			Console.WriteLine( builder.GetDeleteCommand().CommandText );

			builder.Dispose();
		}
		
		[Test]
		public void RefreshSchemaTest()
		{
			BdpCommandBuilder builder = new BdpCommandBuilder(adapter);
			
			Console.WriteLine();
			Console.WriteLine("CommandBuilder - RefreshSchema Method Test - Commands for original SQL statement: ");

			Console.WriteLine( builder.GetInsertCommand().CommandText );
			Console.WriteLine( builder.GetUpdateCommand().CommandText );
			Console.WriteLine( builder.GetDeleteCommand().CommandText );

            adapter.SelectCommand.CommandText = "select int_field, char_Field from TEST where BIGINT_FIELD = ?";
            adapter.SelectCommand.Parameters[0].ParameterName = "@bigint_field";

            builder.RefreshSchema();

            Console.WriteLine();
			Console.WriteLine("CommandBuilder - RefreshSchema Method Test - Commands for new SQL statement: ");
						
			Console.WriteLine( builder.GetInsertCommand().CommandText );
			Console.WriteLine( builder.GetUpdateCommand().CommandText );
			Console.WriteLine( builder.GetDeleteCommand().CommandText );

			builder.Dispose();
		}
		
		[Test]
		public void DeriveParameters()
		{
			BdpCommandBuilder builder = new BdpCommandBuilder();
			
			BdpCommand command = new BdpCommand("GETVARCHARFIELD", Connection);
			
			command.CommandType = CommandType.StoredProcedure;
						
			BdpCommandBuilder.DeriveParameters(command);
			
			Console.WriteLine("Derived Parameters");
			
			for (int i = 0; i < command.Parameters.Count; i++)
			{
				Console.WriteLine("Parameter name: {0}\tParameter Source Column:{1}\tDirection:{2}",
					command.Parameters[i].ParameterName,
					command.Parameters[i].SourceColumn,
					command.Parameters[i].Direction);
			}
		}

		[Test]
		public void DeriveParameters2()
		{
			BdpTransaction transaction = Connection.BeginTransaction();

			BdpCommandBuilder builder = new BdpCommandBuilder();
			
			BdpCommand command = new BdpCommand("GETVARCHARFIELD", Connection, transaction);
			
			command.CommandType = CommandType.StoredProcedure;
						
			BdpCommandBuilder.DeriveParameters(command);
			
			Console.WriteLine("Derived Parameters");
			
			for (int i = 0; i < command.Parameters.Count; i++)
			{
				Console.WriteLine("Parameter name: {0}\tParameter Source Column:{1}\tDirection:{2}",
					command.Parameters[i].ParameterName,
					command.Parameters[i].SourceColumn,
					command.Parameters[i].Direction);
			}

			transaction.Commit();
		}

		[Test]
        [Ignore("The BdpCommandBuilder seems to need the Connection to be open.")]
		public void TestWithClosedConnection()
		{
			Connection.Close();

			BdpCommandBuilder builder = new BdpCommandBuilder(adapter);
			
			Console.WriteLine();
			Console.WriteLine("CommandBuilder - RefreshSchema Method Test - Commands for original SQL statement: ");

			Console.WriteLine( builder.GetInsertCommand().CommandText );
			Console.WriteLine( builder.GetUpdateCommand().CommandText );
			Console.WriteLine( builder.GetDeleteCommand().CommandText );

            adapter.SelectCommand.CommandText = "select * from TEST where BIGINT_FIELD = ?";
            adapter.SelectCommand.Parameters[0].ParameterName = "@bigint_field";

            builder.RefreshSchema();
			
			Console.WriteLine();
			Console.WriteLine("CommandBuilder - RefreshSchema Method Test - Commands for new SQL statement: ");
						
			Console.WriteLine( builder.GetInsertCommand().CommandText );
			Console.WriteLine( builder.GetUpdateCommand().CommandText );
			Console.WriteLine( builder.GetDeleteCommand().CommandText );

			builder.Dispose();
		}
	}
}
