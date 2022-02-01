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
using System.Reflection;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Scaffolding.Internal;

public class FbProviderCodeGenerator : ProviderCodeGenerator
{
	static readonly MethodInfo UseFirebirdMethodInfo
		= typeof(FbDbContextOptionsBuilderExtensions).GetRequiredRuntimeMethod(
			nameof(FbDbContextOptionsBuilderExtensions.UseFirebird),
			typeof(DbContextOptionsBuilder),
			typeof(string),
			typeof(Action<FbDbContextOptionsBuilder>));

	public FbProviderCodeGenerator(ProviderCodeGeneratorDependencies dependencies)
		: base(dependencies)
	{ }

	public override MethodCallCodeFragment GenerateUseProvider(string connectionString, MethodCallCodeFragment providerOptions)
	{
		return new MethodCallCodeFragment(
			UseFirebirdMethodInfo,
			providerOptions == null
				? new object[] { connectionString }
				: new object[] { connectionString, new NestedClosureCodeFragment("x", providerOptions) });
	}
}
