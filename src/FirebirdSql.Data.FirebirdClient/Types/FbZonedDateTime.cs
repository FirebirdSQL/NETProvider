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
public readonly struct FbZonedDateTime : IEquatable<FbZonedDateTime>
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

	public static bool operator ==(FbZonedDateTime lhs, FbZonedDateTime rhs) => lhs.Equals(rhs);

	public static bool operator !=(FbZonedDateTime lhs, FbZonedDateTime rhs) => lhs.Equals(rhs);
}
