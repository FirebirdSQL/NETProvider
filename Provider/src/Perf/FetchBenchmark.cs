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

//$Authors = Jiri Cincura (jiri@cincura.net)

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using FirebirdSql.Data.FirebirdClient;

namespace Perf
{
	[Config(typeof(Config))]
	public class FetchBenchmark
	{
		class Config : ManualConfig
		{
			public Config()
			{
				var baseJob = Job.Default
					.With(CsProjCoreToolchain.Current.Value)
					.With(Platform.X64)
					.With(Jit.RyuJit)
					.With(Runtime.Core);
				Add(MemoryDiagnoser.Default);
				Add(baseJob.WithCustomBuildConfiguration("Release").WithId("Project"));
				Add(baseJob.WithCustomBuildConfiguration("ReleaseNuGet").WithId("NuGet"));
			}
		}

		const string ConnectionString = "database=localhost:benchmark.fdb;user=sysdba;password=masterkey";

		[Params("int", "bigint", "varchar(10) character set utf8")]
		public string DataType { get; set; }

		[GlobalSetup(Target = nameof(Fetch200k))]
		public void Fetch200kSetup()
		{
			FbConnection.CreateDatabase(ConnectionString, true);
			using (var conn = new FbConnection(ConnectionString))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $"create table foobar (x {DataType})";
					cmd.ExecuteNonQuery();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = @"execute block as
declare cnt int;
begin
	cnt = 200000;
	while (cnt > 0) do
	begin
		insert into foobar values (:cnt);
		cnt = cnt - 1;
	end
end";
					cmd.ExecuteNonQuery();
				}
			}
		}

		[Benchmark]
		public void Fetch200k()
		{
			using (var conn = new FbConnection(ConnectionString))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "select x from foobar";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var dummy = reader[0];
						}
					}
				}
			}
		}

		[GlobalCleanup(Target = nameof(Fetch200k))]
		public void Fetch200kCleanup()
		{
			FbConnection.ClearAllPools();
			FbConnection.DropDatabase(ConnectionString);
		}
	}
}
