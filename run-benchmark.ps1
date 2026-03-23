param(
	[ValidateSet('CommandBenchmark','ConnectionBenchmark','LargeFetchBenchmark')]
	$Benchmark = 'CommandBenchmark',

	[switch]$Disasm,
	[switch]$Profile
)

$ErrorActionPreference = 'Stop'

$projectFile = '.\src\FirebirdSql.Data.FirebirdClient.Benchmarks\FirebirdSql.Data.FirebirdClient.Benchmarks.csproj'

$extraArgs = @()
if ($Disasm) { $extraArgs += '--disasm' }
if ($Profile) { $extraArgs += '--profiler', 'ETW' }

# Run selected benchmark
dotnet run `
    --project $projectFile `
    --configuration 'Release' `
    -- `
    --filter "*$($Benchmark)*" `
    @extraArgs
