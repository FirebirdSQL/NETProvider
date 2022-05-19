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
using System.Collections.Generic;
using System.Data;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;

public class FbTypeMappingSource : RelationalTypeMappingSource
{
	public const int BinaryMaxSize = Int32.MaxValue;
	public const int UnicodeVarcharMaxSize = VarcharMaxSize / 4;
	public const int VarcharMaxSize = 32765;
	public const int DefaultDecimalPrecision = 18;
	public const int DefaultDecimalScale = 2;

	readonly FbBoolTypeMapping _boolean = new FbBoolTypeMapping();

	readonly ShortTypeMapping _smallint = new ShortTypeMapping("SMALLINT", DbType.Int16);
	readonly IntTypeMapping _integer = new IntTypeMapping("INTEGER", DbType.Int32);
	readonly LongTypeMapping _bigint = new LongTypeMapping("BIGINT", DbType.Int64);

	readonly FbStringTypeMapping _char = new FbStringTypeMapping("CHAR", DbType.StringFixedLength, FbDbType.Char);
	readonly FbStringTypeMapping _varchar = new FbStringTypeMapping("VARCHAR", DbType.String, FbDbType.VarChar);
	readonly FbStringTypeMapping _clob = new FbStringTypeMapping("BLOB SUB_TYPE TEXT", DbType.String, FbDbType.Text);

	readonly FbByteArrayTypeMapping _binary = new FbByteArrayTypeMapping();

	readonly FloatTypeMapping _float = new FloatTypeMapping("FLOAT");
	readonly DoubleTypeMapping _double = new DoubleTypeMapping("DOUBLE PRECISION");
	readonly DecimalTypeMapping _decimal = new DecimalTypeMapping($"DECIMAL({DefaultDecimalPrecision},{DefaultDecimalScale})");

	readonly FbDateTimeTypeMapping _timestamp = new FbDateTimeTypeMapping("TIMESTAMP", FbDbType.TimeStamp);
	readonly FbDateTimeTypeMapping _date = new FbDateTimeTypeMapping("DATE", FbDbType.Date);
	readonly FbDateOnlyTypeMapping _dateOnly = new FbDateOnlyTypeMapping("DATE");

	readonly FbTimeSpanTypeMapping _timeSpan = new FbTimeSpanTypeMapping("TIME", FbDbType.Time);
	readonly FbTimeOnlyTypeMapping _timeOnly = new FbTimeOnlyTypeMapping("TIME");

	readonly FbGuidTypeMapping _guid = new FbGuidTypeMapping();

	readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;
	readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;
	readonly HashSet<string> _disallowedMappings;

	public FbTypeMappingSource(TypeMappingSourceDependencies dependencies, RelationalTypeMappingSourceDependencies relationalDependencies)
		: base(dependencies, relationalDependencies)
	{
		_storeTypeMappings = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
			{
				{ "BOOLEAN", _boolean },
				{ "SMALLINT", _smallint },
				{ "INTEGER", _integer },
				{ "BIGINT", _bigint },
				{ "CHAR", _char },
				{ "VARCHAR", _varchar },
				{ "BLOB SUB_TYPE TEXT", _clob },
				{ "BLOB SUB_TYPE BINARY", _binary },
				{ "FLOAT", _float },
				{ "DOUBLE PRECISION", _double },
				{ "DECIMAL", _decimal },
				{ "TIMESTAMP", _timestamp },
				{ "DATE", _date },
				{ "TIME", _timeSpan },
				{ "CHAR(16) CHARACTER SET OCTETS", _guid },
			};

		_clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>()
			{
				{ typeof(bool), _boolean },
				{ typeof(short), _smallint },
				{ typeof(int), _integer },
				{ typeof(long), _bigint },
				{ typeof(float), _float },
				{ typeof(double), _double},
				{ typeof(decimal), _decimal },
				{ typeof(DateTime), _timestamp },
				{ typeof(TimeSpan), _timeSpan },
				{ typeof(Guid), _guid },
				{ typeof(DateOnly), _dateOnly },
				{ typeof(TimeOnly), _timeOnly },
			};

		_disallowedMappings = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
					"CHARACTER",
					"CHAR",
					"VARCHAR",
					"CHARACTER VARYING",
					"CHAR VARYING",
			};
	}

	protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
	{
		return FindRawMapping(mappingInfo)?.Clone(mappingInfo) ?? base.FindMapping(mappingInfo);
	}

	protected override void ValidateMapping(CoreTypeMapping mapping, IProperty property)
	{
		var relationalMapping = mapping as RelationalTypeMapping;

		if (_disallowedMappings.Contains(relationalMapping?.StoreType))
		{
			if (property == null)
			{
				throw new ArgumentException($"Data type '{relationalMapping.StoreType}' is not supported in this form. Either specify the length explicitly in the type name or remove the data type and use APIs such as HasMaxLength.");
			}

			throw new ArgumentException($"Data type '{relationalMapping.StoreType}' for property '{property}' is not supported in this form. Either specify the length explicitly in the type name or remove the data type and use APIs such as HasMaxLength.");
		}
	}

	RelationalTypeMapping FindRawMapping(RelationalTypeMappingInfo mappingInfo)
	{
		var clrType = mappingInfo.ClrType;
		var storeTypeName = mappingInfo.StoreTypeName;
		var storeTypeNameBase = mappingInfo.StoreTypeNameBase;
		var isUnicode = IsUnicode(mappingInfo.IsUnicode);

		if (storeTypeName != null)
		{
			if (clrType == typeof(float)
				&& mappingInfo.Size != null
				&& mappingInfo.Size <= 24
				&& (storeTypeNameBase.Equals("FLOAT", StringComparison.OrdinalIgnoreCase)
					|| storeTypeNameBase.Equals("DOUBLE PRECISION", StringComparison.OrdinalIgnoreCase)))
			{
				return _float;
			}

			if (_storeTypeMappings.TryGetValue(storeTypeName, out var mapping) || _storeTypeMappings.TryGetValue(storeTypeNameBase, out mapping))
			{
				return clrType == null || mapping.ClrType == clrType
					? mapping
					: null;
			}
		}

		if (clrType != null)
		{
			if (_clrTypeMappings.TryGetValue(clrType, out var mapping))
			{
				return mapping;
			}

			if (clrType == typeof(string))
			{
				var isFixedLength = mappingInfo.IsFixedLength == true;
				var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? 256 : (int?)null);
				var maxSize = isUnicode ? UnicodeVarcharMaxSize : VarcharMaxSize;

				if (size > maxSize)
				{
					size = isFixedLength ? maxSize : (int?)null;
				}

				if (size == null)
				{
					return _clob;
				}
				else
				{
					if (!isFixedLength)
					{
						return new FbStringTypeMapping($"VARCHAR({size})", DbType.String, FbDbType.VarChar, size, isUnicode);
					}
					else
					{
						return new FbStringTypeMapping($"CHAR({size})", DbType.StringFixedLength, FbDbType.Char, size, isUnicode);
					}
				}
			}

			if (clrType == typeof(byte[]))
			{
				return _binary;
			}
		}

		return null;
	}

	public static bool IsUnicode(RelationalTypeMapping mapping) => IsUnicode(mapping?.IsUnicode);
	public static bool IsUnicode(RelationalTypeMappingInfo mappingInfo) => IsUnicode(mappingInfo.IsUnicode);
	public static bool IsUnicode(bool? isUnicode) => isUnicode ?? true;
}
