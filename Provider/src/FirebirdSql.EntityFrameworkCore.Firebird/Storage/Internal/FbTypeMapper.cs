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
using System.Collections.Generic;
using System.Data;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.EntityFrameworkCore.Firebird.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal
{
	public class FbTypeMapper : RelationalTypeMapper
	{
		public const int BinaryMaxSize = Int32.MaxValue;
		public const int VarcharMaxSize = 32765;
		public const int NVarcharMaxSize = VarcharMaxSize / 4;
		public const int DefaultDecimalPrecision = 18;
		public const int DefaultDecimalScale = 2;

		readonly FbBoolTypeMapping _boolean = new FbBoolTypeMapping();

		readonly ShortTypeMapping _smallint = new ShortTypeMapping("SMALLINT", DbType.Int16);
		readonly IntTypeMapping _integer = new IntTypeMapping("INTEGER", DbType.Int32);
		readonly LongTypeMapping _bigint = new LongTypeMapping("BIGINT", DbType.Int64);

		readonly FbStringTypeMapping _char = new FbStringTypeMapping("CHAR", FbDbType.Char);
		readonly FbStringTypeMapping _varchar = new FbStringTypeMapping("VARCHAR", FbDbType.VarChar);
		readonly FbStringTypeMapping _varcharMax = new FbStringTypeMapping($"VARCHAR({VarcharMaxSize})", FbDbType.VarChar, size: VarcharMaxSize);
		readonly FbStringTypeMapping _nvarcharMax = new FbStringTypeMapping($"VARCHAR({NVarcharMaxSize})", FbDbType.VarChar, size: NVarcharMaxSize);
		readonly FbStringTypeMapping _varchar256 = new FbStringTypeMapping($"VARCHAR(256)", FbDbType.VarChar, size: 256);
		readonly FbStringTypeMapping _nvarchar256 = new FbStringTypeMapping($"VARCHAR(256)", FbDbType.VarChar, size: 256);
		readonly FbStringTypeMapping _clob = new FbStringTypeMapping("BLOB SUB_TYPE TEXT", FbDbType.Text);

		readonly FbByteArrayTypeMapping _binary = new FbByteArrayTypeMapping();

		readonly FloatTypeMapping _float = new FloatTypeMapping("FLOAT");
		readonly DoubleTypeMapping _double = new DoubleTypeMapping("DOUBLE PRECISION");
		readonly DecimalTypeMapping _decimal = new DecimalTypeMapping($"DECIMAL({DefaultDecimalPrecision},{DefaultDecimalScale})");

		readonly FbDateTimeTypeMapping _timeStamp = new FbDateTimeTypeMapping("TIMESTAMP", FbDbType.TimeStamp);
		readonly FbDateTimeTypeMapping _date = new FbDateTimeTypeMapping("DATE", FbDbType.Date);
		readonly FbDateTimeTypeMapping _time = new FbDateTimeTypeMapping("TIME", FbDbType.Time);

		readonly FbGuidTypeMapping _guid = new FbGuidTypeMapping();

		readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;
		readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;
		readonly HashSet<string> _disallowedMappings;

		public FbTypeMapper(RelationalTypeMapperDependencies dependencies)
			: base(dependencies)
		{
			_storeTypeMappings = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
			{
				{ "BOOLEAN", _boolean },
				{ "SMALLINT", _smallint },
				{ "INTEGER", _integer },
				{ "BIGINT", _binary },
				{ "CHAR", _char },
				{ "VARCHAR", _varchar },
				{ "BLOB SUB_TYPE TEXT", _clob },
				{ "FLOAT", _float },
				{ "DOUBLE PRECISION", _double },
				{ "DECIMAL", _decimal },
				{ "TIMESTAMP", _timeStamp },
				{ "DATE", _date },
				{ "TIME", _time },
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
				{ typeof(DateTime), _timeStamp },
				{ typeof(TimeSpan), _time },
				{ typeof(Guid), _guid },
			};

			_disallowedMappings = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
					"CHARACTER",
					"CHAR",
					"VARCHAR",
					"CHARACTER VARYING",
					"CHAR VARYING",
			};

			ByteArrayMapper = new ByteArrayRelationalTypeMapper(
				maxBoundedLength: BinaryMaxSize,
				defaultMapping: _binary,
				unboundedMapping: _binary,
				keyMapping: _binary,
				rowVersionMapping: null,
				createBoundedMapping: _ => _binary);

			StringMapper = new StringRelationalTypeMapper(
				maxBoundedAnsiLength: VarcharMaxSize,
				defaultAnsiMapping: _varcharMax,
				unboundedAnsiMapping: _varcharMax,
				keyAnsiMapping: _varchar256,
				createBoundedAnsiMapping: size => new FbStringTypeMapping($"VARCHAR({size})", FbDbType.VarChar, size),
				maxBoundedUnicodeLength: NVarcharMaxSize,
				defaultUnicodeMapping: _nvarcharMax,
				unboundedUnicodeMapping: _nvarcharMax,
				keyUnicodeMapping: _nvarchar256,
				createBoundedUnicodeMapping: size => new FbStringTypeMapping($"VARCHAR({size})", FbDbType.VarChar, size));
		}

		public override IByteArrayRelationalTypeMapper ByteArrayMapper { get; }
		public override IStringRelationalTypeMapper StringMapper { get; }

		public override void ValidateTypeName(string storeType)
		{
			if (_disallowedMappings.Contains(storeType))
				throw new ArgumentException($"Data type '{storeType}' is invalid.");
		}

		protected override string GetColumnType(IProperty property)
			=> property.Firebird().ColumnType;

		protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
			=> _clrTypeMappings;

		protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
			=> _storeTypeMappings;

		public override RelationalTypeMapping FindMapping(Type clrType)
		{
			clrType = clrType.UnwrapNullableType().UnwrapEnumType();

			return clrType == typeof(string)
				? _nvarcharMax
				: clrType == typeof(byte[])
					? _binary
					: base.FindMapping(clrType);
		}

		protected override bool RequiresKeyMapping(IProperty property)
			=> base.RequiresKeyMapping(property) || property.IsIndex();
	}
}
