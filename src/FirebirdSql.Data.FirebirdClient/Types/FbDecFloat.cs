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
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Types;

[StructLayout(LayoutKind.Auto)]
public readonly struct FbDecFloat : IEquatable<FbDecFloat>
{
	internal DecimalType Type { get; }
	internal bool Negative { get; }
	public BigInteger Coefficient { get; }
	public int Exponent { get; }

	public static FbDecFloat NegativeZero { get; } = new FbDecFloat(DecimalType.Finite, true, BigInteger.Zero, 0);
	public static FbDecFloat PositiveInfinity { get; } = new FbDecFloat(DecimalType.Infinity, false, default, default);
	public static FbDecFloat NegativeInfinity { get; } = new FbDecFloat(DecimalType.Infinity, true, default, default);
	public static FbDecFloat PositiveNaN { get; } = new FbDecFloat(DecimalType.NaN, false, default, default);
	public static FbDecFloat NegativeNaN { get; } = new FbDecFloat(DecimalType.NaN, true, default, default);
	public static FbDecFloat PositiveSignalingNaN { get; } = new FbDecFloat(DecimalType.SignalingNaN, false, default, default);
	public static FbDecFloat NegativeSignalingNaN { get; } = new FbDecFloat(DecimalType.SignalingNaN, true, default, default);

	internal FbDecFloat(DecimalType type, bool negative, BigInteger coefficient, int exponent)
	{
		Type = type;
		Negative = negative;
		Coefficient = coefficient;
		Exponent = exponent;
	}

	public FbDecFloat(BigInteger coefficient, int exponent = 0)
		: this(DecimalType.Finite, coefficient.Sign == -1, coefficient, exponent)
	{ }

	public static implicit operator FbDecFloat(byte value) => new FbDecFloat(value);
	public static implicit operator FbDecFloat(sbyte value) => new FbDecFloat(value);
	public static implicit operator FbDecFloat(short value) => new FbDecFloat(value);
	public static implicit operator FbDecFloat(ushort value) => new FbDecFloat(value);
	public static implicit operator FbDecFloat(int value) => new FbDecFloat(value);
	public static implicit operator FbDecFloat(uint value) => new FbDecFloat(value);
	public static implicit operator FbDecFloat(long value) => new FbDecFloat(value);
	public static implicit operator FbDecFloat(ulong value) => new FbDecFloat(value);
	public static explicit operator FbDecFloat(float value) => value switch
	{
		float.NaN => PositiveNaN,
		float.PositiveInfinity => PositiveInfinity,
		float.NegativeInfinity => NegativeInfinity,
		_ => ParseNumber(value, "0.#######"),
	};
	public static explicit operator FbDecFloat(double value) => value switch
	{
		double.NaN => PositiveNaN,
		double.PositiveInfinity => PositiveInfinity,
		double.NegativeInfinity => NegativeInfinity,
		_ => ParseNumber(value, "0.###############"),
	};
	public static explicit operator FbDecFloat(decimal value) => ParseNumber(value, "0.############################");

	public override string ToString()
	{
		if (this == NegativeZero)
		{
			return "-0";
		}
		if (this == PositiveInfinity)
		{
			return "inf";
		}
		if (this == NegativeInfinity)
		{
			return "-inf";
		}
		if (this == PositiveNaN)
		{
			return "nan";
		}
		if (this == PositiveSignalingNaN)
		{
			return "snan";
		}
		if (this == NegativeNaN)
		{
			return "-nan";
		}
		if (this == NegativeSignalingNaN)
		{
			return "-snan";
		}
		return $"{Coefficient}E{Exponent}";
	}

	public override bool Equals(object obj)
	{
		return obj is FbDecFloat fbDecFloat && Equals(fbDecFloat);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hash = (int)2166136261;
			hash = (hash * 16777619) ^ Type.GetHashCode();
			hash = (hash * 16777619) ^ Negative.GetHashCode();
			hash = (hash * 16777619) ^ Coefficient.GetHashCode();
			hash = (hash * 16777619) ^ Exponent.GetHashCode();
			return hash;
		}
	}

	public bool Equals(FbDecFloat other)
	{
		if (!(Type.Equals(other.Type) && Negative.Equals(other.Negative)))
			return false;
		if (Coefficient.Equals(other.Coefficient) && Exponent.Equals(other.Exponent))
			return true;
		if (Exponent < other.Exponent)
		{
			var difference = other.Exponent - Exponent;
			var value = other.Coefficient * BigInteger.Pow(10, difference);
			return value.Equals(Coefficient);
		}
		if (Exponent > other.Exponent)
		{
			var difference = Exponent - other.Exponent;
			var value = Coefficient * BigInteger.Pow(10, difference);
			return value.Equals(other.Coefficient);
		}
		return false;
	}

	public static bool operator ==(FbDecFloat lhs, FbDecFloat rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(FbDecFloat lhs, FbDecFloat rhs)
	{
		return lhs.Equals(rhs);
	}

	static FbDecFloat ParseNumber(IFormattable formattable, string format)
	{
		var s = formattable.ToString(format, CultureInfo.InvariantCulture);
		var pos = s.IndexOf('.');
		return new FbDecFloat(BigInteger.Parse(s.Remove(pos, 1), CultureInfo.InvariantCulture), -(s.Length - pos - 1));
	}
}
