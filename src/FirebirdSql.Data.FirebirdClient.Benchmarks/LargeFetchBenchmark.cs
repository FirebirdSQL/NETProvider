using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Validators;

namespace FirebirdSql.Data.FirebirdClient.Benchmarks;

[Config(typeof(Config))]
public class LargeFetchBenchmark
{
	class Config : ManualConfig
	{
		public Config()
		{
			var baseJob = Job.Default
				.WithWarmupCount(3)
				.WithPlatform(Platform.X64)
				.WithJit(Jit.RyuJit);

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

	protected const string ConnectionString = "database=localhost:benchmark.fdb;user=sysdba;password=masterkey";

	[Params(100_000)]
	public int Count { get; set; }

	[Params(
		"BIGINT",
		"CHAR(255) CHARACTER SET UTF8",
		"CHAR(255) CHARACTER SET OCTETS",
		"BLOB SUB_TYPE TEXT CHARACTER SET UTF8",
		"BLOB SUB_TYPE BINARY"
	)]
	public string DataType { get; set; }

	[GlobalSetup(Target = nameof(Fetch))]
	public void FetchGlobalSetup()
	{
		FbConnection.CreateDatabase(ConnectionString, 8192, false, true);
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		FbConnection.ClearAllPools();
		FbConnection.DropDatabase(ConnectionString);
	}

	[Benchmark]
	public void Fetch()
	{
		using var conn = new FbConnection(ConnectionString);
		conn.Open();

		using var cmd = conn.CreateCommand();
		cmd.CommandText = $@"
			EXECUTE BLOCK RETURNS (result {DataType}) AS
			DECLARE cnt INTEGER;
			BEGIN
				SELECT {GetFillExpression(DataType)} FROM rdb$database INTO result;
				cnt = {Count};
				WHILE (cnt > 0) DO
				BEGIN
					SUSPEND;
					cnt = cnt - 1;
				END
			END
		";

		using var reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			_ = reader[0];
		}
	}
	private static string GetFillExpression(string dataType) =>
		dataType switch
		{
			{ } when dataType.StartsWith("BLOB") => $"LPAD('', 1023, '{dataType};')",
			{ } when dataType.StartsWith("CHAR") => $"LPAD('', 255, '{dataType};')",
			_ => "9223372036854775807" /* BIGINT */
		};
}