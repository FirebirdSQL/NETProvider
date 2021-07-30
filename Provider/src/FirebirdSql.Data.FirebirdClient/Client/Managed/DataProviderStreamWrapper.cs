/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
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
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed
{
	sealed class DataProviderStreamWrapper : IDataProvider
	{
		readonly Stream _stream;

		public DataProviderStreamWrapper(Stream stream)
		{
			_stream = stream;
		}

		public async ValueTask<int> Read(byte[] buffer, int offset, int count, AsyncWrappingCommonArgs async) => await async.AsyncSyncCall(_stream.ReadAsync, _stream.Read, buffer, offset, count).ConfigureAwait(false);

		public async ValueTask Write(byte[] buffer, int offset, int count, AsyncWrappingCommonArgs async) => await async.AsyncSyncCall(_stream.WriteAsync, _stream.Write, buffer, offset, count).ConfigureAwait(false);

		public async ValueTask Flush(AsyncWrappingCommonArgs async) => await async.AsyncSyncCall(_stream.FlushAsync, _stream.Flush).ConfigureAwait(false);
	}
}
