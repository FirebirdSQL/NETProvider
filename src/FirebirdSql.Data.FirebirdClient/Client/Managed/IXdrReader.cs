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
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.Client.Managed;

interface IXdrReader
{
	byte[] ReadBytes(byte[] buffer, int count);
	ValueTask<byte[]> ReadBytesAsync(byte[] buffer, int count, CancellationToken cancellationToken = default);

	byte[] ReadOpaque(int length);
	ValueTask<byte[]> ReadOpaqueAsync(int length, CancellationToken cancellationToken = default);

	byte[] ReadBuffer();
	ValueTask<byte[]> ReadBufferAsync(CancellationToken cancellationToken = default);

	string ReadString();
	ValueTask<string> ReadStringAsync(CancellationToken cancellationToken = default);

	string ReadString(int length);
	ValueTask<string> ReadStringAsync(int length, CancellationToken cancellationToken = default);

	string ReadString(Charset charset);
	ValueTask<string> ReadStringAsync(Charset charset, CancellationToken cancellationToken = default);

	string ReadString(Charset charset, int length);
	ValueTask<string> ReadStringAsync(Charset charset, int length, CancellationToken cancellationToken = default);

	short ReadInt16();
	ValueTask<short> ReadInt16Async(CancellationToken cancellationToken = default);

	int ReadInt32();
	ValueTask<int> ReadInt32Async(CancellationToken cancellationToken = default);

	long ReadInt64();
	ValueTask<long> ReadInt64Async(CancellationToken cancellationToken = default);

	Guid ReadGuid(int sqlType);
	ValueTask<Guid> ReadGuidAsync(int sqlType, CancellationToken cancellationToken = default);

	float ReadSingle();
	ValueTask<float> ReadSingleAsync(CancellationToken cancellationToken = default);

	double ReadDouble();
	ValueTask<double> ReadDoubleAsync(CancellationToken cancellationToken = default);

	DateTime ReadDateTime();
	ValueTask<DateTime> ReadDateTimeAsync(CancellationToken cancellationToken = default);

	DateTime ReadDate();
	ValueTask<DateTime> ReadDateAsync(CancellationToken cancellationToken = default);

	TimeSpan ReadTime();
	ValueTask<TimeSpan> ReadTimeAsync(CancellationToken cancellationToken = default);

	decimal ReadDecimal(int type, int scale);
	ValueTask<decimal> ReadDecimalAsync(int type, int scale, CancellationToken cancellationToken = default);

	bool ReadBoolean();
	ValueTask<bool> ReadBooleanAsync(CancellationToken cancellationToken = default);

	FbZonedDateTime ReadZonedDateTime(bool isExtended);
	ValueTask<FbZonedDateTime> ReadZonedDateTimeAsync(bool isExtended, CancellationToken cancellationToken = default);

	FbZonedTime ReadZonedTime(bool isExtended);
	ValueTask<FbZonedTime> ReadZonedTimeAsync(bool isExtended, CancellationToken cancellationToken = default);

	FbDecFloat ReadDec16();
	ValueTask<FbDecFloat> ReadDec16Async(CancellationToken cancellationToken = default);

	FbDecFloat ReadDec34();
	ValueTask<FbDecFloat> ReadDec34Async(CancellationToken cancellationToken = default);

	BigInteger ReadInt128();
	ValueTask<BigInteger> ReadInt128Async(CancellationToken cancellationToken = default);

	IscException ReadStatusVector();
	ValueTask<IscException> ReadStatusVectorAsync(CancellationToken cancellationToken = default);

	int ReadOperation();
	ValueTask<int> ReadOperationAsync(CancellationToken cancellationToken = default);
}
