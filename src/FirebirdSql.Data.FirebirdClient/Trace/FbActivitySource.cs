using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Trace
{
	internal static class FbActivitySource
	{
		internal static readonly ActivitySource Source = new("FirebirdSql.Data", "1.0.0");

		internal static Activity CommandStart(FbCommand command)
		{
			// Reference: https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md
			var dbName = command.Connection.Database;

			string dbOperationName = null;
			string dbCollectionName = null;
			string activityName;

			switch (command.CommandType)
			{
				case CommandType.StoredProcedure:
					dbOperationName = "EXECUTE PROCEDURE";
					activityName = $"{dbOperationName} {command.CommandText}";
					break;

				case CommandType.TableDirect:
					dbOperationName = "SELECT";
					dbCollectionName = command.CommandText;
					activityName = $"{dbOperationName} {dbCollectionName}";
					break;

				case CommandType.Text:
					activityName = dbName;
					break;

				default:
					throw new InvalidEnumArgumentException($"Invalid value for 'System.Data.CommandType' ({(int)command.CommandType}).");
			}

			var activity = Source.StartActivity(activityName, ActivityKind.Client);
			if (activity.IsAllDataRequested)
			{
				activity.SetTag("db.system", "firebird");

				if (dbCollectionName != null)
				{
					activity.SetTag("db.collection.name", dbCollectionName);
				}

				// db.namespace

				if (dbOperationName != null)
				{
					activity.SetTag("db.operation.name", dbOperationName);
				}

				// db.response.status_code

				// error.type (handled by RecordException)

				// server.port

				// db.operation.batch.size

				// db.query_summary

				activity.SetTag("db.query.text", command.CommandText);

				// network.peer.address

				// network.peer.port

				if (command.Connection.DataSource != null)
				{
					activity.SetTag("server.address", command.Connection.DataSource);
				}

				foreach (FbParameter p in command.Parameters)
				{
					var name = p.ParameterName;
					var value = NormalizeDbNull(p.InternalValue);
					activity.SetTag($"db.query.parameter.{name}", value);

				}

				// Only for explicit transactions.
				if (command.Transaction != null)
				{
					FbTransactionInfo fbInfo = new FbTransactionInfo(command.Transaction);

					var transactionId = fbInfo.GetTransactionId();
					activity.SetTag($"db.transaction_id", transactionId);

					// TODO: Firebird 4+ only (or remove?)
					/*
					var snapshotId = fbInfo.GetTransactionSnapshotNumber();
					if (snapshotId != 0)
					{
						activity.SetTag($"db.snapshot_id", snapshotId);
					}
					*/
				}
			}

			return activity;
		}

		internal static void CommandException(Activity activity, Exception exception, bool escaped = true)
		{
			// Reference: https://github.com/open-telemetry/semantic-conventions/blob/main/docs/exceptions/exceptions-spans.md
			activity.AddEvent(
				new("exception", tags: new()
				{
					{ "exception.message", exception.Message },
					{ "exception.type", exception.GetType().FullName },
					{ "exception.escaped", escaped },
					{ "exception.stacktrace", exception.ToString() },
				})
			);

			string errorDescription = exception is FbException fbException
				? fbException.SQLSTATE
				: exception.Message;

			activity.SetStatus(ActivityStatusCode.Error, errorDescription);
			activity.Dispose();
		}

		private static object NormalizeDbNull(object value) =>
			value == DBNull.Value || value == null
				? null
				: value;
	}
}
