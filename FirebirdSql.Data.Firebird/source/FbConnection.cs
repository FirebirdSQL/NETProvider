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
using FirebirdSql.Data.Firebird.Gds;
using FirebirdSql.Data.Firebird.DbSchema;

namespace FirebirdSql.Data.Firebird
{	
	/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/overview/*'/>
	[ToolboxItem(true),
	ToolboxBitmap(typeof(FbConnection), "Resources.ToolboxBitmaps.FbConnection.bmp"),    
	DefaultEvent("InfoMessage")]
	public sealed class FbConnection : Component, IDbConnection, ICloneable
	{	
		#region EVENTS

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/event[@name="StateChange"]/*'/>
		public event StateChangeEventHandler StateChange;

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/event[@name="InfoMessage"]/*'/>
		public event FbInfoMessageEventHandler InfoMessage;
		
		#endregion

		#region FIELDS

		private string			connectionString;
		private GdsAttachParams	parameters;
		private FbDbConnection	dbConnection;
		private ConnectionState state;
		private bool			disposed;
		private FbDataReader	dataReader;
		private FbTransaction	activeTransaction;
		private ArrayList		activeCommands;

		private GdsWarningMessageEventHandler dbWarningHandler;

		#endregion
		
		#region PROPERTIES

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ConnectionString"]/*'/>
		[Category("Data"), 
		RecommendedAsConfigurableAttribute(true),
		RefreshProperties(RefreshProperties.All),
		DefaultValue("")]
		#if (!_MONO)
		[Editor(typeof(Design.ConnectionStringUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
		#endif
		public string ConnectionString
		{
			get { return this.connectionString; }
			set
			{ 
				if (this.state == ConnectionState.Closed)
				{
					parameters.ParseConnectionString(value);
					this.connectionString = value;
				}
			}
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ConnectionTimeout"]/*'/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int ConnectionTimeout
		{
			get { return this.parameters.Timeout; }
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="Database"]/*'/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Database
		{
			get { return this.parameters.Database; }
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="DataSource"]/*'/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string DataSource
		{
			get { return this.parameters.DataSource; }
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ServerVersion"]/*'/>
		[Browsable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ServerVersion
		{
			get
			{
				if (this.state == ConnectionState.Closed)
				{
					throw new InvalidOperationException("The connection is closed.");
				}

				FbDatabaseInfo info = new FbDatabaseInfo(this);

				return info.IscVersion;
			}
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="State"]/*'/>
		[Browsable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ConnectionState State
		{
			get { return this.state; }
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="PacketSize"]/*'/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PacketSize
		{
			get { return this.parameters.PacketSize; }
		}
				
		#endregion		

		#region INTERNAL_PROPERTIES

		internal FbDbConnection DbConnection
		{
			get { return this.dbConnection; }
			set { this.dbConnection = value; }
		}

		internal FbDataReader DataReader
		{
			get { return this.dataReader; }
			set { this.dataReader = value; }
		}

		internal FbTransaction ActiveTransaction
		{
			get { return this.activeTransaction; }
			set { this.activeTransaction = value; }
		}

		internal Encoding Encoding
		{
			get { return this.dbConnection.Parameters.Charset.Encoding; }
		}

		internal ArrayList ActiveCommands
		{
			get { return this.activeCommands; }
		}

		internal GdsAttachParams Parameters
		{
			get { return this.parameters; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/constructor[@name="ctor"]/*'/>
		public FbConnection() : base()
		{
			this.parameters	= new GdsAttachParams();
			this.state		= ConnectionState.Closed;

			GC.SuppressFinalize(this);
		}

    	/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/constructor[@name="ctor(System.String)"]/*'/>	
		public FbConnection(string connectionString) : this()
		{
			this.ConnectionString = connectionString;
		}		

		#endregion

		#region DISPOSE_METHODS

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="Dispose(System.Boolean)"]/*'/>
		protected override void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				try
				{	
					if (disposing)
					{
						// release any managed resources
						this.Close();
						this.dbConnection = null;
					}

					// release any unmanaged resources					
				}
				finally
				{
					base.Dispose(disposing);
				}

				this.disposed = true;
			}			
		}

		#endregion

		#region ICLONEABLE_METHODS

		object ICloneable.Clone()
		{
			return new FbConnection(this.ConnectionString);
		}

		#endregion

		#region STATIC_METHODS

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="CreateDatabase(System.Collections.Hashtable)"]/*'/>
		public static void CreateDatabase(Hashtable values)
		{
			bool	overwrite	= false;
			int		index		= 0;
			byte	dialect		= 3;

			if (!values.ContainsKey("User")		||
				!values.ContainsKey("Password") ||
				!values.ContainsKey("Database"))
			{
				throw new ArgumentException("values has no valid keys.");
			}
			
			if (!values.ContainsKey("DataSource"))
			{
				values.Add("DataSource", "localhost");
			}
			
			if (!values.ContainsKey("Port"))
			{
				values.Add("Port", 3050);
			}
			
			if (values.ContainsKey("Dialect"))
			{
				dialect = Convert.ToByte(values["Dialect"]);
			}
			
			if (values.ContainsKey("Overwrite"))
			{
				overwrite = Convert.ToBoolean(values["Overwrite"]);
			}

			if (dialect < 1 || dialect > 3)
			{
				throw new ArgumentException("Incorrect dialect.");
			}

			try 
			{
				// Configure Attachment
				GdsAttachParams p = new GdsAttachParams();

				p.DataSource	= values["DataSource"].ToString();
				p.Port			= Convert.ToInt32(values["Port"]);
				p.Dialect		= dialect;
				p.Database		= values["Database"].ToString();
				p.UserName		= values["User"].ToString();
				p.UserPassword	= values["Password"].ToString();
			
				// DPB configuration
				GdsDpbBuffer dpb = new GdsDpbBuffer();
				
				// Dummy packet interval
				dpb.Append(GdsCodes.isc_dpb_dummy_packet_interval, 
					new byte[] {120, 10, 0, 0});

				// User name
				dpb.Append(GdsCodes.isc_dpb_user_name, 
					values["User"].ToString());

				// User password
				dpb.Append(GdsCodes.isc_dpb_password, 
					values["Password"].ToString());

				// Database dialect
				dpb.Append(GdsCodes.isc_dpb_sql_dialect, 
						new byte[] {dialect, 0, 0, 0});

				// Page Size
				if (values.ContainsKey("PageSize"))
				{
					dpb.Append(GdsCodes.isc_dpb_page_size, 
						Convert.ToInt32(values["PageSize"]));
				}

				// Forced writes
				if (values.ContainsKey("ForcedWrite"))
				{
					dpb.Append(GdsCodes.isc_dpb_force_write, 
						(short)(Convert.ToBoolean(values["ForcedWrite"]) ? 1 : 0));
				}

				// Character set
				if (values.ContainsKey("Charset"))
				{
					index = GdsDbAttachment.CharSets.IndexOf(values["Charset"].ToString());

					if (index == -1)
					{
						throw new ArgumentException("Incorrect Character set.");
					}
					else
					{
						dpb.Append(
							GdsCodes.isc_dpb_set_db_charset, 
							GdsDbAttachment.CharSets[index].Name);
					}
				}

				if (!overwrite)
				{
					// Check if the database exists
					try
					{
						GdsDbAttachment connect = new GdsDbAttachment(p);
						connect.Attach();
						connect.Detach();

						GdsException ex = new GdsException(GdsCodes.isc_db_or_file_exists);

						throw new FbException(ex.Message, ex);
					}
					catch(FbException ex)
					{
						throw ex;
					}
					catch (Exception)
					{
					}
				}

				GdsDbAttachment db = new GdsDbAttachment(p);
												
				db.CreateDatabase(p, dpb);
				db.Detach();
			}
			catch (GdsException ex) 
			{
				throw new FbException(ex.Message, ex);
			}
		}

		#endregion

		#region METHODS

		IDbTransaction IDbConnection.BeginTransaction()
		{
			return this.BeginTransaction();
		}

		IDbTransaction IDbConnection.BeginTransaction(IsolationLevel level)
		{
			return this.BeginTransaction(level);
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction"]/*'/>
		public FbTransaction BeginTransaction()
		{
			if (this.state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
			}

			if (this.activeTransaction != null && 
				!this.activeTransaction.IsUpdated)
			{
				throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
			}

			if (this.DataReader != null)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection. The connection's current state is Open, Fetching.");
			}
			
			try
			{
				this.activeTransaction = new FbTransaction(this);
				this.activeTransaction.BeginTransaction();				 
			}
			catch(GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			return this.activeTransaction;
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(System.String)"]/*'/>
		public FbTransaction BeginTransaction(string transactionName)
		{
			if (this.state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
			}

			if (this.activeTransaction != null && 
				!this.activeTransaction.IsUpdated)
			{
				throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
			}

			if (this.DataReader != null)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection. The connection's current state is Open, Fetching.");
			}
			
			try
			{
				this.activeTransaction = new FbTransaction(this);
				this.activeTransaction.BeginTransaction();
				this.activeTransaction.Save(transactionName);
			}
			catch(GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			return this.activeTransaction;
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(System.Data.IsolationLevel)"]/*'/>
		public FbTransaction BeginTransaction(IsolationLevel level)
		{
			if (this.state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
			}

			if (this.activeTransaction != null && 
				!this.activeTransaction.IsUpdated)
			{
				throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
			}

			if (this.DataReader != null)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection. The connection's current state is Open, Fetching.");
			}

			try
			{
				this.activeTransaction = new FbTransaction(this, level);
				this.activeTransaction.BeginTransaction();
			}
			catch(GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			return this.activeTransaction;			
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(System.Data.IsolationLevel,System.String)"]/*'/>
		public FbTransaction BeginTransaction(IsolationLevel level, string transactionName)
		{
			if (this.state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
			}

			if (this.activeTransaction != null && 
				!activeTransaction.IsUpdated)
			{
				throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
			}

			if (this.DataReader != null)
			{
				throw new InvalidOperationException("BeginTransaction requires an open and available Connection. The connection's current state is Open, Fetching.");
			}

			try
			{
				this.activeTransaction = new FbTransaction(this, level);
				this.activeTransaction.BeginTransaction();
				this.activeTransaction.Save(transactionName);
			}
			catch(GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			return this.activeTransaction;			
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="ChangeDatabase(System.String)"]/*'/>
		public void ChangeDatabase(string db)
		{
			if (this.state == ConnectionState.Closed)
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

			string oldDb = this.dbConnection.Parameters.Database;

			try
			{
				/* Close current connection	*/
				this.Close();

				/* Set up the new Database	*/
				this.dbConnection.Parameters.Database = db;

				/* Open new connection	*/
				this.Open();
			}
			catch (GdsException ex)
			{
				this.dbConnection.Parameters.Database = oldDb;
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="Open"]/*'/>
		public void Open()
		{
			if (this.connectionString == String.Empty)
			{
				throw new InvalidOperationException("Connection String is not initialized.");
			}
			if (this.state != ConnectionState.Closed && 
				this.state != ConnectionState.Connecting)
			{
				throw new InvalidOperationException("Connection already Open.");
			}

			try
			{
				this.dbConnection = new FbDbConnection(
					this.connectionString, 
					this.parameters);

				this.state = ConnectionState.Connecting;
				
				if (this.dbConnection.Parameters.Pooling)
				{		
					// Use Connection Pooling
					this.dbConnection = FbConnectionPool.GetConnection(
						this.dbConnection.ConnectionString,
						this.dbConnection);
				}
				else
				{
					// Do not use Connection Pooling
					this.dbConnection.Pooled = false;
					this.dbConnection.Connect();
				}

				this.dbWarningHandler = new GdsWarningMessageEventHandler(OnDbWarningMessage);
				this.dbConnection.DB.DbWarningMessage += this.dbWarningHandler;
				
				this.state = ConnectionState.Open;
				if (this.StateChange != null)
				{
					this.StateChange(this, new StateChangeEventArgs(ConnectionState.Closed,state));
				}

				this.activeCommands = new ArrayList();
			}
			catch(GdsException ex)
			{
				this.state = ConnectionState.Closed;
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="Close"]/*'/>
		public void Close()
		{
			if (this.state == ConnectionState.Open)
			{
				try
				{		
					lock (this.dbConnection)
					{
						// Unbind Warning messages event
						this.dbConnection.DB.DbWarningMessage -= dbWarningHandler;

						// Dispose active DataReader if exists
						if (this.dataReader != null &&
							!this.dataReader.IsClosed)
						{
							this.dataReader.Close();
							this.dataReader = null;
						}						

						// Dispose Transaction
						if (this.activeTransaction != null)
						{
							this.activeTransaction.Dispose();
							this.activeTransaction = null;
						}						

						// Dispose all active statemenets
						this.disposeActiveCommands();

						// Stop event thread if running
						this.dbConnection.CancelEvents();

						if (this.dbConnection.Parameters.Pooling)
						{
							// Send connection to the Pool
							FbConnectionPool.FreeConnection(this.dbConnection);
						}
						else
						{
							this.dbConnection.Disconnect();
						}
					}

					// Update state
					this.state = ConnectionState.Closed;

					// Raise event
					if (this.StateChange != null)
					{
						this.StateChange(this, new StateChangeEventArgs(ConnectionState.Open,state));
					}
				}
				catch(GdsException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}
		}

		IDbCommand IDbConnection.CreateCommand()
		{			
			return this.CreateCommand();
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="CreateCommand"]/*'/>
		public FbCommand CreateCommand()
		{		
			FbCommand command	= new FbCommand();
			command.Connection	= this;
	
			return command;
		}

		/// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetDbSchemaTable"]/*'/>
		public DataTable GetDbSchemaTable(FbDbSchemaType schema, object[] restrictions)
		{
			if (DataReader != null)
			{
				throw new InvalidOperationException("GetDbSchemaTable requires an open and available Connection. The connection's current state is Open, Fetching.");
			}

			IDbSchema dbSchema = FbDbSchemaFactory.GetSchema(schema);

			if (dbSchema == null)
			{
				throw new NotSupportedException("Specified schema type is not supported.");
			}
			if (restrictions != null)
			{
				if (restrictions.Length > dbSchema.RestrictionColumns.Count)
				{
					throw new InvalidOperationException("The number of specified restrictions is not valid.");
				}
			}

			return dbSchema.GetDbSchemaTable(this, restrictions);
		}

		#endregion

		#region PRIVATE_METHODS

		private void disposeActiveCommands()
		{
			if (this.activeCommands != null)
			{
				if (this.activeCommands.Count > 0)
				{
					foreach (FbCommand command in activeCommands)
					{
						// Commit implicit transaction
						command.CommitImplicitTransaction();

						// Drop statement handle
						if (command.Statement != null)
						{
							command.Statement.Drop();
							command.Statement = null;
						}
					}
				}

				this.activeCommands.Clear();
				this.activeCommands = null;				
			}
		}

		private void OnDbWarningMessage(object sender, GdsWarningMessageEventArgs e)
		{
			if (this.InfoMessage != null)
			{
				this.InfoMessage(this, new FbInfoMessageEventArgs(e.Exception));
			}
		}

		#endregion
	}
}
