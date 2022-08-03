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

using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Client.Managed;

sealed class DataProviderStreamWrapper : IDataProvider
{
	readonly Stream _stream;

	public DataProviderStreamWrapper(Stream stream)
	{
		_stream = stream;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Read(byte[] buffer, int offset, int count)
	{
		return _stream.Read(buffer, offset, count);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValueTask<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
	{
		return new ValueTask<int>(_stream.ReadAsync(buffer, offset, count, cancellationToken));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Write(byte[] buffer, int offset, int count)
	{
		_stream.Write(buffer, offset, count);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValueTask WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
	{
		return new ValueTask(_stream.WriteAsync(buffer, offset, count, cancellationToken));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Flush()
	{
		_stream.Flush();
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ValueTask FlushAsync(CancellationToken cancellationToken = default)
	{
		return new ValueTask(_stream.FlushAsync(cancellationToken));
	}
}
