/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Text;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Logging;

static class LogMessages
{
	public static void CommandExecution(IFbLogger log, FbCommand command)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Command execution:");
		sb.AppendLine(command.CommandText);
		if (FbLogManager.IsParameterLoggingEnabled)
		{
			sb.AppendLine("Parameters:");
			if (!command.HasParameters)
			{
				sb.AppendLine("<no parameters>");
			}
			else
			{
				foreach (FbParameter parameter in command.Parameters)
				{
					var name = parameter.ParameterName;
					var type = parameter.FbDbType;
					var value = !IsNullParameterValue(parameter.InternalValue) ? parameter.InternalValue : "<null>";
					sb.AppendLine($"Name:{name}\tType:{type}\tUsed Value:{value}");
				}
			}
		}
		log.Debug(sb.ToString());
	}
	public static void CommandExecution(IFbLogger log, FbBatchCommand command)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Command execution:");
		sb.AppendLine(command.CommandText);
		if (FbLogManager.IsParameterLoggingEnabled)
		{
			sb.AppendLine("Parameters:");
			if (command.HasParameters)
			{
				sb.AppendLine("<no parameters>");
			}
			else
			{
				foreach (var batchParameter in command.BatchParameters)
				{
					foreach (FbParameter parameter in batchParameter)
					{
						var name = parameter.ParameterName;
						var type = parameter.FbDbType;
						var value = !IsNullParameterValue(parameter.InternalValue) ? parameter.InternalValue : "<null>";
						sb.AppendLine($"Name:{name}\tType:{type}\tUsed Value:{value}");
					}
				}
			}
		}
		log.Debug(sb.ToString());
	}

	public static void ConnectionOpening(IFbLogger log, FbConnection connection)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Opening connection:");
		sb.AppendLine($"Connection String: {connection.ConnectionString}");
		log.Debug(sb.ToString());
	}
	public static void ConnectionOpened(IFbLogger log, FbConnection connection)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Opened connection:");
		sb.AppendLine($"Connection String: {connection.ConnectionString}");
		log.Debug(sb.ToString());
	}
	public static void ConnectionClosing(IFbLogger log, FbConnection connection)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Closing connection:");
		sb.AppendLine($"Connection String: {connection.ConnectionString}");
		log.Debug(sb.ToString());
	}
	public static void ConnectionClosed(IFbLogger log, FbConnection connection)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Closed connection:");
		sb.AppendLine($"Connection String: {connection.ConnectionString}");
		log.Debug(sb.ToString());
	}

	public static void TransactionBeginning(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Beginning transaction:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionBegan(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Began transaction:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionCommitting(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Committing transaction:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionCommitted(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Committed transaction:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionRollingBack(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Rolling back transaction:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionRolledBack(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Rolled back transaction:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionSaving(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Creating savepoint:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionSaved(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Created savepoint:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionReleasingSavepoint(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Releasing savepoint:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionReleasedSavepoint(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Released savepoint:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionRollingBackSavepoint(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Rolling back savepoint:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionRolledBackSavepoint(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Rolled back savepoint:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionCommittingRetaining(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Committing (retaining) transaction:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionCommittedRetaining(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Committed (retaining) transaction:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionRollingBackRetaining(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Rolling back (retaining) transaction:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}
	public static void TransactionRolledBackRetaining(IFbLogger log, FbTransaction transaction)
	{
		if (!log.IsEnabled(FbLogLevel.Debug))
			return;

		var sb = new StringBuilder();
		sb.AppendLine("Rolled back (retaining) transaction:");
		sb.AppendLine($"Isolation Level: {transaction.IsolationLevel}");
		log.Debug(sb.ToString());
	}

	static bool IsNullParameterValue(object value) => value == DBNull.Value || value == null;
}