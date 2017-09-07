using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using FirebirdClientConnection = FirebirdSql.Data.FirebirdClient.FbConnection;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Scaffolding.Internal
{
	public class FbDatabaseModelFactory : IDatabaseModelFactory
	{
		DatabaseModel _databaseModel;
		FirebirdClientConnection _connection;
		Version _serverVersion;

		public DatabaseModel Create(string connectionString, IEnumerable<string> tables, IEnumerable<string> schemas)
		{
			using (var connection = new FirebirdClientConnection(connectionString))
			{
				return Create(connection, tables, schemas);
			}
		}

		public DatabaseModel Create(DbConnection connection, IEnumerable<string> tables, IEnumerable<string> schemas)
		{
			ResetState();

			_connection = (FirebirdClientConnection)connection;

			var connectionStartedOpen = _connection.State == ConnectionState.Open;
			if (!connectionStartedOpen)
			{
				_connection.Open();
			}
			try
			{
				_databaseModel.DatabaseName = _connection.Database;
				_serverVersion = Data.Services.FbServerProperties.ParseServerVersion(_connection.ServerVersion);
#warning Finish
				return _databaseModel;
			}
			finally
			{
				if (!connectionStartedOpen)
				{
					_connection.Close();
				}
			}
		}

		void ResetState()
		{
			_databaseModel = new DatabaseModel();
			_connection = null;
			_serverVersion = null;
		}
	}
}
