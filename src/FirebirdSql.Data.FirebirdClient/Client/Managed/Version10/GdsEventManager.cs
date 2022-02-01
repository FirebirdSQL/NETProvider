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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10;

internal class GdsEventManager
{
	bool _closing;
	int _handle;
	string _ipAddress;
	int _portNumber;
	int _timeout;
	GdsDatabase _database;

	public GdsEventManager(int handle, string ipAddress, int portNumber, int timeout)
	{
		_closing = false;
		_handle = handle;
		_ipAddress = ipAddress;
		_portNumber = portNumber;
		_timeout = timeout;
	}

	public void Open()
	{
		var connection = new GdsConnection(_ipAddress, _portNumber, _timeout);
		connection.Connect();
		_database = new GdsDatabase(connection);
	}
	public async ValueTask OpenAsync(CancellationToken cancellationToken = default)
	{
		var connection = new GdsConnection(_ipAddress, _portNumber, _timeout);
		await connection.ConnectAsync(cancellationToken).ConfigureAwait(false);
		_database = new GdsDatabase(connection);
	}

	// this is a special method that's not awaited
	public async Task StartWaitingForEvents(RemoteEvent remoteEvent)
	{
		while (true)
		{
			try
			{
				var operation = await _database.ReadOperationAsync(CancellationToken.None).ConfigureAwait(false);

				switch (operation)
				{
					case IscCodes.op_event:
						var dbHandle = await _database.Xdr.ReadInt32Async(CancellationToken.None).ConfigureAwait(false);
						var buffer = await _database.Xdr.ReadBufferAsync(CancellationToken.None).ConfigureAwait(false);
						var ast = new byte[8];
						await _database.Xdr.ReadBytesAsync(ast, 8, CancellationToken.None).ConfigureAwait(false);
						var eventId = await _database.Xdr.ReadInt32Async(CancellationToken.None).ConfigureAwait(false);

						remoteEvent.EventCounts(buffer);

						await remoteEvent.Database.QueueEventsAsync(remoteEvent, CancellationToken.None).ConfigureAwait(false);

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

	public void Close()
	{
		Volatile.Write(ref _closing, true);
		_database.CloseConnection();
	}
	public ValueTask CloseAsync(CancellationToken cancellationToken = default)
	{
		Volatile.Write(ref _closing, true);
		return _database.CloseConnectionAsync(cancellationToken);
	}
}
