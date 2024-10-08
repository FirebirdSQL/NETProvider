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
using System.Runtime.InteropServices;

namespace FirebirdSql.Data.Types;

[StructLayout(LayoutKind.Auto)]
public readonly struct FbZonedDateTime : IConvertible, IEquatable<FbZonedDateTime>
{
	public DateTime DateTime { get; }
	public string TimeZone { get; }
	public TimeSpan? Offset { get; }

	internal FbZonedDateTime(DateTime dateTime, string timeZone, TimeSpan? offset)
	{
		if (dateTime.Kind != DateTimeKind.Utc)
			throw new ArgumentException("Value must be in UTC.", nameof(dateTime));
		if (timeZone == null)
			throw new ArgumentNullException(nameof(timeZone));
		if (string.IsNullOrWhiteSpace(timeZone))
			throw new ArgumentException(nameof(timeZone));

		DateTime = dateTime;
		TimeZone = timeZone;
		Offset = offset;
	}

	public FbZonedDateTime(DateTime dateTime, string timeZone)
		: this(dateTime, timeZone, null)
	{ }

	public override string ToString()
	{
		if (Offset != null)
		{
			return $"{DateTime} {TimeZone} ({Offset})";
		}
		return $"{DateTime} {TimeZone}";
	}

	public override bool Equals(object obj)
	{
		return obj is FbZonedDateTime fbZonedDateTime && Equals(fbZonedDateTime);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hash = (int)2166136261;
			hash = (hash * 16777619) ^ DateTime.GetHashCode();
			hash = (hash * 16777619) ^ TimeZone.GetHashCode();
			if (Offset != null)
				hash = (hash * 16777619) ^ Offset.GetHashCode();
			return hash;
		}
	}

	public bool Equals(FbZonedDateTime other) => DateTime.Equals(other.DateTime) && TimeZone.Equals(other.TimeZone, StringComparison.OrdinalIgnoreCase);

	public TypeCode GetTypeCode() => TypeCode.Object;

	public bool ToBoolean(IFormatProvider provider) => throw new InvalidCastException(nameof(Boolean));

	public byte ToByte(IFormatProvider provider) => throw new InvalidCastException(nameof(Byte));

	public char ToChar(IFormatProvider provider) => throw new InvalidCastException(nameof(Char));

	public DateTime ToDateTime(IFormatProvider provider) => DateTime;

	public decimal ToDecimal(IFormatProvider provider) => throw new InvalidCastException(nameof(Decimal));

	public double ToDouble(IFormatProvider provider) => throw new InvalidCastException(nameof(Double));

	public short ToInt16(IFormatProvider provider) => throw new InvalidCastException(nameof(Int16));

	public int ToInt32(IFormatProvider provider) => throw new InvalidCastException(nameof(Int32));

	public long ToInt64(IFormatProvider provider) => throw new InvalidCastException(nameof(Int64));

	public sbyte ToSByte(IFormatProvider provider) => throw new InvalidCastException(nameof(SByte));

	public float ToSingle(IFormatProvider provider) => throw new InvalidCastException(nameof(Single));

	public string ToString(IFormatProvider provider) => throw new InvalidCastException(nameof(String));

	public object ToType(Type conversionType, IFormatProvider provider) => throw new InvalidCastException(nameof(conversionType));

	public ushort ToUInt16(IFormatProvider provider) => throw new InvalidCastException(nameof(UInt16));

	public uint ToUInt32(IFormatProvider provider) => throw new InvalidCastException(nameof(UInt32));

	public ulong ToUInt64(IFormatProvider provider) => throw new InvalidCastException(nameof(UInt64));

	public static bool operator ==(FbZonedDateTime lhs, FbZonedDateTime rhs) => lhs.Equals(rhs);

	public static bool operator !=(FbZonedDateTime lhs, FbZonedDateTime rhs) => lhs.Equals(rhs);
}
