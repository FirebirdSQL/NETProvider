/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *  
 *  Contributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Threading;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	public sealed class FbRemoteEvent
	{
		#region Events

		public event FbRemoteEventEventHandler RemoteEventCounts;

		#endregion

		#region Fields

		private FbConnection connection;
		private RemoteEvent revent;
		private SynchronizationContext synchronizationContext;

		#endregion

		#region Indexers

		public string this[int index]
		{
			get { return this.revent.Events[index]; }
		}

		#endregion

		#region Properties

		public FbConnection Connection
		{
			get { return this.connection; }
			set { this.connection = value; }
		}

		public bool HasChanges
		{
			get { return this.revent.HasChanges; }
		}

		public int RemoteEventId
		{
			get
			{
				if (this.revent != null)
				{
					return this.revent.RemoteId;
				}

				return -1;
			}
		}

		#endregion

		#region Constructors

		public FbRemoteEvent(FbConnection connection)
			: this(connection, null)
		{
		}

		public FbRemoteEvent(FbConnection connection, params string[] events)
		{
			if (connection == null || connection.State != System.Data.ConnectionState.Open)
			{
				throw new InvalidOperationException("Connection must be valid and open");
			}

			this.connection = connection;
			this.revent = connection.InnerConnection.Database.CreateEvent();
			this.revent.EventCountsCallback = new RemoteEventCountsCallback(this.OnRemoteEventCounts);
			this.synchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();

			if (events != null)
			{
				this.AddEvents(events);
			}
		}

		#endregion

		#region Methods

		public void AddEvents(params string[] events)
		{
			if (events == null)
			{
				throw new ArgumentNullException("events cannot be null.");
			}
			if (events.Length > 15)
			{
				throw new ArgumentException("Max number of events for request interest is 15");
			}

			if (events.Length != this.revent.Events.Count)
			{
				this.revent.ResetCounts();
			}
			else
			{
				string[] actualEvents = new string[this.revent.Events.Count];
				this.revent.Events.CopyTo(actualEvents, 0);

				for (int i = 0; i < actualEvents.Length; i++)
				{
					if (events[i] != actualEvents[i])
					{
						this.revent.ResetCounts();
						break;
					}
				}
			}

			this.revent.Events.Clear();

			for (int i = 0; i < events.Length; i++)
			{
				this.revent.Events.Add(events[i]);
			}
		}

		public void QueueEvents()
		{
			try
			{
				this.revent.QueueEvents();
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public void CancelEvents()
		{
			try
			{
				this.revent.CancelEvents();
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		#endregion

		#region Callbacks Handlers

		private void OnRemoteEventCounts()
		{
			bool canceled = false;

			int[] actualCounts = (int[])this.revent.ActualCounts.Clone();
			if (this.revent.PreviousCounts != null)
			{
				for (int i = 0; i < this.revent.ActualCounts.Length; i++)
				{
					actualCounts[i] -= this.revent.PreviousCounts[i];
				}
			}

			// Send individual event notifications
			for (int i = 0; i < actualCounts.Length; i++)
			{
				FbRemoteEventEventArgs args = new FbRemoteEventEventArgs(this.revent.Events[i], actualCounts[i]);
				if (this.RemoteEventCounts != null)
				{
					this.synchronizationContext.Send(_ =>
					{
						this.RemoteEventCounts(this, args);
					}, null);
				}

				if (args.Cancel)
				{
					canceled = true;
					break;
				}
			}

			if (canceled)
			{
				// Requeque
				this.CancelEvents();
			}
			else
			{
				// Requeque
				this.QueueEvents();
			}
		}

		#endregion
	}
}
