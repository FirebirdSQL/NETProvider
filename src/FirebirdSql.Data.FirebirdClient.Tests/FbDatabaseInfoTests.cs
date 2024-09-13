/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
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
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests;

[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
public class FbDatabaseInfoTests : FbTestsBase
{
	public FbDatabaseInfoTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
		: base(serverType, compression, wireCrypt)
	{ }

	[Test]
	public void CompleteDatabaseInfoTest()
	{
		var dbInfo = new FbDatabaseInfo(Connection);
		foreach (var m in dbInfo.GetType()
			.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
			.Where(x => !x.IsSpecialName)
			.Where(x => x.Name.EndsWith("Async")))
		{
			if (ServerVersion < new Version(4, 0, 0, 0)
				 && new[] {
						nameof(FbDatabaseInfo.GetWireCryptAsync),
						nameof(FbDatabaseInfo.GetCryptPluginAsync),
						nameof(FbDatabaseInfo.GetNextAttachmentAsync),
						nameof(FbDatabaseInfo.GetNextStatementAsync),
						nameof(FbDatabaseInfo.GetReplicaModeAsync),
						nameof(FbDatabaseInfo.GetDbFileIdAsync),
						nameof(FbDatabaseInfo.GetDbGuidAsync),
						nameof(FbDatabaseInfo.GetCreationTimestampAsync),
						nameof(FbDatabaseInfo.GetProtocolVersionAsync),
						nameof(FbDatabaseInfo.GetStatementTimeoutDatabaseAsync),
						nameof(FbDatabaseInfo.GetStatementTimeoutAttachmentAsync),
				}.Contains(m.Name))
				continue;

			Assert.DoesNotThrowAsync(() => (Task)m.Invoke(dbInfo, new object[] { CancellationToken.None }), m.Name);
		}
	}

	[Test]
	public async Task PerformanceAnalysis_SELECT_Test()
	{
		var tableNameList = GetTableNameList();
		var tableIdTest = tableNameList["TEST"];

		var dbInfo = new FbDatabaseInfo(Connection);
		var insertCount = await dbInfo.GetInsertCountAsync();
		var updateCount = await dbInfo.GetUpdateCountAsync();
		var readSeqCount = await dbInfo.GetReadSeqCountAsync();
		var readIdxCount = await dbInfo.GetReadIdxCountAsync();

		var fbCommand = new FbCommand("SELECT MAX(INT_FIELD) FROM TEST", Connection);
		var maxIntField = await fbCommand.ExecuteScalarAsync() as int?;

		insertCount = GetAffectedTables(insertCount, await dbInfo.GetInsertCountAsync());
		updateCount = GetAffectedTables(updateCount, await dbInfo.GetUpdateCountAsync());
		readSeqCount = GetAffectedTables(readSeqCount, await dbInfo.GetReadSeqCountAsync());
		readIdxCount = GetAffectedTables(readIdxCount, await dbInfo.GetReadIdxCountAsync());

		Assert.That(insertCount.ContainsKey(tableIdTest), Is.False);
		Assert.That(updateCount.ContainsKey(tableIdTest), Is.False);
		Assert.That(readSeqCount.ContainsKey(tableIdTest), Is.True);
		Assert.That(readSeqCount[tableIdTest], Is.EqualTo(maxIntField + 1));
		Assert.That(readIdxCount.ContainsKey(tableIdTest), Is.False);
	}

	[Test]
	public async Task PerformanceAnalysis_INSERT_Test()
	{
		var tableNameList = GetTableNameList();
		var tableIdTest = tableNameList["TEST"];

		var dbInfo = new FbDatabaseInfo(Connection);
		var insertCount = await dbInfo.GetInsertCountAsync();
		var updateCount = await dbInfo.GetUpdateCountAsync();
		var readSeqCount = await dbInfo.GetReadSeqCountAsync();
		var readIdxCount = await dbInfo.GetReadIdxCountAsync();

		var fbCommand = new FbCommand("INSERT INTO TEST (INT_FIELD) VALUES (900)", Connection);
		await fbCommand.ExecuteNonQueryAsync();

		insertCount = GetAffectedTables(insertCount, await dbInfo.GetInsertCountAsync());
		updateCount = GetAffectedTables(updateCount, await dbInfo.GetUpdateCountAsync());
		readSeqCount = GetAffectedTables(readSeqCount, await dbInfo.GetReadSeqCountAsync());
		readIdxCount = GetAffectedTables(readIdxCount, await dbInfo.GetReadIdxCountAsync());

		Assert.That(insertCount.ContainsKey(tableIdTest), Is.True);
		Assert.That(insertCount[tableIdTest], Is.EqualTo(1));
		Assert.That(updateCount.ContainsKey(tableIdTest), Is.False);
		Assert.That(readSeqCount.ContainsKey(tableIdTest), Is.False);
		Assert.That(readIdxCount.ContainsKey(tableIdTest), Is.False);
	}

	[Test]
	public async Task PerformanceAnalysis_UPDATE_Test()
	{
		var tableNameList = GetTableNameList();
		var tableIdTest = tableNameList["TEST"];

		var fbCommand = new FbCommand("INSERT INTO TEST (INT_FIELD) VALUES (900)", Connection);
		fbCommand.ExecuteNonQuery();

		var dbInfo = new FbDatabaseInfo(Connection);
		var insertCount = await dbInfo.GetInsertCountAsync();
		var updateCount = await dbInfo.GetUpdateCountAsync();
		var readSeqCount = await dbInfo.GetReadSeqCountAsync();
		var readIdxCount = await dbInfo.GetReadIdxCountAsync();

		fbCommand.CommandText = "UPDATE TEST SET SMALLINT_FIELD = 900 WHERE (INT_FIELD = 900)";
		await fbCommand.ExecuteNonQueryAsync();

		insertCount = GetAffectedTables(insertCount, await dbInfo.GetInsertCountAsync());
		updateCount = GetAffectedTables(updateCount, await dbInfo.GetUpdateCountAsync());
		readSeqCount = GetAffectedTables(readSeqCount, await dbInfo.GetReadSeqCountAsync());
		readIdxCount = GetAffectedTables(readIdxCount, await dbInfo.GetReadIdxCountAsync());

		Assert.That(insertCount.ContainsKey(tableIdTest), Is.False);
		Assert.That(updateCount.ContainsKey(tableIdTest), Is.True);
		Assert.That(updateCount[tableIdTest], Is.EqualTo(1));
		Assert.That(readSeqCount.ContainsKey(tableIdTest), Is.False);
		Assert.That(readIdxCount.ContainsKey(tableIdTest), Is.True);
		Assert.That(readIdxCount[tableIdTest], Is.EqualTo(1));
	}

	IDictionary<short, ulong> GetAffectedTables(IDictionary<short, ulong> aStatisticInfoBefore, IDictionary<short, ulong> aStatisticInfoAfter)
	{
		var result = new Dictionary<short, ulong>();
		foreach (var keyValuePair in aStatisticInfoAfter)
		{
			if (aStatisticInfoBefore.TryGetValue(keyValuePair.Key, out var value))
			{
				var counter = keyValuePair.Value - value;
				if (counter > 0)
				{
					result.Add(keyValuePair.Key, counter);
				}
			}
			else
				result.Add(keyValuePair.Key, keyValuePair.Value);
		}
		return result;
	}

	IDictionary<string, short> GetTableNameList()
	{
		IDictionary<string, short> result = new Dictionary<string, short>();

		var command = new FbCommand("select R.RDB$RELATION_ID, R.RDB$RELATION_NAME from RDB$RELATIONS R WHERE RDB$SYSTEM_FLAG = 0", Connection);
		var reader = command.ExecuteReader();
		while (reader.Read())
		{
			result.Add(reader.GetString(1).Trim(), reader.GetInt16(0));
		}
		return result;
	}
}
