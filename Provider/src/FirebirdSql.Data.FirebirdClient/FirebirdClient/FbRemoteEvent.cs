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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	public sealed class FbRemoteEvent : IDisposable
#if !(NET48 || NETSTANDARD2_0)
		, IAsyncDisposable
#endif
	{
		private FbConnectionInternal _connection;
		private RemoteEvent _revent;
		private SynchronizationContext _synchronizationContext;

		public event EventHandler<FbRemoteEventCountsEventArgs> RemoteEventCounts;
		public event EventHandler<FbRemoteEventErrorEventArgs> RemoteEventError;

		public string this[int index] => _revent != null ? _revent.Events[index] : throw new InvalidOperationException();
		public int RemoteEventId => _revent != null ? _revent.RemoteId : throw new InvalidOperationException();

		public FbRemoteEvent(string connectionString)
		{
			_connection = new FbConnectionInternal(new ConnectionString(connectionString));
		}

		public void Open() => OpenImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task OpenAsync(CancellationToken cancellationToken = default) => OpenImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task OpenImpl(AsyncWrappingCommonArgs async)
		{
			if (_revent != null)
				throw new InvalidOperationException($"{nameof(FbRemoteEvent)} already open.");

			await _connection.Connect(async).ConfigureAwait(false);
			_revent = new RemoteEvent(_connection.Database);
			_revent.EventCountsCallback = OnRemoteEventCounts;
			_revent.EventErrorCallback = OnRemoteEventError;
			_synchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();
		}

		public void Dispose() => DisposeImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
#if !(NET48 || NETSTANDARD2_0)
		public async ValueTask DisposeAsync() => await DisposeImpl(new AsyncWrappingCommonArgs(true)).ConfigureAwait(false);
#endif
		private Task DisposeImpl(AsyncWrappingCommonArgs async)
		{
			return _connection.Disconnect(async);
		}

		public void QueueEvents(params string[] events) => QueueEventsImpl(events, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task QueueEventsAsync(string[] events, CancellationToken cancellationToken = default) => QueueEventsImpl(events, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task QueueEventsImpl(string[] events, AsyncWrappingCommonArgs async)
		{
			if (_revent == null)
				throw new InvalidOperationException($"{nameof(FbRemoteEvent)} must be opened.");

			try
			{
				await _revent.QueueEvents(events, async).ConfigureAwait(false);
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		public void CancelEvents() => CancelEventsImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task CancelEventsAsync(CancellationToken cancellationToken) => CancelEventsImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task CancelEventsImpl(AsyncWrappingCommonArgs async)
		{
			try
			{
				await _revent.CancelEvents(async).ConfigureAwait(false);
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
