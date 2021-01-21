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
	public partial class CommandBenchmark
	{
		class Config : ManualConfig
		{
			public Config()
			{
				var baseJob = Job.Default
					.WithToolchain(CsProjCoreToolchain.NetCoreApp50)
					.WithPlatform(Platform.X64)
					.WithJit(Jit.RyuJit);
				AddDiagnoser(MemoryDiagnoser.Default);
				AddJob(baseJob.WithCustomBuildConfiguration("Release").WithId("Project"));
				AddJob(baseJob.WithCustomBuildConfiguration("ReleaseNuGet").WithId("NuGet").AsBaseline());
			}
		}

		protected const string ConnectionString = "database=localhost:benchmark.fdb;user=sysdba;password=masterkey";

		[Params("bigint", "varchar(10) character set utf8")]
		public string DataType { get; set; }

		void GlobalSetupBase()
		{
			FbConnection.CreateDatabase(ConnectionString, 16 * 1024, false, true);
		}

		[GlobalCleanup]
		public void GlobalCleanup()
		{
			FbConnection.ClearAllPools();
			FbConnection.DropDatabase(ConnectionString);
		}
	}
}
