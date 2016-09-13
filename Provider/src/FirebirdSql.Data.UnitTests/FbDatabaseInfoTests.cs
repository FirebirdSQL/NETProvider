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

using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	[FbTestFixture(FbServerType.Embedded, default(bool))]
	public class FbDatabaseInfoTests : TestsBase
	{
		#region Constructors

		public FbDatabaseInfoTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{ }

		#endregion

		#region Unit Tests

		[Test]
		public void DatabaseInfoTest()
		{
			FbDatabaseInfo dbInfo = new FbDatabaseInfo(Connection);

			TestContext.WriteLine("Server Version: {0}", dbInfo.ServerVersion);
			TestContext.WriteLine("ISC Version : {0}", dbInfo.IscVersion);
			TestContext.WriteLine("Server Class : {0}", dbInfo.ServerClass);
			TestContext.WriteLine("Max memory : {0}", dbInfo.MaxMemory);
			TestContext.WriteLine("Current memory : {0}", dbInfo.CurrentMemory);
			TestContext.WriteLine("Page size : {0}", dbInfo.PageSize);
			TestContext.WriteLine("ODS Mayor version : {0}", dbInfo.OdsVersion);
			TestContext.WriteLine("ODS Minor version : {0}", dbInfo.OdsMinorVersion);
			TestContext.WriteLine("Allocation pages: {0}", dbInfo.AllocationPages);
			TestContext.WriteLine("Base level: {0}", dbInfo.BaseLevel);
			TestContext.WriteLine("Database id: {0}", dbInfo.DbId);
			TestContext.WriteLine("Database implementation: {0}", dbInfo.Implementation);
			TestContext.WriteLine("No reserve: {0}", dbInfo.NoReserve);
			TestContext.WriteLine("Forced writes: {0}", dbInfo.ForcedWrites);
			TestContext.WriteLine("Sweep interval: {0}", dbInfo.SweepInterval);
			TestContext.WriteLine("Number of page fetches: {0}", dbInfo.Fetches);
			TestContext.WriteLine("Number of page marks: {0}", dbInfo.Marks);
			TestContext.WriteLine("Number of page reads: {0}", dbInfo.Reads);
			TestContext.WriteLine("Number of page writes: {0}", dbInfo.Writes);
			TestContext.WriteLine("Removals of a version of a record: {0}", dbInfo.BackoutCount);
			TestContext.WriteLine("Number of database deletes: {0}", dbInfo.DeleteCount);
			TestContext.WriteLine("Number of removals of a record and all of its ancestors: {0}", dbInfo.ExpungeCount);
			TestContext.WriteLine("Number of inserts: {0}", dbInfo.InsertCount);
			TestContext.WriteLine("Number of removals of old versions of fully mature records: {0}", dbInfo.PurgeCount);
			TestContext.WriteLine("Number of reads done via an index: {0}", dbInfo.ReadIdxCount);
			TestContext.WriteLine("Number of sequential sequential table scans: {0}", dbInfo.ReadSeqCount);
			TestContext.WriteLine("Number of database updates: {0}", dbInfo.UpdateCount);
			TestContext.WriteLine("Database size in pages: {0}", dbInfo.DatabaseSizeInPages);
			TestContext.WriteLine("Number of the oldest transaction: {0}", dbInfo.OldestTransaction);
			TestContext.WriteLine("Number of the oldest active transaction: {0}", dbInfo.OldestActiveTransaction);
			TestContext.WriteLine("Number of the oldest active snapshot: {0}", dbInfo.OldestActiveSnapshot);
			TestContext.WriteLine("Number of the next transaction: {0}", dbInfo.NextTransaction);
			TestContext.WriteLine("Number of active transactions: {0}", dbInfo.ActiveTransactions);
		}

		#endregion
	}
}
