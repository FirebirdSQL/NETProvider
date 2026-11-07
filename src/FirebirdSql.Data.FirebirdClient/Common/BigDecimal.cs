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

//$Authors = F.D.Castel

using System;
using System.Globalization;
using System.Numerics;

namespace FirebirdSql.Data.Common;

// Exact value of an INT128-backed NUMERIC/DECIMAL (value = Unscaled / 10^Scale) that cannot be represented
// exactly as System.Decimal. Created only when that conversion is impossible, so ToDecimal always throws;
// the exact value is available through ToString and approximations through ToDouble/ToSingle.
internal readonly struct BigDecimal : IConvertible
{
	public BigInteger Unscaled { get; }
	public int Scale { get; }

	public BigDecimal(BigInteger unscaled, int scale)
	{
		Unscaled = unscaled;
		Scale = scale;
	}

	// Returns unscaled / 10^scale as System.Decimal when that is exact, otherwise as a BigDecimal.
	// Either way the smallest scale that represents the value is used (trailing zeros removed),
	// which is what the previous decimal-only decoding produced for the values it could handle.
	public static object Create(BigInteger unscaled, int scale)
	{
		if (unscaled.IsZero)
		{
			return 0m;
		}
		while (scale > 0)
		{
			var quotient = BigInteger.DivRem(unscaled, 10, out var remainder);
			if (!remainder.IsZero)
				break;
			unscaled = quotient;
			scale--;
		}
		var magnitude = BigInteger.Abs(unscaled);
		if (scale > 28 || scale < 0 || magnitude > MaxDecimalMagnitude)
		{
			return new BigDecimal(unscaled, scale);
		}
		var bits = (UInt128)magnitude;
		return new decimal((int)(uint)bits, (int)(uint)(bits >> 32), (int)(uint)(bits >> 64), unscaled.Sign < 0, (byte)scale);
	}

	public bool TryToBigInteger(out BigInteger result)
	{
		if (Scale <= 0)
		{
			result = Unscaled * BigInteger.Pow(10, -Scale);
			return true;
		}
		result = BigInteger.DivRem(Unscaled, BigInteger.Pow(10, Scale), out var remainder);
		return remainder.IsZero;
	}

	static readonly BigInteger MaxDecimalMagnitude = (BigInteger)decimal.MaxValue;

	public override string ToString() => ToString(CultureInfo.CurrentCulture);

	public string ToString(IFormatProvider provider)
	{
		var format = NumberFormatInfo.GetInstance(provider);
		var digits = BigInteger.Abs(Unscaled).ToString(CultureInfo.InvariantCulture);
		var sign = Unscaled.Sign < 0 ? format.NegativeSign : string.Empty;
		if (Scale <= 0)
		{
			return sign + digits + new string('0', -Scale);
		}
		if (digits.Length <= Scale)
		{
			digits = new string('0', Scale - digits.Length + 1) + digits;
		}
		return sign + digits.Substring(0, digits.Length - Scale) + format.NumberDecimalSeparator + digits.Substring(digits.Length - Scale);
	}

	OverflowException CannotConvert(string type)
	{
		return new OverflowException($"Value {ToString(CultureInfo.InvariantCulture)} cannot be represented exactly as {type}.");
	}

	// Rounds to the nearest integer, ties to even (same as Convert.ToInt64(decimal)).
	BigInteger ToInteger()
	{
		if (Scale <= 0)
			return Unscaled * BigInteger.Pow(10, -Scale);
		var divisor = BigInteger.Pow(10, Scale);
		var quotient = BigInteger.DivRem(Unscaled, divisor, out var remainder);
		var twice = BigInteger.Abs(remainder) * 2;
		if (twice > divisor || (twice == divisor && !quotient.IsEven))
		{
			quotient += Unscaled.Sign;
		}
		return quotient;
	}

	public TypeCode GetTypeCode() => TypeCode.Object;
	public bool ToBoolean(IFormatProvider provider) => !Unscaled.IsZero;
	public char ToChar(IFormatProvider provider) => throw new InvalidCastException($"Invalid cast from {nameof(BigDecimal)} to {nameof(Char)}.");
	public DateTime ToDateTime(IFormatProvider provider) => throw new InvalidCastException($"Invalid cast from {nameof(BigDecimal)} to {nameof(DateTime)}.");
	public decimal ToDecimal(IFormatProvider provider) => throw CannotConvert(nameof(Decimal));
	public double ToDouble(IFormatProvider provider) => double.Parse(ToString(CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture);
	public float ToSingle(IFormatProvider provider) => float.Parse(ToString(CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture);
	public sbyte ToSByte(IFormatProvider provider) => (sbyte)ToInteger();
	public byte ToByte(IFormatProvider provider) => (byte)ToInteger();
	public short ToInt16(IFormatProvider provider) => (short)ToInteger();
	public ushort ToUInt16(IFormatProvider provider) => (ushort)ToInteger();
	public int ToInt32(IFormatProvider provider) => (int)ToInteger();
	public uint ToUInt32(IFormatProvider provider) => (uint)ToInteger();
	public long ToInt64(IFormatProvider provider) => (long)ToInteger();
	public ulong ToUInt64(IFormatProvider provider) => (ulong)ToInteger();
	public object ToType(Type conversionType, IFormatProvider provider)
	{
		if (conversionType == typeof(BigDecimal) || conversionType == typeof(object))
			return this;
		if (conversionType == typeof(string))
			return ToString(provider);
		return Type.GetTypeCode(conversionType) switch
		{
			TypeCode.Boolean => ToBoolean(provider),
			TypeCode.SByte => ToSByte(provider),
			TypeCode.Byte => ToByte(provider),
			TypeCode.Int16 => ToInt16(provider),
			TypeCode.UInt16 => ToUInt16(provider),
			TypeCode.Int32 => ToInt32(provider),
			TypeCode.UInt32 => ToUInt32(provider),
			TypeCode.Int64 => ToInt64(provider),
			TypeCode.UInt64 => ToUInt64(provider),
			TypeCode.Single => ToSingle(provider),
			TypeCode.Double => ToDouble(provider),
			TypeCode.Decimal => ToDecimal(provider),
			_ => throw new InvalidCastException($"Invalid cast from {nameof(BigDecimal)} to {conversionType}."),
		};
	}
}
