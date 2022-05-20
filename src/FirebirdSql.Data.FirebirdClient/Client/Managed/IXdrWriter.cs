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
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.Client.Managed;

interface IXdrWriter
{
	void Flush();
	ValueTask FlushAsync(CancellationToken cancellationToken = default);

	void WriteBytes(byte[] buffer, int count);
	ValueTask WriteBytesAsync(byte[] buffer, int count, CancellationToken cancellationToken = default);

	void WriteOpaque(byte[] buffer);
	ValueTask WriteOpaqueAsync(byte[] buffer, CancellationToken cancellationToken = default);

	void WriteOpaque(byte[] buffer, int length);
	ValueTask WriteOpaqueAsync(byte[] buffer, int length, CancellationToken cancellationToken = default);

	void WriteBuffer(byte[] buffer);
	ValueTask WriteBufferAsync(byte[] buffer, CancellationToken cancellationToken = default);

	void WriteBuffer(byte[] buffer, int length);
	ValueTask WriteBufferAsync(byte[] buffer, int length, CancellationToken cancellationToken = default);

	void WriteBlobBuffer(byte[] buffer);
	ValueTask WriteBlobBufferAsync(byte[] buffer, CancellationToken cancellationToken = default);

	void WriteTyped(int type, byte[] buffer);
	ValueTask WriteTypedAsync(int type, byte[] buffer, CancellationToken cancellationToken = default);

	void Write(string value);
	ValueTask WriteAsync(string value, CancellationToken cancellationToken = default);

	void Write(short value);
	ValueTask WriteAsync(short value, CancellationToken cancellationToken = default);

	void Write(int value);
	ValueTask WriteAsync(int value, CancellationToken cancellationToken = default);

	void Write(long value);
	ValueTask WriteAsync(long value, CancellationToken cancellationToken = default);

	void Write(float value);
	ValueTask WriteAsync(float value, CancellationToken cancellationToken = default);

	void Write(double value);
	ValueTask WriteAsync(double value, CancellationToken cancellationToken = default);

	void Write(decimal value, int type, int scale);
	ValueTask WriteAsync(decimal value, int type, int scale, CancellationToken cancellationToken = default);

	void Write(bool value);
	ValueTask WriteAsync(bool value, CancellationToken cancellationToken = default);

	void Write(DateTime value);
	ValueTask WriteAsync(DateTime value, CancellationToken cancellationToken = default);

	void Write(Guid value, int sqlType);
	ValueTask WriteAsync(Guid value, int sqlType, CancellationToken cancellationToken = default);

	void Write(FbDecFloat value, int size);
	ValueTask WriteAsync(FbDecFloat value, int size, CancellationToken cancellationToken = default);

	void Write(BigInteger value);
	ValueTask WriteAsync(BigInteger value, CancellationToken cancellationToken = default);

	void WriteDate(DateTime value);
	ValueTask WriteDateAsync(DateTime value, CancellationToken cancellationToken = default);

	void WriteTime(TimeSpan value);
	ValueTask WriteTimeAsync(TimeSpan value, CancellationToken cancellationToken = default);
}
