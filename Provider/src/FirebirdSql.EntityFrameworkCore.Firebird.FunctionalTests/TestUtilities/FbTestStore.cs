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
using System.Collections.Generic;
using FirebirdSql.Data.FirebirdClient;
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

		static readonly Dictionary<string, int> DatabasesCounter = new Dictionary<string, int>();

		public FbTestStore(string name, bool shared)
			: base(name, shared)
		{
			var csb = new FbConnectionStringBuilder
			{
				Database = name,
				DataSource = "localhost",
				UserID = "sysdba",
				Password = "masterkey",
				Pooling = false,
				Charset = "utf8"
			};
			ConnectionString = csb.ToString();
			lock (DatabasesCounter)
			{
				if (DatabasesCounter.TryGetValue(Name, out var counter))
				{
					DatabasesCounter[Name] = counter + 1;
				}
				else
				{
					DatabasesCounter.Add(Name, 1);
					FbConnection.CreateDatabase(ConnectionString, pageSize: 16384, forcedWrites: false, overwrite: true);
				}
			}
			Connection = new FbConnection(ConnectionString);
			Connection.Open();
		}

		protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed)
		{
			using (var context = createContext())
			{
				context.Database.EnsureCreated();
				seed(context);
			}
		}

		public override void Dispose()
		{
			Connection.Close();
			Connection.Dispose();
			lock (DatabasesCounter)
			{
				var counter = DatabasesCounter[Name];
				if (counter > 1)
				{
					DatabasesCounter[Name] = counter - 1;
				}
				else
				{
					DatabasesCounter.Remove(Name);
					FbConnection.DropDatabase(ConnectionString);
				}
			}
			base.Dispose();
		}

		public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
			=> builder.UseFirebird(Connection);

		public override void Clean(DbContext context)
		{ }
	}
}
