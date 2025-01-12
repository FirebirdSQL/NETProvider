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
public readonly struct FbZonedTime : IEquatable<FbZonedTime>, IConvertible
{
	public TimeSpan Time { get; }
	public string TimeZone { get; }
	public TimeSpan? Offset { get; }

	internal FbZonedTime(TimeSpan time, string timeZone, TimeSpan? offset)
	{
		if (timeZone == null)
			throw new ArgumentNullException(nameof(timeZone));
		if (string.IsNullOrWhiteSpace(timeZone))
			throw new ArgumentException(nameof(timeZone));

		Time = time;
		TimeZone = timeZone;
		Offset = offset;
	}

	public FbZonedTime(TimeSpan time, string timeZone)
		: this(time, timeZone, null)
	{ }

	public override string ToString()
	{
		if (Offset != null)
		{
			return $"{Time} {TimeZone} ({Offset})";
		}
		return $"{Time} {TimeZone}";
	}

	public override bool Equals(object obj)
	{
		return obj is FbZonedTime fbZonedTime && Equals(fbZonedTime);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hash = (int)2166136261;
			hash = (hash * 16777619) ^ Time.GetHashCode();
			hash = (hash * 16777619) ^ TimeZone.GetHashCode();
			if (Offset != null)
				hash = (hash * 16777619) ^ Offset.GetHashCode();
			return hash;
		}
	}

	public bool Equals(FbZonedTime other) => Time.Equals(other.Time) && TimeZone.Equals(other.TimeZone, StringComparison.OrdinalIgnoreCase);

	TypeCode IConvertible.GetTypeCode() => TypeCode.Object;

	string IConvertible.ToString(IFormatProvider provider) => ToString();

	object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		=> ReferenceEquals(conversionType, typeof(FbZonedTime))
			? this
			: ReferenceEquals(conversionType, typeof(TimeSpan))
				? Time
				: throw new InvalidCastException(conversionType?.FullName);

	bool IConvertible.ToBoolean(IFormatProvider provider) => throw new InvalidCastException(nameof(Boolean));
	byte IConvertible.ToByte(IFormatProvider provider) => throw new InvalidCastException(nameof(Byte));
	char IConvertible.ToChar(IFormatProvider provider) => throw new InvalidCastException(nameof(Char));
	DateTime IConvertible.ToDateTime(IFormatProvider provider) => throw new InvalidCastException(nameof(DateTime));
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

	public static bool operator ==(FbZonedTime lhs, FbZonedTime rhs) => lhs.Equals(rhs);

	public static bool operator !=(FbZonedTime lhs, FbZonedTime rhs) => lhs.Equals(rhs);
}
