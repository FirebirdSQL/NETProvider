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

- **Baseline job** (`NuGet100`): `ReleaseNuGet` build configuration — references a pinned released `FirebirdSql.Data.FirebirdClient` NuGet package (currently `10.3.1`, set in the benchmark `.csproj`).
- **Candidate job** (`Core100`): `Release` build configuration — references the local project source.
- **Runtime**: .NET 10
- **Diagnostics**: Memory allocations (`MemoryDiagnoser`)
- **Export**: GitHub-flavored Markdown table (written to `BenchmarkDotNet.Artifacts/`)
- **Ordering**: Fastest to slowest

The NuGet baseline lets you compare the locally built (unreleased) provider against the **last released version**, so a regression introduced on a branch shows up before it ships. The baseline version is intentionally pinned for reproducible results — bump it to the newest stable release when cutting a release, so "regression vs. last release" stays meaningful.

## Interpreting results

For the fetch and connection benchmarks the **`Mean` is dominated by network/server I/O and is noisy** (look at `StdDev` — it is often a large fraction of the mean). Small or even moderate `Mean` differences between the baseline and candidate jobs are frequently just noise.

The **`Allocated` column (and `Alloc Ratio` vs. the baseline) is the primary signal** for provider-side regressions: managed allocations are deterministic and independent of I/O timing. When comparing the `Core100` (local) job against the `NuGet100` (released) baseline, an `Alloc Ratio` well above `1.00` means the local code allocates more than the last release.

Worked example — issue [#1272](https://github.com/FirebirdSQL/NETProvider/issues/1272): fetching `CHAR(255) CHARACTER SET UTF8` regressed from `369.8 MB` to `4339 MB` allocated (≈11.7×). The allocation column shows the regression far more sharply and reliably than the I/O-bound `Mean` — which is why string/UTF8 `CHAR` types are part of the suite.

## Available Benchmarks

### `CommandBenchmark`

Measures command execution over three data types (`BIGINT`, `VARCHAR(10) CHARACTER SET UTF8`, `CHAR(100) CHARACTER SET UTF8`). The fixed-length `CHAR ... UTF8` type exercises the per-code-point rune handling on both the read (fetch) and write (parameter validate) paths:

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
