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
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 *
 * Contributors:
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
    /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/overview/*'/>
#if	(!NETCF)
    [DefaultEvent("InfoMessage")]
#endif
    public sealed class FbConnection : DbConnection, ICloneable
    {
        #region · Static Properties ·

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ConnectionPoolsCount"]/*'/>
        public static int ConnectionPoolsCount
        {
            get { return FbPoolManager.Instance.PoolsCount; }
        }

        #endregion

        #region · Static Pool Handling Methods ·

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetPooledConnectionCount(FbConnection)"]/*'/>
        public static int GetPooledConnectionCount(FbConnection connection)
        {
            return FbPoolManager.Instance.GetPooledConnectionCount(connection.ConnectionString);
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="ClearAllPools"]/*'/>
        public static void ClearAllPools()
        {
            FbPoolManager.Instance.ClearAllPools();
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="ClearPool(FbConnection)"]/*'/>
        public static void ClearPool(FbConnection connection)
        {
            FbPoolManager.Instance.ClearPool(connection.ConnectionString);
        }

        #endregion

        #region · Static Database Creation/Drop methods ·

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="CreateDatabase(System.String)"]/*'/>
        public static void CreateDatabase(string connectionString)
        {
            FbConnection.CreateDatabase(connectionString, 4096, true, false);
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="CreateDatabase(System.String, System.Boolean)"]/*'/>
        public static void CreateDatabase(string connectionString, bool overwrite)
        {
            FbConnection.CreateDatabase(connectionString, 4096, true, overwrite);
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="CreateDatabase(System.String,System.Int32,System.Boolean,System.Boolean)"]/*'/>
        public static void CreateDatabase(string connectionString, int pageSize, bool forcedWrites, bool overwrite)
        {
            FbConnectionString options = new FbConnectionString(connectionString);
            options.Validate();

            try
            {
                // DPB configuration
                DatabaseParameterBuffer dpb = new DatabaseParameterBuffer();

                // Dpb version
                dpb.Append(IscCodes.isc_dpb_version1);

                // Dummy packet	interval
                dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });

                // User	name
                dpb.Append(IscCodes.isc_dpb_user_name, options.UserID);

                // User	password
                dpb.Append(IscCodes.isc_dpb_password, options.Password);

                // Database	dialect
                dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { options.Dialect, 0, 0, 0 });

                // Character set
                if (options.Charset.Length > 0)
                {
                    Charset charset = Charset.GetCharset(options.Charset);

                    if (charset == null)
                    {
                        throw new ArgumentException("Character set is not valid.");
                    }
                    else
                    {
                        dpb.Append(IscCodes.isc_dpb_set_db_charset, charset.Name);
                    }
                }

                // Forced writes
                dpb.Append(IscCodes.isc_dpb_force_write, (short)(forcedWrites ? 1 : 0));

                // Database overwrite
                dpb.Append(IscCodes.isc_dpb_overwrite, (overwrite ? 1 : 0));

                // Page	Size
                if (pageSize > 0)
                {
                    dpb.Append(IscCodes.isc_dpb_page_size, pageSize);
                }

                // Create the new database
                FbConnectionInternal db = new FbConnectionInternal(options);
                db.CreateDatabase(dpb);
            }
            catch (IscException ex)
            {
                throw new FbException(ex.Message, ex);
            }
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="DropDatabase(System.String)"]/*'/>
        public static void DropDatabase(string connectionString)
        {
            // Configure Attachment
            FbConnectionString options = new FbConnectionString(connectionString);
            options.Validate();

            try
            {
                // Drop	the	database	
                FbConnectionInternal db = new FbConnectionInternal(options);
                db.DropDatabase();
            }
            catch (IscException ex)
            {
                throw new FbException(ex.Message, ex);
            }
        }

        #endregion

        #region · Events ·

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/event[@name="StateChange"]/*'/>
        public override event StateChangeEventHandler StateChange;

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/event[@name="InfoMessage"]/*'/>
        public event FbInfoMessageEventHandler InfoMessage;

        #endregion

        #region · Fields ·

        private FbConnectionInternal innerConnection;
        private ConnectionState state;
        private FbConnectionString options;
        private bool disposed;
        private string connectionString;

        #endregion

        #region · Properties ·

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ConnectionString"]/*'/>
#if	(NET)
        [Category("Data")]
        [SettingsBindable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [DefaultValue("")]
#endif
        public override string ConnectionString
        {
            get { return this.connectionString; }
            set
            {
                lock (this)
                {
                    if (this.state == ConnectionState.Closed)
                    {
                        if (value == null)
                        {
                            value = "";
                        }

                        this.options.Load(value);
                        this.options.Validate();
                        this.connectionString = value;
                    }
                }
            }
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ConnectionTimeout"]/*'/>
#if	(!NETCF)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public override int ConnectionTimeout
        {
            get { return this.options.ConnectionTimeout; }
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="Database"]/*'/>
#if	(!NETCF)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public override string Database
        {
            get { return this.options.Database; }
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="DataSource"]/*'/>
#if	(!NETCF)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public override string DataSource
        {
            get { return this.options.DataSource; }
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="ServerVersion"]/*'/>
#if	(!NETCF)
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public override string ServerVersion
        {
            get
            {
                if (this.state == ConnectionState.Closed)
                {
                    throw new InvalidOperationException("The connection is closed.");
                }

                if (this.innerConnection != null)
                {
                    return this.innerConnection.Database.ServerVersion;
                }

                return String.Empty;
            }
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="State"]/*'/>
#if	(!NETCF)
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public override ConnectionState State
        {
            get { return this.state; }
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/property[@name="PacketSize"]/*'/>
#if	(!NETCF)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public int PacketSize
        {
            get { return this.options.PacketSize; }
        }

        #endregion

        #region · Internal Properties ·

        internal FbConnectionInternal InnerConnection
        {
            get { return this.innerConnection; }
        }

        internal FbConnectionString ConnectionOptions
        {
            get { return this.options; }
        }

        internal bool IsClosed
        {
            get { return this.state == ConnectionState.Closed; }
        }

        #endregion

        #region · Constructors ·

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/constructor[@name="ctor"]/*'/>
        public FbConnection()
            : this(null)
        {
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/constructor[@name="ctor(System.String)"]/*'/>	
        public FbConnection(string connectionString)
            : base()
        {
            this.options = new FbConnectionString();
            this.state = ConnectionState.Closed;
            this.connectionString = "";

            if (connectionString != null)
            {
                this.ConnectionString = connectionString;
            }
        }

        #endregion

        #region · IDisposable methods ·

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="Dispose(System.Boolean)"]/*'/>
        protected override void Dispose(bool disposing)
        {
            lock (this)
            {
                if (!this.disposed)
                {
                    try
                    {
                        // release any unmanaged resources
                        this.Close();

                        if (disposing)
                        {
                            // release any managed resources
                            this.innerConnection = null;
                            this.options = null;
                            this.connectionString = null;
                        }

                        this.disposed = true;
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

        #region · ICloneable Methods ·

        object ICloneable.Clone()
        {
            return new FbConnection(this.ConnectionString);
        }

        #endregion

        #region · Transaction Handling Methods ·

        public new FbTransaction BeginTransaction()
        {
            return this.BeginTransaction(IsolationLevel.ReadCommitted, null);
        }

        public new FbTransaction BeginTransaction(IsolationLevel level)
        {
            return this.BeginTransaction(level, null);
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(System.String)"]/*'/>
        public FbTransaction BeginTransaction(string transactionName)
        {
            return this.BeginTransaction(IsolationLevel.ReadCommitted, transactionName);
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(System.Data.IsolationLevel,System.String)"]/*'/>
        public FbTransaction BeginTransaction(IsolationLevel level, string transactionName)
        {
            if (this.IsClosed)
            {
                throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
            }

            return this.innerConnection.BeginTransaction(level, transactionName);
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(FbTransactionOptions)"]/*'/>
        public FbTransaction BeginTransaction(FbTransactionOptions options)
        {
            return this.BeginTransaction(options, null);
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="BeginTransaction(FbTransactionOptions, System.String)"]/*'/>
        public FbTransaction BeginTransaction(FbTransactionOptions options, string transactionName)
        {
            if (this.IsClosed)
            {
                throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
            }

            return this.innerConnection.BeginTransaction(options, transactionName);
        }

        #endregion

        #region · Transaction Enlistement ·

#if (NET)

        public override void EnlistTransaction(System.Transactions.Transaction transaction)
        {
            if (this.State != ConnectionState.Open)
            {
                throw new Exception();
            }

            this.innerConnection.EnlistTransaction(transaction);
        }

#endif

        #endregion

        #region · DbConnection methods ·

        protected override DbCommand CreateDbCommand()
        {
            return new FbCommand(null, this);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return this.BeginTransaction(isolationLevel);
        }

        #endregion

        #region · Database Schema Methods ·

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetSchema"]/*'/>
        public override DataTable GetSchema()
        {
            return this.GetSchema("MetaDataCollections");
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetSchema(System.String)"]/*'/>
        public override DataTable GetSchema(string collectionName)
        {
            return this.GetSchema(collectionName, null);
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="GetSchema(System.String, System.String[])"]/*'/>
        public override DataTable GetSchema(string collectionName, string[] restrictions)
        {
            if (this.IsClosed)
            {
                throw new InvalidOperationException("The connection is closed.");
            }

            return this.innerConnection.GetSchema(collectionName, restrictions);
        }

        #endregion

        #region · Methods ·

        public new FbCommand CreateCommand()
        {
            return (FbCommand)this.CreateDbCommand();
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="ChangeDatabase(System.String)"]/*'/>
        public override void ChangeDatabase(string db)
        {
#if (!NETCF)
            lock (this)
            {
                if (this.IsClosed)
                {
                    throw new InvalidOperationException("ChangeDatabase requires an open and available Connection.");
                }

                if (db == null || db.Trim().Length == 0)
                {
                    throw new InvalidOperationException("Database name is not valid.");
                }

                string cs = this.connectionString;

                try
                {
                    FbConnectionStringBuilder csb = new FbConnectionStringBuilder(this.connectionString);

                    /* Close current connection	*/
                    this.Close();

                    /* Set up the new Database	*/
                    csb.Database = db;

                    /* Open	new	connection	*/
                    this.Open();
                }
                catch (IscException ex)
                {
                    this.ConnectionString = cs;
                    throw new FbException(ex.Message, ex);
                }
            }
#endif
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="Open"]/*'/>
        public override void Open()
        {
            lock (this)
            {
                if (this.connectionString == null || this.connectionString.Length == 0)
                {
                    throw new InvalidOperationException("Connection String is not initialized.");
                }
                if (!this.IsClosed && this.state != ConnectionState.Connecting)
                {
                    throw new InvalidOperationException("Connection already Open.");
                }

                this.DemandPermission();

                try
                {
                    this.OnStateChange(this.state, ConnectionState.Connecting);

                    if (this.options.Pooling)
                    {
                        this.innerConnection = FbPoolManager.Instance.GetPool(this.connectionString).CheckOut();
                        this.innerConnection.OwningConnection = this;
                    }
                    else
                    {
                        // Do not use Connection Pooling
                        this.innerConnection = new FbConnectionInternal(this.options, this);
                        this.innerConnection.Connect();
                    }

                    // Enlist the transaction
                    this.innerConnection.EnlistTransaction(System.Transactions.Transaction.Current);

                    // Bind	Warning	messages event
                    this.innerConnection.Database.WarningMessage = new WarningMessageCallback(this.OnWarningMessage);

                    // Update the connection state
                    this.OnStateChange(this.state, ConnectionState.Open);
                }
                catch (IscException ex)
                {
                    this.OnStateChange(this.state, ConnectionState.Closed);
                    throw new FbException(ex.Message, ex);
                }
                catch (Exception)
                {
                    this.OnStateChange(this.state, ConnectionState.Closed);
                    throw;
                }
            }
        }

        /// <include file='Doc/en_EN/FbConnection.xml' path='doc/class[@name="FbConnection"]/method[@name="Close"]/*'/>
        public override void Close()
        {
            if (!this.IsClosed && this.innerConnection != null)
            {
                lock (this)
                {
                    try
                    {
                        lock (this.innerConnection)
                        {
                            // Close the Remote	Event Manager
                            this.innerConnection.CloseEventManager();

                            // Unbind Warning messages event
                            if (this.innerConnection.Database != null)
                            {
                                this.innerConnection.Database.WarningMessage = null;
                            }

                            // Dispose Transaction
                            this.innerConnection.DisposeTransaction();

                            // Dispose all active statemenets
                            this.innerConnection.DisposePreparedCommands();

                            // Close connection	or send	it back	to the pool
                            if (this.innerConnection.Pooled)
                            {
                                // Send	connection to the Pool
                                FbPoolManager.Instance.GetPool(this.connectionString).CheckIn(this.innerConnection);
                            }
                            else
                            {
                                if (!this.innerConnection.IsEnlisted)
                                {
                                    this.innerConnection.Dispose();
                                }
                                this.innerConnection = null;
                            }
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        // Update connection state
                        this.OnStateChange(this.state, ConnectionState.Closed);
                    }
                }
            }
        }

        #endregion

        #region · Private Methods ·

        internal void DemandPermission()
        {
#if (!NETCF)
            FirebirdClientPermission permission = new FirebirdClientPermission(this.connectionString);
            permission.Demand();
#endif
        }

        #endregion

        #region · Event Handlers ·

        private void OnWarningMessage(IscException warning)
        {
            if (this.InfoMessage != null)
            {
                this.InfoMessage(this, new FbInfoMessageEventArgs(warning));
            }
        }

        private void OnStateChange(ConnectionState originalState, ConnectionState currentState)
        {
            this.state = currentState;
            if (this.StateChange != null)
            {
                this.StateChange(this, new StateChangeEventArgs(originalState, currentState));
            }
        }

        #endregion
    }
}
