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

//$Authors = Hajime Nakagami (nakagami@gmail.com), Jiri Cincura (jiri@cincura.net)

using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Srp;

/// <remarks>
/// http://srp.stanford.edu/design.html
/// </remarks>
abstract class SrpClientBase
{
	public abstract string Name { get; }

	public string SessionKeyName { get; } = "Symmetric";

	private const int SRP_KEY_SIZE = 128;
	private const int SRP_SALT_SIZE = 32;
	private static readonly BigInteger N = BigInteger.Parse("00E67D2E994B2F900C3F41F08F5BB2627ED0D49EE1FE767A52EFCD565CD6E768812C3E1E9CE8F0A8BEA6CB13CD29DDEBF7A96D4A93B55D488DF099A15C89DCB0640738EB2CBDD9A8F7BAB561AB1B0DC1C6CDABF303264A08D1BCA932D1F1EE428B619D970F342ABA9A65793B8B2F041AE5364350C16F735F56ECBCA87BD57B29E7", NumberStyles.HexNumber);
	private static readonly BigInteger g = new BigInteger(2);
	private static readonly BigInteger k = BigInteger.Parse("1277432915985975349439481660349303019122249719989");
	private static readonly byte[] SEPARATOR_BYTES = Encoding.UTF8.GetBytes(":");

	public BigInteger PublicKey { get; } // A
	public string PublicKeyHex => Pad(PublicKey).ToHexString();
	public BigInteger PrivateKey { get; } // a
	public byte[] Proof { get; private set; } // M
	public byte[] SessionKey { get; private set; } // K

	public SrpClientBase()
	{
		PrivateKey = GetSecret();
		PublicKey = BigInteger.ModPow(g, PrivateKey, N);
	}

	public byte[] ClientProof(string user, string password, byte[] salt, BigInteger serverPublicKey)
	{
		var K = GetClientSessionKey(user, password, salt, serverPublicKey);

		var n1 = BigIntegerFromByteArray(ComputeSHA1Hash(BigIntegerToByteArray(N)));
		var n2 = BigIntegerFromByteArray(ComputeSHA1Hash(BigIntegerToByteArray(g)));

		n1 = BigInteger.ModPow(n1, n2, N);
		n2 = BigIntegerFromByteArray(ComputeSHA1Hash(Encoding.UTF8.GetBytes(user)));
		var M = ComputeHash(BigIntegerToByteArray(n1), BigIntegerToByteArray(n2), salt, BigIntegerToByteArray(PublicKey), BigIntegerToByteArray(serverPublicKey), K);

		SessionKey = K;
		Proof = M;

		return Proof;
	}

	public byte[] ClientProof(string user, string password, byte[] authData)
	{
		var saltLength = authData[0] + authData[1] * 256;
		var salt = new byte[saltLength];
		Array.Copy(authData, 2, salt, 0, saltLength);

		var serverKeyStart = saltLength + 4;
		var serverKeyLength = authData.Length - saltLength - 4;
		var hexServerPublicKey = new byte[serverKeyLength];
		Array.Copy(authData, serverKeyStart, hexServerPublicKey, 0, serverKeyLength);
		var hexServerPublicKeyString = Encoding.UTF8.GetString(hexServerPublicKey);
		var serverPublicKey = BigInteger.Parse($"00{hexServerPublicKeyString}", NumberStyles.HexNumber);
		return ClientProof(user, password, salt, serverPublicKey);
	}

	public (BigInteger, BigInteger) ServerSeed(string user, string password, byte[] salt)
	{
		var v = BigInteger.ModPow(g, GetUserHash(user, password, salt), N);
		var b = GetSecret();
		var gb = BigInteger.ModPow(g, b, N);
		BigInteger.DivRem(k * v, N, out var kv);
		BigInteger.DivRem(BigInteger.Add(kv, gb), N, out var B);
		return (B, b);
	}

	public byte[] GetServerSessionKey(string user, string password, byte[] salt, BigInteger A, BigInteger B, BigInteger b)
	{
		var u = GetScramble(A, B);
		var v = BigInteger.ModPow(g, GetUserHash(user, password, salt), N);
		var vu = BigInteger.ModPow(v, u, N);
		BigInteger.DivRem(A * vu, N, out var Avu);
		var sessionSecret = BigInteger.ModPow(Avu, b, N);
		return ComputeSHA1Hash(BigIntegerToByteArray(sessionSecret));
	}

	public byte[] GetSalt()
	{
		return GetRandomBytes(SRP_SALT_SIZE);
	}

	private BigInteger GetSecret()
	{
		return new BigInteger(GetRandomBytes(SRP_KEY_SIZE / 8).Concat(new byte[] { 0 }).ToArray());
	}

	private byte[] GetClientSessionKey(string user, string password, byte[] salt, BigInteger serverPublicKey)
	{
		var u = GetScramble(PublicKey, serverPublicKey);
		var x = GetUserHash(user, password, salt);
		var gx = BigInteger.ModPow(g, x, N);
		BigInteger.DivRem(k * gx, N, out var kgx);
		var Bkgx = serverPublicKey - kgx;
		if (Bkgx < 0)
		{
			Bkgx = Bkgx + N;
		}
		BigInteger.DivRem(Bkgx, N, out var diff);
		BigInteger.DivRem(u * x, N, out var ux);
		BigInteger.DivRem(PrivateKey + ux, N, out var aux);
		var sessionSecret = BigInteger.ModPow(diff, aux, N);
		return ComputeSHA1Hash(BigIntegerToByteArray(sessionSecret));
	}

	protected abstract byte[] ComputeHash(params byte[][] ba);

	private static BigInteger GetUserHash(string user, string password, byte[] salt)
	{
		var userBytes = Encoding.UTF8.GetBytes(user);
		var passwordBytes = Encoding.UTF8.GetBytes(password);
		var hash1 = ComputeSHA1Hash(userBytes, SEPARATOR_BYTES, passwordBytes);
		var hash2 = ComputeSHA1Hash(salt, hash1);
		return BigIntegerFromByteArray(hash2);
	}

	private static BigInteger BigIntegerFromByteArray(byte[] b)
	{
		return new BigInteger(b.Reverse().Concat(new byte[] { 0 }).ToArray());
	}

	private static byte[] BigIntegerToByteArray(BigInteger n)
	{
		return n.ToByteArray().Reverse().SkipWhile((e, i) => i == 0 && e == 0).ToArray();
	}

	private static byte[] ComputeSHA1Hash(params byte[][] ba)
	{
		using (var hash = SHA1.Create())
		{
			return hash.ComputeHash(ba.SelectMany(x => x).ToArray());
		}
	}

	private static byte[] Pad(BigInteger n)
	{
		var bn = BigIntegerToByteArray(n);
		return bn.SkipWhile((_, i) => i < bn.Length - SRP_KEY_SIZE).ToArray();
	}

	private static BigInteger GetScramble(BigInteger x, BigInteger y)
	{
		return BigIntegerFromByteArray(ComputeSHA1Hash(Pad(x), Pad(y)));
	}

	private static byte[] GetRandomBytes(int count)
	{
		var result = new byte[count];
		using (var random = RandomNumberGenerator.Create())
		{
			random.GetBytes(result);
		}
		return result;
	}
}
