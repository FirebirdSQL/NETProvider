using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Logging;

namespace FirebirdSql.Data.Trace
{
	internal static class FbActivitySource
	{
		static readonly string Version = typeof(FbActivitySource).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

		internal static readonly ActivitySource Source = new("FirebirdSql.Data", Version);

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
			if (activity is not { IsAllDataRequested: true })
				return activity;

			activity.SetTag("db.system.name", "firebirdsql");

			if (dbCollectionName != null)
			{
				activity.SetTag("db.collection.name", dbCollectionName);
			}

			// db.namespace
			if (dbName != null)
			{
				activity.SetTag("db.namespace", dbName);
			}

			if (dbOperationName != null)
			{
				activity.SetTag("db.operation.name", dbOperationName);
			}

			// db.response.status_code

			// error.type (handled by RecordException)

			// server.port

			// db.operation.batch.size

			// db.query_summary

			if (FbLogManager.IsQueryTextTracingEnabled)
			{
				activity.SetTag("db.query.text", command.CommandText);
			}

			// network.peer.address

			// network.peer.port

			if (command.Connection.DataSource != null)
			{
				activity.SetTag("server.address", command.Connection.DataSource);
			}

			if (FbLogManager.IsParameterLoggingEnabled)
			{
				foreach (FbParameter p in command.Parameters)
				{
					activity.SetTag($"db.query.parameter.{p.ParameterName}", p.InternalValue == DBNull.Value ? null : p.InternalValue);
				}
			}

			return activity;
		}

		internal static void CommandException(Activity activity, Exception exception)
		{
			activity.AddEvent(
				new("exception", tags: new()
				{
					{ "exception.message", exception.Message },
					{ "exception.type", exception.GetType().FullName },
					{ "exception.stacktrace", exception.ToString() },
				})
			);

			string errorDescription = exception is FbException fbException
				? fbException.SQLSTATE
				: exception.Message;

			activity.SetTag("error.type", exception is FbException fbEx
				? fbEx.SQLSTATE ?? exception.GetType().FullName
				: exception.GetType().FullName);

			activity.SetStatus(ActivityStatusCode.Error, errorDescription);
			activity.Dispose();
		}
	}
}
