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
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests2
{
	public class FbTestStore : RelationalTestStore
	{
		public FbTestStore(string name)
			: base(name, false)
		{
			var csb = new FbConnectionStringBuilder
			{
				Database = name,
				DataSource = "localhost",
				UserID = "sysdba",
				Password = "masterkey",
				Pooling = false,
			};
			ConnectionString = csb.ToString();
			Connection = new FbConnection(ConnectionString);
			FbConnection.CreateDatabase(ConnectionString, pageSize: 16384, forcedWrites: false, overwrite: true);
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
			FbConnection.DropDatabase(ConnectionString);
			Connection.Dispose();
			base.Dispose();
		}

		public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
			=> builder.UseFirebird(Connection);

		public override void Clean(DbContext context)
		{ }
	}
}
