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
using System.Text;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;

using FirebirdSql.Data.INGDS;
using FirebirdSql.Data.NGDS;


namespace FirebirdSql.Data.Firebird
{	
	#region INFO_MESSAGE_EVENT_ARGS

	/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="T:FirebirdSql.Data.Firebird.FbInfoMessageEventArgs"]/*'/>
	public sealed class FbInfoMessageEventArgs : EventArgs
	{
		private FbErrorCollection errors = new FbErrorCollection();
		private string			  message = String.Empty;

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="P:FirebirdSql.Data.Firebird.FbInfoMessageEventArgs.Errors"]/*'/>
		public FbErrorCollection Errors
		{
			get { return errors; }
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="P:FirebirdSql.Data.Firebird.FbInfoMessageEventArgs.Message"]/*'/>
		public string Message
		{
			get { return message; }
		}

		internal FbInfoMessageEventArgs(GDSException ex)
		{
			this.message = ex.Message;
			
			foreach (GDSError error in ex.Errors)
			{
				errors.Add(error.Message, error.ErrorCode);
			}
		}
	}

	#endregion

	/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="D:FirebirdSql.Data.Firebird.FbInfoMessageEventHandler"]/*'/>
	public delegate void FbInfoMessageEventHandler(object sender, FbInfoMessageEventArgs e);
	
	/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="T:FbConnection"]/*'/>
	#if (!_MONO)
	[ToolboxBitmap(typeof(FbConnection), "Resources.ToolboxBitmaps.FbConnection.bmp")]	
	#endif
	[DefaultEvent("InfoMessage")]
	public sealed class FbConnection : Component, IDbConnection, ICloneable
	{	
		#region EVENTS

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="E:FirebirdSql.Data.Firebird.FbConnection.StateChange"]/*'/>
		public event StateChangeEventHandler StateChange;

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="E:FirebirdSql.Data.Firebird.FbConnection.InfoMessage"]/*'/>
		public event FbInfoMessageEventHandler InfoMessage;
		
		#endregion

		#region FIELDS

		private FbIscConnection iscConnection;
		private ConnectionState state;
		private bool			disposed;
		private string			connectionString;
		private string			database;
		private FbDataReader	dataReader;
		private Encoding		encoding;

		private FbTransaction	activeTxn;

		private ArrayList		activeCommands;

		private DbWarningMessageEventHandler dbWarningngHandler;

		#endregion
		
		#region PROPERTIES

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="P:ConnectionString"]/*'/>
		public string ConnectionString
		{
			get { return connectionString; }
			set
			{ 
				if (state == ConnectionState.Closed)
				{
					FbIscConnection tmpConn = new FbIscConnection(value);					
					
					connectionString = value;
					
					tmpConn = null;
				}
			}
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="P:ConnectionTimeout"]/*'/>
		public int ConnectionTimeout
		{
			get 
			{ 
				if (iscConnection != null)
				{
					return iscConnection.ConnectionTimeout;
				}
				else
				{
					return 15; 
				}
			}
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="P:Database"]/*'/>
		public string Database
		{
			get { return database; }
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="P:DataSource"]/*'/>
		public string DataSource
		{
			get 
			{ 
				if (iscConnection != null)
				{
					return iscConnection.DataSource;
				}
				else
				{
					return String.Empty; 
				}
			}
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="P:ServerVersion"]/*'/>
		public string ServerVersion
		{
			get
			{
				if (this.State == ConnectionState.Closed)
				{
					throw new InvalidOperationException("The connection is closed.");
				}

				return IscConnection.DatabaseInfo.FirebirdVersion;
			}
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="P:State"]/*'/>
		public ConnectionState State
		{
			get { return state; }
		}

		internal ArrayList ActiveCommands
		{
			get { return activeCommands; }
		}
		
		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="P:Encoding"]/*'/>
		internal Encoding Encoding
		{
			get { return encoding; }
		}
		
		internal FbDataReader DataReader
		{
			get { return dataReader; }
			set { dataReader = value; }
		}

		internal FbIscConnection IscConnection
		{
			get { return iscConnection; }
			set { iscConnection = value; }
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="P:PacketSize"]/*'/>
		public int PacketSize
		{
			get 
			{ 
				if (iscConnection != null)
				{
					return iscConnection.PacketSize;
				}
				else
				{
					return 8192; 
				}
			}
		}

		#endregion		

		#region CONSTRUCTORS

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public FbConnection()
		{
			state				= ConnectionState.Closed;
			disposed			= false;
			connectionString	= String.Empty;
			database			= String.Empty;
			dataReader			= null;
			encoding			= Encoding.Default;
			activeTxn			= null;
			dbWarningngHandler	= null;
		}
    		
		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:#ctor(System.String)"]/*'/>
		public FbConnection(string connString) : this()
		{			
			this.ConnectionString	= connString;
		}		

		#endregion

		#region DESTRUCTORS

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:Dispose(System.Boolean)"]/*'/>
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				try
				{	
					if (disposing)
					{
						// release any managed resources
						Close();

						connectionString	= null;
						database			= null;
						encoding			= null;
					}

					// release any unmanaged resources
				}
				finally
				{
					base.Dispose(disposing);
				}

				disposed = true;
			}			
		}

		#endregion

		#region ICLONEABLE_METHODS

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:ICloneable#Clone"]/*'/>
		object ICloneable.Clone()
		{
			return new FbConnection(ConnectionString);
		}

		#endregion

		#region STATIC_METHODS

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:CreateDabase(System.String, System.Int32, System.String, System.String, System.String, System.Byte, System.Int16, System.String)"]/*'/>
		public static void CreateDatabase(string dataSource, int port, string database, string user, string password, byte dialect, bool forceWrite, short pageSize, string charset)
		{			
			IGDS					gds	= GDSFactory.NewGDS();
			isc_db_handle_impl		db	= null;
			FbConnectionRequestInfo	cri	= new FbConnectionRequestInfo();
				
			db	= (isc_db_handle_impl)gds.get_new_isc_db_handle();
			try 
			{
				// New instance for Database handler
				db	= (isc_db_handle_impl)gds.get_new_isc_db_handle();
			
				// DPB configuration
				cri.SetProperty(GdsCodes.isc_dpb_dummy_packet_interval, 
					new byte[] {120, 10, 0, 0});
				cri.SetProperty(GdsCodes.isc_dpb_sql_dialect, 
					new byte[] {dialect, 0, 0, 0});
				cri.SetUser(user);
				cri.SetPassword(password);
				cri.SetProperty(GdsCodes.isc_dpb_page_size, pageSize);
				cri.SetProperty(GdsCodes.isc_dpb_force_write, (short)(forceWrite ? 1 : 0));
				cri.SetProperty(GdsCodes.isc_dpb_lc_ctype, charset);
												
				gds.isc_create_database(dataSource + "/" + port.ToString() + ":" + database, db, cri.Dpb);
				gds.isc_detach_database(db);
			}
			catch (Exception ex) 
			{
				throw ex;
			}
		}

		#endregion

		#region METHODS

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:BeginTransaction"]/*'/>
		IDbTransaction IDbConnection.BeginTransaction()
		{
			return BeginTransaction();
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:BeginTransaction(System.Data.IsolationLevel)"]/*'/>
		IDbTransaction IDbConnection.BeginTransaction(IsolationLevel level)
		{
			return BeginTransaction(level);
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:BeginTransaction"]/*'/>
		public FbTransaction BeginTransaction()
		{
			if (state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
			}

			if (activeTxn != null && !activeTxn.IsUpdated)
			{
				throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
			}

			if (DataReader != null)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection. The connection's current state is Open, Fetching.");
			}
			
			try
			{
				activeTxn = new FbTransaction(this);
				activeTxn.BeginTransaction();				 
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			return this.activeTxn;
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:BeginTransaction(System.String)"]/*'/>
		public FbTransaction BeginTransaction(string transactionName)
		{
			if (state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
			}

			if (activeTxn != null && !activeTxn.IsUpdated)
			{
				throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
			}

			if (DataReader != null)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection. The connection's current state is Open, Fetching.");
			}
			
			try
			{
				activeTxn = new FbTransaction(this);
				activeTxn.BeginTransaction();
				activeTxn.Save(transactionName);
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			return this.activeTxn;
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:BeginTransaction(System.Data.IsolationLevel)"]/*'/>
		public FbTransaction BeginTransaction(IsolationLevel level)
		{
			if (state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
			}

			if (activeTxn != null && !activeTxn.IsUpdated)
			{
				throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
			}

			if (DataReader != null)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection. The connection's current state is Open, Fetching.");
			}

			try
			{
				activeTxn = new FbTransaction(this, level);
				activeTxn.BeginTransaction();
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			return this.activeTxn;			
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:BeginTransaction(System.Data.IsolationLevel,System.String)"]/*'/>
		public FbTransaction BeginTransaction(IsolationLevel level, string transactionName)
		{
			if (state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
			}

			if (activeTxn != null && !activeTxn.IsUpdated)
			{
				throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
			}

			if (DataReader != null)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection. The connection's current state is Open, Fetching.");
			}

			try
			{
				activeTxn = new FbTransaction(this, level);
				activeTxn.BeginTransaction();
				activeTxn.Save(transactionName);
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			return this.activeTxn;			
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:ChangeDatabase"]/*'/>
		public void ChangeDatabase(string db)
		{
			if (state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("ChangeDatabase requires an open and available Connection.");
			}

			if (db == null || db.Trim().Length == 0)
			{
				throw new InvalidOperationException("Database name is not valid.");
			}

			if (this.DataReader != null)
			{
				throw new InvalidOperationException("ChangeDatabase requires an open and available Connection. The connection's current state is Open, Fetching.");
			}

			string oldDb = database;

			try
			{
				/* Close current connection	*/
				Close();

				/* Set up the new Database	*/
				database = db;

				/* Open new connection	*/
				Open();
			}
			catch (FbException ex)
			{
				database = oldDb;
				throw ex;
			}
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:Open"]/*'/>
		public void Open()
		{
			if (state != ConnectionState.Closed)
			{
				throw new InvalidOperationException("Connection already Open.");
			}

			try
			{
				state = ConnectionState.Connecting;

				iscConnection = new FbIscConnection(connectionString);

				if (iscConnection.Pooling)
				{		
					// Use Connection Pooling
					iscConnection = FbConnectionPool.GetConnection(connectionString);
				}
				else
				{
					// Do not use Connection Pooling
					iscConnection.Pooled = false;
					iscConnection.Open();
				}

				dbWarningngHandler = new DbWarningMessageEventHandler(OnDbWarningMessage);
				iscConnection.db.DbWarningMessage += dbWarningngHandler;
				
				// Set up the encoding field
				encoding = Encodings.GetFromFirebirdEncoding(iscConnection.Charset);

				state = ConnectionState.Open;
				
				if (StateChange != null)
				{
					StateChange(this, new StateChangeEventArgs(ConnectionState.Closed,state));
				}

				activeCommands = new ArrayList();
			}
			catch(GDSException ex)
			{
				state = ConnectionState.Closed;
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:Close"]/*'/>
		public void Close()
		{
			if (state == ConnectionState.Open)
			{
				try
				{		
					lock (iscConnection)
					{
						// Unbind Warning messages event
						iscConnection.db.DbWarningMessage -= dbWarningngHandler;

						// Dispose active DataReader if exists

						if (dataReader != null)
						{
							dataReader.Dispose();
						}

						// Dispose all active statemenets
						DisposeActiveCommands();

						if (activeTxn != null)
						{
							// Dispose Transaction
							activeTxn.Dispose();
							activeTxn = null;
						}

						if (iscConnection.Pooling)
						{
							IscConnection.ClearWarnings();
							FbConnectionPool.FreeConnection(IscConnection);
						}
						else
						{	
							IscConnection.Close();
							IscConnection = null;
						}
					}

					// Update state
					state = ConnectionState.Closed;

					// Raise event
					if (StateChange != null)
					{
						StateChange(this, new StateChangeEventArgs(ConnectionState.Open,state));
					}
				}
				catch(GDSException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:CreateCommand"]/*'/>
		IDbCommand IDbConnection.CreateCommand()
		{			
			return CreateCommand();
		}

		/// <include file='xmldoc/fbconnection.xml' path='doc/member[@name="M:CreateCommand"]/*'/>
		public FbCommand CreateCommand()
		{		
			FbCommand command = new FbCommand();

			command.Connection = this;
	
			return command;
		}

		private void DisposeActiveCommands()
		{
			if (activeCommands != null)
			{
				if (activeCommands.Count > 0)
				{
					FbCommand[] commands = new FbCommand[activeCommands.Count];

					activeCommands.CopyTo(0, commands, 0, commands.Length);
					foreach (FbCommand command in commands)
					{
						command.Dispose();					
					}
					
					commands = null;
				}

				activeCommands.Clear();
				activeCommands = null;				
			}
		}

		private void OnDbWarningMessage(object sender, DbWarningMessageEventArgs e)
		{
			if (InfoMessage != null)
			{
				InfoMessage(this, new FbInfoMessageEventArgs(e.Exception));
			}
		}

		#endregion
	}
}
