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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal class GdsEventManager
	{
		bool _closing;
		int _handle;
		string _ipAddress;
		int _portNumber;
		GdsDatabase _database;

		public GdsEventManager(int handle, string ipAddress, int portNumber)
		{
			_closing = false;
			_handle = handle;
			_ipAddress = ipAddress;
			_portNumber = portNumber;
		}

		public async Task Open(AsyncWrappingCommonArgs async)
		{
			var connection = new GdsConnection(_ipAddress, _portNumber);
			await connection.Connect(async).ConfigureAwait(false);
			_database = new GdsDatabase(connection);
		}

		public async Task WaitForEvents(RemoteEvent remoteEvent, AsyncWrappingCommonArgs async)
		{
			while (true)
			{
				try
				{
					var operation = await _database.ReadOperation(async).ConfigureAwait(false);

					switch (operation)
					{
						case IscCodes.op_event:
							var dbHandle = await _database.Xdr.ReadInt32(async).ConfigureAwait(false);
							var buffer = await _database.Xdr.ReadBuffer(async).ConfigureAwait(false);
							var ast = new byte[8];
							await _database.Xdr.ReadBytes(ast, 8, async).ConfigureAwait(false);
							var eventId = await _database.Xdr.ReadInt32(async).ConfigureAwait(false);

							await remoteEvent.EventCounts(buffer, async).ConfigureAwait(false);

							break;

						default:
							Debug.Assert(false);
							break;
					}
				}
				catch (Exception) when (Volatile.Read(ref _closing))
				{
					return;
				}
				catch (Exception ex)
				{
					remoteEvent.EventError(ex);
					break;
				}
			}
		}

		public Task Close(AsyncWrappingCommonArgs async)
		{
			Volatile.Write(ref _closing, true);
			return _database.CloseConnection(async);
		}
	}
}
