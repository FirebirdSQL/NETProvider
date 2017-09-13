/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net), Jean Ressouche, Rafael Almeida (ralms@ralms.net)

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
