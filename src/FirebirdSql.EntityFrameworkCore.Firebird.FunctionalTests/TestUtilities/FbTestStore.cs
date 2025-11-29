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
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;

public class FbTestStore : RelationalTestStore
{
	public static FbTestStore Create(string name)
		=> new FbTestStore(name, shared: false);

	public static FbTestStore GetOrCreate(string name)
		=> new FbTestStore(name, shared: true);

	public FbTestStore(string name, bool shared)
		: base(name, shared, CreateConnection(name, shared))
	{ }

	protected override string OpenDelimiter => "\"";
	protected override string CloseDelimiter => "\"";

	public Version ServerVersion { get; private set; }
	public bool ServerLessThan4() => ServerVersion < new Version(4, 0, 0, 0);

	public bool ServerLessThan5() => ServerVersion < new Version(5, 0, 0, 0);

	protected override async Task InitializeAsync(Func<DbContext> createContext, Func<DbContext, Task> seed, Func<DbContext, Task> clean)
	{
		if (Connection.State != ConnectionState.Closed)
		{
			// Connections are maintained between tests, they have to be closed to create a new database
			await Connection.CloseAsync();
		}
		// create database explicitly to specify Page Size and Forced Writes
		await FbConnection.CreateDatabaseAsync(ConnectionString, pageSize: 16384, forcedWrites: false, overwrite: true);
		await using (var context = createContext())
		{
			try
			{
				await context.Database.EnsureCreatedAsync();
			}
			catch (FbException ex) when (ServerLessThan4() && ex.Message.EndsWith("Name longer than database column size", StringComparison.Ordinal))
			{
				return;
			}
			if (clean != null)
			{
				await clean(context);
			}
			if (seed != null)
			{
				await seed(context);
			}
		}
	}

	public override void OpenConnection()
	{
		base.OpenConnection();
		ServerVersion = FbServerProperties.ParseServerVersion(Connection.ServerVersion);
		if (ServerVersion >= new Version(4, 0, 0, 0))
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "set bind of decfloat to legacy";
				cmd.ExecuteNonQuery();
				cmd.CommandText = "set bind of int128 to legacy";
				cmd.ExecuteNonQuery();
				cmd.CommandText = "set bind of time zone to legacy";
				cmd.ExecuteNonQuery();
			}
		}
	}

	public override async Task OpenConnectionAsync()
	{
		await base.OpenConnectionAsync();
		if (FbServerProperties.ParseServerVersion(Connection.ServerVersion) >= new Version(4, 0, 0, 0))
		{
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "set bind of decfloat to legacy";
				await cmd.ExecuteNonQueryAsync();
				cmd.CommandText = "set bind of int128 to legacy";
				await cmd.ExecuteNonQueryAsync();
				cmd.CommandText = "set bind of time zone to legacy";
				await cmd.ExecuteNonQueryAsync();
			}
		}
	}

	public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
		=> builder.UseFirebird(Connection,
			x => x.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery));

	static FbConnection CreateConnection(string name, bool shared)
	{
		var csb = new FbConnectionStringBuilder
		{
			Database = $"EFCore_{name}.fdb",
			DataSource = "localhost",
			UserID = "sysdba",
			Password = "masterkey",
			Pooling = false,
			Charset = "utf8"
		};
		return new FbConnection(csb.ToString());
	}
}
