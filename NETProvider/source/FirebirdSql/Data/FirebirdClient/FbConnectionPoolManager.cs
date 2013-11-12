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
 *  Copyright (c) 2013 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FirebirdSql.Data.FirebirdClient
{
	sealed class FbConnectionPoolManager : IDisposable
	{
		static Lazy<FbConnectionPoolManager> _instanceLazy = new Lazy<FbConnectionPoolManager>(() => new FbConnectionPoolManager(), LazyThreadSafetyMode.PublicationOnly);

		internal static FbConnectionPoolManager Instance
		{
			get { return _instanceLazy.Value; }
		}

		sealed class Pool : IDisposable
		{
			sealed class Item : IDisposable
			{
				bool _disposed;

				public DateTimeOffset Created { get; private set; }
				public FbConnectionInternal Connection { get; private set; }

				public Item()
				{
					_disposed = false;
				}

				public Item(DateTimeOffset created, FbConnectionInternal connection)
					: this()
				{
					Created = created;
					Connection = connection;
				}

				public void Dispose()
				{
					if (_disposed)
						return;
					_disposed = true;
					Created = default(DateTimeOffset);
					Connection.Dispose();
					Connection = null;
				}
			}

			bool _disposed;
			object _syncRoot;
			FbConnectionString _connectionString;
			TimeSpan _lifeTime;
			Queue<Item> _available;
			List<FbConnectionInternal> _busy;

			public Pool(FbConnectionString connectionString)
			{
				_disposed = false;
				_syncRoot = new object();
				_connectionString = connectionString;
				_available = new Queue<Item>();
				_busy = new List<FbConnectionInternal>();
			}

			public void Dispose()
			{
				lock (_syncRoot)
				{
					if (_disposed)
						return;
					_disposed = true;
					_connectionString = null;
					_lifeTime = default(TimeSpan);
					CleanConnectionsImpl();
					_available = null;
					_busy = null;
				}
			}

			public FbConnectionInternal GetConnection()
			{
				lock (_syncRoot)
				{
					CheckDisposedImpl();

					var connection = _available.Any()
						? _available.Dequeue().Connection
						: CreateNewConnection(_connectionString);
					_busy.Add(connection);
					return connection;
				}
			}

			public void ReleaseConnection(FbConnectionInternal connection)
			{
				lock (_syncRoot)
				{
					CheckDisposedImpl();

					var removed = _busy.Remove(connection);
					if (removed)
						_available.Enqueue(new Item(DateTimeOffset.UtcNow, connection));
				}
			}

			public void CleanupPool()
			{
				lock (_syncRoot)
				{
					CheckDisposedImpl();

					var now = DateTimeOffset.UtcNow;
					var available = _available.ToArray();
					var keep = available.Where(x => x.Created.Add(_lifeTime) > now).ToArray();
					var release = available.Except(keep).ToArray();
					release.AsParallel().ForAll(x => x.Connection.Dispose());
					_available = new Queue<Item>(keep);
				}
			}

			static FbConnectionInternal CreateNewConnection(FbConnectionString connectionString)
			{
				var result = new FbConnectionInternal(connectionString);
				result.Connect();
				return result;
			}

			void CleanConnectionsImpl()
			{
				foreach (var item in _available)
					item.Dispose();
				foreach (var item in _busy)
					item.Dispose();
			}

			void CheckDisposedImpl()
			{
				if (_disposed)
					throw new ObjectDisposedException(typeof(Pool).Name);
			}
		}

		int _disposed;
		ConcurrentDictionary<string, Pool> _pools;
		Timer _cleanupTimer;

		FbConnectionPoolManager()
		{
			_disposed = 0;
			_pools = new ConcurrentDictionary<string, Pool>();
		}

		public FbConnectionInternal Get(string connectionString)
		{
			CheckDisposed();

			return _pools.GetOrAdd(connectionString, CreateNewPool).GetConnection();
		}

		public void Release(FbConnectionInternal connection)
		{
			CheckDisposed();

			_pools.GetOrAdd(connection.Options.NormalizedConnectionString, CreateNewPool).ReleaseConnection(connection);
		}

		public void Dispose()
		{
			if (Interlocked.Exchange(ref _disposed, 1) == 1)
				return;
			_cleanupTimer.Dispose();
			_cleanupTimer = null;
			_pools.Values.AsParallel().ForAll(x => x.Dispose());
			_pools = null;
		}

		static Pool CreateNewPool(string connectionString)
		{
			var pool = new Pool(new FbConnectionString(connectionString));
			return pool;
		}

		void CleanupCallback(object o)
		{
			if (Volatile.Read(ref _disposed) == 1)
				return;
			_pools.Values.AsParallel().ForAll(p => p.CleanupPool());
		}

		void CheckDisposed()
		{
			if (Volatile.Read(ref _disposed) == 1)
				throw new ObjectDisposedException(typeof(FbConnectionPoolManager).Name);
		}
	}
}
