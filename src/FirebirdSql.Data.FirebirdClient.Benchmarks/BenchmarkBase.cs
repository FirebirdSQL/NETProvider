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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using BenchmarkDotNet.Attributes;

namespace FirebirdSql.Data.FirebirdClient.Benchmarks;

public abstract class BenchmarkBase
{
	const string DefaultConnectionString = "database=localhost:benchmark.fdb;user=sysdba;password=masterkey";

	protected static readonly string ConnectionString =
		Environment.GetEnvironmentVariable("FIREBIRD_BENCHMARK_CS") ?? DefaultConnectionString;

	protected static void CreateDatabase(int pageSize = 16 * 1024)
	{
		FbConnection.CreateDatabase(ConnectionString, pageSize, false, true);
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		FbConnection.ClearAllPools();
		FbConnection.DropDatabase(ConnectionString);
	}
}
