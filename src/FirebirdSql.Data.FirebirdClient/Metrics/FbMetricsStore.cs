using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Metrics
{
	internal static class FbMetricsStore
	{
		private const string ConnectionPoolNameAttributeName = "db.client.connection.pool.name";
		private const string ConnectionStateAttributeName = "db.client.connection.state";
		private const string ConnectionStateIdleValue = "idle";
		private const string ConnectionStateUsedValue = "used";

		internal static readonly Meter Source = new("FirebirdSql.Data", "1.0.0");

		static readonly Histogram<double> OperationDuration;
		static readonly Histogram<double> ConnectionCreateTime;

		static FbMetricsStore()
		{
			// Reference: https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md

			OperationDuration = Source.CreateHistogram<double>(
				"db.client.operation.duration",
				unit: "s",
				description: "Duration of database client operations."
			);

			Source.CreateObservableUpDownCounter(
				"db.client.connection.count",
				GetConnectionCount,
				unit: "{connection}",
				description: "The number of connections that are currently in state described by the 'state' attribute."
			);

			// db.client.connection.idle.max
			//   The maximum number of idle open connections allowed

			// db.client.connection.idle.min
			//   The minimum number of idle open connections allowed

			Source.CreateObservableUpDownCounter(
				"db.client.connection.max",
				GetConnectionMax,
				unit: "{connection}",
				description: "The maximum number of open connections allowed."
			);

			// db.client.connection.pending_requests
			//   The number of current pending requests for an open connection

			// db.client.connection.timeouts
			//   The number of connection timeouts that have occurred trying to obtain a connection from the pool

			ConnectionCreateTime = Source.CreateHistogram<double>(
				"db.client.connection.create_time",
				unit: "s",
				description: "The time it took to create a new connection."
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
				var elapsedTicks = Stopwatch.GetTimestamp() - startedAtTicks;
				var elapsedSeconds = TimeSpan.FromTicks(elapsedTicks).TotalSeconds;

				OperationDuration.Record(elapsedSeconds, connection.MetricsConnectionAttributes);
			}
		}

		internal static long ConnectionOpening() => Stopwatch.GetTimestamp();

		internal static void ConnectionOpened(long startedAtTicks, string poolName)
		{
			if (ConnectionCreateTime.Enabled && startedAtTicks > 0)
			{
				var elapsedTicks = Stopwatch.GetTimestamp() - startedAtTicks;
				var elapsedSeconds = TimeSpan.FromTicks(elapsedTicks).TotalSeconds;

				ConnectionCreateTime.Record(elapsedSeconds, [new(ConnectionPoolNameAttributeName, poolName)]);
			}
		}

		static IEnumerable<Measurement<int>> GetConnectionCount() =>
			FbConnectionPoolManager.Instance.GetMetrics()
				.SelectMany(kvp => new List<Measurement<int>>
				{
					new(
						kvp.Value.idleCount,
						new(ConnectionPoolNameAttributeName, kvp.Key),
						new(ConnectionStateAttributeName, ConnectionStateIdleValue)
					),

					new(
						kvp.Value.busyCount,
						new(ConnectionPoolNameAttributeName, kvp.Key),
						new(ConnectionStateAttributeName, ConnectionStateUsedValue)
					),
				});

		static IEnumerable<Measurement<int>> GetConnectionMax() =>
			FbConnectionPoolManager.Instance.GetMetrics()
				.SelectMany(kvp => new List<Measurement<int>>
				{
					new(
						kvp.Value.maxSize,
						[new(ConnectionPoolNameAttributeName, kvp.Key)]
					),
				});
	}
}
