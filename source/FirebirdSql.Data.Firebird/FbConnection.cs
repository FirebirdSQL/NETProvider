/*
 *	Firebird ADO.NET Data provider for .NET	and	Mono 
 * 
 *	   The contents	of this	file are subject to	the	Initial	
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this	file except	in compliance with the 
 *	   License.	You	may	obtain a copy of the License at	
 *	   http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *	   Software	distributed	under the License is distributed on	
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied.	See	the	License	for	the	specific 
 *	   language	governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002,	2004 Carlos	Guzman Alvarez
 *	All	Rights Reserved.
 */

using System;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.Firebird.DbSchema;

namespace FirebirdSql.Data.Firebird
{	
	///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/overview/*'/>
	[ToolboxItem(true),
	ToolboxBitmap(typeof(FbConnection),	"Resources.FbConnection.bmp"),	  
	DefaultEvent("InfoMessage")]
	public sealed class	FbConnection : Component, IDbConnection, ICloneable
	{	
		#region	Events

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/event[@name="StateChange"]/*'/>
		public event StateChangeEventHandler StateChange;

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/event[@name="InfoMessage"]/*'/>
		public event FbInfoMessageEventHandler InfoMessage;
		
		#endregion

		#region	Fields

		private	string				connectionString;
		private	AttachmentParams	parameters;
		private	FbDbConnection		dbConnection;
		private	ConnectionState		state;
		private	bool				disposed;
		private	FbTransaction		activeTransaction;
		private	ArrayList			activeCommands;

		private	WarningMessageEventHandler dbWarningHandler;

		#endregion
		
		#region	Properties

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ConnectionString"]/*'/>
		[Category("Data"), 
		RecommendedAsConfigurableAttribute(true),
		RefreshProperties(RefreshProperties.All),
		DefaultValue("")]
		#if	(!MONO)
		[Editor(typeof(Design.FbConnectionStringUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
		#endif
		public string ConnectionString
		{
			get	{ return this.connectionString;	}
			set
			{
				lock (this)
				{
					if (this.state == ConnectionState.Closed)
					{
						this.connectionString =	value;
						this.parameters.ConnectionString = value;
					}
				}
			}
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ConnectionTimeout"]/*'/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int ConnectionTimeout
		{
			get	{ return this.parameters.Timeout; }
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="Database"]/*'/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Database
		{
			get	{ return this.parameters.Database; }
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="DataSource"]/*'/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string DataSource
		{
			get	{ return this.parameters.DataSource; }
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ServerVersion"]/*'/>
		[Browsable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ServerVersion
		{
			get
			{
				if (this.state == ConnectionState.Closed)
				{
					throw new InvalidOperationException("The connection	is closed.");
				}

				if (this.dbConnection != null)
				{
					return this.dbConnection.DB.ServerVersion;
				}

				return String.Empty;
			}
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="State"]/*'/>
		[Browsable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ConnectionState State
		{
			get	{ return this.state; }
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="PacketSize"]/*'/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PacketSize
		{
			get	{ return this.parameters.PacketSize; }
		}
				
		#endregion		

		#region	Internal Properties

		internal FactoryBase Factory
		{
			get	{ return this.dbConnection.DB.Factory; }
		}

		internal FbDbConnection	DbConnection
		{
			get	{ return this.dbConnection;	}
		}

		internal IDbAttachment IscDb
		{
			get	{ return this.dbConnection.DB; }
		}

		internal FbTransaction ActiveTransaction
		{
			get	{ return this.activeTransaction; }
			set	{ this.activeTransaction = value; }
		}

		internal ArrayList ActiveCommands
		{
			get	{ return this.activeCommands; }
		}

		internal AttachmentParams Parameters
		{
			get	{ return this.parameters; }
		}

		#endregion

		#region	Constructors

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/constructor[@name="ctor"]/*'/>
		public FbConnection() :	base()
		{
			this.parameters	= new AttachmentParams();
			this.state		= ConnectionState.Closed;

			GC.SuppressFinalize(this);
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/constructor[@name="ctor(System.String)"]/*'/>	
		public FbConnection(string connectionString) : this()
		{
			this.ConnectionString =	connectionString;
		}		

		#endregion

		#region	IDisposable	Methods

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="Dispose(System.Boolean)"]/*'/>
		protected override void	Dispose(bool disposing)
		{
			lock (this)
			{
				if (!this.disposed)
				{
					try
					{	
						if (disposing)
						{
							// release any managed resources
							this.Close();
							this.dbConnection =	null;
						}

						// release any unmanaged resources

						this.disposed =	true;
					}
					catch
					{
					}
					finally
					{
						base.Dispose(disposing);
					}
				}
			}
		}

		#endregion

		#region	ICloneable Methods

		object ICloneable.Clone()
		{
			return new FbConnection(this.ConnectionString);
		}

		#endregion

		#region	Static Methods

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="ClearAllPools"]/*'/>
		public static void ClearAllPools()
		{
			FbPoolManager manager =	FbPoolManager.Instance;

			manager.ClearAllPools();
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="ClearPool(FbConnection)"]/*'/>
		public static void ClearPool(FbConnection connection)
		{
			FbPoolManager manager =	FbPoolManager.Instance;

			manager.ClearPool(connection.ConnectionString);
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="CreateDatabase(Hashtable)"]/*'/>
		public static void CreateDatabase(Hashtable	values)
		{
			bool			overwrite	= false;
			int				index		= 0;
			byte			dialect		= 3;
			FactoryBase		factory		= null;
			IDbAttachment	db			= null;
			int				serverType	= 0;

			if (!values.ContainsKey("User")		||
				!values.ContainsKey("Password")	||
				!values.ContainsKey("Database"))
			{
				throw new ArgumentException("CreateDatabase	requires a user	name, password and database	path.");
			}

			if (values.ContainsKey("ServerType"))
			{
				serverType = Convert.ToInt32(values["ServerType"], CultureInfo.InvariantCulture.NumberFormat);
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
				dialect	= Convert.ToByte(values["Dialect"],	CultureInfo.InvariantCulture.NumberFormat);
			}

			if (dialect	< 1	|| dialect > 3)
			{
				throw new ArgumentException("Incorrect database	dialect	it should be 1,	2, or 3.");
			}

			if (values.ContainsKey("Overwrite"))
			{
				overwrite =	(bool)values["Overwrite"];
			}

			factory	= ClientFactory.GetInstance(serverType);
			db		= null;

			try	
			{
				// Configure Attachment
				AttachmentParams p = new AttachmentParams();

				p.DataSource	= values["DataSource"].ToString();
				p.Port			= Convert.ToInt32(values["Port"], CultureInfo.InvariantCulture.NumberFormat);
				p.Database		= values["Database"].ToString();
				p.UserName		= values["User"].ToString();
				p.UserPassword	= values["Password"].ToString();
			
				// DPB configuration
				DpbBuffer dpb =	new	DpbBuffer();
				
				// Dpb version
				dpb.Append(IscCodes.isc_dpb_version1);

				// Dummy packet	interval
				dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, 
					new	byte[] {120, 10, 0,	0});

				// User	name
				dpb.Append(IscCodes.isc_dpb_user_name, 
					values["User"].ToString());

				// User	password
				dpb.Append(IscCodes.isc_dpb_password, 
					values["Password"].ToString());

				// Database	dialect
				dpb.Append(IscCodes.isc_dpb_sql_dialect, 
					new	byte[] {dialect, 0,	0, 0});

				// Database overwrite
				dpb.Append(IscCodes.isc_dpb_overwrite, (short)(overwrite ? 1 : 0));

				// Page	Size
				if (values.ContainsKey("PageSize"))
				{
					dpb.Append(IscCodes.isc_dpb_page_size, Convert.ToInt32(values["PageSize"], CultureInfo.InvariantCulture.NumberFormat));
				}

				// Forced writes
				if (values.ContainsKey("ForcedWrite"))
				{
					dpb.Append(IscCodes.isc_dpb_force_write, 
						(short)((bool)values["ForcedWrite"]	? 1	: 0));
				}

				// Character set
				if (values.ContainsKey("Charset"))
				{
					index =	Charset.SupportedCharsets.IndexOf(values["Charset"].ToString());

					if (index == -1)
					{
						throw new ArgumentException("Character set is not valid.");
					}
					else
					{
						dpb.Append(
							IscCodes.isc_dpb_set_db_charset, 
							Charset.SupportedCharsets[index].Name);
					}
				}

				if (!overwrite)
				{
					// Check if	the	database exists
					try
					{
						IDbAttachment connect =	factory.CreateDbConnection(p);
						connect.Attach();
						connect.Detach();

						IscException ex	= new IscException(IscCodes.isc_db_or_file_exists);

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
								
				db = factory.CreateDbConnection(p);
				db.CreateDatabase(p, dpb);
			}
			catch (IscException	ex)	
			{
				throw new FbException(ex.Message, ex);
			}
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="DropDatabase(Hashtable)"]/*'/>
		public static void DropDatabase(Hashtable values)
		{
			FactoryBase		factory		= null;
			IDbAttachment	db			= null;
			int				serverType	= 0;

			if (!values.ContainsKey("User")		||
				!values.ContainsKey("Password")	||
				!values.ContainsKey("Database"))
			{
				throw new ArgumentException("CreateDatabase	requires a user	name, password and database	path.");
			}

			if (!values.ContainsKey("DataSource"))
			{
				values.Add("DataSource", "localhost");
			}

			if (!values.ContainsKey("Port"))
			{
				values.Add("Port", 3050);
			}

			if (values.ContainsKey("ServerType"))
			{
				serverType = Convert.ToInt32(values["ServerType"], CultureInfo.InvariantCulture.NumberFormat);
			}

			try	
			{
				// Configure Attachment
				AttachmentParams p = new AttachmentParams();

				p.DataSource	= values["DataSource"].ToString();
				p.Port			= Convert.ToInt32(values["Port"], CultureInfo.InvariantCulture.NumberFormat);
				p.Database		= values["Database"].ToString();
				p.UserName		= values["User"].ToString();
				p.UserPassword	= values["Password"].ToString();

				// Drop	the	database			
				factory	= ClientFactory.GetInstance(serverType);
				db		= factory.CreateDbConnection(p);
							
				db.Attach();
				db.DropDatabase();
			}
			catch (IscException	ex)	
			{
				throw new FbException(ex.Message, ex);
			}
		}

		#endregion

		#region	Methods

		IDbTransaction IDbConnection.BeginTransaction()
		{
			return this.BeginTransaction();
		}

		IDbTransaction IDbConnection.BeginTransaction(IsolationLevel level)
		{
			return this.BeginTransaction(level);
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction"]/*'/>
		public FbTransaction BeginTransaction()
		{
			lock (this)
			{
				if (this.state == ConnectionState.Closed)
				{
					throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
				}

				if (this.activeTransaction != null && 
					!this.activeTransaction.IsUpdated)
				{
					throw new InvalidOperationException("A transaction is currently	active.	Parallel transactions are not supported.");
				}
			
				try
				{
					this.activeTransaction = new FbTransaction(this);
					this.activeTransaction.BeginTransaction();				 
				}
				catch(IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}

			return this.activeTransaction;
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(System.String)"]/*'/>
		public FbTransaction BeginTransaction(string transactionName)
		{
			lock (this)
			{
				if (this.state == ConnectionState.Closed)
				{
					throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
				}

				if (this.activeTransaction != null && 
					!this.activeTransaction.IsUpdated)
				{
					throw new InvalidOperationException("A transaction is currently	active.	Parallel transactions are not supported.");
				}
			
				try
				{
					this.activeTransaction = new FbTransaction(this);
					this.activeTransaction.BeginTransaction();
					this.activeTransaction.Save(transactionName);
				}
				catch(IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}

			return this.activeTransaction;
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(System.Data.IsolationLevel)"]/*'/>
		public FbTransaction BeginTransaction(IsolationLevel level)
		{
			lock (this)
			{
				if (this.state == ConnectionState.Closed)
				{
					throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
				}

				if (this.activeTransaction != null && 
					!this.activeTransaction.IsUpdated)
				{
					throw new InvalidOperationException("A transaction is currently	active.	Parallel transactions are not supported.");
				}

				try
				{
					this.activeTransaction = new FbTransaction(this, level);
					this.activeTransaction.BeginTransaction();
				}
				catch(IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}

			return this.activeTransaction;			
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(System.Data.IsolationLevel,System.String)"]/*'/>
		public FbTransaction BeginTransaction(
			IsolationLevel	level, 
			string			transactionName)
		{
			lock (this)
			{
				if (this.state == ConnectionState.Closed)
				{
					throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
				}

				if (this.activeTransaction != null && 
					!activeTransaction.IsUpdated)
				{
					throw new InvalidOperationException("A transaction is currently	active.	Parallel transactions are not supported.");
				}

				try
				{
					this.activeTransaction = new FbTransaction(this, level);
					this.activeTransaction.BeginTransaction();
					this.activeTransaction.Save(transactionName);
				}
				catch(IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}

			return this.activeTransaction;			
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(FbTransactionOptions)"]/*'/>
		public FbTransaction BeginTransaction(FbTransactionOptions options)
		{
			lock (this)
			{
				if (this.state == ConnectionState.Closed)
				{
					throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
				}

				if (this.activeTransaction != null && 
					!activeTransaction.IsUpdated)
				{
					throw new InvalidOperationException("A transaction is currently	active.	Parallel transactions are not supported.");
				}

				try
				{
					this.activeTransaction = new FbTransaction(
						this, 
						IsolationLevel.Unspecified);
					this.activeTransaction.BeginTransaction(options);
				}
				catch(IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}

			return this.activeTransaction;
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(FbTransactionOptions, System.String)"]/*'/>
		public FbTransaction BeginTransaction(
			FbTransactionOptions	options, 
			string					transactionName)
		{
			lock (this)
			{
				if (this.state == ConnectionState.Closed)
				{
					throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
				}

				if (this.activeTransaction != null && 
					!activeTransaction.IsUpdated)
				{
					throw new InvalidOperationException("A transaction is currently	active.	Parallel transactions are not supported.");
				}

				try
				{
					this.activeTransaction = new FbTransaction(
						this, 
						IsolationLevel.Unspecified);
					this.activeTransaction.BeginTransaction(options);
					this.activeTransaction.Save(transactionName);
				}
				catch(IscException ex)
				{
					throw new FbException(ex.Message, ex);
				}
			}

			return this.activeTransaction;			
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="ChangeDatabase(System.String)"]/*'/>
		public void	ChangeDatabase(string db)
		{
			lock (this)
			{
				if (this.state == ConnectionState.Closed)
				{
					throw new InvalidOperationException("ChangeDatabase	requires an	open and available Connection.");
				}

				if (db == null || db.Trim().Length == 0)
				{
					throw new InvalidOperationException("Database name is not valid.");
				}

				string oldDb = this.dbConnection.Parameters.Database;

				try
				{
					/* Close current connection	*/
					this.Close();

					/* Set up the new Database	*/
					this.dbConnection.Parameters.Database =	db;

					/* Open	new	connection	*/
					this.Open();
				}
				catch (IscException	ex)
				{
					this.dbConnection.Parameters.Database =	oldDb;
					throw new FbException(ex.Message, ex);
				}
			}
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="Open"]/*'/>
		public void	Open()
		{
			lock (this)
			{
				if (this.connectionString == null ||
					this.connectionString.Length ==	0)
				{
					throw new InvalidOperationException("Connection	String is not initialized.");
				}
				if (this.state != ConnectionState.Closed &&	
					this.state != ConnectionState.Connecting)
				{
					throw new InvalidOperationException("Connection	already	Open.");
				}

				try
				{
					this.state = ConnectionState.Connecting;

					if (this.parameters.Pooling)
					{
						// Use Connection Pooling
						FbConnectionPool pool = FbPoolManager.Instance.CreatePool(this.parameters);

						// Grab	a connection from the pool
						this.dbConnection = pool.CheckOut();
					}
					else
					{
						// Do not use Connection Pooling
						this.dbConnection = new FbDbConnection(this.parameters);

						this.dbConnection.Pooled = false;
						this.dbConnection.Connect();
					}

					this.dbWarningHandler = new WarningMessageEventHandler(OnDbWarningMessage);
					this.dbConnection.DB.DbWarningMessage += this.dbWarningHandler;

					this.state = ConnectionState.Open;
					if (this.StateChange != null)
					{
						this.StateChange(
							this,
							new StateChangeEventArgs(
								ConnectionState.Closed, state));
					}

					this.activeCommands = new ArrayList();
				}
				catch (IscException ex)
				{
					this.state = ConnectionState.Closed;
					throw new FbException(ex.Message, ex);
				}
				catch (Exception)
				{ 
					this.state = ConnectionState.Closed;
					throw;
				}
			}
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="Close"]/*'/>
		public void	Close()
		{
			lock (this)
			{
				if (this.state == ConnectionState.Open)
				{
					try
					{		
						lock (this.dbConnection)
						{
							// Unbind Warning messages event
							this.dbConnection.DB.DbWarningMessage -= dbWarningHandler;

							// Dispose Transaction
							if (this.activeTransaction != null)
							{
								this.activeTransaction.Dispose();
								this.activeTransaction = null;
							}						

							// Dispose all active statemenets
							this.disposeActiveCommands();

							// Close connection	or send	it back	to the pool
							if (this.dbConnection.Pooled)
							{
								// Get Connection Pool
								FbConnectionPool pool =	
									FbPoolManager.Instance.FindPool(this.connectionString);

								// Send	connection to the Pool
								pool.CheckIn(this.dbConnection);
							}
							else
							{
								this.dbConnection.Disconnect();
							}
						}

						// Update state
						this.state = ConnectionState.Closed;

						// Raise event
						if (this.StateChange !=	null)
						{
							this.StateChange(
								this, 
								new	StateChangeEventArgs(
									ConnectionState.Open, state));
						}
					}
					catch(IscException ex)
					{
						throw new FbException(ex.Message, ex);
					}
				}
			}
		}

		IDbCommand IDbConnection.CreateCommand()
		{
			return this.CreateCommand();
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="CreateCommand"]/*'/>
		public FbCommand CreateCommand()
		{
			FbCommand command =	new	FbCommand();

			lock (this)
			{
				command.Connection = this;
			}
	
			return command;
		}

		#endregion

		#region	Database Schema

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetSchema"]/*'/>
		public DataTable GetSchema()
		{
			if (this.state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("The conneciton	is closed.");
			}

			return this.GetSchema("MetaDataCollections");
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetSchema(System.String)"]/*'/>
		public DataTable GetSchema(string collectionName)
		{
			if (this.state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("The conneciton	is closed.");
			}

			return this.GetSchema(collectionName, null);
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetSchema(System.String,	System.String[])"]/*'/>
		public DataTable GetSchema(string collectionName, string[] restrictions)
		{
			if (this.state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("The conneciton	is closed.");
			}

			return FbDbSchemaFactory.GetSchema(this, collectionName, restrictions);
		}

		///	<include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetDbSchemaTable(System.String, Systemn.Object[])"]/*'/>
		[Obsolete]
		public DataTable GetDbSchemaTable(
			FbDbSchemaType	schema,	
			object[]		restrictions)
		{
			if (this.state == ConnectionState.Closed)
			{
				throw new InvalidOperationException("The conneciton	is closed.");
			}

			return FbDbSchemaFactory.GetSchema(this, schema.ToString(),	restrictions);
		}

		#endregion

		#region	Private	Methods

		private	void disposeActiveCommands()
		{
			if (this.activeCommands	!= null)
			{
				if (this.activeCommands.Count >	0)
				{
					foreach	(FbCommand command in activeCommands)
					{
						// Rollback	implicit transaction
						command.RollbackImplicitTransaction();

						// Release statement handle
						command.Release();

						// command.Transaction = null;
						// command.Connection	= null;
					}
				}

				this.activeCommands.Clear();
				this.activeCommands	= null;				
			}
		}

		private	void OnDbWarningMessage(
			object					sender,	
			WarningMessageEventArgs	e)
		{
			if (this.InfoMessage !=	null)
			{
				this.InfoMessage(
					this, new FbInfoMessageEventArgs(e.Exception));
			}
		}

		#endregion
	}
}
