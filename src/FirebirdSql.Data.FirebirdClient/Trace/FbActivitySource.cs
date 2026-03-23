using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using FirebirdSql.Data.Common;
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
					dbOperationName = ExtractSqlVerb(command.CommandText);
					activityName = dbOperationName ?? dbName;
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

			if (dbName != null)
			{
				activity.SetTag("db.namespace", dbName);
			}

			if (dbOperationName != null)
			{
				activity.SetTag("db.operation.name", dbOperationName);
			}

			activity.SetTag("db.query.summary", activityName);

			if (command.CommandType == CommandType.StoredProcedure)
			{
				activity.SetTag("db.stored_procedure.name", command.CommandText);
			}

			if (FbLogManager.IsQueryTextTracingEnabled)
			{
				activity.SetTag("db.query.text", command.CommandText);
			}

			if (command.Connection.DataSource != null)
			{
				activity.SetTag("server.address", command.Connection.DataSource);
			}

			var port = command.Connection.ConnectionOptions.Port;
			if (port != ConnectionString.DefaultValuePortNumber)
			{
				activity.SetTag("server.port", port);
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

		static string ExtractSqlVerb(string sql)
		{
			if (string.IsNullOrEmpty(sql))
				return null;
			var span = sql.AsSpan().TrimStart();
			var spaceIndex = span.IndexOfAny(' ', '\t', '\n', '\r');
			if (spaceIndex <= 0)
				return span.Length > 0 ? span.ToString().ToUpperInvariant() : null;
			return span.Slice(0, spaceIndex).ToString().ToUpperInvariant();
		}
	}
}
