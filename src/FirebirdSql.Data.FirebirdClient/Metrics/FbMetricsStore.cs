using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Trace;

namespace FirebirdSql.Data.Metrics
{
	internal static class FbMetricsStore
	{
		private const string ConnectionPoolNameAttributeName = "db.client.connection.pool.name";
		private const string ConnectionStateAttributeName = "db.client.connection.state";
		private const string ConnectionStateIdleValue = "idle";
		private const string ConnectionStateUsedValue = "used";

		static readonly string Version = typeof(FbMetricsStore).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

		internal static readonly Meter Source = new(FbTelemetry.MeterName, Version);

		static readonly Histogram<double> OperationDuration;
		static readonly Histogram<double> ConnectionCreateTime;

		static FbMetricsStore()
		{
#if NET9_0_OR_GREATER
			var durationAdvice = new InstrumentAdvice<double>
			{
				HistogramBucketBoundaries = [0.001, 0.005, 0.01, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10],
			};
#endif

			OperationDuration = Source.CreateHistogram<double>(
				"db.client.operation.duration",
				unit: "s",
				description: "Duration of database client operations."
#if NET9_0_OR_GREATER
				, advice: durationAdvice
#endif
			);

			Source.CreateObservableUpDownCounter(
				"db.client.connection.count",
				GetConnectionCount,
				unit: "{connection}",
				description: "The number of connections that are currently in state described by the 'state' attribute."
			);

			Source.CreateObservableUpDownCounter(
				"db.client.connection.max",
				GetConnectionMax,
				unit: "{connection}",
				description: "The maximum number of open connections allowed."
			);

			ConnectionCreateTime = Source.CreateHistogram<double>(
				"db.client.connection.create_time",
				unit: "s",
				description: "The time it took to create a new connection."
#if NET9_0_OR_GREATER
				, advice: durationAdvice
#endif
			);

			// db.client.connection.wait_time
			//   The time it took to obtain an open connection from the pool

			// db.client.connection.use_time
			//   The time between borrowing a connection and returning it to the pool
		}

		internal static long CommandStart() => Stopwatch.GetTimestamp();

		internal static void CommandStop(long startedAtTicks, FbConnection connection)
		{
			if (OperationDuration.Enabled && startedAtTicks > 0)
			{
				var elapsed = Stopwatch.GetElapsedTime(startedAtTicks);

				OperationDuration.Record(elapsed.TotalSeconds, connection.MetricsConnectionAttributes);
			}
		}

		internal static long ConnectionOpening() => Stopwatch.GetTimestamp();

		internal static void ConnectionOpened(long startedAtTicks, string poolName)
		{
			if (ConnectionCreateTime.Enabled && startedAtTicks > 0)
			{
				var elapsed = Stopwatch.GetElapsedTime(startedAtTicks);

				ConnectionCreateTime.Record(elapsed.TotalSeconds, [new(ConnectionPoolNameAttributeName, poolName)]);
			}
		}

		static IEnumerable<Measurement<int>> GetConnectionCount() =>
			FbConnectionPoolManager.Instance.GetMetrics()
				.SelectMany(m => new List<Measurement<int>>
				{
					new(
						m.idleCount,
						new(ConnectionPoolNameAttributeName, m.poolName),
						new(ConnectionStateAttributeName, ConnectionStateIdleValue)
					),

					new(
						m.busyCount,
						new(ConnectionPoolNameAttributeName, m.poolName),
						new(ConnectionStateAttributeName, ConnectionStateUsedValue)
					),
				});

		static IEnumerable<Measurement<int>> GetConnectionMax() =>
			FbConnectionPoolManager.Instance.GetMetrics()
				.Select(m => new Measurement<int>(
					m.maxSize,
					[new(ConnectionPoolNameAttributeName, m.poolName)]
				));
	}
}
