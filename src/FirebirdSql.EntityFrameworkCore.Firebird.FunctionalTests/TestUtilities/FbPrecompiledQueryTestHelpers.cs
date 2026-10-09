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

using System.Collections.Generic;
using System.Reflection;
using FirebirdSql.EntityFrameworkCore.Firebird.Infrastructure.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;

public class FbPrecompiledQueryTestHelpers : PrecompiledQueryTestHelpers
{
	public static readonly FbPrecompiledQueryTestHelpers Instance = new();

	protected override IEnumerable<MetadataReference> BuildProviderMetadataReferences()
	{
#pragma warning disable EF1001
		yield return MetadataReference.CreateFromFile(typeof(FbOptionsExtension).Assembly.Location);
#pragma warning restore EF1001
		yield return MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location);
	}
}
