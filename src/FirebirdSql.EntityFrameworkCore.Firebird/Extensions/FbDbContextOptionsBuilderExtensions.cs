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
using System.Data.Common;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore;

public static class FbDbContextOptionsBuilderExtensions
{
	public static DbContextOptionsBuilder UseFirebird(this DbContextOptionsBuilder optionsBuilder, string connectionString, Action<FbDbContextOptionsBuilder> fbOptionsAction = null)
	{
		var extension = (FbOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnectionString(connectionString);
		((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
		fbOptionsAction?.Invoke(new FbDbContextOptionsBuilder(optionsBuilder));
		return optionsBuilder;
	}

	public static DbContextOptionsBuilder UseFirebird(this DbContextOptionsBuilder optionsBuilder, DbConnection connection, Action<FbDbContextOptionsBuilder> fbOptionsAction = null)
	{
		var extension = (FbOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnection(connection);
		((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
		fbOptionsAction?.Invoke(new FbDbContextOptionsBuilder(optionsBuilder));
		return optionsBuilder;
	}

	public static DbContextOptionsBuilder<TContext> UseFirebird<TContext>(this DbContextOptionsBuilder<TContext> optionsBuilder, string connectionString, Action<FbDbContextOptionsBuilder> fbOptionsAction = null)
		where TContext : DbContext
	{
		return (DbContextOptionsBuilder<TContext>)UseFirebird((DbContextOptionsBuilder)optionsBuilder, connectionString, fbOptionsAction);
	}

	public static DbContextOptionsBuilder<TContext> UseFirebird<TContext>(this DbContextOptionsBuilder<TContext> optionsBuilder, DbConnection connection, Action<FbDbContextOptionsBuilder> fbOptionsAction = null)
		where TContext : DbContext
	{
		return (DbContextOptionsBuilder<TContext>)UseFirebird((DbContextOptionsBuilder)optionsBuilder, connection, fbOptionsAction);
	}

	static FbOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
		=> optionsBuilder.Options.FindExtension<FbOptionsExtension>()
			?? new FbOptionsExtension();
}
