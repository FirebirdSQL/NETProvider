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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.EntityFrameworkCore.Firebird.Design.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests.Design;

#pragma warning disable EF1001
public class FbCSharpRuntimeAnnotationCodeGeneratorTests
{
	ServiceProvider _serviceProvider;
	IRelationalTypeMappingSource _typeMappingSource;
	ICSharpRuntimeAnnotationCodeGenerator _generator;

	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		var services = new ServiceCollection().AddEntityFrameworkDesignTimeServices();
		new FbDesignTimeServices().ConfigureDesignTimeServices(services);
		_serviceProvider = services.BuildServiceProvider();
		_typeMappingSource = _serviceProvider.GetRequiredService<IRelationalTypeMappingSource>();
		_generator = _serviceProvider.GetRequiredService<ICSharpRuntimeAnnotationCodeGenerator>();
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		_serviceProvider.Dispose();
	}

	[Test]
	public void DesignTimeServicesUseFirebirdGenerator()
	{
		Assert.IsInstanceOf<FbCSharpRuntimeAnnotationCodeGenerator>(_generator);
	}

	[Test]
	public void AllTypeMappingsHaveDefaultUsableByCompiledModel()
	{
		var mappingTypes = typeof(FbTypeMappingSource).Assembly.GetTypes()
			.Where(t => typeof(RelationalTypeMapping).IsAssignableFrom(t) && !t.IsAbstract)
			.ToList();
		Assert.IsNotEmpty(mappingTypes);

		foreach (var mappingType in mappingTypes)
		{
			// Mirrors the requirements of CSharpRuntimeAnnotationCodeGenerator.CreateDefaultTypeMapping.
			var defaultProperty = mappingType.GetProperty("Default");
			Assert.IsNotNull(defaultProperty, mappingType.Name);
			Assert.IsTrue(defaultProperty.GetMethod.IsStatic && defaultProperty.GetMethod.IsPublic, mappingType.Name);
			Assert.IsTrue(mappingType.IsAssignableFrom(defaultProperty.PropertyType), mappingType.Name);
			Assert.IsInstanceOf(mappingType, defaultProperty.GetValue(null), mappingType.Name);
		}
	}

	[TestCase(typeof(DateTime), "TIMESTAMP", null)]
	[TestCase(typeof(DateTime), "DATE", FbDbType.Date)]
	[TestCase(typeof(TimeSpan), "TIME", null)]
	[TestCase(typeof(string), "VARCHAR(50)", FbDbType.VarChar)]
	[TestCase(typeof(string), "CHAR(10)", FbDbType.Char)]
	[TestCase(typeof(string), "BLOB SUB_TYPE TEXT", null)]
	public void GeneratesFbDbTypeOnlyWhenItDiffersFromDefault(Type clrType, string storeType, FbDbType? expectedFbDbType)
	{
		var mapping = _typeMappingSource.FindMapping(clrType, storeType);

		var code = Generate(mapping);

		if (expectedFbDbType == null)
		{
			StringAssert.DoesNotContain("fbDbType", code);
		}
		else
		{
			StringAssert.Contains($"property.TypeMapping = (({mapping.GetType().Name})property.TypeMapping).Clone(fbDbType: FbDbType.{expectedFbDbType})", code);
		}
	}

	[TestCase(typeof(DateTime), "DATE")]
	[TestCase(typeof(DateTime), "TIMESTAMP")]
	[TestCase(typeof(string), "VARCHAR(50)")]
	[TestCase(typeof(string), "CHAR(10)")]
	[TestCase(typeof(string), "BLOB SUB_TYPE TEXT")]
	[TestCase(typeof(TimeSpan), "TIME")]
	public void RecreatedMappingBehavesLikeOriginal(Type clrType, string storeType)
	{
		var mapping = _typeMappingSource.FindMapping(clrType, storeType);
		var fbDbType = (FbDbType)mapping.GetType().GetProperty("FbDbType").GetValue(mapping);

		// What the generated code does: Default.Clone(mappingInfo: ...), followed by Clone(fbDbType: ...) when needed.
		var defaultMapping = (RelationalTypeMapping)mapping.GetType().GetProperty("Default", BindingFlags.Public | BindingFlags.Static).GetValue(null);
		var recreated = defaultMapping.Clone(new RelationalTypeMappingInfo(storeTypeName: mapping.StoreType, size: mapping.Size, fixedLength: mapping.IsFixedLength, dbType: mapping.DbType));
		recreated = (RelationalTypeMapping)recreated.GetType().GetMethod("Clone", [typeof(FbDbType)]).Invoke(recreated, [fbDbType]);

		var value = clrType == typeof(DateTime) ? new DateTime(2024, 5, 6, 7, 8, 9) : clrType == typeof(TimeSpan) ? new TimeSpan(7, 8, 9) : (object)"x";
		using var command = new FbCommand();
		var original = (FbParameter)mapping.CreateParameter(command, "p", value);
		var copy = (FbParameter)recreated.CreateParameter(command, "p", value);

		Assert.AreEqual(mapping.StoreType, recreated.StoreType);
		Assert.AreEqual(original.FbDbType, copy.FbDbType);
		// FbTimeSpanTypeMapping cannot generate literals at all: its format string leaves the '.' unescaped.
		if (clrType != typeof(TimeSpan))
		{
			Assert.AreEqual(mapping.GenerateSqlLiteral(value), recreated.GenerateSqlLiteral(value));
		}
	}

	string Generate(RelationalTypeMapping mapping)
	{
		var parameters = new CSharpRuntimeAnnotationCodeGeneratorParameters(
			"property",
			"TestEntityType",
			"TestNamespace",
			new IndentedStringBuilder(),
			new IndentedStringBuilder(),
			new HashSet<string>(),
			new Dictionary<string, object>(),
			new Dictionary<object, string>(),
			new Dictionary<ITypeBase, string>(),
			nullable: false,
			nativeAot: true);
		_generator.Create(mapping, parameters);
		return parameters.MainBuilder.ToString();
	}
}
#pragma warning restore EF1001
