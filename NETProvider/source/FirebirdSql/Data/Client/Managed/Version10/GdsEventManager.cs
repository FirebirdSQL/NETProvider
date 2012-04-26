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
using System.Collections;
using System.IO;
using System.Threading;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal class GdsEventManager
	{
		#region · Fields ·

		private GdsDatabase database;
		private Thread eventsThread;
		private Hashtable events;
		private int handle;
#if (NET_CF)
		private System.Windows.Forms.Control syncControl;
#else
		private SynchronizationContext syncContext;
#endif

		#endregion

		#region · Properties ·

		//public Hashtable EventList
		//{
		//    get { return this.events; }
		//}

		#endregion

		#region · Constructors ·

		public GdsEventManager(int handle, string ipAddress, int portNumber)
		{
			this.events = new Hashtable();
			this.events = Hashtable.Synchronized(this.events);
			this.handle = handle;
#if (NET_CF)
			this.syncControl = new System.Windows.Forms.Control();
			IntPtr h = this.syncControl.Handle; // force handle creation
#else
			this.syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
#endif
					

			// Initialize the connection
			if (this.database == null)
			{
				GdsConnection connection = new GdsConnection(ipAddress, portNumber);

				connection.Connect();

				this.database = new GdsDatabase(connection);
			}
		}

		#endregion

		#region · Methods ·

		public void QueueEvents(RemoteEvent remoteEvent)
		{
			lock (this)
			{
				if (!this.events.ContainsKey(remoteEvent.LocalId))
				{
					lock (this.events.SyncRoot)
					{
						this.events.Add(remoteEvent.LocalId, remoteEvent);
					}
				}

#if	(!NET_CF)
				if (this.eventsThread == null || this.eventsThread.ThreadState.HasFlag(ThreadState.Stopped | ThreadState.Unstarted))
#else
				if (this.eventsThread == null)
#endif
				{
#if (NET_CF)
					this.eventsThread = new Thread(new ThreadStart(() => ThreadHandler(this.syncControl)));
#else
					this.eventsThread = new Thread(new ThreadStart(() => ThreadHandler(this.syncContext)));
#endif
					this.eventsThread.IsBackground = true;
					this.eventsThread.Name = "FirebirdClient - Events Thread";
					this.eventsThread.Start();
				}
			}
		}

		public void CancelEvents(RemoteEvent remoteEvent)
		{
			lock (this.events.SyncRoot)
			{
				this.events.Remove(remoteEvent.LocalId);
			}
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
#if (!NET_CF)
					// we don't have here clue about disposing vs. finalizer
#warning in CF this will cause problems again (how to detect this on CF?, maybe better to redesign these classes)
					if (!Environment.HasShutdownStarted)
#endif
					{
						this.eventsThread.Abort();
						this.eventsThread.Join();
					}

					this.eventsThread = null;
				}
			}
		}

		#endregion

		#region · Private Methods ·

		private void ThreadHandler(object o)
		{
			int operation = -1;
			int dbHandle = 0;
			int eventId = 0;
			byte[] buffer = null;
			byte[] ast = null;

			try
			{
				while (this.events.Count > 0)
				{
					operation = this.database.NextOperation();

					switch (operation)
					{
						case IscCodes.op_response:
							this.database.ReadResponse();
							break;

						case IscCodes.op_exit:
						case IscCodes.op_disconnect:
							this.Close();
							return;

						case IscCodes.op_event:
							dbHandle = this.database.ReadInt32();
							buffer = this.database.ReadBuffer();
							ast = this.database.ReadBytes(8);
							eventId = this.database.ReadInt32();

							if (this.events.ContainsKey(eventId))
							{
								RemoteEvent currentEvent = (RemoteEvent)this.events[eventId];

								lock (this.events.SyncRoot)
								{
									// Remove event	from the list
									this.events.Remove(eventId);
								}

								// Notify new event	counts
#if (NET_CF)
								((System.Windows.Forms.Control)o).Invoke((Action)delegate
								{
									currentEvent.EventCounts(buffer);
								}, null);
#else
								((SynchronizationContext)o).Send(delegate
								{
									currentEvent.EventCounts(buffer);
								}, null);
#endif

								if (this.events.Count == 0)
								{
									return;
								}
							}
							break;
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

		#endregion
	}
}
