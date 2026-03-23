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

using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace FirebirdSql.Data.FirebirdClient.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class ConnectionBenchmark : BenchmarkBase
{
	[GlobalSetup]
	public void GlobalSetup()
	{
		CreateDatabase();
	}

	[Benchmark]
	public void OpenClose()
	{
		using var conn = new FbConnection(ConnectionString);
		conn.Open();
	}

	[Benchmark]
	public async Task OpenCloseAsync()
	{
		await using var conn = new FbConnection(ConnectionString);
		await conn.OpenAsync();
	}
}
