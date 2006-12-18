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

using FirebirdSql.Data.INGDS;
using FirebirdSql.Data.NGDS;

namespace FirebirdSql.Data.Firebird
{
	internal class FbConnectionPool : MarshalByRefObject
	{
		#region FIELDS

		private static ConnectionPool pool = null;

		#endregion
    
		#region METHODS

		public static void Init()
		{
			if (pool ==  null)
			{
				pool = new ConnectionPool();
			}
		}

		public static FbIscConnection GetConnection(string connectionString)
		{
			Init();

			return ((FbIscConnection)pool.CheckOut(connectionString));
		}
    
		public static void FreeConnection(FbIscConnection c) 
		{
			pool.CheckIn(c);
		}

		#endregion
	}
	
	internal class ConnectionPool
	{
		#region FIELDS

		private ArrayList	locked;
		private ArrayList	unlocked;
		private Thread		cleanUpThread;

		#endregion

		#region CONSTRUCTORS

		public ConnectionPool()		
		{			
			locked	 = ArrayList.Synchronized(new ArrayList());
			unlocked = ArrayList.Synchronized(new ArrayList());

			cleanUpThread		= new Thread(new ThreadStart(RunCleanUp));
			cleanUpThread.Name	= "CleanUp Thread";			
			cleanUpThread.Start();
			cleanUpThread.IsBackground = true;
		}

		#endregion

		#region METHODS
		
		public FbIscConnection CheckOut(string connectionString)
		{
			FbIscConnection newConnection	= null;
			long			now				= System.DateTime.Now.Ticks;

			lock (typeof(FbConnectionPool))
			{
				if (unlocked.Count > 0)
				{
					FbIscConnection[] list = new FbIscConnection[unlocked.Count];
					unlocked.CopyTo(0, list, 0, list.Length);

					foreach(FbIscConnection connection in list)
					{
						if (Validate(connection, connectionString))
						{
							if (connection.Lifetime != 0)
							{
								if ((now - connection.Created) > connection.Lifetime)
								{
									unlocked.Remove(connection);
									Expire(connection);
								}
								else
								{
									unlocked.Remove(connection);
									locked.Add(connection);
									
									return(connection);
								}
							}
							else
							{
								unlocked.Remove(connection);
								locked.Add(connection);
								
								return(connection);
							}
						}
						else
						{						
							unlocked.Remove(connection);
							Expire(connection);
						}
					}			
				}

				newConnection			= Create(connectionString);		
				newConnection.Created	= System.DateTime.Now.Ticks;

				locked.Add(newConnection);
			}
			
			return(newConnection);
		}	

		public void CheckIn(FbIscConnection connection)
		{			
			lock (typeof(FbConnectionPool))
			{				
				connection.Created = System.DateTime.Now.Ticks;

				locked.Remove(connection);
				unlocked.Add(connection);
			}
		}

		private void RunCleanUp()
		{		
			TimeSpan interval = new TimeSpan(0, 0, 10);

			while (true)
			{
				CleanUp(null);

				Thread.Sleep(interval);
			}
		}

		private FbIscConnection Create(string connectionString)
		{
			try 
			{
				FbIscConnection connection = new FbIscConnection(connectionString);
				connection.Open();

				return connection;
			}
			catch (Exception ex) 
			{
				throw ex;
			}
		}
    
		private bool Validate(FbIscConnection connection, string connectionString)
		{
			try 
			{								
				return (connection.ConnectionString == connectionString &&
						connection.VerifyAttachedDB());
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private void Expire(FbIscConnection connection)
		{
			try 
			{
				if (connection.VerifyAttachedDB())
				{
					connection.Close();
				}
			}
			catch (Exception)
			{
				throw new FbException("Error closing database connection.");
			}
		}
		
		private void CleanUp(object State)
		{
			long	now = System.DateTime.Now.Ticks;
			
			lock (unlocked.SyncRoot)
			{
				if (unlocked.Count > 0)
				{
					FbIscConnection[] list = new FbIscConnection[unlocked.Count];
				
					unlocked.CopyTo(0, list, 0, list.Length);
					foreach(FbIscConnection connection in list)
					{
						if (connection.Lifetime != 0)
						{
							if ((now - connection.Created) > connection.Lifetime)
							{
								unlocked.Remove(connection);
								Expire(connection);
							}
						}
					}
				}
			}
		}

		#endregion
	}
}