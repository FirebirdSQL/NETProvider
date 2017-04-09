/*
 *	Firebird ADO.NET Data provider for .NET	and	Mono
 *
 *	   The contents	of this	file are subject to	the	Initial
 *	   Developer's Public License Version 1.0 (the "License");
 *	   you may not use this	file except	in compliance with the
 *	   License.	You	may	obtain a copy of the License at
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software	distributed	under the License is distributed on
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *	   express or implied.	See	the	License	for	the	specific
 *	   language	governing rights and limitations under the License.
 *
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	Copyright (c) 2015-2016 Jiri Cincura (jiri@cincura.net)
 *	All	Rights Reserved.
 */

using System;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

using NUnit.Framework;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;
using System.Diagnostics;

namespace FirebirdSql.Data.UnitTests
{
	public class TestsBase
	{
		#region	Fields

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

		public TestsBase(FbServerType serverType, bool compression)
		{
			FbServerType = serverType;
			Compression = compression;
		}

		#endregion

		#region	SetUp and TearDown Methods

		[SetUp]
		public virtual void SetUp()
		{
			TestsSetup.SetUp(FbServerType, Compression);

			string cs = BuildConnectionString(FbServerType, Compression);
			InsertTestData(cs);
			_connection = new FbConnection(cs);
			_connection.Open();
		}

		[TearDown]
		public virtual void TearDown()
		{
			string cs = BuildConnectionString(FbServerType, Compression);
			_connection.Dispose();
			DeleteAllData(cs);
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

						for (int i = 0; i < 100; i++)
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
			FbConnectionStringBuilder cs = new FbConnectionStringBuilder();
			cs.UserID = TestsSetup.UserID;
			cs.Password = TestsSetup.Password;
			cs.DataSource = TestsSetup.DataSource;
			if (includeDatabase)
			{
				cs.Database = TestsSetup.Database(serverType, compression);
			}
			cs.ServerType = serverType;
			return cs.ToString();
		}

		public static FbConnectionStringBuilder BuildConnectionStringBuilder(FbServerType serverType, bool compression)
		{
			FbConnectionStringBuilder cs = new FbConnectionStringBuilder();
			cs.UserID = TestsSetup.UserID;
			cs.Password = TestsSetup.Password;
			cs.DataSource = TestsSetup.DataSource;
			cs.Database = TestsSetup.Database(serverType, compression);
			cs.Port = TestsSetup.Port;
			cs.Charset = TestsSetup.Charset;
			cs.Pooling = TestsSetup.Pooling;
			cs.Compression = compression;
			cs.ServerType = serverType;
			return cs;
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
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			byte[] buffer = new byte[4];
			rng.GetBytes(buffer);
			return BitConverter.ToInt32(buffer, 0);
		}

		#endregion
	}
}
