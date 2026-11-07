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
		// shortest representation that round-trips to the same float
		_ => ParseRoundTrip(value.ToString("R", CultureInfo.InvariantCulture)),
	};
	public static explicit operator FbDecFloat(double value) => value switch
	{
		double.NaN => PositiveNaN,
		double.PositiveInfinity => PositiveInfinity,
		double.NegativeInfinity => NegativeInfinity,
		// shortest representation that round-trips to the same double
		_ => ParseRoundTrip(value.ToString("R", CultureInfo.InvariantCulture)),
	};
	public static explicit operator FbDecFloat(decimal value)
	{
		// exact: the decimal's 96-bit integer and scale are the coefficient and (negated) exponent
		Span<int> bits = stackalloc int[4];
		decimal.GetBits(value, bits);
		BigInteger coefficient = ((UInt128)(uint)bits[2] << 64) | ((UInt128)(uint)bits[1] << 32) | (uint)bits[0];
		var negative = bits[3] < 0;
		var scale = (bits[3] >> 16) & 0xFF;
		return new FbDecFloat(DecimalType.Finite, negative, negative ? -coefficient : coefficient, -scale);
	}

	// Exact (the scale is kept when possible, e.g. 110E-2 -> 1.10m).
	// Throws OverflowException when the value cannot be represented exactly as decimal, including NaN and infinities.
	public static explicit operator decimal(FbDecFloat value)
	{
		if (value.Type != DecimalType.Finite)
		{
			throw value.CannotRepresent(nameof(Decimal));
		}
		var coefficient = BigInteger.Abs(value.Coefficient);
		var exponent = value.Exponent;
		if (exponent > 0)
		{
			if (!coefficient.IsZero)
			{
				if (exponent > 28)
				{
					throw value.CannotRepresent(nameof(Decimal));
				}
				coefficient *= BigInteger.Pow(10, exponent);
			}
			exponent = 0;
		}
		// only trailing zeros can be dropped
		while (exponent < 0 && (exponent < -28 || coefficient > MaxDecimalCoefficient))
		{
			var quotient = BigInteger.DivRem(coefficient, 10, out var remainder);
			if (!remainder.IsZero)
			{
				break;
			}
			coefficient = quotient;
			exponent++;
		}
		if (exponent < -28 || coefficient > MaxDecimalCoefficient)
		{
			throw value.CannotRepresent(nameof(Decimal));
		}
		var bits = (UInt128)coefficient;
		return new decimal((int)(uint)bits, (int)(uint)(bits >> 32), (int)(uint)(bits >> 64), value.Negative, (byte)-exponent);
	}
	// Nearest double (overflows to infinity, underflows to zero); NaN and infinities are mapped.
	public static explicit operator double(FbDecFloat value) => value.Type switch
	{
		DecimalType.Infinity => value.Negative ? double.NegativeInfinity : double.PositiveInfinity,
		DecimalType.NaN or DecimalType.SignalingNaN => double.NaN,
		_ => value.Negative && value.Coefficient.IsZero ? -0d : double.Parse(value.ToInvariantScientificString(), NumberStyles.Float, CultureInfo.InvariantCulture),
	};
	// Nearest float (overflows to infinity, underflows to zero); NaN and infinities are mapped.
	public static explicit operator float(FbDecFloat value) => value.Type switch
	{
		DecimalType.Infinity => value.Negative ? float.NegativeInfinity : float.PositiveInfinity,
		DecimalType.NaN or DecimalType.SignalingNaN => float.NaN,
		_ => value.Negative && value.Coefficient.IsZero ? -0f : float.Parse(value.ToInvariantScientificString(), NumberStyles.Float, CultureInfo.InvariantCulture),
	};

	static readonly BigInteger MaxDecimalCoefficient = (BigInteger)decimal.MaxValue;

	string ToInvariantScientificString() => $"{Coefficient.ToString(CultureInfo.InvariantCulture)}E{Exponent.ToString(CultureInfo.InvariantCulture)}";

	OverflowException CannotRepresent(string type) => new OverflowException($"Value {this} cannot be represented exactly as {type}.");

	// Nearest integer, ties to even (as Convert.ToInt64(decimal) does).
	// Throws OverflowException for NaN, infinities and values far beyond the 64-bit range.
	internal BigInteger ToIntegerRoundingToEven()
	{
		if (Type != DecimalType.Finite)
		{
			throw CannotRepresent(nameof(BigInteger));
		}
		if (Exponent >= 0)
		{
			if (Exponent > 40 && !Coefficient.IsZero)
			{
				throw CannotRepresent(nameof(BigInteger));
			}
			return Coefficient * BigInteger.Pow(10, Exponent);
		}
		var digitsToDrop = -(long)Exponent;
		if (digitsToDrop > BigInteger.Abs(Coefficient).GetBitLength())
		{
			// |value| < 0.5
			return BigInteger.Zero;
		}
		var divisor = BigInteger.Pow(10, (int)digitsToDrop);
		var quotient = BigInteger.DivRem(Coefficient, divisor, out var remainder);
		var twice = BigInteger.Abs(remainder) * 2;
		if (twice > divisor || (twice == divisor && !quotient.IsEven))
		{
			quotient += Coefficient.Sign;
		}
		return quotient;
	}

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
		var (coefficient, exponent) = Normalize();
		return HashCode.Combine(Type, Negative, coefficient, exponent);
	}

	public bool Equals(FbDecFloat other)
	{
		if (Type != other.Type || Negative != other.Negative)
			return false;
		if (Coefficient.Equals(other.Coefficient) && Exponent == other.Exponent)
			return true;
		return Normalize() == other.Normalize();
	}

	// Values with the same numeric value (e.g. 1E1 and 10E0) normalize to the same representation.
	(BigInteger coefficient, int exponent) Normalize()
	{
		if (Coefficient.IsZero)
			return (BigInteger.Zero, 0);
		var coefficient = Coefficient;
		var exponent = Exponent;
		while (true)
		{
			var quotient = BigInteger.DivRem(coefficient, 10, out var remainder);
			if (!remainder.IsZero)
				return (coefficient, exponent);
			coefficient = quotient;
			exponent++;
		}
	}

	public static bool operator ==(FbDecFloat lhs, FbDecFloat rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(FbDecFloat lhs, FbDecFloat rhs)
	{
		return !lhs.Equals(rhs);
	}

	// Parses the invariant "R" format of float/double, e.g. "2", "-0", "6.6", "1E-20", "1.7976931348623157E+308".
	static FbDecFloat ParseRoundTrip(string s)
	{
		var exponent = 0;
		var exponentPos = s.IndexOf('E');
		if (exponentPos >= 0)
		{
			exponent = int.Parse(s.AsSpan(exponentPos + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
			s = s.Substring(0, exponentPos);
		}
		var pointPos = s.IndexOf('.');
		if (pointPos >= 0)
		{
			exponent -= s.Length - pointPos - 1;
			s = s.Remove(pointPos, 1);
		}
		var coefficient = BigInteger.Parse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
		return new FbDecFloat(DecimalType.Finite, s[0] == '-', coefficient, exponent);
	}
}
