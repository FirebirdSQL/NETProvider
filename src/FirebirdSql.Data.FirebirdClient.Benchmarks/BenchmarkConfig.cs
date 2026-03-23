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

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Validators;

namespace FirebirdSql.Data.FirebirdClient.Benchmarks;

class BenchmarkConfig : ManualConfig
{
	public BenchmarkConfig()
	{
		var baseJob = Job.Default
			.WithWarmupCount(3);

		AddJob(
			baseJob
				.WithToolchain(CsProjCoreToolchain.NetCoreApp80)
				.WithCustomBuildConfiguration("ReleaseNuGet")
				.WithId("NuGet80")
				.AsBaseline()
		);

		AddJob(
			baseJob
				.WithToolchain(CsProjCoreToolchain.NetCoreApp80)
				.WithCustomBuildConfiguration("Release")
				.WithId("Core80")
		);

		AddDiagnoser(MemoryDiagnoser.Default);

		AddValidator(BaselineValidator.FailOnError);
		AddValidator(JitOptimizationsValidator.FailOnError);
	}
}
