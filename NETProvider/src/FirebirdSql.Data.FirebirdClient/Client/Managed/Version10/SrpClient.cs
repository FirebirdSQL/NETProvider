/*
 *  Firebird ADO.NET Data provider for .NET and Mono
 *
 *     The contents of this file are subject to the Initial
 *     Developer's Public License Version 1.0 (the "License");
 *     you may not use this file except in compliance with the
 *     License. You may obtain a copy of the License at
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *     express or implied.  See the License for the specific
 *     language governing rights and limitations under the License.
 *
 *  Copyright (c) 2015 Hajime Nakagami (nakagami@gmail.com)
 *  Copyright (c) 2016 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Globalization;
using System.Text;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	/// <remarks>
	/// http://srp.stanford.edu/design.html
	/// </remarks>
	internal sealed class SrpClient
	{
		private const int SRP_KEY_SIZE = 128;
		private const int SRP_SALT_SIZE = 32;
		private static readonly BigInteger N = BigInteger.Parse("00E67D2E994B2F900C3F41F08F5BB2627ED0D49EE1FE767A52EFCD565CD6E768812C3E1E9CE8F0A8BEA6CB13CD29DDEBF7A96D4A93B55D488DF099A15C89DCB0640738EB2CBDD9A8F7BAB561AB1B0DC1C6CDABF303264A08D1BCA932D1F1EE428B619D970F342ABA9A65793B8B2F041AE5364350C16F735F56ECBCA87BD57B29E7", NumberStyles.HexNumber);
		private static readonly BigInteger g = new BigInteger(2);
		private static readonly BigInteger k = BigInteger.Parse("1277432915985975349439481660349303019122249719989");

		private static byte[] SEPARATOR_BYTES = Encoding.UTF8.GetBytes(":");

		private BigInteger _publicKey;   /* A */
		private BigInteger _privateKey;  /* a */
		private byte[] _proof;           /* M */
		private byte[] _sessionKey;      /* K */

		public SrpClient()
		{
			_privateKey = GetSecret();
			_publicKey = BigInteger.ModPow(g, _privateKey, N);
		}

		public byte[] GetSalt()
		{
			byte[] b = new byte[SRP_SALT_SIZE];
			RandomNumberGenerator random = RandomNumberGenerator.Create();
			random.GetBytes(b);
			return b;
		}

		public string GetPublicKeyHex()
		{
			return ToHexString(Pad(_publicKey));
		}

		public byte[] ClientProof(string user, string password, byte[] salt, BigInteger serverPublicKey)
		{
			byte[] K = GetClientSessionKey(user, password, salt, serverPublicKey);

			BigInteger n1 = FromBigByteArray(Sha1(ToBigByteArray(N)));
			BigInteger n2 = FromBigByteArray(Sha1(ToBigByteArray(g)));

			n1 = BigInteger.ModPow(n1, n2, N);
			n2 = FromBigByteArray(Sha1(Encoding.UTF8.GetBytes(user.ToUpper())));
			byte[] M = Sha1(ToBigByteArray(n1), ToBigByteArray(n2), salt, ToBigByteArray(_publicKey), ToBigByteArray(serverPublicKey), K);

			_sessionKey = K;
			_proof = M;

			return _proof;
		}

		public byte[] ClientProof(string user, string password, byte[] authData)
		{
			int saltLength = authData[0] + authData[1] * 256;
			byte[] salt = new byte[saltLength];
			Array.Copy(authData, 2, salt, 0, saltLength);

			int serverKeyStart = saltLength + 4;
			int serverKeyLength = authData.Length - saltLength - 4;
			byte[] hexServerPublicKey = new byte[serverKeyLength];
			Array.Copy(authData, serverKeyStart, hexServerPublicKey, 0, serverKeyLength);
			string hexServerPublicKeyString = Encoding.UTF8.GetString(hexServerPublicKey);
			BigInteger serverPublicKey = BigInteger.Parse("00" + hexServerPublicKeyString, NumberStyles.HexNumber);
			return ClientProof(user.ToUpper(), password, salt, serverPublicKey);
		}

		public byte[] GetSessionKey()
		{
			return _sessionKey;
		}

		public Tuple<BigInteger, BigInteger> ServerSeed(string user, string password, byte[] salt)
		{
			BigInteger v = BigInteger.ModPow(g, GetUserHash(user, password, salt), N);
			BigInteger b = GetSecret();
			BigInteger gb = BigInteger.ModPow(g, b, N);
			BigInteger kv;
			BigInteger.DivRem(BigInteger.Multiply(k, v), N, out kv);
			BigInteger B;
			BigInteger.DivRem(BigInteger.Add(kv, gb), N, out B);

			return new Tuple<BigInteger, BigInteger>(B, b);
		}

		public byte[] GetServerSessionKey(string user, string password, byte[] salt, BigInteger A, BigInteger B, BigInteger b)
		{
			BigInteger u = GetScramble(A, B);
			BigInteger v = BigInteger.ModPow(g, GetUserHash(user, password, salt), N);
			BigInteger vu = BigInteger.ModPow(v, u, N);
			BigInteger Avu;
			BigInteger.DivRem(BigInteger.Multiply(A, vu), N, out Avu);
			BigInteger sessionSecret = BigInteger.ModPow(Avu, b, N);
			return Sha1(ToBigByteArray(sessionSecret));
		}

		public BigInteger GetPublicKey()
		{
			return _publicKey;
		}

		private BigInteger GetPrivateKey()
		{
			return _privateKey;
		}

		private BigInteger GetSecret()
		{
			byte[] b = new byte[SRP_KEY_SIZE / 8];
			RandomNumberGenerator random = RandomNumberGenerator.Create();
			random.GetBytes(b);
			return new BigInteger(b.Concat(new byte[] { 0 }).ToArray());
		}

		private byte[] GetClientSessionKey(string user, string password, byte[] salt, BigInteger serverPublicKey)
		{
			BigInteger u = GetScramble(_publicKey, serverPublicKey);
			BigInteger x = GetUserHash(user, password, salt);
			BigInteger gx = BigInteger.ModPow(g, x, N);
			BigInteger kgx;
			BigInteger.DivRem(BigInteger.Multiply(k, gx), N, out kgx);
			BigInteger Bkgx = BigInteger.Subtract(serverPublicKey, kgx);    // B-kgx
			if (BigInteger.Compare(Bkgx, 0) < 0)
			{
				Bkgx = BigInteger.Add(Bkgx, N);
			}
			BigInteger diff;
			BigInteger.DivRem(Bkgx, N, out diff);
			BigInteger ux;
			BigInteger.DivRem(BigInteger.Multiply(u, x), N, out ux);
			BigInteger aux;
			BigInteger.DivRem(BigInteger.Add(_privateKey, ux), N, out aux);
			BigInteger sessionSecret = BigInteger.ModPow(diff, aux, N);

			return Sha1(ToBigByteArray(sessionSecret));
		}

		private static BigInteger GetUserHash(string user, string password, byte[] salt)
		{
			byte[] userBytes = Encoding.UTF8.GetBytes(user.ToUpper());
			byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
			byte[] hash1 = Sha1(userBytes, SEPARATOR_BYTES, passwordBytes);
			byte[] hash2 = Sha1(salt, hash1);

			BigInteger rc = FromBigByteArray(hash2);
			return rc;
		}

		private static BigInteger FromBigByteArray(byte[] b)
		{
			Array.Reverse(b);
			return new BigInteger(b.Concat(new byte[] { 0 }).ToArray());
		}

		private static byte[] ToBigByteArray(BigInteger n)
		{
			byte[] b = n.ToByteArray();
			int length = b.Length;
			if (b[length - 1] == 0)
			{
				length -= 1;
			}
			byte[] b2 = new byte[length];
			Array.Copy(b, 0, b2, 0, length);
			Array.Reverse(b2);
			return b2;
		}

		private static string ToHexString(byte[] b)
		{
			return BitConverter.ToString(b).Replace("-", string.Empty);
		}

		private static byte[] FromHexString(string s)
		{
			return Enumerable.Range(0, s.Length / 2).Select(i => Convert.ToByte(s.Substring(i * 2, 2), 16)).ToArray();
		}

		private static byte[] Sha1(params byte[][] ba)
		{
			using (SHA1 hash = SHA1.Create())
			{
				return hash.ComputeHash(ba.SelectMany(x => x).ToArray());
			}
		}

		private static byte[] Pad(BigInteger n)
		{
			byte[] bn = ToBigByteArray(n);
			if (bn.Length > SRP_KEY_SIZE)
			{
				byte[] buf = new byte[SRP_KEY_SIZE];
				Array.Copy(bn, bn.Length - SRP_KEY_SIZE, buf, 0, SRP_KEY_SIZE);
				return buf;
			}
			return bn;
		}

		private static BigInteger GetScramble(BigInteger x, BigInteger y)
		{
			return FromBigByteArray(Sha1(Pad(x), Pad(y)));
		}
	}
}
