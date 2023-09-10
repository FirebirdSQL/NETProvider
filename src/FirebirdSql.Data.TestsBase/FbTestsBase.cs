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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;
using NUnit.Framework;

namespace FirebirdSql.Data.TestsBase;

public abstract class FbTestsBase
{
	#region	Fields

	private readonly bool _insertTestData;

	#endregion

	#region	Properties

	public FbServerType ServerType { get; }
	public bool Compression { get; }
	public FbWireCrypt WireCrypt { get; }

	public FbConnection Connection { get; private set; }
	public Version ServerVersion { get; private set; }

	#endregion

	#region	Constructors

	public FbTestsBase(FbServerType serverType, bool compression, FbWireCrypt wireCrypt, bool insertTestData = true)
	{
		ServerType = serverType;
		Compression = compression;
		WireCrypt = wireCrypt;
		_insertTestData = insertTestData;
	}

	#endregion

	#region	SetUp and TearDown Methods

	[SetUp]
	public virtual async Task SetUp()
	{
		await FbTestsSetup.SetUp(ServerType, Compression, WireCrypt);

		var cs = BuildConnectionString(ServerType, Compression, WireCrypt);
		if (_insertTestData)
		{
			await InsertTestData(cs);
		}
		Connection = new FbConnection(cs);
		await Connection.OpenAsync();
		ServerVersion = FbServerProperties.ParseServerVersion(Connection.ServerVersion);
	}

	[TearDown]
	public virtual async Task TearDown()
	{
		var cs = BuildConnectionString(ServerType, Compression, WireCrypt);
		Connection.Dispose();
		if (_insertTestData)
		{
			await DeleteAllData(cs);
		}
		FbConnection.ClearAllPools();
	}

	#endregion

	#region	Database Creation Methods

	private static async Task InsertTestData(string connectionString)
	{
		await using (var connection = new FbConnection(connectionString))
		{
			await connection.OpenAsync();

			var commandText = @"
insert into test (int_field, char_field, varchar_field, bigint_field, smallint_field, float_field, double_field, numeric_field, date_field, time_field, timestamp_field, clob_field, blob_field)
values(@int_field, @char_field, @varchar_field, @bigint_field, @smallint_field, @float_field, @double_field, @numeric_field, @date_field, @time_field, @timestamp_field, @clob_field, @blob_field)";

			await using (var transaction = await connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand(commandText, connection, transaction))
				{
					command.Parameters.Add("@int_field", FbDbType.Integer);
					command.Parameters.Add("@char_field", FbDbType.Char);
					command.Parameters.Add("@varchar_field", FbDbType.VarChar);
					command.Parameters.Add("@bigint_field", FbDbType.BigInt);
					command.Parameters.Add("@smallint_field", FbDbType.SmallInt);
					command.Parameters.Add("@float_field", FbDbType.Double);
					command.Parameters.Add("@double_field", FbDbType.Double);
					command.Parameters.Add("@numeric_field", FbDbType.Numeric);
					command.Parameters.Add("@date_field", FbDbType.Date);
					command.Parameters.Add("@time_Field", FbDbType.Time);
					command.Parameters.Add("@timestamp_field", FbDbType.TimeStamp);
					command.Parameters.Add("@clob_field", FbDbType.Text);
					command.Parameters.Add("@blob_field", FbDbType.Binary);

					await command.PrepareAsync();

					for (var i = 0; i < 100; i++)
					{
						command.Parameters["@int_field"].Value = i;
						command.Parameters["@char_field"].Value = "IRow " + i.ToString();
						command.Parameters["@varchar_field"].Value = "IRow Number " + i.ToString();
						command.Parameters["@bigint_field"].Value = i;
						command.Parameters["@smallint_field"].Value = i;
						command.Parameters["@float_field"].Value = (float)(i + 10) / 5;
						command.Parameters["@double_field"].Value = (double)(i + 10) / 5;
						command.Parameters["@numeric_field"].Value = (decimal)(i + 10) / 5;
						command.Parameters["@date_field"].Value = DateTime.Now;
						command.Parameters["@time_field"].Value = DateTime.Now;
						command.Parameters["@timestamp_field"].Value = DateTime.Now;
						command.Parameters["@clob_field"].Value = "IRow Number " + i.ToString();
						command.Parameters["@blob_field"].Value = Encoding.UTF8.GetBytes("IRow Number " + i.ToString());

						await command.ExecuteNonQueryAsync();
					}

					await transaction.CommitAsync();
				}
			}
		}
	}

	private static async Task DeleteAllData(string connectionString)
	{
		await using (var connection = new FbConnection(connectionString))
		{
			await connection.OpenAsync();

			var commandText = @"
execute block as
declare name type of column rdb$relations.rdb$relation_name;
begin
    for select rdb$relation_name from rdb$relations where coalesce(rdb$system_flag, 0) = 0 into name do
    begin
        execute statement 'delete from ' || name;
    end
end";

			await using (var transaction = await connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand(commandText, connection, transaction))
				{
					await command.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
			}
		}
	}

	#endregion

	#region	ConnectionString Building methods

	public static string BuildConnectionString(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
	{
		return BuildConnectionStringBuilder(serverType, compression, wireCrypt).ToString();
	}

	public static string BuildServicesConnectionString(FbServerType serverType, bool compression, FbWireCrypt wireCrypt, bool includeDatabase)
	{
		return BuildServicesConnectionStringBuilder(serverType, compression, wireCrypt, includeDatabase).ToString();
	}

	public static FbConnectionStringBuilder BuildServicesConnectionStringBuilder(FbServerType serverType, bool compression, FbWireCrypt wireCrypt, bool includeDatabase)
	{
		var builder = new FbConnectionStringBuilder();
		builder.UserID = FbTestsSetup.UserID;
		builder.Password = FbTestsSetup.Password;
		builder.DataSource = FbTestsSetup.DataSource;
		if (includeDatabase)
		{
			builder.Database = FbTestsSetup.Database(serverType, compression, wireCrypt);
		}
		builder.ServerType = serverType;
		builder.Port = FbTestsSetup.Port;
		return builder;
	}

	public static FbConnectionStringBuilder BuildConnectionStringBuilder(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
	{
		var builder = new FbConnectionStringBuilder();
		builder.UserID = FbTestsSetup.UserID;
		builder.Password = FbTestsSetup.Password;
		builder.DataSource = FbTestsSetup.DataSource;
		builder.Database = FbTestsSetup.Database(serverType, compression, wireCrypt);
		builder.Port = FbTestsSetup.Port;
		builder.Charset = FbTestsSetup.Charset;
		builder.Pooling = FbTestsSetup.Pooling;
		builder.ServerType = serverType;
		builder.Compression = compression;
		builder.WireCrypt = wireCrypt;
		return builder;
	}

	#endregion

	#region	Methods

	protected async Task<int> GetActiveConnections()
	{
		var csb = BuildConnectionStringBuilder(ServerType, Compression, WireCrypt);
		csb.Pooling = false;
		await using (var conn = new FbConnection(csb.ToString()))
		{
			await conn.OpenAsync();
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "select count(*) from mon$attachments where mon$attachment_id <> current_connection";
				return Convert.ToInt32(await cmd.ExecuteScalarAsync());
			}
		}
	}

	protected bool EnsureServerVersionAtLeast(Version serverVersion)
	{
		if (FbTestsSetup.ServerVersionAtLeast(ServerVersion, serverVersion))
			return true;
		Assert.Inconclusive("Not supported on this version.");
		return false;
	}
	protected bool EnsureServerVersionAtMost(Version serverVersion)
	{
		if (FbTestsSetup.ServerVersionAtMost(ServerVersion, serverVersion))
			return true;
		Assert.Inconclusive("Not supported on this version.");
		return false;
	}

	protected bool EnsureServerType(FbServerType serverType)
	{
		if (ServerType == serverType)
			return true;
		Assert.Inconclusive($"Not supported on this {nameof(FbServerType)}.");
		return false;
	}

	protected bool EnsureWireCrypt(FbWireCrypt wireCrypt)
	{
		if (WireCrypt == wireCrypt)
			return true;
		Assert.Inconclusive($"Not supported with this {nameof(FbWireCrypt)}.");
		return false;
	}

	#endregion
}
