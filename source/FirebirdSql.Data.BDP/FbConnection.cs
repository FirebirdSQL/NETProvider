/*
 *  Firebird BDP - Borland Data provider Firebird
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
 *  Copyright (c) 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

using Borland.Data.Common;
using Borland.Data.Schema;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Bdp
{
	public class FbConnection : ISQLConnection
	{
		#region Fields

		private IDatabase		db;
		private string			connectionOptions;
		private Hashtable		transactions;
		private Hashtable		connectionProps;
		private IscException	lastError;
		private int				internalTransactionId;
		private int				transactionId;

		#endregion

		#region Internal Properties

		internal IDatabase Database
		{
			get { return this.db; }
		}

		internal Hashtable Transactions
		{
			get 
			{
				if (this.transactions == null)
				{
					this.transactions = Hashtable.Synchronized(new Hashtable());
				}
				return this.transactions; 
			}
		}

		internal Hashtable ConnectionProps
		{
			get 
			{
				if (this.connectionProps == null)
				{
					this.connectionProps = Hashtable.Synchronized(new Hashtable());
				}
				
				return this.connectionProps; 
			}
		}

		#endregion
		
		#region Constructors

		public FbConnection()
		{
			this.db				= null;
			this.transactionId	= -1;
		}

		#endregion

		#region ISQLConnection Methods

		public int Connect(
			string database, 
			string user,
			string password,
			string hostName)
		{
			int		port		= 3050;
			string	dataSource	= hostName;
			string	dbPath		= database;

			try
			{
				string connectionString =
					String.Format(
					"{0}={1};{2}={3};{4}={5};{6}={7};{8}",
					"DataSource", hostName,
					"Database", database,
					"User", user,
					"Password", password,
					this.connectionOptions);

				FbConnectionString cs = new FbConnectionString();
				cs.ConnectionString = connectionString;

				Regex r = new Regex(@"(?<datasource>.*)/(?<port>[0-9]*):(?<database>.*)", RegexOptions.ExplicitCapture);

				Match m = r.Match(database);

				if (m != null)
				{
					if (m.Groups["datasource"].Success)
					{
						dataSource = m.Groups["datasource"].Value;
					}

					if (m.Groups["port"].Success)
					{
						port = Int32.Parse(m.Groups["port"].Value);
					}
				
					if (m.Groups["database"].Success)
					{
						dbPath = m.Groups["database"].Value;
					}
				}

				//  Create database instance
				this.db = ClientFactory.CreateDatabase(cs.ServerType);

				// Build DPB
				DatabaseParameterBuffer dpb = this.db.CreateDatabaseParameterBuffer();

				dpb.Append(IscCodes.isc_dpb_version1);
				dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, 
					new byte[] {120, 10, 0, 0});
				dpb.Append(IscCodes.isc_dpb_sql_dialect, 
					new byte[] {Convert.ToByte(cs.Dialect), 0, 0, 0});
				dpb.Append(IscCodes.isc_dpb_lc_ctype, cs.Charset);
				if (cs.RoleName != null)
				{
					if (cs.RoleName.Length > 0)
					{
						dpb.Append(IscCodes.isc_dpb_sql_role_name, cs.RoleName);
					}
				}
				dpb.Append(IscCodes.isc_dpb_user_name, user);
				dpb.Append(IscCodes.isc_dpb_password, password);

				// Perform attach
				this.db.Attach(dpb, dataSource, port, dbPath);
			}
			catch (IscException e)
			{
				this.lastError = e;
			}

			return this.GetErrorCode();
		}

		public int Disconnect()
		{
			try
			{
                if (this.Transactions.Count > 0)
                {
                    IDictionaryEnumerator e = this.Transactions.GetEnumerator();

                    while (e.MoveNext())
                    {
                        try
                        {
                            // Rollback active transactions
                            ITransaction transaction = (ITransaction)e.Value;
                            transaction.Rollback();
                        }
                        finally
                        {
                        }
                    }
                }

                this.db.Detach();
				
				this.Transactions.Clear();
				this.internalTransactionId	= -1;
				this.transactionId			= -1;
				this.lastError				= null;
				this.db						= null;
			}
			catch (IscException e)
			{
				this.lastError = e;
			}

			return this.GetErrorCode();
		}

		public int BeginTransaction(int transID, int isolationLevel)
		{
			IsolationLevel isolLevel = (IsolationLevel)isolationLevel;

			try
			{
				ITransaction transaction = this.db.BeginTransaction(this.BuildTpb(isolLevel));                
				this.Transactions.Add(transID, transaction);

				if (transID > 0)
				{
					this.transactionId = transID;
				}
			}
			catch (IscException e)
			{
				this.lastError = e;
			}

			return this.GetErrorCode();
		}

		public int Commit(int transID)
		{
			try
			{
				if (this.Transactions.ContainsKey(transID))
				{
					ITransaction transaction = (ITransaction)this.Transactions[transID];
					if (transaction.State != TransactionState.NoTransaction)
					{
						transaction.Commit();
					}
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
			catch (IscException e)
			{
				this.lastError = e;
			}
			finally
			{
				this.Transactions.Remove(transID);

				if (transID > 0)
				{
					this.transactionId = -1;
				}
			}

			return this.GetErrorCode();
		}

		public int Rollback(int transID)
		{
			try
			{
				if (this.Transactions.ContainsKey(transID))
				{
					ITransaction transaction = (ITransaction)this.Transactions[transID];
					if (transaction.State != TransactionState.NoTransaction)
					{
						transaction.Rollback();
					}
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
			catch (IscException e)
			{
				this.lastError = e;
			}
			finally
			{
				this.Transactions.Remove(transID);

				if (transID > 0)
				{
					this.transactionId = -1;
				}
			}

			return this.GetErrorCode();
		}

		public int ChangeDatabase(
			string	database, 
			string	user, 
			string	password, 
			bool	connected)
		{
			throw new NotSupportedException();
		}

		public int FreeConnect()
		{
			this.lastError				= null;
			this.db						= null;
			this.connectionOptions		= null;
			this.transactions			= null;
			this.connectionProps		= null;
			this.internalTransactionId	= -1;
			this.transactionId			= -1;

			return 0;
		}

		public void GetProperty(ConnectionProps property, out object value)
		{
			value = this.ConnectionProps[property];
		}

		public void SetProperty(ConnectionProps property, object value)
		{
			if (this.ConnectionProps.ContainsKey(property))
			{
				this.ConnectionProps[property] = value;
			}
			else
			{
				this.ConnectionProps.Add(property, value);
			}
		}

		public int SetOptions(string connOptions)
		{
			this.connectionOptions = connOptions;
			
			return 0;
		}

		public ISQLResolver GetResolver()
		{
			return new FbResolver(this);
		}

		public ISQLMetaData GetMetaData()
		{
			return new FbMetaData(this);
		}

		public ISQLCommand GetSQLCommand()
		{
			return new FbCommand(this);
		}

		public int GetErrorMessage(ref StringBuilder errorMessage)
		{
			if (this.lastError != null)
			{
				errorMessage.Append(this.lastError.Message);

				this.lastError = null;
			}

			return 0;
		}

		#endregion

		#region Internal Methods

		internal int GetTransactionId()
		{
			lock (this)
			{
				if (this.transactionId < 0)
				{
					return --this.internalTransactionId;
				}
				else
				{
					return this.transactionId;
				}
			}
		}

		#endregion

		#region Private Methods

		private int GetErrorCode()
		{
			return (this.lastError != null ? this.lastError.ErrorCode : 0);
		}

		private TransactionParameterBuffer BuildTpb(IsolationLevel isolationLevel)
		{
			TransactionParameterBuffer tpb = this.db.CreateTransactionParameterBuffer();

			tpb.Append(IscCodes.isc_tpb_version3);
			tpb.Append(IscCodes.isc_tpb_write);
			tpb.Append(IscCodes.isc_tpb_wait);

			/* Isolation level */
			switch (isolationLevel)
			{
				case IsolationLevel.Serializable:
					tpb.Append(IscCodes.isc_tpb_consistency);
					break;

				case IsolationLevel.RepeatableRead:			
					tpb.Append(IscCodes.isc_tpb_concurrency);
					break;

				case IsolationLevel.ReadUncommitted:
					tpb.Append(IscCodes.isc_tpb_read_committed);
					tpb.Append(IscCodes.isc_tpb_rec_version);
					break;

				case IsolationLevel.ReadCommitted:
				default:					
					tpb.Append(IscCodes.isc_tpb_read_committed);
					tpb.Append(IscCodes.isc_tpb_no_rec_version);
					break;
			}

			return tpb;
		}

		#endregion
	}
}