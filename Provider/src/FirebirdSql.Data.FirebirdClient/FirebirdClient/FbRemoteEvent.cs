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

		public event EventHandler<FbRemoteEventEventArgs> RemoteEventCounts;

		#endregion

		#region Fields

		private FbConnection _connection;
		private RemoteEvent _revent;
		private SynchronizationContext _synchronizationContext;

		#endregion

		#region Indexers

		public string this[int index]
		{
			get { return _revent.Events[index]; }
		}

		#endregion

		#region Properties

		public FbConnection Connection
		{
			get { return _connection; }
		}

		public int RemoteEventId
		{
			get { return _revent?.RemoteId ?? -1; }
		}

		#endregion

		#region Constructors

		public FbRemoteEvent(FbConnection connection)
			: this(connection, null)
		{ }

		public FbRemoteEvent(FbConnection connection, params string[] events)
		{
			if (connection == null || connection.State != System.Data.ConnectionState.Open)
			{
				throw new InvalidOperationException("Connection must be valid and open");
			}

			_connection = connection;
			_revent = connection.InnerConnection.Database.CreateEvent();
			_revent.EventCountsCallback = new RemoteEventCountsCallback(OnRemoteEventCounts);
			_synchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();

			if (events != null)
			{
				AddEvents(events);
			}
		}

		#endregion

		#region Methods

		public void AddEvents(params string[] events)
		{
			if (events == null)
				throw new ArgumentNullException(nameof(events));
			if (events.Length > 15)
				throw new ArgumentOutOfRangeException(nameof(events), "Maximum number of events is 15.");

			if (events.Length != _revent.Events.Count)
			{
				_revent.ResetCounts();
			}
			else
			{
				string[] actualEvents = new string[_revent.Events.Count];
				_revent.Events.CopyTo(actualEvents, 0);

				for (int i = 0; i < actualEvents.Length; i++)
				{
					if (events[i] != actualEvents[i])
					{
						_revent.ResetCounts();
						break;
					}
				}
			}

			_revent.Events.Clear();

			for (int i = 0; i < events.Length; i++)
			{
				_revent.Events.Add(events[i]);
			}
		}

		public void QueueEvents()
		{
			try
			{
				_revent.QueueEvents();
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
				_revent.CancelEvents();
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

			int[] actualCounts = (int[])_revent.ActualCounts.Clone();
			if (_revent.PreviousCounts != null)
			{
				for (int i = 0; i < _revent.ActualCounts.Length; i++)
				{
					actualCounts[i] -= _revent.PreviousCounts[i];
				}
			}

			for (int i = 0; i < actualCounts.Length; i++)
			{
				FbRemoteEventEventArgs args = new FbRemoteEventEventArgs(_revent.Events[i], actualCounts[i]);
				_synchronizationContext.Send(_ =>
				{
					RemoteEventCounts?.Invoke(this, args);
				}, null);

				if (args.Cancel)
				{
					canceled = true;
					break;
				}
			}

			if (canceled)
			{
				CancelEvents();
			}
			else
			{
				QueueEvents();
			}
		}

		#endregion
	}
}
