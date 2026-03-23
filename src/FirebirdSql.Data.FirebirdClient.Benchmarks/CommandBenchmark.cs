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

using BenchmarkDotNet.Attributes;

namespace FirebirdSql.Data.FirebirdClient.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public partial class CommandBenchmark
{
	protected const string ConnectionString = "database=localhost:benchmark.fdb;user=sysdba;password=masterkey";

	[Params("BIGINT", "VARCHAR(10) CHARACTER SET UTF8")]
	public string DataType { get; set; }

	[Params(100)]
	public int Count { get; set; }

	static void CreateDatabase()
	{
		FbConnection.CreateDatabase(ConnectionString, 16 * 1024, false, true);
	}

	[GlobalCleanup]
	public static void GlobalCleanup()
	{
		FbConnection.ClearAllPools();
		FbConnection.DropDatabase(ConnectionString);
	}
}
