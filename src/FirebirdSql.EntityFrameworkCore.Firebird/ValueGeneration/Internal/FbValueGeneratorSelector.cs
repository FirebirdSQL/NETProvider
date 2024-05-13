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
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace FirebirdSql.EntityFrameworkCore.Firebird.ValueGeneration.Internal;

public class FbValueGeneratorSelector : RelationalValueGeneratorSelector
{
	readonly IFbSequenceValueGeneratorFactory _sequenceFactory;
	readonly IFbRelationalConnection _connection;
	readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
	readonly IRelationalCommandDiagnosticsLogger _commandLogger;

	public FbValueGeneratorSelector(ValueGeneratorSelectorDependencies dependencies, IFbSequenceValueGeneratorFactory sequenceFactory, IFbRelationalConnection connection, IRawSqlCommandBuilder rawSqlCommandBuilder, IRelationalCommandDiagnosticsLogger commandLogger)
		: base(dependencies)
	{
		_sequenceFactory = sequenceFactory;
		_connection = connection;
		_rawSqlCommandBuilder = rawSqlCommandBuilder;
		_commandLogger = commandLogger;
	}

	public new virtual IFbValueGeneratorCache Cache => (IFbValueGeneratorCache)base.Cache;

	public override ValueGenerator Select(IProperty property, ITypeBase entityType)
	{
		if (property.GetValueGeneratorFactory() != null
			|| property.GetValueGenerationStrategy() != FbValueGenerationStrategy.HiLo)
		{
			return base.Select(property, entityType);
		}

		var propertyType = property.ClrType.UnwrapNullableType().UnwrapEnumType();

		var generator = _sequenceFactory.TryCreate(
			property,
			propertyType,
			Cache.GetOrAddSequenceState(property, _connection),
			_connection,
			_rawSqlCommandBuilder,
			_commandLogger);

		if (generator != null)
		{
			return generator;
		}

		var converter = property.GetTypeMapping().Converter;
		if (converter != null
			&& converter.ProviderClrType != propertyType)
		{
			generator = _sequenceFactory.TryCreate(
				property,
				converter.ProviderClrType,
				Cache.GetOrAddSequenceState(property, _connection),
				_connection,
				_rawSqlCommandBuilder,
				_commandLogger);

			if (generator != null)
			{
				return generator.WithConverter(converter);
			}
		}

		throw new ArgumentException(
			CoreStrings.InvalidValueGeneratorFactoryProperty(
				nameof(FbSequenceValueGeneratorFactory), property.Name, property.DeclaringType.DisplayName()));
	}

	protected override ValueGenerator FindForType(IProperty property, ITypeBase entityType, Type clrType)
		=> property.ClrType.UnwrapNullableType() == typeof(Guid)
			? property.ValueGenerated == ValueGenerated.Never || property.GetDefaultValueSql() != null
				? new TemporaryGuidValueGenerator()
				: new SequentialGuidValueGenerator()
			: base.FindForType(property, entityType, clrType);
}
