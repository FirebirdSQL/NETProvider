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

//$Authors = Jiri Cincura (jiri@cincura.net), Jean Ressouche, Rafael Almeida (ralms@ralms.net)

using System;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;

public class FbDatabaseCreator : RelationalDatabaseCreator
{
	readonly IFbRelationalConnection _connection;
	readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

	public FbDatabaseCreator(RelationalDatabaseCreatorDependencies dependencies, IFbRelationalConnection connection, IRawSqlCommandBuilder rawSqlCommandBuilder)
		: base(dependencies)
	{
		_connection = connection;
		_rawSqlCommandBuilder = rawSqlCommandBuilder;
	}

	public override void Create()
	{
		FbConnection.CreateDatabase(_connection.ConnectionString);
	}
	public override Task CreateAsync(CancellationToken cancellationToken = default)
	{
		return FbConnection.CreateDatabaseAsync(_connection.ConnectionString, cancellationToken: cancellationToken);
	}

	public override void Delete()
	{
		FbConnection.ClearPool((FbConnection)_connection.DbConnection);
		FbConnection.DropDatabase(_connection.ConnectionString);
	}
	public override Task DeleteAsync(CancellationToken cancellationToken = default)
	{
		FbConnection.ClearPool((FbConnection)_connection.DbConnection);
		return FbConnection.DropDatabaseAsync(_connection.ConnectionString, cancellationToken);
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
	public override async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await _connection.OpenAsync(cancellationToken);
			return true;
		}
		catch (FbException)
		{
			return false;
		}
		finally
		{
			await _connection.CloseAsync();
		}
	}

	public override bool HasTables()
	{
		return Dependencies.ExecutionStrategy.Execute(
			_connection,
			connection => Convert.ToInt64(CreateHasTablesCommand().ExecuteScalar(
				new RelationalCommandParameterObject(
					connection,
					null,
					null,
					Dependencies.CurrentContext.Context,
					Dependencies.CommandLogger)))
				!= 0);
	}
	public override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default)
	{
		return Dependencies.ExecutionStrategy.ExecuteAsync(
			_connection,
			async (connection, ct) => Convert.ToInt64(await CreateHasTablesCommand().ExecuteScalarAsync(
				new RelationalCommandParameterObject(
					connection,
					null,
					null,
					Dependencies.CurrentContext.Context,
					Dependencies.CommandLogger),
				ct))
				!= 0,
			cancellationToken);
	}

	IRelationalCommand CreateHasTablesCommand()
	   => _rawSqlCommandBuilder
		   .Build("SELECT COUNT(*) FROM rdb$relations WHERE COALESCE(rdb$system_flag, 0) = 0 AND rdb$view_blr IS NULL");
}
