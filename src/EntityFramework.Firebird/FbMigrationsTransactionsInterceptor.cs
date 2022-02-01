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
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Diagnostics;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;

namespace EntityFramework.Firebird;

// Migrations are executed in Serializable transaction. Because of my "AUTONOMOUS TRANSACTION" usage
// I better use ReadCommitted. Here I plug in, in case of Migrations.
class FbMigrationsTransactionsInterceptor : IDbConnectionInterceptor
{
	public void BeginningTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
	{
		if (connection is FbConnection
			&& interceptionContext.IsolationLevel == IsolationLevel.Serializable
			&& IsInMigrations())
		{
			interceptionContext.Result = connection.BeginTransaction(IsolationLevel.ReadCommitted);
		}
	}

	public static bool IsInMigrations()
	{
		var stackTrace = new StackTrace(false);
		return stackTrace.GetFrames().Any(f => f.GetMethod().ReflectedType?.Namespace.Equals("System.Data.Entity.Migrations", StringComparison.Ordinal) ?? false);
	}

	public void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
	{ }

	public void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{ }

	public void Closing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{ }

	public void ConnectionStringGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{ }

	public void ConnectionStringGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{ }

	public void ConnectionStringSet(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
	{ }

	public void ConnectionStringSetting(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
	{ }

	public void ConnectionTimeoutGetting(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
	{ }

	public void ConnectionTimeoutGot(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
	{ }

	public void DataSourceGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{ }

	public void DataSourceGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{ }

	public void DatabaseGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{ }

	public void DatabaseGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{ }

	public void Disposed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{ }

	public void Disposing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{ }

	public void EnlistedTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
	{ }

	public void EnlistingTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
	{ }

	public void Opened(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{ }

	public void Opening(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
	{ }

	public void ServerVersionGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{ }

	public void ServerVersionGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
	{ }

	public void StateGetting(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext)
	{ }

	public void StateGot(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext)
	{ }
}
