using System;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FirebirdClientConnection = FirebirdSql.Data.FirebirdClient.FbConnection;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal
{
	public class FbDatabaseCreator : RelationalDatabaseCreator
	{
		readonly IFbConnection _connection;
		readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

		public FbDatabaseCreator(RelationalDatabaseCreatorDependencies dependencies, IFbConnection connection, IRawSqlCommandBuilder rawSqlCommandBuilder)
			: base(dependencies)
		{
			_connection = connection;
			_rawSqlCommandBuilder = rawSqlCommandBuilder;
		}

		public override void Create()
		{
			FirebirdClientConnection.CreateDatabase(_connection.ConnectionString);
		}

		public override void Delete()
		{
			FirebirdClientConnection.ClearPool((FirebirdClientConnection)_connection.DbConnection);
			FirebirdClientConnection.DropDatabase(_connection.ConnectionString);
		}

		public override bool Exists()
		{
			try
			{
				_connection.Open();
				return true;
			}
			catch (FbException)
			{
				return false;
			}
			finally
			{
				_connection.Close();
			}
		}

		protected override bool HasTables()
			=> Dependencies.ExecutionStrategyFactory.Create().Execute(_connection, connection => Convert.ToInt32(CreateHasTablesCommand().ExecuteScalar(connection)) != 0);

		IRelationalCommand CreateHasTablesCommand()
		   => _rawSqlCommandBuilder
			   .Build("SELECT COUNT(*) FROM rdb$relations WHERE COALESCE(r.rdb$system_flag, 0) = 0 AND rdb$view_blr IS NULL");
	}
}
