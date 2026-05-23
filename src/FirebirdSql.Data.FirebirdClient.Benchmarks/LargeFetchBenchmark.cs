using BenchmarkDotNet.Attributes;

namespace FirebirdSql.Data.FirebirdClient.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class LargeFetchBenchmark : BenchmarkBase
{
	const int Count = 100_000;

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
		CreateDatabase(pageSize: 8192);
	}

	[Benchmark]
	public object Fetch()
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

		object last = null;
		using var reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			last = reader[0];
		}
		return last;
	}
	private static string GetFillExpression(string dataType) =>
		dataType switch
		{
			{ } when dataType.StartsWith("BLOB") => $"LPAD('', 1023, '{dataType};')",
			{ } when dataType.StartsWith("CHAR") => $"LPAD('', 255, '{dataType};')",
			_ => "9223372036854775807" /* BIGINT */
		};
}