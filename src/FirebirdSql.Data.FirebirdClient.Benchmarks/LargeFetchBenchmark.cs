using BenchmarkDotNet.Attributes;

namespace FirebirdSql.Data.FirebirdClient.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class LargeFetchBenchmark
{
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