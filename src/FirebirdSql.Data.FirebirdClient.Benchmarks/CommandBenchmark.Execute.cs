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

public partial class CommandBenchmark
{
	[GlobalSetup(Target = nameof(Execute))]
	public void ExecuteGlobalSetup()
	{
		CreateDatabase();

		using var conn = new FbConnection(ConnectionString);
		conn.Open();

		using var cmd = conn.CreateCommand();
		cmd.CommandText = $"CREATE TABLE foobar (x {DataType})";
		cmd.ExecuteNonQuery();
	}

	[Benchmark]
	public void Execute()
	{
		using var conn = new FbConnection(ConnectionString);
		conn.Open();

		using var cmd = conn.CreateCommand();
		cmd.CommandText = @"INSERT INTO foobar (x) VALUES (@cnt)";

		var p = new FbParameter() { ParameterName = "@cnt" };
		cmd.Parameters.Add(p);

		for (var i = 0; i < Count; i++)
		{
			p.Value = i;
			cmd.ExecuteNonQuery();
		}
	}
}
