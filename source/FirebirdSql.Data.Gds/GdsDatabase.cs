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
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Gds
{
	internal sealed class GdsDatabase : IDatabase
	{
		#region Callbacks

		public WarningMessageCallback WarningMessage
		{
			get { return this.warningMessage; }
			set { this.warningMessage = value; }
		}

		#endregion

		#region Fields

		private WarningMessageCallback warningMessage;

		private GdsConnection	connection;
		private GdsEventManager eventManager;
		private Charset			charset;
		private int				handle;
		private int				transactionCount;
		private string			serverVersion;
		private short			packetSize;
		private short			dialect;
		private int				eventsId;
		private bool			disposed;

		#endregion

		#region Properties

		public int Handle
		{
			get { return this.handle; }
		}

		public int TransactionCount
		{
			get { return this.transactionCount; }
			set { this.transactionCount = value; }
		}

		public string ServerVersion
		{
			get { return this.serverVersion; }
		}

		public Charset Charset
		{
			get { return this.charset; }
			set { this.charset = value; }
		}

		public short PacketSize
		{
			get { return this.packetSize; }
			set { this.packetSize = value; }
		}

		public short Dialect
		{
			get { return this.dialect; }
			set { this.dialect = value; }
		}

		public bool HasRemoteEventSupport
		{
			get { return true; }
		}

		#endregion

		#region Internal properties

		internal XdrStream Send
		{
			get { return this.connection.Send; }
		}

		internal XdrStream Receive
		{
			get { return this.connection.Receive; }
		}

		#endregion

		#region Constructors

		public GdsDatabase(GdsConnection connection)
		{
			this.connection		= connection;
			this.charset		= Charset.DefaultCharset;
			this.dialect		= 3;
			this.packetSize		= 8192;

			GC.SuppressFinalize(this);
		}

		#endregion

		#region Finalizer

		~GdsDatabase()
		{
			this.Dispose(false);
		}

		#endregion

		#region IDisposable	methods

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!this.disposed)
				{
					try
					{
						// release any unmanaged resources
						this.Detach();

						// release any managed resources
						if (disposing)
						{
							this.connection		= null;
							this.charset		= null;
							this.eventManager	= null;
							this.serverVersion	= null;
							this.dialect		= 0;
							this.eventsId		= 0;
							this.handle			= 0;
							this.packetSize		= 0;
							this.warningMessage = null;
							this.transactionCount = 0;
						}
					}
					finally
					{
					}

					this.disposed = true;
				}
			}
		}

		#endregion

		#region Database Methods

		public void CreateDatabase(DatabaseParameterBuffer dpb, string database)
		{
			lock (this)
			{
				try
				{
					this.connection.Connect();
					this.Send.Write(IscCodes.op_create);
					this.Send.Write((int)0);
					this.Send.Write(database);
					this.Send.WriteBuffer(dpb.ToArray());
					this.Send.Flush();

					try
					{
						this.handle = this.ReadGenericResponse().ObjectHandle;
						this.Detach();
					}
					catch (IscException)
					{
						try
						{
							this.connection.Disconnect();
						}
						catch (Exception)
						{
						}

						throw;
					}
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_write_err);
				}
			}
		}

		public void DropDatabase()
		{
			lock (this)
			{
				try
				{
					this.Send.Write(IscCodes.op_drop_database);
					this.Send.Write(this.handle);
					this.Send.Flush();

					this.ReadGenericResponse();

					this.handle = 0;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_network_error);
				}
				finally
				{
					try
					{
						this.Detach();
					}
					catch
					{
					}
				}
			}
		}

		#endregion

		#region Auxiliary connection

		public void ConnectionRequest(out int auxHandle, out string ipAddress, out int portNumber)
		{
			lock (this)
			{
				try
				{
					this.Send.Write(IscCodes.op_connect_request);
					this.Send.Write(IscCodes.P_REQ_async);	// Connection type
					this.Send.Write(this.handle);			// Related object
					this.Send.Write(0);						// Partner identification

					this.Send.Flush();

					this.ReadOperation();

					auxHandle = this.Receive.ReadInt32();

					// socketaddr_in (non XDR encoded)

					// sin_port
					portNumber = IscHelper.VaxInteger(this.Receive.ReadBytes(2), 0, 2);

					// sin_Family
					this.Receive.ReadBytes(2);

					// sin_addr
					byte[] buffer = this.Receive.ReadBytes(4);
					ipAddress = String.Format(
						CultureInfo.InvariantCulture,
						"{0}.{1}.{2}.{3}",
						buffer[3], buffer[2], buffer[1], buffer[0]);

					// sin_zero	+ garbage
					this.Receive.ReadBytes(12);

					// Read	Status Vector
					this.connection.ReadStatusVector();
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		#endregion

		#region Remote Events Methods

		public void CloseEventManager()
		{
			lock (this)
			{
				if (this.eventManager != null)
				{
					this.eventManager.Close();
					this.eventManager = null;
				}
			}
		}

		public RemoteEvent CreateEvent()
		{
			return new RemoteEvent(this);
		}

		public void QueueEvents(RemoteEvent events)
		{
			if (this.eventManager == null)
			{
				string	ipAddress	= string.Empty;
				int		portNumber	= 0;
				int		auxHandle	= 0;

				this.ConnectionRequest(out auxHandle, out ipAddress, out portNumber);

				this.eventManager = new GdsEventManager(auxHandle, ipAddress, portNumber);
			}

			lock (this)
			{
				try
				{
					events.LocalId = ++this.eventsId;

					EventParameterBuffer epb = events.ToEpb();

					this.Send.Write(IscCodes.op_que_events);// Op codes
					this.Send.Write(this.handle);			// Database	object id
					this.Send.WriteBuffer(epb.ToArray());	// Event description block
					this.Send.Write(0);						// Address of ast routine
					this.Send.Write(0);						// Argument	to ast routine						
					this.Send.Write(events.LocalId);		// Client side id of remote	event

					this.Send.Flush();

					// Update event	Remote event ID
					events.RemoteId = this.ReadGenericResponse().ObjectHandle;

					// Enqueue events in the event manager
					this.eventManager.QueueEvents(events);
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public void CancelEvents(RemoteEvent events)
		{
			lock (this)
			{
				try
				{
					this.Send.Write(IscCodes.op_cancel_events);	// Op code
					this.Send.Write(this.handle);				// Database	object id
					this.Send.Write(events.LocalId);			// Event ID

					this.Send.Flush();

					this.ReadGenericResponse();

					this.eventManager.CancelEvents(events);
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		#endregion

		#region Methods

		public void Attach(DatabaseParameterBuffer dpb, string database)
		{
			lock (this)
			{
				try
				{
					this.Send.Write(IscCodes.op_attach);
					this.Send.Write((int)0);				// Database	object ID
					this.Send.Write(database);				// Database	PATH
					this.Send.WriteBuffer(dpb.ToArray());	// DPB Parameter buffer
					this.Send.Flush();

					try
					{
						this.handle = this.ReadGenericResponse().ObjectHandle;
					}
					catch (IscException)
					{
						try
						{
							this.connection.Disconnect();
						}
						catch
						{
						}

						throw;
					}
				}
				catch (IOException)
				{
					this.connection.Disconnect();

					throw new IscException(IscCodes.isc_net_write_err);
				}

				// Get server version
				this.serverVersion = this.GetServerVersion();
			}
		}

		public void Detach()
		{
			lock (this)
			{
				if (this.TransactionCount > 0)
				{
					throw new IscException(IscCodes.isc_open_trans, this.TransactionCount);
				}

				try
				{
					this.Send.Write(IscCodes.op_detach);
					this.Send.Write(this.handle);
					this.Send.Flush();

					this.ReadGenericResponse();

					// Close the Event Manager
					this.CloseEventManager();

					// Close the connection	to the server
					this.connection.Disconnect();

					this.transactionCount	= 0;
					this.handle				= 0;
					this.dialect			= 0;
					this.packetSize			= 0;
					this.charset			= null;
					this.connection			= null;
					this.serverVersion		= null;
				}
				catch (IOException)
				{
					try
					{
						this.connection.Disconnect();
					}
					catch (IOException)
					{
						throw new IscException(IscCodes.isc_network_error);
					}

					throw new IscException(IscCodes.isc_network_error);
				}
			}
		}

		#endregion

		#region Transaction	methods

		public ITransaction BeginTransaction(TransactionParameterBuffer tpb)
		{
			GdsTransaction transaction = new GdsTransaction(this);

			transaction.BeginTransaction(tpb);

			return transaction;
		}

		#endregion

		#region Statement creation methods

		public StatementBase CreateStatement()
		{
			return new GdsStatement(this);
		}

		public StatementBase CreateStatement(ITransaction transaction)
		{
			return new GdsStatement(this, transaction as GdsTransaction);
		}

		#endregion

		#region Parameter Buffer creation methods

		public BlobParameterBuffer CreateBlobParameterBuffer()
		{
			return new BlobParameterBuffer(false);
		}

		public DatabaseParameterBuffer CreateDatabaseParameterBuffer()
		{
			return new DatabaseParameterBuffer(false);
		}

		public EventParameterBuffer CreateEventParameterBuffer()
		{
			return new EventParameterBuffer();
		}

		public TransactionParameterBuffer CreateTransactionParameterBuffer()
		{
			return new TransactionParameterBuffer(false);
		}

		#endregion

		#region Database Information methods

        public string GetServerVersion()
        {
            byte[] items = new byte[]
			{
				IscCodes.isc_info_isc_version,
				IscCodes.isc_info_end
			};

            return this.GetDatabaseInfo(items, IscCodes.BUFFER_SIZE_128)[0].ToString();
        }

        public ArrayList GetDatabaseInfo(byte[] items)
        {
            return this.GetDatabaseInfo(items, IscCodes.MAX_BUFFER_SIZE);
        }

        public ArrayList GetDatabaseInfo(byte[] items, int bufferLength)
        {
            byte[] buffer = new byte[bufferLength];

            this.DatabaseInfo(items, buffer, buffer.Length);

            return IscHelper.ParseDatabaseInfo(buffer);
        }

		#endregion

		#region Internal Methods

		internal void ReleaseObject(int op, int id)
		{
			lock (this)
			{
				try
				{
					this.Send.Write(op);
					this.Send.Write(id);
					this.Send.Flush();

					this.ReadGenericResponse();
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		internal GdsResponse ReadGenericResponse()
		{
			GdsResponse response = this.connection.ReadGenericResponse();

			if (response != null && response.Warning != null)
			{
				if (this.warningMessage != null)
				{
					this.warningMessage(response.Warning);
				}
			}

			return response;
		}

		internal int ReadOperation()
		{
			return this.connection.Receive.ReadOperation();
		}

		internal int NextOperation()
		{
            return this.connection.Receive.ReadNextOperation();
		}

		internal bool IsAssigned()
		// RPH - Added for use by Free in GdsStatement to help eliminate
		//       exceptions.
		// Returns true if connection is assigned.
		{
			return connection != null;
		}

		#endregion

		#region Private	Methods

		/// <summary>
		/// isc_database_info
		/// </summary>
		private void DatabaseInfo(byte[] items, byte[] buffer, int bufferLength)
		{
            lock (this)
            {
                try
                {
                    // see src/remote/protocol.h for packet	definition (p_info struct)					
                    this.Send.Write(IscCodes.op_info_database);	//	operation
                    this.Send.Write(this.handle);				//	db_handle
                    this.Send.Write(0);							//	incarnation
                    this.Send.WriteBuffer(items, items.Length);	//	items
                    this.Send.Write(bufferLength);				//	result buffer length
                    this.Send.Flush();

                    GdsResponse response        = this.ReadGenericResponse();
                    int         responseLength  = bufferLength;

                    if (response.Data.Length < bufferLength)
                    {
                        responseLength = response.Data.Length;
                    }

                    Buffer.BlockCopy(response.Data, 0, buffer, 0, responseLength);
                }
                catch (IOException)
                {
                    throw new IscException(IscCodes.isc_network_error);
                }
            }
		}

		#endregion
	}
}