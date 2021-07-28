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
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class FbTransactionTests : FbTestsBase
	{
		public FbTransactionTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[Test]
		public async Task CommitTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task RollbackTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await transaction.RollbackAsync();
			}
		}

		[Test]
		public async Task SavePointTest()
		{
			await using (var command = new FbCommand())
			{
				await using (var transaction = await Connection.BeginTransactionAsync("InitialSavePoint"))
				{
					command.Connection = Connection;
					command.Transaction = transaction;

					command.CommandText = "insert into TEST (INT_FIELD) values (200)";
					await command.ExecuteNonQueryAsync();

					await transaction.SaveAsync("FirstSavePoint");

					command.CommandText = "insert into TEST (INT_FIELD) values (201)";
					await command.ExecuteNonQueryAsync();
					await transaction.SaveAsync("SecondSavePoint");

					command.CommandText = "insert into TEST (INT_FIELD) values (202)";
					await command.ExecuteNonQueryAsync();
					await transaction.RollbackAsync("InitialSavePoint");

					await transaction.CommitAsync();
				}
			}
		}

		[Test]
		public async Task AbortTransaction()
		{
			FbTransaction transaction = null;
			FbCommand command = null;

			try
			{
				transaction = await Connection.BeginTransactionAsync();

				command = new FbCommand("ALTER TABLE \"TEST\" drop \"INT_FIELD\"", Connection, transaction);
				await command.ExecuteNonQueryAsync();

				await transaction.CommitAsync();
			}
			catch (Exception)
			{
				await transaction.RollbackAsync();
			}
			finally
			{
				if (transaction != null)
				{
					await command.DisposeAsync();
				}
				if (transaction != null)
				{
					await command.DisposeAsync();
				}
			}
		}

		[Test]
		public async Task ReadCommittedReadConsistency()
		{
			if (!EnsureServerVersion(new Version(4, 0, 0, 0)))
				return;

			await using (var transaction = await Connection.BeginTransactionAsync(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.ReadConsistency }))
			{
				await transaction.DisposeAsync();
			}
		}
	}
}
