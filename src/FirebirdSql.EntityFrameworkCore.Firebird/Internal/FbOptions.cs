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
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Internal;

public class FbOptions : IFbOptions
{
	public virtual void Initialize(IDbContextOptions options)
	{
		var fbOptions = options.FindExtension<FbOptionsExtension>() ?? new FbOptionsExtension();

		ExplicitParameterTypes = fbOptions.ExplicitParameterTypes ?? true;
		ExplicitStringLiteralTypes = fbOptions.ExplicitStringLiteralTypes ?? true;
	}

	public virtual void Validate(IDbContextOptions options)
	{
		var fbOptions = options.FindExtension<FbOptionsExtension>() ?? new FbOptionsExtension();

		if (ExplicitParameterTypes != (fbOptions.ExplicitParameterTypes ?? true))
		{
			throw new InvalidOperationException($"A call was made to '{nameof(FbDbContextOptionsBuilder.WithExplicitParameterTypes)}' that changed an option that must be constant within a service provider, but Entity Framework is not building its own internal service provider. Either allow EF to build the service provider by removing the call to '{nameof(DbContextOptionsBuilder.UseInternalServiceProvider)}', or ensure that the configuration for '{nameof(FbDbContextOptionsBuilder.WithExplicitParameterTypes)}' does not change for all uses of a given service provider passed to '{nameof(DbContextOptionsBuilder.UseInternalServiceProvider)}'.");
		}
		if (ExplicitStringLiteralTypes != (fbOptions.ExplicitStringLiteralTypes ?? true))
		{
			throw new InvalidOperationException($"A call was made to '{nameof(FbDbContextOptionsBuilder.WithExplicitStringLiteralTypes)}' that changed an option that must be constant within a service provider, but Entity Framework is not building its own internal service provider. Either allow EF to build the service provider by removing the call to '{nameof(DbContextOptionsBuilder.UseInternalServiceProvider)}', or ensure that the configuration for '{nameof(FbDbContextOptionsBuilder.WithExplicitStringLiteralTypes)}' does not change for all uses of a given service provider passed to '{nameof(DbContextOptionsBuilder.UseInternalServiceProvider)}'.");
		}
	}

	public virtual bool ExplicitParameterTypes { get; private set; }
	public virtual bool ExplicitStringLiteralTypes { get; private set; }
}
