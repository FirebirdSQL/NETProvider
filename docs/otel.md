# OpenTelemetry Integration

The Firebird ADO.NET provider includes built-in support for [OpenTelemetry](https://opentelemetry.io/) distributed tracing and metrics using the native .NET `System.Diagnostics` APIs. No dependency on the OpenTelemetry SDK is required in the provider itself — your application opts in by configuring the appropriate listeners.

## Distributed Tracing

The provider emits `Activity` spans for database command execution using `System.Diagnostics.ActivitySource`.

### Enabling Traces

```csharp
using OpenTelemetry.Trace;

var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(FirebirdSql.Data.FirebirdClient.FbTelemetry.ActivitySourceName)
    .AddConsoleExporter() // or any other exporter
    .Build();
```

The `ActivitySource` name is `"FirebirdSql.Data"`, also available as the constant `FbTelemetry.ActivitySourceName`.

### Span Attributes

Spans follow the [OTel Database Client Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/database/database-spans/):

| Attribute | Description |
|-----------|-------------|
| `db.system.name` | Always `"firebirdsql"` |
| `db.namespace` | The database name from the connection |
| `db.operation.name` | The SQL verb (`SELECT`, `INSERT`, etc.) or `EXECUTE PROCEDURE` |
| `db.collection.name` | The table name (for `TableDirect` commands) |
| `db.stored_procedure.name` | The stored procedure name (for `StoredProcedure` commands) |
| `db.query.summary` | A low-cardinality summary of the operation |
| `db.query.text` | The full SQL text (**opt-in**, see below) |
| `db.query.parameter.*` | Parameter values (**opt-in**, see below) |
| `server.address` | The database server hostname |
| `server.port` | The database server port (only when non-default, i.e. != 3050) |
| `error.type` | The SQLSTATE code (for `FbException`) or exception type name |

### Opt-In Sensitive Attributes

By default, `db.query.text` and `db.query.parameter.*` are **not** collected, as they may contain sensitive data. Enable them explicitly:

```csharp
using FirebirdSql.Data.Logging;

// Enable SQL text in traces
FbLogManager.EnableQueryTextTracing();

// Enable parameter values in traces (and logs)
FbLogManager.EnableParameterLogging();
```

## Metrics

The provider emits metrics via `System.Diagnostics.Metrics.Meter`.

### Enabling Metrics

```csharp
using OpenTelemetry.Metrics;

var meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddMeter(FirebirdSql.Data.FirebirdClient.FbTelemetry.MeterName)
    .AddConsoleExporter() // or any other exporter
    .Build();
```

The `Meter` name is `"FirebirdSql.Data"`, also available as the constant `FbTelemetry.MeterName`.

### Available Metrics

Metrics follow the [OTel Database Client Metrics Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/database/database-metrics/):

| Metric | Type | Unit | Description |
|--------|------|------|-------------|
| `db.client.operation.duration` | Histogram | `s` | Duration of database client operations |
| `db.client.connection.create_time` | Histogram | `s` | Time to create a new connection |
| `db.client.connection.count` | ObservableUpDownCounter | `{connection}` | Current connection count by state (`idle`/`used`) |
| `db.client.connection.max` | ObservableUpDownCounter | `{connection}` | Maximum number of open connections allowed |
