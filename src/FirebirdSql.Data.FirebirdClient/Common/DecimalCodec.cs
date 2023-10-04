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
using System.Diagnostics;
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.Common;

// based on Jaybird's implementation
class DecimalCodec
{
	class DecimalFormat
	{
		const int SignBits = 1;
		const int CombinationBits = 5;
		const int BitsPerGroup = DenselyPackedDecimalCodec.BitsPerGroup;
		const int DigitsPerGroup = DenselyPackedDecimalCodec.DigitsPerGroup;

		public DecimalFormat(int formatBitLength, int coefficientDigits)
		{
			FormatBitLength = formatBitLength;
			CoefficientDigits = coefficientDigits;
			FormatByteLength = FormatBitLength / 8;
			CoefficientContinuationBits = BitsPerGroup * (CoefficientDigits - 1) / DigitsPerGroup;
			ExponentContinuationBits = FormatBitLength - SignBits - CombinationBits - CoefficientContinuationBits;
			ELimit = 3 * (1 << ExponentContinuationBits) - 1;
			EMin = -ELimit / 2;
			ExponentBias = -EMin + CoefficientDigits - 1;
		}

		public int FormatBitLength { get; }
		public int CoefficientDigits { get; }
		public int FormatByteLength { get; }
		public int CoefficientContinuationBits { get; }
		public int ExponentContinuationBits { get; }
		public int ELimit { get; }
		public int EMin { get; }
		public int ExponentBias { get; }

		public void ValidateByteLength(byte[] decBytes)
		{
			if (decBytes.Length != FormatByteLength)
			{
				throw new ArgumentException(nameof(decBytes), $"{nameof(decBytes)} argument must be {FormatByteLength} bytes.");
			}
		}

		public int BiasedExponent(int unbiasedExponent)
		{
			return unbiasedExponent + ExponentBias;
		}

		public int UnbiasedExponent(int biasedExponent)
		{
			return biasedExponent - ExponentBias;
		}
	}

	// Byte pattern that signals that the combination field contains 1 bit of the first digit (for value 8 or 9).
	const int Combination2 = 0b0_11000_00;
	const int NegativeBit = 0b1000_0000;
	const int NegativeSignum = DenselyPackedDecimalCodec.NegativeSignum;

	const byte TypeMask = 0b0_11111_10;
	const byte Infinity0 = 0b0_11110_00;
	const byte Infinity2 = 0b0_11110_10;
	const byte NaNQuiet = 0b0_11111_00;
	const byte NaNSignal = 0b0_11111_10;

	readonly DecimalFormat _decimalFormat;
	readonly DenselyPackedDecimalCodec _coefficientCoder;

	public DecimalCodec(int formatBitLength, int coefficientDigits)
	{
		_decimalFormat = new DecimalFormat(formatBitLength, coefficientDigits);
		_coefficientCoder = new DenselyPackedDecimalCodec(coefficientDigits);
	}

	public static DecimalCodec DecFloat16 { get; } = new DecimalCodec(64, 16);
	public static DecimalCodec DecFloat34 { get; } = new DecimalCodec(128, 34);

	// Parse an IEEE-754 decimal format to a FbDecFloat.
	public FbDecFloat ParseBytes(byte[] decBytes)
	{
		// this (and related) code works with BE
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(decBytes);
		}

		_decimalFormat.ValidateByteLength(decBytes);

		var firstByte = decBytes[0] & 0xff;
		var signum = -1 * (firstByte >>> 7) | 1;
		var decimalType = DecimalTypeFromFirstByte(firstByte);
		switch (decimalType)
		{
			case DecimalType.Infinity:
				return signum == NegativeSignum ? FbDecFloat.NegativeInfinity : FbDecFloat.PositiveInfinity;
			case DecimalType.NaN:
				return signum == NegativeSignum ? FbDecFloat.NegativeNaN : FbDecFloat.PositiveNaN;
			case DecimalType.SignalingNaN:
				return signum == NegativeSignum ? FbDecFloat.NegativeSignalingNaN : FbDecFloat.PositiveSignalingNaN;
			case DecimalType.Finite:
				{
					// NOTE: get exponent MSB from combination field and first 2 bits of exponent continuation in one go
					int exponentMSB;
					int firstDigit;
					if ((firstByte & Combination2) != Combination2)
					{
						exponentMSB = (firstByte >>> 3) & 0b01100 | (firstByte & 0b011);
						firstDigit = (firstByte >>> 2) & 0b0111;
					}
					else
					{
						exponentMSB = (firstByte >>> 1) & 0b01100 | (firstByte & 0b011);
						firstDigit = 0b01000 | ((firstByte >>> 2) & 0b01);
					}
					var exponentBitsRemaining = _decimalFormat.ExponentContinuationBits - 2;
					Debug.Assert(exponentBitsRemaining == _decimalFormat.FormatBitLength - 8 - _decimalFormat.CoefficientContinuationBits, $"Unexpected exponent remaining length {exponentBitsRemaining}.");
					var exponent = _decimalFormat.UnbiasedExponent(DecodeExponent(decBytes, exponentMSB, exponentBitsRemaining));
					var coefficient = _coefficientCoder.DecodeValue(signum, firstDigit, decBytes);
					return new FbDecFloat(DecimalType.Finite, signum == NegativeSignum, coefficient, exponent);
				}
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	// Encodes a FbDecFloat to its IEEE-754 format.
	public byte[] EncodeDecimal(FbDecFloat @decimal)
	{
		var decBytes = new byte[_decimalFormat.FormatByteLength];

		if (@decimal.Negative)
		{
			decBytes[0] = NegativeBit;
		}

		if (@decimal.Type == DecimalType.Finite)
		{
			EncodeFinite(@decimal, decBytes);
		}
		else
		{
			decBytes[0] |= GetSpecialBits(@decimal.Type);
		}

		// this (and related) code works with BE
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(decBytes);
		}
		return decBytes;
	}

	void EncodeFinite(FbDecFloat @decimal, byte[] decBytes)
	{
		var biasedExponent = _decimalFormat.BiasedExponent(@decimal.Exponent);
		var coefficient = @decimal.Coefficient;
		var mostSignificantDigit = _coefficientCoder.EncodeValue(coefficient, decBytes);
		var expMSB = biasedExponent >>> _decimalFormat.ExponentContinuationBits;
		var expTwoBitCont = (biasedExponent >>> _decimalFormat.ExponentContinuationBits - 2) & 0b011;
		if (mostSignificantDigit <= 7)
		{
			decBytes[0] |= (byte)((expMSB << 5)
					| (mostSignificantDigit << 2)
					| expTwoBitCont);
		}
		else
		{
			decBytes[0] |= (byte)(Combination2
					| (expMSB << 3)
					| ((mostSignificantDigit & 0b01) << 2)
					| expTwoBitCont);
		}
		EncodeExponentContinuation(decBytes, biasedExponent, _decimalFormat.ExponentContinuationBits - 2);
	}

	static void EncodeExponentContinuation(byte[] decBytes, int expAndBias, int expBitsRemaining)
	{
		var expByteIndex = 1;
		while (expBitsRemaining > 8)
		{
			decBytes[expByteIndex++] = (byte)(expAndBias >>> expBitsRemaining - 8);
			expBitsRemaining -= 8;
		}
		if (expBitsRemaining > 0)
		{
			decBytes[expByteIndex] |= (byte)(expAndBias << 8 - expBitsRemaining);
		}
	}

	static int DecodeExponent(byte[] decBytes, int exponentMSB, int exponentBitsRemaining)
	{
		var exponent = exponentMSB;
		var byteIndex = 1;
		while (exponentBitsRemaining > 8)
		{
			exponent = (exponent << 8) | (decBytes[byteIndex] & 0xFF);
			exponentBitsRemaining -= 8;
			byteIndex += 1;
		}
		if (exponentBitsRemaining > 0)
		{
			exponent = (exponent << exponentBitsRemaining)
				| ((decBytes[byteIndex] & 0xFF) >>> (8 - exponentBitsRemaining));
		}
		return exponent;
	}

	static DecimalType DecimalTypeFromFirstByte(int firstByte)
	{
		return (firstByte & TypeMask) switch
		{
			Infinity0 => DecimalType.Infinity,
			Infinity2 => DecimalType.Infinity,
			NaNQuiet => DecimalType.NaN,
			NaNSignal => DecimalType.SignalingNaN,
			_ => DecimalType.Finite,
		};
	}

	static byte GetSpecialBits(DecimalType decimalType)
	{
		return decimalType switch
		{
			DecimalType.Finite => throw new InvalidOperationException($"{nameof(DecimalType)} {nameof(DecimalType.Finite)} has no special bits."),
			DecimalType.Infinity => Infinity0,
			DecimalType.NaN => NaNQuiet,
			DecimalType.SignalingNaN => NaNSignal,
			_ => throw new ArgumentOutOfRangeException(),
		};
	}
}
