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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities
{
	public class FbTestStore : RelationalTestStore
	{
		public static FbTestStore Create(string name)
			=> new FbTestStore(name, shared: false);

		public static FbTestStore GetOrCreate(string name)
			=> new FbTestStore(name, shared: true);

		public FbTestStore(string name, bool shared)
			: base(name, shared)
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
			ConnectionString = csb.ToString();
			Connection = new FbConnection(ConnectionString);
		}

		protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed, Action<DbContext> clean)
		{
			using (var context = createContext())
			{
				// create database explicitly to specify Page Size and Forced Writes
				FbConnection.CreateDatabase(ConnectionString, pageSize: 16384, forcedWrites: false, overwrite: true);
				context.Database.EnsureCreated();
				clean?.Invoke(context);
				Clean(context);
				seed?.Invoke(context);
			}
		}

		public override void OpenConnection()
		{
			base.OpenConnection();
			if (FbServerProperties.ParseServerVersion(Connection.ServerVersion) >= new Version(4, 0, 0, 0))
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
				using (var cmd = Connection.CreateCommand())
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
			=> builder.UseFirebird(Connection);

		public override void Clean(DbContext context)
		{ }
	}
}
