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
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Data;
using System.Collections;
using System.Threading;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird
{
	internal sealed class FbPoolManager
	{
		#region Static fields

		public static readonly FbPoolManager Instance = new FbPoolManager();

		#endregion

		#region Fields

		private Hashtable	pools;
		private Hashtable	handlers;
		private object		syncObject;

		#endregion

		#region Properties

		public int PoolsCount
		{
			get
			{
				if (this.pools != null)
				{
					return this.pools.Count;
				}
				return 0;
			}
		}

        public object SyncObject
        {
            get
            {
                if (this.syncObject == null)
                {
                    Interlocked.CompareExchange(ref this.syncObject, new object(), null);
                }

                return this.syncObject;
            }
        }

		#endregion

		#region Constructors

		private FbPoolManager()
		{
			this.pools		= Hashtable.Synchronized(new Hashtable());
			this.handlers	= Hashtable.Synchronized(new Hashtable());
		}

		#endregion

		#region Methods

		public FbConnectionPool FindPool(string connectionString)
		{
			FbConnectionPool pool = null;

			lock (this.SyncObject)
			{
				if (this.pools.ContainsKey(connectionString.GetHashCode()))
				{
					pool = (FbConnectionPool)pools[connectionString.GetHashCode()];
				}
			}

			return pool;
		}

		public FbConnectionPool CreatePool(string connectionString)
		{
			FbConnectionPool pool = null;

			lock (this.SyncObject)
			{
				pool = this.FindPool(connectionString);

				if (pool == null)
				{
					lock (this.pools.SyncRoot)
					{
						int hashcode = connectionString.GetHashCode();

						// Create an empty pool	handler
						EmptyPoolEventHandler handler = new EmptyPoolEventHandler(this.OnEmptyPool);

						this.handlers.Add(hashcode, handler);

						// Create the new connection pool
						pool = new FbConnectionPool(connectionString);

						this.pools.Add(hashcode, pool);

						pool.EmptyPool += handler;
					}
				}
			}

			return pool;
		}

		public void ClearAllPools()
		{
			lock (this.SyncObject)
			{
				lock (this.pools.SyncRoot)
				{
					FbConnectionPool[] tempPools = new FbConnectionPool[this.pools.Count];

					this.pools.Values.CopyTo(tempPools, 0);

					foreach (FbConnectionPool pool in tempPools)
					{
						// Clear pool
						pool.Clear();
					}

					// Clear Hashtables
					this.pools.Clear();
					this.handlers.Clear();
				}
			}
		}

		public void ClearPool(string connectionString)
		{
			lock (this.SyncObject)
			{
				lock (this.pools.SyncRoot)
				{
					int hashCode = connectionString.GetHashCode();

					if (this.pools.ContainsKey(hashCode))
					{
						FbConnectionPool pool = (FbConnectionPool)this.pools[hashCode];

						// Clear pool
						pool.Clear();
					}
				}
			}
		}

		#endregion

		#region Private	Methods

		private void OnEmptyPool(object sender, EventArgs e)
		{
			lock (this.pools.SyncRoot)
			{
				int hashCode = (int)sender;

				if (this.pools.ContainsKey(hashCode))
				{
					FbConnectionPool pool = (FbConnectionPool)this.pools[hashCode];
					EmptyPoolEventHandler handler = (EmptyPoolEventHandler)this.handlers[hashCode];

					pool.EmptyPool -= handler;

					this.pools.Remove(hashCode);
					this.handlers.Remove(hashCode);

					pool = null;
					handler = null;
				}
			}
		}

		#endregion
	}

	internal delegate void EmptyPoolEventHandler(object sender, EventArgs e);

	internal class FbConnectionPool : MarshalByRefObject
	{
		#region Types
		private enum MoveType {LockedToUnlocked, UnlockedToLocked}
		#endregion

		#region Fields

		private FbConnectionString	options;
		private ArrayList			locked;
		private ArrayList			unlocked;
		private Thread				cleanUpThread;
		private string				connectionString;
		private bool				isRunning;
		private long				lifeTime;
		private	object				syncObject;

		#endregion

		#region Events

		public event EmptyPoolEventHandler EmptyPool;

		#endregion

		#region Properties

		public int Count
		{
			get 
			{
				lock (this.unlocked.SyncRoot)
				{
					return this.unlocked.Count + this.locked.Count;
				} 
			}
		}

		public bool HasUnlocked
		{
			get { return this.unlocked.Count > 0; }			
		}

        public object SyncObject
        {
            get
            {
                if (this.syncObject == null)
                {
                    Interlocked.CompareExchange(ref this.syncObject, new object(), null);
                }

                return this.syncObject;
            }
        }

		#endregion

		#region Constructors

		public FbConnectionPool(string connectionString)
		{
			this.connectionString	= connectionString;
			this.options			= new FbConnectionString(connectionString);
			this.lifeTime			= this.options.ConnectionLifeTime * TimeSpan.TicksPerSecond;

			if (this.options.MaxPoolSize == 0)
			{
				this.locked = ArrayList.Synchronized(new ArrayList());
				this.unlocked = ArrayList.Synchronized(new ArrayList());
			}
			else
			{
				this.locked = ArrayList.Synchronized(new ArrayList(this.options.MaxPoolSize));
				this.unlocked = ArrayList.Synchronized(new ArrayList(this.options.MaxPoolSize));
			}

			// If a	minimun	number of connections is requested
			// initialize the pool
			this.Initialize();

			// Start the cleanup thread	only if	needed
			if (this.lifeTime != 0)
			{
				this.isRunning = true;

				this.cleanUpThread = new Thread(new ThreadStart(this.RunCleanup));
				this.cleanUpThread.Name = "Cleanup Thread";
				this.cleanUpThread.Start();
				this.cleanUpThread.IsBackground = true;
			}
		}

		#endregion

		#region Methods

		private void MoveConnection(FbConnectionInternal connection, MoveType moveType)
		{
            if (null == connection)
            {
                return;
            }

			lock (this.unlocked.SyncRoot)
			{
				switch (moveType)
				{
					case MoveType.LockedToUnlocked:
						this.locked.Remove(connection);
						this.unlocked.Add(connection);
						break;

					case MoveType.UnlockedToLocked:
						this.unlocked.Remove(connection);
						this.locked.Add(connection);
						break;
				}
			}
		}

		public void CheckIn(FbConnectionInternal connection)
		{
			connection.OwningConnection = null;
			connection.Created = System.DateTime.Now.Ticks;

			this.MoveConnection(connection, MoveType.LockedToUnlocked);
		}

		public FbConnectionInternal CheckOut()
		{
			FbConnectionInternal newConnection = null;

			lock (this.SyncObject)
			{
				// 1. Try to get a connection from the unlocked connection list
				newConnection = this.GetConnection();
                if (newConnection != null)
                {
                    return newConnection;
                }

				// 2. Check if we have reached the max number of allowed connections
				this.CheckMaxPoolSize();

				// 3. Try to get a connection from the unlocked connection list
				newConnection = this.GetConnection();
                if (newConnection != null)
                {
                    return newConnection;
                }

				// 4. In any other case create a new connection
				newConnection = this.Create();

				// Set connection pooling settings to the new connection
				newConnection.Lifetime = this.options.ConnectionLifeTime;
				newConnection.Pooled = true;

				// Added to	the	locked connections list.
				this.locked.Add(newConnection);
			}

			return newConnection;
		}

		public void Clear()
		{
			lock (this.SyncObject)
			{
				// Stop	cleanup	thread
				if (this.cleanUpThread != null)
				{
					this.cleanUpThread.Abort();
					this.cleanUpThread.Join();
				}

				// Close all unlocked connections
				FbConnectionInternal[] list = (FbConnectionInternal[]) this.unlocked.ToArray(typeof(FbConnectionInternal));

				foreach (FbConnectionInternal connection in list)
				{
					connection.Disconnect();
				}

				// Close all locked	connections
				list = (FbConnectionInternal[])this.locked.ToArray(typeof(FbConnectionInternal));

				foreach (FbConnectionInternal connection in list)
				{
					connection.Disconnect();
				}

				// Clear lists
				this.unlocked.Clear();
				this.locked.Clear();

				// Raise EmptyPool event
				if (this.EmptyPool != null)
				{
					this.EmptyPool(this.connectionString.GetHashCode(), null);
				}

				// Reset fields
				this.unlocked			= null;
				this.locked				= null;
				this.connectionString	= null;
				this.cleanUpThread		= null;
				this.EmptyPool			= null;
			}
		}

		#endregion

		#region Private	Methods

        private void Initialize()
        {
            lock (this.SyncObject)
            {
                for (int i = 0; i < this.options.MinPoolSize; i++)
                {
                    this.unlocked.Add(this.Create());
                }
            }
        }

		private bool CheckMinPoolSize()
		{
			return !(this.options.MinPoolSize > 0 && this.Count <= this.options.MinPoolSize);
		}

		private void CheckMaxPoolSize()
		{
			if (this.options.MaxPoolSize > 0 && this.Count >= this.options.MaxPoolSize)
			{
				long timeout = this.options.ConnectionTimeout * TimeSpan.TicksPerSecond;
				long start = DateTime.Now.Ticks;

				/* 
				 Loop brakes without errors in next situations:
					1. connection was returned from locked to unlocked by calling CheckIn in other thread (HasUnlocked = true) 
					2. connection was moved from locked to unlocked (by Checkin) and then cleaned (removed from unlocked by Cleanup)
				*/
				while (true)
				{
                    if (this.Count >= this.options.MaxPoolSize && this.HasUnlocked == false)
                    {
                        if ((DateTime.Now.Ticks - start) > timeout)
                        {
                            throw new SystemException("Timeout exceeded.");
                        }

                        Thread.Sleep(100);
                    }
                    else
                    {
                        break;
                    }
				}
			}
		}

		private FbConnectionInternal Create()
		{
			FbConnectionInternal connection = new FbConnectionInternal(this.options);
			connection.Connect();

			connection.Pooled = true;
			connection.Created = DateTime.Now.Ticks;

			return connection;
		}

		private FbConnectionInternal GetConnection()
		{
			FbConnectionInternal result = null;
			long check = -1;

			lock (this.unlocked.SyncRoot)
			{
				FbConnectionInternal[] connections = (FbConnectionInternal[]) this.unlocked.ToArray(typeof(FbConnectionInternal));

				for (int i = connections.Length - 1; i >= 0; i--) 
				{
					if (connections[i].Verify())
					{
						if (this.lifeTime != 0)
						{
							long now    = DateTime.Now.Ticks;
							long expire = connections[i].Created + this.lifeTime;

							if (now >= expire)
							{
								if (this.CheckMinPoolSize())
								{
									this.unlocked.Remove(connections[i]);
									this.Expire(connections[i]);
								}
							}
							else
							{
								if (expire > check)
								{
									check = expire;
									result = connections[i];
								}
							}
						}
						else
						{
							result = connections[i];
							break;
						}
					}
					else
					{
						this.unlocked.Remove(connections[i]);
						this.Expire(connections[i]);
					}
				}

                if (result != null)
                {
                    this.MoveConnection(result, MoveType.UnlockedToLocked);
                }
			}

			return result;
		}

		private void RunCleanup()
		{
			int interval = Convert.ToInt32(TimeSpan.FromTicks(this.lifeTime).TotalMilliseconds);

			if (interval > 60000)
			{
				interval = 60000;
			}

			try
			{
				while (this.isRunning)
				{
					Thread.Sleep(interval);

					this.Cleanup();

					if (this.Count == 0)
					{
						lock (this.SyncObject)
						{
							// Empty pool
							if (this.EmptyPool != null)
							{
								this.EmptyPool(this.connectionString.GetHashCode(), null);
							}

							// Stop	running
							this.isRunning = false;
						}
					}
				}
			}
			catch (ThreadAbortException)
			{
				this.isRunning = false;
			}
		}

		private void Expire(FbConnectionInternal connection)
		{
            try
            {
                if (connection.Verify())
                {
                    connection.Disconnect();
                }
            }
            catch (Exception)
            {
                // Do not raise an exception as the connection could be invalid due to several reasons
                // ( network problems, server shutdown, ... )
            }
        }

		private void Cleanup()
		{
			lock (this.unlocked.SyncRoot)
			{
				if (this.unlocked.Count > 0 && this.lifeTime != 0)
				{
					FbConnectionInternal[] list = (FbConnectionInternal[])this.unlocked.ToArray(typeof(FbConnectionInternal));

					foreach (FbConnectionInternal connection in list)
					{
						long now = DateTime.Now.Ticks;
						long expire = connection.Created + this.lifeTime;

						if (now >= expire)
						{
							if (this.CheckMinPoolSize())
							{
								this.unlocked.Remove(connection);
								this.Expire(connection);
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
