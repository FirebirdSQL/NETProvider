/*
 *	Firebird ADO.NET Data provider for .NET and Mono
 *
 *	   The contents of this file are subject to the Initial
 *	   Developer's Public License Version 1.0 (the "License");
 *	   you may not use this file except in compliance with the
 *	   License. You may obtain a copy of the License at
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *	   express or implied. See the License for the specific
 *	   language governing rights and limitations under the License.
 *
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal class GdsEventManager
	{
		#region Fields

		private GdsDatabase _database;
		private Thread _eventsThread;
		private ConcurrentDictionary<int, RemoteEvent> _events;
		private int _handle;

		#endregion

		#region Constructors

		public GdsEventManager(int handle, string ipAddress, int portNumber)
		{
			_events = new ConcurrentDictionary<int, RemoteEvent>();
			_handle = handle;

			// Initialize the connection
			if (_database == null)
			{
				GdsConnection connection = new GdsConnection(ipAddress, portNumber);

				connection.Connect();

				_database = new GdsDatabase(connection);
			}
		}

		#endregion

		#region Methods

		public void QueueEvents(RemoteEvent remoteEvent)
		{
			lock (this)
			{
				_events[remoteEvent.LocalId] = remoteEvent;

				// Jiri Cincura: I'm pretty sure this is a race condition.
				if (_eventsThread == null || _eventsThread.ThreadState.HasFlag(ThreadState.Stopped | ThreadState.Unstarted))
				{
					_eventsThread = new Thread(ThreadHandler);
					_eventsThread.IsBackground = true;
					_eventsThread.Name = "FirebirdClient - Events Thread";
					_eventsThread.Start();
				}
			}
		}

		public void CancelEvents(RemoteEvent remoteEvent)
		{
			RemoteEvent dummy;
			_events.TryRemove(remoteEvent.LocalId, out dummy);
		}

		public void Close()
		{
			lock (_database.SyncObject)
			{
				if (_database != null)
				{
					_database.CloseConnection();
				}

				if (_eventsThread != null)
				{
					// we don't have here clue about disposing vs. finalizer
					if (!Environment.HasShutdownStarted)
					{
						_eventsThread.Abort();
						_eventsThread.Join();
					}

					_eventsThread = null;
				}
			}
		}

		#endregion

		#region Private Methods

		private void ThreadHandler(object o)
		{
			try
			{
				while (GetEventsCountLocked() > 0)
				{
					var operation = _database.NextOperation();

					switch (operation)
					{
						case IscCodes.op_response:
							_database.ReadResponse();
							continue;

						case IscCodes.op_exit:
						case IscCodes.op_disconnect:
							Close();
							return;

						case IscCodes.op_event:
							var dbHandle = _database.XdrStream.ReadInt32();
							var buffer = _database.XdrStream.ReadBuffer();
							var ast = _database.XdrStream.ReadBytes(8);
							var eventId = _database.XdrStream.ReadInt32();

							RemoteEvent currentEvent;
							if (_events.TryRemove(eventId, out currentEvent))
							{
								// Notify new event counts
								currentEvent.EventCounts(buffer);
							}

							continue;
					}
				}
			}
			catch (ThreadAbortException)
			{
				return;
			}
			catch
			{
				return;
			}
		}

		private int GetEventsCountLocked()
		{
			return _events.Count;
		}

		#endregion
	}
}
