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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Numerics;
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.Common;

internal sealed class DbField
{
	#region Fields

	private short _dataType;
	private short _numericScale;
	private short _subType;
	private short _length;
	private short _nullFlag;
	private string _name;
	private string _relation;
	private string _owner;
	private string _alias;
	private int _charCount;
	private DbValue _dbValue;
	private Charset _charset;
	private ArrayBase _arrayHandle;

	#endregion

	#region Properties

	public DbDataType DbDataType
	{
		get { return TypeHelper.GetDbDataTypeFromSqlType(SqlType, SubType, NumericScale, Length, Charset); }
	}

	public int SqlType
	{
		get { return _dataType & ~1; }
	}

	public short DataType
	{
		get { return _dataType; }
		set { _dataType = value; }
	}

	public short NumericScale
	{
		get { return _numericScale; }
		set { _numericScale = value; }
	}

	public short SubType
	{
		get { return _subType; }
		set
		{
			_subType = value;
			if (IsCharacter())
			{
				// Bits 0-7 of sqlsubtype is charset_id (127 is a special value -
				// current attachment charset).
				// Bits 8-17 hold collation_id for this value.
				var cs = BitConverter.GetBytes(value);
				_charset = Charset.TryGetById(cs[0], out var charset)
					? charset
					: Charset.DefaultCharset;
			}
		}
	}

	public short Length
	{
		get { return _length; }
		set
		{
			_length = value;
			if (IsCharacter())
			{
				_charCount = _length / _charset.BytesPerCharacter;
			}
		}
	}

	public short NullFlag
	{
		get { return _nullFlag; }
		set { _nullFlag = value; }
	}

	public string Name
	{
		get { return _name; }
		set { _name = value.Trim(); }
	}

	public string Relation
	{
		get { return _relation; }
		set { _relation = value.Trim(); }
	}

	public string Owner
	{
		get { return _owner; }
		set { _owner = value.Trim(); }
	}

	public string Alias
	{
		get { return _alias; }
		set { _alias = value.Trim(); }
	}

	public Charset Charset
	{
		get { return _charset; }
	}

	public int CharCount
	{
		get { return _charCount; }
	}

	public ArrayBase ArrayHandle
	{
		get
		{
			EnsureArray();
			return _arrayHandle;
		}

		set
		{
			EnsureArray();
			_arrayHandle = value;
		}
	}

	public DbValue DbValue
	{
		get { return _dbValue; }
	}

	#endregion

	#region Constructors

	public DbField()
	{
		_charCount = -1;
		_name = string.Empty;
		_relation = string.Empty;
		_owner = string.Empty;
		_alias = string.Empty;
		_dbValue = new DbValue(this, DBNull.Value);
	}

	#endregion

	#region Methods

	public bool IsNumeric()
	{
		if (_dataType == 0)
		{
			return false;
		}

		switch (DbDataType)
		{
			case DbDataType.SmallInt:
			case DbDataType.Integer:
			case DbDataType.BigInt:
			case DbDataType.Numeric:
			case DbDataType.Decimal:
			case DbDataType.Float:
			case DbDataType.Double:
				return true;

			default:
				return false;
		}
	}

	public bool IsDecimal()
	{
		if (_dataType == 0)
		{
			return false;
		}

		switch (DbDataType)
		{
			case DbDataType.Numeric:
			case DbDataType.Decimal:
				return true;

			default:
				return false;
		}
	}

	public bool IsLong()
	{
		if (_dataType == 0)
		{
			return false;
		}

		switch (DbDataType)
		{
			case DbDataType.Binary:
			case DbDataType.Text:
				return true;

			default:
				return false;
		}
	}

	public bool IsCharacter()
	{
		if (_dataType == 0)
		{
			return false;
		}

		switch (DbDataType)
		{
			case DbDataType.Char:
			case DbDataType.VarChar:
			case DbDataType.Text:
				return true;

			default:
				return false;
		}
	}

	public bool IsArray()
	{
		if (_dataType == 0)
		{
			return false;
		}

		switch (DbDataType)
		{
			case DbDataType.Array:
				return true;

			default:
				return false;
		}
	}

	public bool IsAliased()
	{
		return (Name != Alias) ? true : false;
	}

	public int GetSize()
	{
		if (IsLong())
		{
			return int.MaxValue;
		}
		else
		{
			if (IsCharacter())
			{
				return CharCount;
			}
			else
			{
				return Length;
			}
		}
	}

	public bool AllowDBNull()
	{
		return ((DataType & 1) == 1);
	}

	public void SetValue(byte[] buffer)
	{
		if (buffer == null || NullFlag == -1)
		{
			DbValue.SetValue(DBNull.Value);
		}
		else
		{
			switch (SqlType)
			{
				case IscCodes.SQL_TEXT:
				case IscCodes.SQL_VARYING:
					if (DbDataType == DbDataType.Guid)
					{
						DbValue.SetValue(TypeDecoder.DecodeGuid(buffer));
					}
					else
					{
						if (Charset.IsOctetsCharset)
						{
							DbValue.SetValue(buffer);
						}
						else
						{
							var s = Charset.GetString(buffer, 0, buffer.Length);

							if ((Length % Charset.BytesPerCharacter) == 0 &&
								s.Length > CharCount)
							{
								s = s.Substring(0, CharCount);
							}

							DbValue.SetValue(s);
						}
					}
					break;

				case IscCodes.SQL_SHORT:
					if (_numericScale < 0)
					{
						DbValue.SetValue(TypeDecoder.DecodeDecimal(BitConverter.ToInt16(buffer, 0), _numericScale, _dataType));
					}
					else
					{
						DbValue.SetValue(BitConverter.ToInt16(buffer, 0));
					}
					break;

				case IscCodes.SQL_LONG:
					if (_numericScale < 0)
					{
						DbValue.SetValue(TypeDecoder.DecodeDecimal(BitConverter.ToInt32(buffer, 0), _numericScale, _dataType));
					}
					else
					{
						DbValue.SetValue(BitConverter.ToInt32(buffer, 0));
					}
					break;

				case IscCodes.SQL_FLOAT:
					DbValue.SetValue(BitConverter.ToSingle(buffer, 0));
					break;

				case IscCodes.SQL_DOUBLE:
				case IscCodes.SQL_D_FLOAT:
					DbValue.SetValue(BitConverter.ToDouble(buffer, 0));
					break;

				case IscCodes.SQL_QUAD:
				case IscCodes.SQL_INT64:
				case IscCodes.SQL_BLOB:
				case IscCodes.SQL_ARRAY:
					if (_numericScale < 0)
					{
						DbValue.SetValue(TypeDecoder.DecodeDecimal(BitConverter.ToInt64(buffer, 0), _numericScale, _dataType));
					}
					else
					{
						DbValue.SetValue(BitConverter.ToInt64(buffer, 0));
					}
					break;

				case IscCodes.SQL_TIMESTAMP:
					{
						var date = TypeDecoder.DecodeDate(BitConverter.ToInt32(buffer, 0));
						var time = TypeDecoder.DecodeTime(BitConverter.ToInt32(buffer, 4));
						DbValue.SetValue(date.Add(time));
						break;
					}

				case IscCodes.SQL_TYPE_TIME:
					DbValue.SetValue(TypeDecoder.DecodeTime(BitConverter.ToInt32(buffer, 0)));
					break;

				case IscCodes.SQL_TYPE_DATE:
					DbValue.SetValue(TypeDecoder.DecodeDate(BitConverter.ToInt32(buffer, 0)));
					break;

				case IscCodes.SQL_BOOLEAN:
					DbValue.SetValue(TypeDecoder.DecodeBoolean(buffer));
					break;

				case IscCodes.SQL_TIMESTAMP_TZ:
					{
						var date = TypeDecoder.DecodeDate(BitConverter.ToInt32(buffer, 0));
						var time = TypeDecoder.DecodeTime(BitConverter.ToInt32(buffer, 4));
						var tzId = BitConverter.ToUInt16(buffer, 8);
						var dt = DateTime.SpecifyKind(date.Add(time), DateTimeKind.Utc);
						DbValue.SetValue(TypeHelper.CreateZonedDateTime(dt, tzId, null));
						break;
					}

				case IscCodes.SQL_TIMESTAMP_TZ_EX:
					{
						var date = TypeDecoder.DecodeDate(BitConverter.ToInt32(buffer, 0));
						var time = TypeDecoder.DecodeTime(BitConverter.ToInt32(buffer, 4));
						var tzId = BitConverter.ToUInt16(buffer, 8);
						var offset = BitConverter.ToInt16(buffer, 10);
						var dt = DateTime.SpecifyKind(date.Add(time), DateTimeKind.Utc);
						DbValue.SetValue(TypeHelper.CreateZonedDateTime(dt, tzId, offset));
						break;
					}

				case IscCodes.SQL_TIME_TZ:
					{
						var time = TypeDecoder.DecodeTime(BitConverter.ToInt32(buffer, 0));
						var tzId = BitConverter.ToUInt16(buffer, 4);
						DbValue.SetValue(TypeHelper.CreateZonedTime(time, tzId, null));
						break;
					}

				case IscCodes.SQL_TIME_TZ_EX:
					{
						var time = TypeDecoder.DecodeTime(BitConverter.ToInt32(buffer, 0));
						var tzId = BitConverter.ToUInt16(buffer, 4);
						var offset = BitConverter.ToInt16(buffer, 6);
						DbValue.SetValue(TypeHelper.CreateZonedTime(time, tzId, offset));
						break;
					}

				case IscCodes.SQL_DEC16:
					DbValue.SetValue(DecimalCodec.DecFloat16.ParseBytes(buffer));
					break;

				case IscCodes.SQL_DEC34:
					DbValue.SetValue(DecimalCodec.DecFloat34.ParseBytes(buffer));
					break;

				case IscCodes.SQL_INT128:
					if (_numericScale < 0)
					{
						DbValue.SetValue(TypeDecoder.DecodeDecimal(Int128Helper.GetInt128(buffer), _numericScale, _dataType));
					}
					else
					{
						DbValue.SetValue(Int128Helper.GetInt128(buffer));
					}
					break;

				default:
					throw TypeHelper.InvalidDataType(SqlType);
			}
		}
	}

	public void FixNull()
	{
		if (NullFlag == -1 && _dbValue.IsDBNull())
		{
			switch (DbDataType)
			{
				case DbDataType.Char:
				case DbDataType.VarChar:
					DbValue.SetValue(string.Empty);
					break;

				case DbDataType.Guid:
					DbValue.SetValue(Guid.Empty);
					break;

				case DbDataType.SmallInt:
					DbValue.SetValue((short)0);
					break;

				case DbDataType.Integer:
					DbValue.SetValue((int)0);
					break;

				case DbDataType.BigInt:
				case DbDataType.Binary:
				case DbDataType.Array:
				case DbDataType.Text:
					DbValue.SetValue((long)0);
					break;

				case DbDataType.Numeric:
				case DbDataType.Decimal:
					DbValue.SetValue((decimal)0);
					break;

				case DbDataType.Float:
					DbValue.SetValue((float)0);
					break;

				case DbDataType.Double:
					DbValue.SetValue((double)0);
					break;

				case DbDataType.Date:
				case DbDataType.TimeStamp:
					DbValue.SetValue(DateTime2.UnixEpoch);
					break;

				case DbDataType.Time:
					DbValue.SetValue(TimeSpan.Zero);
					break;

				case DbDataType.Boolean:
					DbValue.SetValue(false);
					break;

				case DbDataType.TimeStampTZ:
				case DbDataType.TimeStampTZEx:
					DbValue.SetValue(new FbZonedDateTime(DateTime2.UnixEpoch, TimeZoneMapping.DefaultTimeZoneName));
					break;

				case DbDataType.TimeTZ:
				case DbDataType.TimeTZEx:
					DbValue.SetValue(new FbZonedTime(TimeSpan.Zero, TimeZoneMapping.DefaultTimeZoneName));
					break;

				case DbDataType.Dec16:
				case DbDataType.Dec34:
					DbValue.SetValue(new FbDecFloat(0, 0));
					break;

				case DbDataType.Int128:
					DbValue.SetValue((BigInteger)0);
					break;

				default:
					throw IscException.ForStrParam($"Unknown sql data type: {DataType}.");
			}
		}
	}

	public Type GetSystemType()
	{
		return TypeHelper.GetTypeFromDbDataType(DbDataType);
	}

	public bool HasDataType()
	{
		return _dataType != 0;
	}

	#endregion

	#region Private Methods

	private void EnsureArray()
	{
		if (!IsArray())
			throw IscException.ForStrParam("Field is not an array type.");
	}

	#endregion
}
