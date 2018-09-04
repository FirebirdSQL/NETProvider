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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal class GdsEventManager : IDisposable
	{
		bool _disposing;
		int _handle;
		GdsDatabase _database;

		public GdsEventManager(int handle, string ipAddress, int portNumber)
		{
			_disposing = false;
			_handle = handle;
			var connection = new GdsConnection(ipAddress, portNumber);
			connection.Connect();
			_database = new GdsDatabase(connection);
		}

		public async Task WaitForEventsAsync(RemoteEvent remoteEvent)
		{
			while (true)
			{
				try
				{
					var operation = await _database.NextOperationAsync().ConfigureAwait(false);

					switch (operation)
					{
						case IscCodes.op_event:
							var dbHandle = _database.XdrStream.ReadInt32();
							var buffer = _database.XdrStream.ReadBuffer();
							var ast = new byte[8];
							_database.XdrStream.ReadBytes(ast);
							var eventId = _database.XdrStream.ReadInt32();

							remoteEvent.EventCounts(buffer);

							break;

						default:
							Debug.Assert(false);
							break;
					}
				}
				catch (Exception) when (_disposing)
				{
					return;
				}
				catch (Exception ex) when (!_disposing)
				{
					remoteEvent.EventError(ex);
					break;
				}
			}
		}

		public void Dispose()
		{
			_disposing = true;
			_database.CloseConnection();
		}
	}
}
