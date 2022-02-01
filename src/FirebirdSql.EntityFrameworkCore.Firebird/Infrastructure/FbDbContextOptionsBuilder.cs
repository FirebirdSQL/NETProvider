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

using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure;

public class FbDbContextOptionsBuilder : RelationalDbContextOptionsBuilder<FbDbContextOptionsBuilder, FbOptionsExtension>
{
	public FbDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
		: base(optionsBuilder)
	{ }

	public virtual FbDbContextOptionsBuilder WithExplicitParameterTypes(bool explicitParameterTypes = true)
		=> WithOption(e => e.WithExplicitParameterTypes(explicitParameterTypes));

	public virtual FbDbContextOptionsBuilder WithExplicitStringLiteralTypes(bool explicitStringLiteralTypes = true)
		=> WithOption(e => e.WithExplicitStringLiteralTypes(explicitStringLiteralTypes));
}
