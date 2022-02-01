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
public readonly struct FbZonedTime : IEquatable<FbZonedTime>
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

	public static bool operator ==(FbZonedTime lhs, FbZonedTime rhs) => lhs.Equals(rhs);

	public static bool operator !=(FbZonedTime lhs, FbZonedTime rhs) => lhs.Equals(rhs);
}
