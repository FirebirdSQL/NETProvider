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

using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture]
	public class FbTransactionTests : TestsBase
	{
		#region Constructors

		public FbTransactionTests()
			: base(false)
		{
		}

		#endregion

		#region Unit Tests

		[Test]
		public void CommitTest()
		{
			Transaction = Connection.BeginTransaction();
			Transaction.Commit();
		}

		[Test]
		public void RollbackTest()
		{
			Transaction = Connection.BeginTransaction();
			Transaction.Rollback();
		}

		[Test]
		public void SavePointTest()
		{
			FbCommand command = new FbCommand();

			Console.WriteLine("Iniciada nueva transaccion");

			Transaction = Connection.BeginTransaction("InitialSavePoint");

			command.Connection = Connection;
			command.Transaction = Transaction;

			command.CommandText = "insert into TEST (INT_FIELD) values (200) ";
			command.ExecuteNonQuery();

			Transaction.Save("FirstSavePoint");

			command.CommandText = "insert into TEST (INT_FIELD) values (201) ";
			command.ExecuteNonQuery();
			Transaction.Save("SecondSavePoint");

			command.CommandText = "insert into TEST (INT_FIELD) values (202) ";
			command.ExecuteNonQuery();
			Transaction.Rollback("InitialSavePoint");

			Transaction.Commit();
			command.Dispose();
		}

		[Test]
		public void AbortTransaction()
		{
			FbTransaction transaction = null;
			FbCommand command = null;

			try
			{
				transaction = this.Connection.BeginTransaction();

				command = new FbCommand("ALTER TABLE \"TEST\" drop \"INT_FIELD\"", this.Connection, transaction);
				command.ExecuteNonQuery();

				transaction.Commit();
				transaction = null;
			}
			catch (Exception)
			{
				transaction.Rollback();
				transaction = null;
			}
			finally
			{
				if (command != null)
				{
					command.Dispose();
				}
			}
		}

		#endregion
	}
}
