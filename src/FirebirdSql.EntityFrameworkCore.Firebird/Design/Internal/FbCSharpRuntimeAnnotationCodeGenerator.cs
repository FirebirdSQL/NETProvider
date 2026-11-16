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

//$Authors = Niek Schoemaker

using System;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Design.Internal;

#pragma warning disable EF1001
public class FbCSharpRuntimeAnnotationCodeGenerator : RelationalCSharpRuntimeAnnotationCodeGenerator
{
	int _typeMappingNestingCount;

	public FbCSharpRuntimeAnnotationCodeGenerator(
		CSharpRuntimeAnnotationCodeGeneratorDependencies dependencies,
		RelationalCSharpRuntimeAnnotationCodeGeneratorDependencies relationalDependencies)
		: base(dependencies, relationalDependencies)
	{ }

	public override bool Create(
		CoreTypeMapping typeMapping,
		CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
		ValueComparer valueComparer = null,
		ValueComparer keyValueComparer = null,
		ValueComparer providerValueComparer = null)
	{
		_typeMappingNestingCount++;
		try
		{
			var result = base.Create(typeMapping, parameters, valueComparer, keyValueComparer, providerValueComparer);
			AddFbDbType(typeMapping, parameters);
			return result;
		}
		finally
		{
			_typeMappingNestingCount--;
		}
	}

	// The base generator recreates a mapping as Default.Clone(...) from the standard RelationalTypeMappingParameters only,
	// so the FbDbType of mappings that have several variants (e.g. TIMESTAMP and DATE) would silently be the one of Default.
	void AddFbDbType(CoreTypeMapping typeMapping, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
	{
		var (fbDbType, defaultFbDbType) = typeMapping switch
		{
			FbDateTimeTypeMapping mapping => (mapping.FbDbType, FbDateTimeTypeMapping.Default.FbDbType),
			FbStringTypeMapping mapping => (mapping.FbDbType, FbStringTypeMapping.Default.FbDbType),
			FbTimeSpanTypeMapping mapping => (mapping.FbDbType, FbTimeSpanTypeMapping.Default.FbDbType),
			_ => ((FbDbType?)null, (FbDbType?)null),
		};
		if (fbDbType == defaultFbDbType)
		{
			return;
		}

		if (_typeMappingNestingCount > 1)
		{
			throw new NotSupportedException(
				$"Type mapping '{typeMapping.GetType().Name}' with store type '{((RelationalTypeMapping)typeMapping).StoreType}' and FbDbType '{fbDbType}' is not supported as an element type mapping in a compiled model.");
		}

		var code = Dependencies.CSharpHelper;
		AddNamespace(typeof(FbDbType), parameters.Namespaces);
		parameters.MainBuilder
			.AppendLine(";")
			.Append($"{parameters.TargetName}.TypeMapping = (({typeMapping.GetType().Name}){parameters.TargetName}.TypeMapping).Clone(fbDbType: ")
			.Append(code.Literal(fbDbType.Value))
			.Append(")");
	}
}
#pragma warning restore EF1001
