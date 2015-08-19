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
 *	All	Rights Reserved.
 *
 *  Contributors:
 *   Jiri Cincura (jiri@cincura.net)
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

		private FbConnection connection;
		private FbTransaction transaction;
		private bool withTransaction;
		private FbServerType fbServerType;

		#endregion

		#region	Properties

		public FbConnection Connection
		{
			get { return connection; }
		}

		public FbServerType FbServerType
		{
			get { return fbServerType; }
		}

		public FbTransaction Transaction
		{
			get { return transaction; }
			set { transaction = value; }
		}

		#endregion

		#region	Constructors

		public TestsBase(FbServerType serverType)
			: this(serverType, false)
		{
		}

		public TestsBase(FbServerType fbServerType, bool withTransaction)
		{
			this.fbServerType = fbServerType;
			this.withTransaction = withTransaction;
		}

		#endregion

		#region	SetUp and TearDown Methods

		[SetUp]
		public virtual void SetUp()
		{
			string cs = BuildConnectionString(this.fbServerType);
			this.connection = new FbConnection(cs);
			this.connection.Open();

			if (this.withTransaction)
			{
				this.transaction = this.connection.BeginTransaction();
			}
		}

		[TearDown]
		public virtual void TearDown()
		{
			if (this.withTransaction)
			{
				try
				{
					if (!transaction.IsUpdated)
					{
						transaction.Commit();
					}
				}
				catch
				{
				}
				try
				{
					transaction.Dispose();
				}
				catch
				{
				}
			}
			if (connection != null)
			{
				connection.Dispose();
			}
			DeleteAllData(BuildConnectionString(this.fbServerType));
		}

		#endregion

		#region	Database Creation Methods

		internal static void DeleteAllData(string connectionString)
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

		public static string BuildConnectionString(FbServerType serverType)
		{
			return BuildConnectionStringBuilder(serverType).ToString();
		}

		public static string BuildServicesConnectionString(FbServerType serverType)
		{
			return BuildServicesConnectionString(serverType, true);
		}

		public static string BuildServicesConnectionString(FbServerType serverType, bool includeDatabase)
		{
			FbConnectionStringBuilder cs = new FbConnectionStringBuilder();

			cs.UserID = TestsSetup.UserID;
			cs.Password = TestsSetup.Password;
			cs.DataSource = TestsSetup.DataSource;
			if (includeDatabase)
			{
				cs.Database = TestsSetup.Database;
			}
			cs.ServerType = serverType;

			return cs.ToString();
		}

		public static FbConnectionStringBuilder BuildConnectionStringBuilder(FbServerType serverType)
		{
			FbConnectionStringBuilder cs = new FbConnectionStringBuilder();

			cs.UserID = TestsSetup.UserID;
			cs.Password = TestsSetup.Password;
			cs.DataSource = TestsSetup.DataSource;
			cs.Database = TestsSetup.Database;
			cs.Port = TestsSetup.Port;
			cs.Charset = TestsSetup.Charset;
			cs.Pooling = TestsSetup.Pooling;
			cs.ServerType = serverType;
			return cs;
		}

		#endregion

		#region	Methods

		public static int GetId()
		{
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

			byte[] buffer = new byte[4];

			rng.GetBytes(buffer);

			return BitConverter.ToInt32(buffer, 0);
		}

		public static Version GetServerVersion(FbServerType serverType)
		{
			var server = new FbServerProperties();
			server.ConnectionString = BuildServicesConnectionString(serverType);
			return FbServerProperties.ParseServerVersion(server.GetServerVersion());
		}

		#endregion
	}
}
