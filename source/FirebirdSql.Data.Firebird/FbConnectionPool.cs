/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
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
        #region � Static Fields �

        private static readonly FbPoolManager instance = new FbPoolManager();

        #endregion

		#region � Static Properties �

		public static FbPoolManager Instance
		{
			get { return FbPoolManager.instance; }
		}

		#endregion

		#region Fields

		private Hashtable	pools;
		private Hashtable	handlers;
		private object		syncObject;

		#endregion

		#region Constructors

		private FbPoolManager()
		{
			this.pools		= Hashtable.Synchronized(new Hashtable());
			this.handlers	= Hashtable.Synchronized(new Hashtable());
			this.syncObject	= new object();
		}

		#endregion

		#region Methods

		public FbConnectionPool FindPool(string connectionString)
		{
			FbConnectionPool pool = null;

			lock (this.syncObject)
			{
				if (this.pools.ContainsKey(connectionString.GetHashCode()))
				{
					pool = (FbConnectionPool)pools[connectionString.GetHashCode()];
				}
			}

			return pool;
		}

		public FbConnectionPool CreatePool(AttachmentParams parameters)
		{
			FbConnectionPool pool = null;

			lock (this.syncObject)
			{
				pool = this.FindPool(parameters.ConnectionString);

				if (pool == null)
				{
					lock (this.pools.SyncRoot)
					{
						int hashcode = parameters.ConnectionString.GetHashCode();

						// Create an empty pool handler
						EmptyPoolEventHandler handler = new EmptyPoolEventHandler(this.emptyPool);

						this.handlers.Add(hashcode, handler);

						// Create the new connection pool
						pool = new FbConnectionPool(parameters);

						this.pools.Add(hashcode, pool);

						pool.EmptyPool += handler;
					}
				}
			}

			return pool;
		}

		public void ClearAllPools()
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

		public void ClearPool(string connectionString)
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

		#endregion

		#region Private Methods

		private void emptyPool(object sender, EventArgs e)
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

					pool	= null;
					handler = null;
				}
			}
		}

		#endregion
	}
	
	internal delegate void EmptyPoolEventHandler(object sender, EventArgs e);

	internal class FbConnectionPool : MarshalByRefObject
	{
		#region Fields

		private ArrayList			locked;
		private ArrayList			unlocked;
		private Thread				cleanUpThread;
		private string				connectionString;
		private AttachmentParams	parameters;
		private bool				isRunning;
        private long                lifeTime;
		private object				syncObject;

		#endregion

		#region Events

		public event EmptyPoolEventHandler EmptyPool;

		#endregion

		#region Properties

		public int Count
		{
			get { return this.unlocked.Count + this.locked.Count; }
		}

		#endregion

		#region Constructors

		public FbConnectionPool(AttachmentParams parameters)
		{
			this.syncObject			= new object();
			this.parameters			= parameters;
			this.connectionString	= parameters.ConnectionString;
			this.lifeTime			= parameters.LifeTime * TimeSpan.TicksPerSecond;

			if (this.parameters.MaxPoolSize == 0)
			{
				this.locked		= ArrayList.Synchronized(new ArrayList());
				this.unlocked	= ArrayList.Synchronized(new ArrayList());
			}
			else
			{
				this.locked		= ArrayList.Synchronized(new ArrayList(this.parameters.MaxPoolSize));
				this.unlocked	= ArrayList.Synchronized(new ArrayList(this.parameters.MaxPoolSize));
			}

			// If a minimun number of connections is requested
			// initialize the pool
			this.initialize();

			// Start the cleanup thread only if needed
			if (this.lifeTime != 0)
			{
                this.isRunning = true;

				this.cleanUpThread = new Thread(new ThreadStart(this.runCleanUp));
				this.cleanUpThread.Name	= "CleanUp Thread";			
				this.cleanUpThread.Start();
				this.cleanUpThread.IsBackground = true;
			}
		}

		#endregion

		#region Methods

		public void CheckIn(FbDbConnection connection)
		{
			lock (this.syncObject)
			{
				connection.Created = System.DateTime.Now.Ticks;

				this.unlocked.Add(connection);
				this.locked.Remove(connection);
			}
		}

		public FbDbConnection CheckOut()
		{
			FbDbConnection newConnection = null;

			lock (this.syncObject)
			{
				lock (this.unlocked.SyncRoot)
				{
					newConnection = this.getConnection();

					if (newConnection != null)
					{
						return newConnection;
					}
				}

				newConnection = this.create();

				if ((this.Count + 1) < this.parameters.MaxPoolSize ||
					this.parameters.MaxPoolSize == 0)
				{
					// Set the connection as pooled
					newConnection.Pooled = true;

					// Added the new connection to the locked list
					this.locked.Add(newConnection);
				}
				else
				{
					newConnection.Pooled = false;
				}
			}

			return newConnection;
		}	

		public void Clear()
		{
			lock (this.syncObject)
			{
				// Stop cleanup thread
				if (this.cleanUpThread != null)
				{
					this.cleanUpThread.Abort();
					this.cleanUpThread.Join();
				}

				// Close all unlocked connections
				FbDbConnection[] list = (FbDbConnection[])this.unlocked.ToArray(typeof(FbDbConnection));

				foreach (FbDbConnection connection in list)
				{
					connection.Disconnect();
				}

				// Close all locked connections
				list = (FbDbConnection[])this.locked.ToArray(typeof(FbDbConnection));

				foreach (FbDbConnection connection in list)
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

		#region Private Methods

		private bool checkMinPoolSize()
		{
			if (this.parameters.MinPoolSize > 0 && this.Count == this.parameters.MinPoolSize)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		private void runCleanUp()
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

					this.cleanUp(null);
				
					if (this.Count == 0)
					{
						lock (this.syncObject)
						{
							// Empty pool
							if (this.EmptyPool != null)
							{
								this.EmptyPool(this.connectionString.GetHashCode(), null);
							}

							// Stop running
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

		private void initialize()
		{
			lock (this.syncObject)
			{
				for (int i = 0; i < this.parameters.MinPoolSize; i++)
				{
					this.unlocked.Add(this.create());
				}
			}
		}

		private FbDbConnection create()
		{
			try 
			{
				FbDbConnection connection = new FbDbConnection(this.parameters);
				connection.Connect();

				connection.Pooled	= true;
				connection.Created	= System.DateTime.Now.Ticks;

				return connection;
			}
			catch (Exception ex) 
			{
				throw ex;
			}
		}

		private FbDbConnection getConnection()
		{
			FbDbConnection[] list = (FbDbConnection[])this.unlocked.ToArray(typeof(FbDbConnection));
			FbDbConnection result = null;
			long check = -1;

			Array.Reverse(list);

			foreach (FbDbConnection connection in list)
			{
				if (this.validate(connection))
				{
					if (this.lifeTime != 0)
					{
						long now	= DateTime.Now.Ticks;
						long expire = connection.Created + this.lifeTime;

						if (now >= expire)
						{
							if (this.checkMinPoolSize())
							{
								this.unlocked.Remove(connection);
								this.expire(connection);
							}
						}
						else
						{
							if (expire > check)
							{
								check = expire;
								result = connection;
							}
						}
					}
					else
					{
						result = connection;
						break;
					}
				}
				else
				{
					this.unlocked.Remove(connection);
					this.expire(connection);
				}
			}

			if (result != null)
			{
				this.unlocked.Remove(result);
				this.locked.Add(result);
			}

			return result;
		}

		private bool validate(FbDbConnection connection)
		{
			bool isValid = false;

			try 
			{								
				isValid = connection.Verify();
			}
			catch
			{
			}

			return isValid;
		}

		private void expire(FbDbConnection connection)
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
				throw new FbException("Error closing database connection.");
			}
		}
		
		private void cleanUp(object State)
		{
			lock (this.unlocked.SyncRoot)
			{
				if (this.unlocked.Count > 0)
				{
					FbDbConnection[] list = (FbDbConnection[])this.unlocked.ToArray(typeof(FbDbConnection));

					foreach (FbDbConnection connection in list)
					{
						long now	= DateTime.Now.Ticks;
						long expire = connection.Created + this.lifeTime;

						if (now >= expire)
						{
							if (this.checkMinPoolSize())
							{
								this.unlocked.Remove(connection);
								this.expire(connection);
							}
						}
					}
				}
			}
		}

		/*
		private void DebugPool(string format, params object[] args)
		{
			StringBuilder b = new StringBuilder();

			b.AppendFormat(format, args);

			Debug.WriteLine(b.ToString());
		}
		*/

		#endregion
	}
}
