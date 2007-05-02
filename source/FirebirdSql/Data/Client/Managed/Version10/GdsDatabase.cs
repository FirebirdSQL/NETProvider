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

namespace FirebirdSql.Data.Client.Gds
{
	internal class GdsDatabase : IDatabase
	{
		#region · Callbacks ·

		public WarningMessageCallback WarningMessage
		{
			get { return this.warningMessage; }
			set { this.warningMessage = value; }
		}

		#endregion

		#region · Fields ·

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

		#region · Properties ·

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

		#region · Constructors ·

		public GdsDatabase()
		{
			this.connection		= new GdsConnection();
			this.charset		= Charset.DefaultCharset;
			this.dialect		= 3;
			this.packetSize		= 8192;

			GC.SuppressFinalize(this);
		}

		#endregion

		#region · Finalizer ·

		~GdsDatabase()
		{
			this.Dispose(false);
		}

		#endregion

		#region · IDisposable methods ·

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
                    }
                    catch
                    {
                    }
					finally
					{
                        // release any managed resources
                        if (disposing)
                        {
                            this.connection         = null;
                            this.charset            = null;
                            this.eventManager       = null;
                            this.serverVersion      = null;
                            this.dialect            = 0;
                            this.eventsId           = 0;
                            this.handle             = 0;
                            this.packetSize         = 0;
                            this.warningMessage     = null;
                            this.transactionCount   = 0;
                        }

                        this.disposed = true;
					}					
				}
			}
		}

		#endregion

		#region · Database Methods ·

        public virtual void CreateDatabase(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			lock (this)
			{
				try
				{
					this.connection.Connect(dataSource, port, this.packetSize, this.charset);
					this.Send.Write(IscCodes.op_create);
					this.Send.Write((int)0);
					this.Send.Write(database);
					this.Send.WriteBuffer(dpb.ToArray());
					this.Send.Flush();

					try
					{
                        GenericResponse response = (GenericResponse)this.ReadResponse();

                        this.handle = response.ObjectHandle;
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

        public virtual void DropDatabase()
		{
			lock (this)
			{
				try
				{
					this.Send.Write(IscCodes.op_drop_database);
					this.Send.Write(this.handle);
					this.Send.Flush();

					this.ReadResponse();

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

		#region · Auxiliary Connection Methods ·

        public virtual void ConnectionRequest(out int auxHandle, out string ipAddress, out int portNumber)
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

		#region · Remote Events Methods ·

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

                    GenericResponse response = (GenericResponse)this.ReadResponse();

					// Update event	Remote event ID
                    events.RemoteId = response.ObjectHandle;

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

					this.ReadResponse();

					this.eventManager.CancelEvents(events);
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		#endregion

		#region · Methods ·

        public virtual void Attach(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			lock (this)
			{
				try
				{
					this.connection.Connect(dataSource, port, this.packetSize, this.charset);

					this.Identify(database);

					this.Send.Write(IscCodes.op_attach);
					this.Send.Write((int)0);				// Database	object ID
					this.Send.Write(database);				// Database	PATH
					this.Send.WriteBuffer(dpb.ToArray());	// DPB Parameter buffer
					this.Send.Flush();

					try
					{
                        GenericResponse response = (GenericResponse)this.ReadResponse();

						this.handle = response.ObjectHandle;
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

        public virtual void Detach()
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

					this.ReadResponse();

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

		#region · Transaction Methods ·

        public virtual ITransaction BeginTransaction(TransactionParameterBuffer tpb)
		{
			GdsTransaction transaction = new GdsTransaction(this);

			transaction.BeginTransaction(tpb);

			return transaction;
		}

		#endregion

		#region · Statement Creation Methods ·

        public virtual StatementBase CreateStatement()
		{
			return new GdsStatement(this);
		}

        public virtual StatementBase CreateStatement(ITransaction transaction)
		{
			return new GdsStatement(this, transaction as GdsTransaction);
		}

		#endregion

		#region · Parameter Buffer Creation Methods ·

        public virtual BlobParameterBuffer CreateBlobParameterBuffer()
		{
			return new BlobParameterBuffer(false);
		}

        public virtual DatabaseParameterBuffer CreateDatabaseParameterBuffer()
		{
			return new DatabaseParameterBuffer(false);
		}

        public virtual EventParameterBuffer CreateEventParameterBuffer()
		{
			return new EventParameterBuffer();
		}

        public virtual TransactionParameterBuffer CreateTransactionParameterBuffer()
		{
			return new TransactionParameterBuffer(false);
		}

		#endregion

		#region · Database Information Methods ·

        public virtual string GetServerVersion()
		{
			byte[] items = new byte[]
			{
				IscCodes.isc_info_isc_version,
				IscCodes.isc_info_end
			};

			return this.GetDatabaseInfo(items, IscCodes.BUFFER_SIZE_128)[0].ToString();
		}

        public virtual ArrayList GetDatabaseInfo(byte[] items)
		{
			return this.GetDatabaseInfo(items, IscCodes.MAX_BUFFER_SIZE);
		}

        public virtual ArrayList GetDatabaseInfo(byte[] items, int bufferLength)
		{
			byte[] buffer = new byte[bufferLength];

			this.DatabaseInfo(items, buffer, buffer.Length);

			return IscHelper.ParseDatabaseInfo(buffer);
		}

		#endregion

        #region · Trigger Context Methods ·

        public virtual ITriggerContext GetTriggerContext()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region · Response Methods ·

        protected virtual int ReadOperation()
        {
            int op = (this.operation >= 0) ? this.operation : this.NextOperation();
            this.operation = -1;

            return op;
        }

        protected virtual int NextOperation()
        {
            do
            {
                /* loop	as long	as we are receiving	dummy packets, just
                 * throwing	them away--note	that if	we are a server	we won't
                 * be receiving	them, but it is	better to check	for	them at
                 * this	level rather than try to catch them	in all places where
                 * this	routine	is called 
                 */
                this.operation = this.receive.ReadInt32();
            } while (this.operation == IscCodes.op_dummy);

            return this.operation;
        }

        protected virtual IResponse ReadResponse()
        {
            IResponse response = null;

            try
            {
                int operation = this.ReadOperation();

                switch (operation)
                {
                    case IscCodes.op_response:
                        response = new GenericResponse(
                            this.receive.ReadInt32(),
                            this.receive.ReadInt64(),
                            this.receive.ReadBuffer(),
                            this.ReadStatusVector());

                    case IscCodes.op_fetch_response:
                        response = new FetchResponse(this.receive.ReadInt32(), this.receive.ReadInt32());

                    case IscCodes.op_sql_response:
                        response = new SqlResponse(this.receive.ReadInt32());
                }

                if (response != null && response is GenericResponse)
                {
                    GenericResponse genericResponse = (GenericResponse)response;

                    if (this.warningMessage != null && genericResponse.Warning != null)
                    {
                        this.warningMessage(genericResponse.Warning);
                    }
                }
            }
            catch (IOException)
            {
                throw new IscException(IscCodes.isc_net_read_err);
            }
        }

        protected virtual IscException ReadStatusVector()
        {
            IscException exception = null;
            bool eof = false;

            try
            {
                while (!eof)
                {
                    int arg = this.receive.ReadInt32();

                    switch (arg)
                    {
                        case IscCodes.isc_arg_gds:
                            int er = this.receive.ReadInt32();
                            if (er != 0)
                            {
                                if (exception == null)
                                {
                                    exception = new IscException();
                                }
                                exception.Errors.Add(new IscError(arg, er));
                            }
                            break;

                        case IscCodes.isc_arg_end:
                            if (exception != null && exception.Errors.Count != 0)
                            {
                                exception.BuildExceptionMessage();
                            }
                            eof = true;
                            break;

                        case IscCodes.isc_arg_interpreted:
                        case IscCodes.isc_arg_string:
                            exception.Errors.Add(new IscError(arg, this.receive.ReadString()));
                            break;

                        case IscCodes.isc_arg_number:
                            exception.Errors.Add(new IscError(arg, this.receive.ReadInt32()));
                            break;

                        default:
                            {
                                int e = this.receive.ReadInt32();
                                if (e != 0)
                                {
                                    if (exception == null)
                                    {
                                        exception = new IscException();
                                    }
                                    exception.Errors.Add(new IscError(arg, e));
                                }
                            }
                            break;
                    }
                }
            }
            catch (IOException)
            {
                throw new IscException(IscCodes.isc_arg_gds, IscCodes.isc_net_read_err);
            }

            if (exception != null && !exception.IsWarning)
            {
                throw exception;
            }

            return exception;
        }

        internal virtual void SetOperation(int operation)
        {
            this.operation = operation;
        }

        protected virtual void ReleaseObject(int op, int id)
		{
			lock (this)
			{
				try
				{
					this.Send.Write(op);
					this.Send.Write(id);
					this.Send.Flush();

					this.ReadResponse();
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		#endregion

		#region · Protected Methods ·

        protected virtual void Identify(string database)
		{
			try
			{
				// Here	we identify	the	user to	the	engine.	 
				// This	may	or may not be used as login	info to	a database.				
#if	(!NETCF)
				byte[] user = Encoding.Default.GetBytes(System.Environment.UserName);
				byte[] host = Encoding.Default.GetBytes(System.Net.Dns.GetHostName());
#else
				byte[] user = Encoding.Default.GetBytes("fbnetcf");
				byte[] host = Encoding.Default.GetBytes(System.Net.Dns.GetHostName());
#endif

				MemoryStream user_id = new MemoryStream();

				/* User	Name */
				user_id.WriteByte(1);
				user_id.WriteByte((byte)user.Length);
				user_id.Write(user, 0, user.Length);
				
                /* Host	name */
				user_id.WriteByte(4);
				user_id.WriteByte((byte)host.Length);
				user_id.Write(host, 0, host.Length);
				
                /* Attach/create using this	connection 
				 * will	use	user verification
				 */
				user_id.WriteByte(6);
				user_id.WriteByte(0);

				this.Send.Write(IscCodes.op_connect);
				this.Send.Write(IscCodes.op_attach);
				this.Send.Write(IscCodes.CONNECT_VERSION2);	// CONNECT_VERSION2
				this.Send.Write(1);							// Architecture	of client -	Generic

				this.Send.Write(database);					// Database	path
				this.Send.Write(1);							// Protocol	versions understood
				this.Send.WriteBuffer(user_id.ToArray());	// User	identification Stuff

				this.Send.Write(IscCodes.PROTOCOL_VERSION10);//	Protocol version
				this.Send.Write(1);							// Architecture	of client -	Generic
				this.Send.Write(2);							// Minumum type
				this.Send.Write(3);							// Maximum type
				this.Send.Write(2);							// Preference weight

				this.Send.Flush();

				if (this.ReadOperation() == IscCodes.op_accept)
				{
					this.Receive.ReadInt32();	// Protocol	version
					this.Receive.ReadInt32();	// Architecture	for	protocol
					this.Receive.ReadInt32();	// Minimum type
				}
				else
				{
					try
					{
						this.Detach();
					}
					catch (Exception)
					{
					}
					finally
					{
						throw new IscException(IscCodes.isc_connect_reject);
					}
				}
			}
			catch (IOException)
			{
				// throw new IscException(IscCodes.isc_arg_gds,	IscCodes.isc_network_error,	Parameters.DataSource);
				throw new IscException(IscCodes.isc_network_error);
			}
		}

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

                    GenericResponse response = (GenericResponse)this.ReadResponse();

                    int responseLength = bufferLength;

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

        #region IDatabase Members

        public byte[] ReadBytes(int count)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public byte[] ReadOpaque(int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public byte[] ReadBuffer()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string ReadString()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string ReadString(int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string ReadString(Charset charset)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string ReadString(Charset charset, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public short ReadInt16()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int ReadInt32()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public long ReadInt64()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Guid ReadGuid(int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public float ReadSingle()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public double ReadDouble()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public DateTime ReadDateTime()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public DateTime ReadDate()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public DateTime ReadTime()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public decimal ReadDecimal(int type, int scale)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public object ReadValue(DbField field)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void WriteOpaque(byte[] buffer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void WriteOpaque(byte[] buffer, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void WriteBuffer(byte[] buffer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void WriteBuffer(byte[] buffer, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void WriteBlobBuffer(byte[] buffer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void WriteTyped(int type, byte[] buffer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(string value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(short value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(int value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(long value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(float value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(double value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(decimal value, int type, int scale)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(bool value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(DateTime value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void WriteDate(DateTime value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void WriteTime(DateTime value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(Descriptor descriptor)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(DbField param)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Flush()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}