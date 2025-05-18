param(
	[ValidateSet('CommandBenchmark','LargeFetchBenchmark')]
	$Benchmark = 'CommandBenchmark'
)

$ErrorActionPreference = 'Stop'

$projectFile = '.\src\FirebirdSql.Data.FirebirdClient.Benchmarks\FirebirdSql.Data.FirebirdClient.Benchmarks.csproj'

# Run selected benchmark
dotnet run `
    --project $projectFile `
    --configuration 'Release' `
    -- `
    --filter "*$($Benchmark)*"
