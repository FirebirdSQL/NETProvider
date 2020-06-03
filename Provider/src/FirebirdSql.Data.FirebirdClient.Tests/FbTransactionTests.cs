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
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbDefaultServerTypeTestFixtureSource))]
	[TestFixtureSource(typeof(FbEmbeddedServerTypeTestFixtureSource))]
	public class FbTransactionTests : FbTestsBase
	{
		#region Constructors

		public FbTransactionTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

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
			using (var command = new FbCommand())
			{
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
			}
		}

		[Test]
		public void AbortTransaction()
		{
			FbTransaction transaction = null;
			FbCommand command = null;

			try
			{
				transaction = Connection.BeginTransaction();

				command = new FbCommand("ALTER TABLE \"TEST\" drop \"INT_FIELD\"", Connection, transaction);
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

		[Test]
		public void ReadCommittedReadConsistency()
		{
			if (!EnsureVersion(new Version(4, 0, 0, 0)))
				return;

			Transaction = Connection.BeginTransaction(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.ReadConsistency });
			Transaction.Dispose();
		}

		#endregion
	}
}
