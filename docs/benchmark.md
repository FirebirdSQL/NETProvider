# Benchmarks

The benchmark project (`FirebirdSql.Data.FirebirdClient.Benchmarks`) measures performance of the Firebird .NET provider using [BenchmarkDotNet](https://benchmarkdotnet.org/).

## Prerequisites

- A running Firebird server accessible at `localhost` (default configuration)
- .NET 10 SDK

## Running Benchmarks

Use the convenience script from the repository root:

```powershell
.\run-benchmark.ps1
```

By default this runs `CommandBenchmark`. To select a different benchmark class:

```powershell
.\run-benchmark.ps1 -Benchmark ConnectionBenchmark
.\run-benchmark.ps1 -Benchmark LargeFetchBenchmark
```

### Advanced Options

Enable JIT disassembly output:

```powershell
.\run-benchmark.ps1 -Disasm
```

Enable ETW profiling (Windows only):

```powershell
.\run-benchmark.ps1 -Profile
```

Options can be combined:

```powershell
.\run-benchmark.ps1 -Benchmark LargeFetchBenchmark -Disasm
```

### Running Directly with BenchmarkDotNet

For full control over BenchmarkDotNet options, pass arguments directly after `--`:

```powershell
dotnet run --project src\FirebirdSql.Data.FirebirdClient.Benchmarks\FirebirdSql.Data.FirebirdClient.Benchmarks.csproj --configuration Release -- --list flat
dotnet run --project src\FirebirdSql.Data.FirebirdClient.Benchmarks\FirebirdSql.Data.FirebirdClient.Benchmarks.csproj --configuration Release -- --filter "*Fetch*"
```

## Connection String

By default the benchmark connects to:

```
database=localhost:benchmark.fdb;user=sysdba;password=masterkey
```

Override this with the `FIREBIRD_BENCHMARK_CS` environment variable:

```powershell
$env:FIREBIRD_BENCHMARK_CS = "database=myhost:benchmark.fdb;user=sysdba;password=masterkey"
.\run-benchmark.ps1
```

## Configuration

All benchmarks share a common configuration (`BenchmarkConfig`):

- **Baseline job**: `ReleaseNuGet` build configuration — references the latest published `FirebirdSql.Data.FirebirdClient` NuGet package.
- **Candidate job**: `Release` build configuration — references the local project source.
- **Runtime**: .NET 10
- **Diagnostics**: Memory allocations (`MemoryDiagnoser`)
- **Export**: GitHub-flavored Markdown table (written to `BenchmarkDotNet.Artifacts/`)
- **Ordering**: Fastest to slowest

The NuGet baseline lets you compare the locally built provider against the published release to detect regressions or measure improvements.

## Available Benchmarks

### `CommandBenchmark`

Measures command execution over two data types (`BIGINT`, `VARCHAR(10) CHARACTER SET UTF8`):

| Benchmark | Description |
|-----------|-------------|
| `Execute` / `ExecuteAsync` | Inserts 100 rows using `ExecuteNonQuery` / `ExecuteNonQueryAsync` |
| `Fetch` / `FetchAsync` | Reads 100 rows using `ExecuteReader` / `ExecuteReaderAsync` |

### `ConnectionBenchmark`

Measures connection pool throughput:

| Benchmark | Description |
|-----------|-------------|
| `OpenClose` / `OpenCloseAsync` | Opens and closes a pooled connection |

### `LargeFetchBenchmark`

Measures bulk read throughput for 100,000 rows across five data types:

| Data Type | Notes |
|-----------|-------|
| `BIGINT` | Fixed-size integer |
| `CHAR(255) CHARACTER SET UTF8` | Fixed-length string |
| `CHAR(255) CHARACTER SET OCTETS` | Fixed-length binary |
| `BLOB SUB_TYPE TEXT CHARACTER SET UTF8` | Text blob |
| `BLOB SUB_TYPE BINARY` | Binary blob |

## Results

BenchmarkDotNet writes results to `BenchmarkDotNet.Artifacts/` in the repository root. This directory is listed in `.gitignore`. Each run produces:

- A summary table in the console
- A GitHub-flavored Markdown file (`.md`) suitable for pasting into issues or pull requests
- An HTML report
- CSV data
