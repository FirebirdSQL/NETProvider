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
public readonly struct FbZonedDateTime : IEquatable<FbZonedDateTime>, IConvertible
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

	TypeCode IConvertible.GetTypeCode() => TypeCode.Object;

	DateTime IConvertible.ToDateTime(IFormatProvider provider) => DateTime;

	string IConvertible.ToString(IFormatProvider provider) => ToString();

	object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		=> ReferenceEquals(conversionType, typeof(FbZonedDateTime)) ? this : throw new InvalidCastException(conversionType?.FullName);

	bool IConvertible.ToBoolean(IFormatProvider provider) => throw new InvalidCastException(nameof(Boolean));

	byte IConvertible.ToByte(IFormatProvider provider) => throw new InvalidCastException(nameof(Byte));

	char IConvertible.ToChar(IFormatProvider provider) => throw new InvalidCastException(nameof(Char));

	decimal IConvertible.ToDecimal(IFormatProvider provider) => throw new InvalidCastException(nameof(Decimal));

	double IConvertible.ToDouble(IFormatProvider provider) => throw new InvalidCastException(nameof(Double));

	short IConvertible.ToInt16(IFormatProvider provider) => throw new InvalidCastException(nameof(Int16));

	int IConvertible.ToInt32(IFormatProvider provider) => throw new InvalidCastException(nameof(Int32));

	long IConvertible.ToInt64(IFormatProvider provider) => throw new InvalidCastException(nameof(Int64));

	sbyte IConvertible.ToSByte(IFormatProvider provider) => throw new InvalidCastException(nameof(SByte));

	float IConvertible.ToSingle(IFormatProvider provider) => throw new InvalidCastException(nameof(Single));

	ushort IConvertible.ToUInt16(IFormatProvider provider) => throw new InvalidCastException(nameof(UInt16));

	uint IConvertible.ToUInt32(IFormatProvider provider) => throw new InvalidCastException(nameof(UInt32));

	ulong IConvertible.ToUInt64(IFormatProvider provider) => throw new InvalidCastException(nameof(UInt64));

	public static bool operator ==(FbZonedDateTime lhs, FbZonedDateTime rhs) => lhs.Equals(rhs);

	public static bool operator !=(FbZonedDateTime lhs, FbZonedDateTime rhs) => lhs.Equals(rhs);
}
