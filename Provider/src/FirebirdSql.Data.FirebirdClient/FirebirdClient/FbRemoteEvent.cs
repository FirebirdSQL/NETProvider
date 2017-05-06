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
using System.Linq;
using System.Threading;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	public sealed class FbRemoteEvent : IDisposable
	{
		private FbConnectionInternal _connection;
		private RemoteEvent _revent;
		private SynchronizationContext _synchronizationContext;

		public event EventHandler<FbRemoteEventCountsEventArgs> RemoteEventCounts;
		public event EventHandler<FbRemoteEventErrorEventArgs> RemoteEventError;

		public string this[int index] => _revent.Events[index];
		public int RemoteEventId => _revent?.RemoteId ?? -1;

		public FbRemoteEvent(string connectionString)
		{
			_connection = new FbConnectionInternal(new FbConnectionString(connectionString));
			_connection.Connect();
			_revent = new RemoteEvent(_connection.Database);
			_revent.EventCountsCallback = OnRemoteEventCounts;
			_revent.EventErrorCallback = OnRemoteEventError;
			_synchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();
		}

		public void Dispose()
		{
			_connection.Dispose();
		}

		public void QueueEvents(params string[] events)
		{
			try
			{
				_revent.QueueEvents(events);
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

		private void OnRemoteEventCounts(string name, int count)
		{
			var args = new FbRemoteEventCountsEventArgs(name, count);
			_synchronizationContext.Post(_ =>
			{
				RemoteEventCounts?.Invoke(this, args);
			}, null);
		}

		private void OnRemoteEventError(Exception error)
		{
			var args = new FbRemoteEventErrorEventArgs(error);
			_synchronizationContext.Post(_ =>
			{
				RemoteEventError?.Invoke(this, args);
			}, null);
		}
	}
}
