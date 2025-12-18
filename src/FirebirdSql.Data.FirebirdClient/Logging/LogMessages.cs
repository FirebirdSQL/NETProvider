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
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Extensions.Logging;

namespace FirebirdSql.Data.Logging;

static class LogMessages
{
	public static void CommandExecution(ILogger log, FbCommand command)
	{
		if (!log.IsEnabled(LogLevel.Debug))
		{
			return;
		}

		if (FbLogManager.IsParameterLoggingEnabled && command.HasParameters)
		{
			var parameters = FbParameterCollectionToDictionary(command.Parameters);
			log.LogDebug("Command execution: {command}, {parameters}", command.CommandText, parameters);
		}
		else
		{
			log.LogDebug("Command execution: {command}", command.CommandText);
		}
	}

	public static void CommandExecution(ILogger log, FbBatchCommand command)
	{
		if (!log.IsEnabled(LogLevel.Debug))
		{
			return;
		}

		if (FbLogManager.IsParameterLoggingEnabled && command.HasParameters)
		{
			var parameters = command.BatchParameters.SelectMany(FbParameterCollectionToDictionary);
			log.LogDebug("Command execution: {command}, {parameters}", command.CommandText, parameters);
		}
		else
		{
			log.LogDebug("Command execution: {command}", command.CommandText);
		}
	}

	public static void ConnectionOpening(ILogger log, FbConnection connection) =>
		log.LogDebug("Opening connection: {connectionString}", connection.ConnectionString);

	public static void ConnectionOpened(ILogger log, FbConnection connection) =>
		log.LogDebug("Opened connection: {connectionString}", connection.ConnectionString);

	public static void ConnectionClosing(ILogger log, FbConnection connection) =>
		log.LogDebug("Closing connection: {connectionString}", connection.ConnectionString);

	public static void ConnectionClosed(ILogger log, FbConnection connection) =>
		log.LogDebug("Closed connection: {connectionString}", connection.ConnectionString);

	public static void TransactionBeginning(ILogger log, FbTransaction transaction) =>
		// TODO: Transaction Id?
		log.LogDebug("Beginning transaction: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionBegan(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Began transaction: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionCommitting(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Committing transaction: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionCommitted(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Committed transaction: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionRollingBack(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Rolling back transaction: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionRolledBack(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Rolled back transaction: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionSaving(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Creating savepoint: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionSaved(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Created savepoint: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionReleasingSavepoint(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Releasing savepoint: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionReleasedSavepoint(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Released savepoint: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionRollingBackSavepoint(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Rolling back savepoint: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionRolledBackSavepoint(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Rolled back savepoint: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionCommittingRetaining(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Committing (retaining) transaction: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionCommittedRetaining(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Committed (retaining) transaction: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionRollingBackRetaining(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Rolling back (retaining) transaction: {isolationLevel}", transaction.IsolationLevel);

	public static void TransactionRolledBackRetaining(ILogger log, FbTransaction transaction) =>
		log.LogDebug("Rolled back (retaining) transaction: {isolationLevel}", transaction.IsolationLevel);

	private static object NormalizeDbNull(object value) =>
		value == DBNull.Value || value == null
			? null
			: value;

	private static Dictionary<string, object> FbParameterCollectionToDictionary(FbParameterCollection parameters) =>
		parameters
			.Cast<DbParameter>()
			.ToDictionary(
				p => p.ParameterName,
				p => NormalizeDbNull(p.Value)
			);
}
