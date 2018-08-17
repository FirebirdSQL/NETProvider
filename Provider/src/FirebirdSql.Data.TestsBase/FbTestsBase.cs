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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Security.Cryptography;
using System.Text;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;
using NUnit.Framework;

namespace FirebirdSql.Data.TestsBase
{
	public abstract class FbTestsBase
	{
		#region	Fields

		private readonly bool _insertTestData;
		private FbConnection _connection;
		private FbTransaction _transaction;

		#endregion

		#region	Properties

		public FbServerType FbServerType { get; }
		public bool Compression { get; }

		public FbConnection Connection
		{
			get { return _connection; }
		}

		public FbTransaction Transaction
		{
			get { return _transaction; }
			set { _transaction = value; }
		}

		#endregion

		#region	Constructors

		public FbTestsBase(FbServerType serverType, bool compression, bool insertTestData = true)
		{
			FbServerType = serverType;
			Compression = compression;
			_insertTestData = insertTestData;
		}

		#endregion

		#region	SetUp and TearDown Methods

		[SetUp]
		public virtual void SetUp()
		{
			FbTestsSetup.SetUp(FbServerType, Compression);

			var cs = BuildConnectionString(FbServerType, Compression);
			if (_insertTestData)
			{
				InsertTestData(cs);
			}
			_connection = new FbConnection(cs);
			_connection.Open();
		}

		[TearDown]
		public virtual void TearDown()
		{
			var cs = BuildConnectionString(FbServerType, Compression);
			_connection.Dispose();
			if (_insertTestData)
			{
				DeleteAllData(cs);
			}
			FbConnection.ClearAllPools();
		}

		#endregion

		#region	Database Creation Methods

		private static void InsertTestData(string connectionString)
		{
			using (var connection = new FbConnection(connectionString))
			{
				connection.Open();

				var commandText = @"
insert into test (int_field, char_field, varchar_field, bigint_field, smallint_field, float_field, double_field, numeric_field, date_field, time_field, timestamp_field, clob_field, blob_field)
values(@int_field, @char_field, @varchar_field, @bigint_field, @smallint_field, @float_field, @double_field, @numeric_field, @date_field, @time_field, @timestamp_field, @clob_field, @blob_field)";

				using (var transaction = connection.BeginTransaction())
				{
					using (var command = new FbCommand(commandText, connection, transaction))
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

						command.Prepare();

						for (var i = 0; i < 100; i++)
						{
							command.Parameters["@int_field"].Value = i;
							command.Parameters["@char_field"].Value = "IRow " + i.ToString();
							command.Parameters["@varchar_field"].Value = "IRow Number " + i.ToString();
							command.Parameters["@bigint_field"].Value = i;
							command.Parameters["@smallint_field"].Value = i;
							command.Parameters["@float_field"].Value = (float)(i + 10) / 5;
							command.Parameters["@double_field"].Value = Math.Log(i, 10);
							command.Parameters["@numeric_field"].Value = (decimal)(i + 10) / 5;
							command.Parameters["@date_field"].Value = DateTime.Now;
							command.Parameters["@time_field"].Value = DateTime.Now;
							command.Parameters["@timestamp_field"].Value = DateTime.Now;
							command.Parameters["@clob_field"].Value = "IRow Number " + i.ToString();
							command.Parameters["@blob_field"].Value = Encoding.UTF8.GetBytes("IRow Number " + i.ToString());

							command.ExecuteNonQuery();
						}

						transaction.Commit();
					}
				}
			}
		}

		private static void DeleteAllData(string connectionString)
		{
			using (var connection = new FbConnection(connectionString))
			{
				connection.Open();

				var commandText = @"
execute block as
declare name type of column rdb$relations.rdb$relation_name;
begin
    for select rdb$relation_name from rdb$relations where coalesce(rdb$system_flag, 0) = 0 into name do
    begin
        execute statement 'delete from ' || name;
    end
end";

				using (var transaction = connection.BeginTransaction())
				{
					using (var command = new FbCommand(commandText, connection, transaction))
					{
						command.ExecuteNonQuery();
					}
					transaction.Commit();
				}
			}
		}

		#endregion

		#region	ConnectionString Building methods

		public static string BuildConnectionString(FbServerType serverType, bool compression)
		{
			return BuildConnectionStringBuilder(serverType, compression).ToString();
		}

		public static string BuildServicesConnectionString(FbServerType serverType, bool compression, bool includeDatabase)
		{
			return BuildServicesConnectionStringBuilder(serverType, compression, includeDatabase).ToString();
		}

		public static FbConnectionStringBuilder BuildServicesConnectionStringBuilder(FbServerType serverType, bool compression, bool includeDatabase)
		{
			var builder = new FbConnectionStringBuilder();
			builder.UserID = FbTestsSetup.UserID;
			builder.Password = FbTestsSetup.Password;
			builder.DataSource = FbTestsSetup.DataSource;
			if (includeDatabase)
			{
				builder.Database = FbTestsSetup.Database(serverType, compression);
			}
			builder.ServerType = serverType;
			return builder;
		}

		public static FbConnectionStringBuilder BuildConnectionStringBuilder(FbServerType serverType, bool compression)
		{
			var builder = new FbConnectionStringBuilder();
			builder.UserID = FbTestsSetup.UserID;
			builder.Password = FbTestsSetup.Password;
			builder.DataSource = FbTestsSetup.DataSource;
			builder.Database = FbTestsSetup.Database(serverType, compression);
			builder.Port = FbTestsSetup.Port;
			builder.Charset = FbTestsSetup.Charset;
			builder.Pooling = FbTestsSetup.Pooling;
			builder.Compression = compression;
			builder.ServerType = serverType;
			return builder;
		}

		#endregion

		#region	Methods

		protected int GetActiveConnections()
		{
			var csb = BuildConnectionStringBuilder(FbServerType, Compression);
			csb.Pooling = false;
			using (var conn = new FbConnection(csb.ToString()))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "select count(*) from mon$attachments where mon$attachment_id <> current_connection";
					return Convert.ToInt32(cmd.ExecuteScalar());
				}
			}
		}

		protected Version GetServerVersion()
		{
			var server = new FbServerProperties();
			server.ConnectionString = BuildServicesConnectionString(FbServerType, Compression, false);
			return FbServerProperties.ParseServerVersion(server.GetServerVersion());
		}

		protected bool EnsureVersion(Version version)
		{
			if (GetServerVersion() >= version)
				return true;
			Assert.Inconclusive("Not supported on this version.");
			return false;
		}

		protected bool EnsureServerType(FbServerType type)
		{
			if (FbServerType == type)
				return true;
			Assert.Inconclusive("Not supported on this server type.");
			return false;
		}

		protected static int GetId()
		{
			var rng = new RNGCryptoServiceProvider();
			var buffer = new byte[4];
			rng.GetBytes(buffer);
			return BitConverter.ToInt32(buffer, 0);
		}

		#endregion
	}
}
