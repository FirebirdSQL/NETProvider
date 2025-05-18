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
	[GlobalSetup(Target = nameof(Fetch))]
	public void FetchGlobalSetup()
	{
		CreateDatabase();

		using var conn = new FbConnection(ConnectionString);
		conn.Open();

		using (var cmd = conn.CreateCommand())
		{
			cmd.CommandText = $"CREATE TABLE foobar (x {DataType})";
			cmd.ExecuteNonQuery();
		}

		using (var cmd = conn.CreateCommand())
		{
			cmd.CommandText = $"""
				EXECUTE BLOCK AS
				DECLARE cnt INT;
				BEGIN
					cnt = {Count};
					WHILE (cnt > 0) DO
					BEGIN
						INSERT INTO foobar VALUES (:cnt);
						cnt = cnt - 1;
					END
				END
				""";
			cmd.ExecuteNonQuery();
		}
	}

	[Benchmark]
	public void Fetch()
	{
		using var conn = new FbConnection(ConnectionString);
		conn.Open();

		using var cmd = conn.CreateCommand();
		cmd.CommandText = "SELECT x FROM foobar";

		using var reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			var _ = reader[0];
		}
	}
}
