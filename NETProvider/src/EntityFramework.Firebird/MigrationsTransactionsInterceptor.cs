using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Diagnostics;
using System.Linq;

namespace FirebirdSql.Data.EntityFramework6
{
	// Migrations are executed in Serializable transaction. Because of my "AUTONOMOUS TRANSACTION" usage
	// I better use ReadCommitted. Here I plug in, in case of Migrations.
	class MigrationsTransactionsInterceptor : IDbConnectionInterceptor
	{
		public void BeginningTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
		{
			if (interceptionContext.IsolationLevel == IsolationLevel.Serializable
				&& IsInMigrations())
			{
				interceptionContext.Result = connection.BeginTransaction(IsolationLevel.ReadCommitted);
			}
		}

		public static bool IsInMigrations()
		{
			var stackTrace = new StackTrace(false);
			return stackTrace.GetFrames().Any(f => f.GetMethod().ReflectedType.Namespace.Equals("System.Data.Entity.Migrations", StringComparison.Ordinal));
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
}
