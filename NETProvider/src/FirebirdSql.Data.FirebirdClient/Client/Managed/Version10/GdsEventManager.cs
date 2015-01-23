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

		private GdsDatabase database;
		private Thread eventsThread;
		private ConcurrentDictionary<int, RemoteEvent> events;
		private int handle;

		#endregion

		#region Constructors

		public GdsEventManager(int handle, string ipAddress, int portNumber)
		{
			this.events = new ConcurrentDictionary<int, RemoteEvent>();
			this.handle = handle;

			// Initialize the connection
			if (this.database == null)
			{
				GdsConnection connection = new GdsConnection(ipAddress, portNumber);

				connection.Connect();

				this.database = new GdsDatabase(connection);
			}
		}

		#endregion

		#region Methods

		public void QueueEvents(RemoteEvent remoteEvent)
		{
			lock (this)
			{
				this.events[remoteEvent.LocalId] = remoteEvent;

				// Jiri Cincura: I'm pretty sure this is a race condition.
				if (this.eventsThread == null || this.eventsThread.ThreadState.HasFlag(ThreadState.Stopped | ThreadState.Unstarted))
				{
					this.eventsThread = new Thread(ThreadHandler);
					this.eventsThread.IsBackground = true;
					this.eventsThread.Name = "FirebirdClient - Events Thread";
					this.eventsThread.Start();
				}
			}
		}

		public void CancelEvents(RemoteEvent remoteEvent)
		{
			RemoteEvent dummy;
			this.events.TryRemove(remoteEvent.LocalId, out dummy);
		}

		public void Close()
		{
			lock (this.database.SyncObject)
			{
				if (this.database != null)
				{
					this.database.CloseConnection();
				}

				if (this.eventsThread != null)
				{
					// we don't have here clue about disposing vs. finalizer
					if (!Environment.HasShutdownStarted)
					{
						this.eventsThread.Abort();
						this.eventsThread.Join();
					}

					this.eventsThread = null;
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
					var operation = this.database.NextOperation();

					switch (operation)
					{
						case IscCodes.op_response:
							this.database.ReadResponse();
							continue;

						case IscCodes.op_exit:
						case IscCodes.op_disconnect:
							this.Close();
							return;

						case IscCodes.op_event:
							var dbHandle = this.database.ReadInt32();
							var buffer = this.database.ReadBuffer();
							var ast = this.database.ReadBytes(8);
							var eventId = this.database.ReadInt32();

							RemoteEvent currentEvent;
							if (this.events.TryRemove(eventId, out currentEvent))
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
			return this.events.Count;
		}

		#endregion
	}
}
