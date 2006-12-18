//
// Firebird .NET Data Provider - Firebird managed data provider for .NET and Mono
// Copyright (C) 2002-2003  Carlos Guzman Alvarez
//
// Distributable under LGPL license.
// You may obtain a copy of the License at http://www.gnu.org/copyleft/lesser.html
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//

using NUnit.Framework;

using System;
using System.Data;
using FirebirdSql.Data.Firebird;


namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbCommandBuilderTest : BaseTest
	{
		FbConnection 	connection;
		FbTransaction	transaction;
		FbDataAdapter	adapter;
		
		public FbCommandBuilderTest() : base()
		{
		}
		
		[SetUp]
		public void Setup()
		{
			connection = new FbConnection(GetConnectionString());
			connection.Open();
			
			transaction = connection.BeginTransaction();
			
			adapter = new FbDataAdapter();
			adapter.SelectCommand = new FbCommand("select * from TEST_TABLE_01 where VARCHAR_FIELD = ?", connection, transaction);
		}
		
		[TearDown]
		public void TearDown()
		{
			adapter.Dispose();
			transaction.Rollback();
			connection.Close();
		}
		
		[Test]
		public void GetInsertCommandTest()
		{
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("CommandBuilder - GetInsertCommand Method Test");
			
			Console.WriteLine( builder.GetInsertCommand().CommandText );

			builder.Dispose();
		}
		
		[Test]
		public void GetUpdateCommandTest()
		{
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("CommandBuilder - GetUpdateCommand Method Test");
			
			Console.WriteLine( builder.GetUpdateCommand().CommandText );

			builder.Dispose();
		}
		
		[Test]
		public void GetDeleteCommandTest()
		{
			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("CommandBuilder - GetDeleteCommand Method Test");
			
			Console.WriteLine( builder.GetDeleteCommand().CommandText );

			builder.Dispose();
		}
		
		[Test]
		public void RefreshSchemaTest()
		{
			FbCommandBuilder builder = new FbCommandBuilder(adapter);
			
			Console.WriteLine();
			Console.WriteLine("CommandBuilder - RefreshSchema Method Test - Commands for original SQL statement: ");

			Console.WriteLine( builder.GetInsertCommand().CommandText );
			Console.WriteLine( builder.GetUpdateCommand().CommandText );
			Console.WriteLine( builder.GetDeleteCommand().CommandText );
			
			adapter.SelectCommand.CommandText = "select * from TEST_TABLE_01 where BIGINT_FIELD = ?";
			
			builder.RefreshSchema();
			
			Console.WriteLine();
			Console.WriteLine("CommandBuilder - RefreshSchema Method Test - Commands for new SQL statement: ");
						
			Console.WriteLine( builder.GetInsertCommand().CommandText );
			Console.WriteLine( builder.GetUpdateCommand().CommandText );
			Console.WriteLine( builder.GetDeleteCommand().CommandText );

			builder.Dispose();
		}
		
		[Test]
		public void CommandBuilderWithExpressionFieldTest()
		{
			adapter.SelectCommand.CommandText = "select TEST_TABLE_01.*, 0 AS VALOR from TEST_TABLE_01";

			FbCommandBuilder builder = new FbCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("CommandBuilder - GetUpdateCommand Method Test");
			
			Console.WriteLine( builder.GetUpdateCommand().CommandText );

			builder.Dispose();
		}

		[Test]
		public void DeriveParameters()
		{
			FbCommandBuilder builder = new FbCommandBuilder();
			
			FbCommand command = new FbCommand("EXECUTE PROCEDURE GETVARCHARFIELD(?)", connection, transaction);
			
			command.CommandType = CommandType.StoredProcedure;
						
			FbCommandBuilder.DeriveParameters(command);
			
			Console.WriteLine("Derived Parameters");
			
			for (int i = 0; i < command.Parameters.Count; i++)
			{
				Console.WriteLine("Parameter name: {0}\tParameter Source Column:{1}",
				                  command.Parameters[i].ParameterName,
				                  command.Parameters[i].SourceColumn);
			}
		}
	}
}
