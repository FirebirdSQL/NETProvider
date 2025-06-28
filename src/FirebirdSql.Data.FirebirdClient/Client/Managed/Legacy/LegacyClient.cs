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

//$Authors = Osincev Daniil

using System;

namespace FirebirdSql.Data.Client.Managed.Legacy;

internal sealed class LegacyClient
{

	public string PluginName = "Legacy_Auth";

	private static readonly byte[] Rotates = { 1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1 };

	private static readonly byte[] ITOA64 = {
			(byte)'.', (byte)'/', (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5',
			(byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'A', (byte)'B', (byte)'C', (byte)'D',
			(byte)'E', (byte)'F', (byte)'G', (byte)'H', (byte)'I', (byte)'J', (byte)'K', (byte)'L',
			(byte)'M', (byte)'N', (byte)'O', (byte)'P', (byte)'Q', (byte)'R', (byte)'S', (byte)'T',
			(byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y', (byte)'Z', (byte)'a', (byte)'b',
			(byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j',
			(byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p', (byte)'q', (byte)'r',
			(byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x', (byte)'y', (byte)'z'
	};

	private static readonly long[][] PC1ROT = new long[16][];
	private static readonly long[][][] PC2ROT = new long[2][][];
	private static readonly long[][] SPE = new long[8][];
	private static readonly long[][] CF6464 = new long[16][];

	private const int FB_SALT = 754712576;
	private const int ITERATIONS = 25;

	static LegacyClient()
	{
		for (int i = 0; i < 16; i++)
		{
			PC1ROT[i] = new long[16];
			CF6464[i] = new long[16];
			SPE[i % 8] = new long[64];
		}
		for (int j = 0; j < 2; j++)
		{
			PC2ROT[j] = new long[16][];
			for (int i = 0; i < 16; i++)
			{
				PC2ROT[j][i] = new long[16];
			}
		}

		InitTables();
	}

	private static void InitTables()
	{
		byte[] perm = new byte[64];
		byte[] temp = new byte[64];

		byte[] PC1 = {
			57, 49, 41, 33, 25, 17, 9,
			1, 58, 50, 42, 34, 26, 18,
			10, 2, 59, 51, 43, 35, 27,
			19, 11, 3, 60, 52, 44, 36,

			63, 55, 47, 39, 31, 23, 15,
			7, 62, 54, 46, 38, 30, 22,
			14, 6, 61, 53, 45, 37, 29,
			21, 13, 5, 28, 20, 12, 4
		};

		byte[] PC2 = {
			9, 18, 14, 17, 11, 24, 1, 5,
			22, 25, 3, 28, 15, 6, 21, 10,
			35, 38, 23, 19, 12, 4, 26, 8,
			43, 54, 16, 7, 27, 20, 13, 2,

			0, 0, 41, 52, 31, 37, 47, 55,
			0, 0, 30, 40, 51, 45, 33, 48,
			0, 0, 44, 49, 39, 56, 34, 53,
			0, 0, 46, 42, 50, 36, 29, 32
		};

		for (int i = 0; i < 64; i++)
		{
			int k = PC2[i];
			if (k == 0) continue;
			if ((k % 28) < 1) k -= 28;
			k = PC1[k];
			k--;
			k = (k | 0x07) - (k & 0x07);
			k++;
			perm[i] = (byte)k;
		}
		InitPerm(PC1ROT, perm);

		for (int j = 0; j < 2; j++)
		{
			int k;
			Array.Clear(perm, 0, perm.Length);
			Array.Clear(temp, 0, temp.Length);
			for (int i = 0; i < 64; i++)
			{
				if ((k = PC2[i]) == 0) continue;
				temp[k - 1] = (byte)(i + 1);
			}
			for (int i = 0; i < 64; i++)
			{
				if ((k = PC2[i]) == 0) continue;
				k += j;
				if ((k % 28) <= j) k -= 28;
				perm[i] = temp[k];
			}
			InitPerm(PC2ROT[j], perm);
		}

		byte[] IP = {
			58, 50, 42, 34, 26, 18, 10, 2,
			60, 52, 44, 36, 28, 20, 12, 4,
			62, 54, 46, 38, 30, 22, 14, 6,
			64, 56, 48, 40, 32, 24, 16, 8,
			57, 49, 41, 33, 25, 17, 9, 1,
			59, 51, 43, 35, 27, 19, 11, 3,
			61, 53, 45, 37, 29, 21, 13, 5,
			63, 55, 47, 39, 31, 23, 15, 7
		};

		byte[] ExpandTr = {
			32, 1, 2, 3, 4, 5,
			4, 5, 6, 7, 8, 9,
			8, 9, 10, 11, 12, 13,
			12, 13, 14, 15, 16, 17,
			16, 17, 18, 19, 20, 21,
			20, 21, 22, 23, 24, 25,
			24, 25, 26, 27, 28, 29,
			28, 29, 30, 31, 32, 1
		};

		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				int k = (j < 2) ? 0 : IP[ExpandTr[i * 6 + j - 2] - 1];
				if (k > 32) k -= 32;
				else if (k > 0) k--;
				if (k > 0)
				{
					k--;
					k = (k | 0x07) - (k & 0x07);
					k++;
				}
				perm[i * 8 + j] = (byte)k;
			}
		}

		byte[] CIFP = {
			1, 2, 3, 4, 17, 18, 19, 20,
			5, 6, 7, 8, 21, 22, 23, 24,
			9, 10, 11, 12, 25, 26, 27, 28,
			13, 14, 15, 16, 29, 30, 31, 32,

			33, 34, 35, 36, 49, 50, 51, 52,
			37, 38, 39, 40, 53, 54, 55, 56,
			41, 42, 43, 44, 57, 58, 59, 60,
			45, 46, 47, 48, 61, 62, 63, 64
		};

		for (int i = 0; i < 64; i++)
		{
			int k = IP[CIFP[i] - 1];
			k--;
			k = (k | 0x07) - (k & 0x07);
			k++;
			perm[k - 1] = (byte)(i + 1);
		}
		InitPerm(CF6464, perm);

		byte[][] S = {
			new byte[] { 14,4,13,1,2,15,11,8,3,10,6,12,5,9,0,7,
						  0,15,7,4,14,2,13,1,10,6,12,11,9,5,3,8,
						  4,1,14,8,13,6,2,11,15,12,9,7,3,10,5,0,
						  15,12,8,2,4,9,1,7,5,11,3,14,10,0,6,13 },
			new byte[] { 15,1,8,14,6,11,3,4,9,7,2,13,12,0,5,10,
						  3,13,4,7,15,2,8,14,12,0,1,10,6,9,11,5,
						  0,14,7,11,10,4,13,1,5,8,12,6,9,3,2,15,
						  13,8,10,1,3,15,4,2,11,6,7,12,0,5,14,9 },
			new byte[] { 10,0,9,14,6,3,15,5,1,13,12,7,11,4,2,8,
						  13,7,0,9,3,4,6,10,2,8,5,14,12,11,15,1,
						  13,6,4,9,8,15,3,0,11,1,2,12,5,10,14,7,
						  1,10,13,0,6,9,8,7,4,15,14,3,11,5,2,12 },
			new byte[] { 7,13,14,3,0,6,9,10,1,2,8,5,11,12,4,15,
						  13,8,11,5,6,15,0,3,4,7,2,12,1,10,14,9,
						  10,6,9,0,12,11,7,13,15,1,3,14,5,2,8,4,
						  3,15,0,6,10,1,13,8,9,4,5,11,12,7,2,14 },
			new byte[] { 2,12,4,1,7,10,11,6,8,5,3,15,13,0,14,9,
						  14,11,2,12,4,7,13,1,5,0,15,10,3,9,8,6,
						  4,2,1,11,10,13,7,8,15,9,12,5,6,3,0,14,
						  11,8,12,7,1,14,2,13,6,15,0,9,10,4,5,3 },
			new byte[] { 12,1,10,15,9,2,6,8,0,13,3,4,14,7,5,11,
						  10,15,4,2,7,12,9,5,6,1,13,14,0,11,3,8,
						  9,14,15,5,2,8,12,3,7,0,4,10,1,13,11,6,
						  4,3,2,12,9,5,15,10,11,14,1,7,6,0,8,13 },
			new byte[] { 4,11,2,14,15,0,8,13,3,12,9,7,5,10,6,1,
						  13,0,11,7,4,9,1,10,14,3,5,12,2,15,8,6,
						  1,4,11,13,12,3,7,14,10,15,6,8,0,5,9,2,
						  6,11,13,8,1,4,10,7,9,5,0,15,14,2,3,12 },
			new byte[] { 13,2,8,4,6,15,11,1,10,9,3,14,5,0,12,7,
						  1,15,13,8,10,3,7,4,12,5,6,11,0,14,9,2,
						  7,11,4,1,9,12,14,2,0,6,10,13,15,3,5,8,
						  2,1,14,7,4,10,8,13,15,12,9,0,3,5,6,11 }
		};

		byte[] P32Tr = {
			16,7,20,21,29,12,28,17,
			1,15,23,26,5,18,31,10,
			2,8,24,14,32,27,3,9,
			19,13,30,6,22,11,4,25
		};

		for (int i = 0; i < 48; i++)
			perm[i] = P32Tr[ExpandTr[i] - 1];
		for (int t = 0; t < 8; t++)
		{
			for (int j = 0; j < 64; j++)
			{
				int k = ((j & 0x01) << 5) | ((j >> 1 & 0x01) << 3) |
						((j >> 2 & 0x01) << 2) | ((j >> 3 & 0x01) << 1) |
						(j >> 4 & 0x01) | ((j >> 5 & 0x01) << 4);
				k = S[t][k];
				k = ((k >> 3 & 0x01) | ((k >> 2 & 0x01) << 1) |
					 ((k >> 1 & 0x01) << 2) | ((k & 0x01) << 3));
				for (int i = 0; i < 32; i++) temp[i] = 0;
				for (int i = 0; i < 4; i++) temp[4 * t + i] = (byte)((k >> i) & 0x01);
				long kk = 0;
				for (int i = 24; --i >= 0;)
					kk = (kk << 1) | ((long)temp[perm[i] - 1] << 32) | temp[perm[i + 24] - 1];
				SPE[t][j] = ToSixBit(kk);
			}
		}
	}

	private static long ToSixBit(long num)
	{
		ulong u = (ulong)num;
		ulong x =
			(u << 26 & 0xFC000000FC000000UL) |
			(u << 12 & 0x00FC000000FC0000UL) |
			(u >> 2 & 0x0000FC000000FC00UL) |
			(u >> 16 & 0x000000FC000000FCL);
		return unchecked((long)x);
	}

	private static long Perm6464(long c, long[][] p)
	{
		long output = 0L;
		for (int i = 8; --i >= 0;)
		{
			int t = (int)(c & 0xff);
			c >>= 8;
			output |= p[i << 1][t & 0x0f];
			output |= p[(i << 1) + 1][t >> 4];
		}
		return output;
	}

	private static long[] DesSetKey(long keyword)
	{
		long K = Perm6464(keyword, PC1ROT);
		long[] KS = new long[16];
		KS[0] = K & ~0x0303030300000000L;

		for (int i = 1; i < 16; i++)
		{
			KS[i] = K;
			K = Perm6464(K, PC2ROT[Rotates[i] - 1]);
			KS[i] = K & ~0x0303030300000000L;
		}
		return KS;
	}

	private static long DesCipher(long[] KS)
	{
		long L = 0;
		long R = 0;
		for (int iter = 0; iter < ITERATIONS; iter++)
		{
			for (int loop = 0; loop < 8; loop++)
			{
				long kp = KS[loop << 1];
				L ^= OpSPE(OpSALT(R) ^ R ^ kp);

				kp = KS[(loop << 1) + 1];
				R ^= OpSPE(OpSALT(L) ^ L ^ kp);
			}
			L ^= R;
			R ^= L;
			L ^= R;
		}
		L = ((L >> 35) & 0x0f0f0f0fL | (L << 1) & 0xf0f0f0f0L) << 32 |
			((R >> 35) & 0x0f0f0f0fL) | ((R << 1) & 0xf0f0f0f0L);
		L = Perm6464(L, CF6464);
		return L;
	}

	private static long OpSALT(long R)
	{
		long k = ((R >> 32) ^ R) & FB_SALT;
		k |= k << 32;
		return k;
	}

	private static long OpSPE(long B)
	{
		return SPE[0][(int)(B >> 58 & 0x3f)] ^ SPE[1][(int)(B >> 50 & 0x3f)] ^
			   SPE[2][(int)(B >> 42 & 0x3f)] ^ SPE[3][(int)(B >> 34 & 0x3f)] ^
			   SPE[4][(int)(B >> 26 & 0x3f)] ^ SPE[5][(int)(B >> 18 & 0x3f)] ^
			   SPE[6][(int)(B >> 10 & 0x3f)] ^ SPE[7][(int)(B >> 2 & 0x3f)];
	}

	private static void InitPerm(long[][] perm, byte[] p)
	{
		for (int k = 0; k < 8 * 8; k++)
		{
			int l = p[k] - 1;
			if (l < 0) continue;
			int i = l >> 2;
			l = 1 << (l & 0x03);
			for (int j = 0; j < 16; j++)
			{
				int s = (k & 0x07) + ((7 - (k >> 3)) << 3);
				if ((j & l) != 0)
					perm[i][j] |= 1L << s;
			}
		}
	}

	public byte[] ClientProof(string key)
	{
		if (key == null)
		{
			return new byte[] { (byte)'*' };
		}

		int keyLen = key.Length;
		long keyword = 0L;
		for (int i = 0; i < 8; i++)
		{
			keyword = (keyword << 8) |
					(i < keyLen ? (long)(byte)(2 * (byte)key[i]) : 0L);
		}

		long result = DesCipher(DesSetKey(keyword));

		byte[] cryptResult = new byte[11];
		cryptResult[10] = ITOA64[((int)result << 2) & 0x3f];
		result >>= 4;
		for (int i = 10; --i >= 0;)
		{
			cryptResult[i] = ITOA64[(int)result & 0x3f];
			result >>= 6;
		}
		return cryptResult;
	}
}
