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
 *	Copyright (c) 2002 - 2007 Carlos Guzman Alvarez
 *	Copyright (c) 2007 - 2008 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal class GdsDatabase : IDatabase, IDatabaseStream
	{
		#region Callbacks

		public WarningMessageCallback WarningMessage
		{
			get { return this.warningMessage; }
			set { this.warningMessage = value; }
		}

		#endregion

		#region Fields

		protected WarningMessageCallback warningMessage;

		private GdsConnection connection;
		private GdsEventManager eventManager;
		private Charset charset;
		protected int handle;
		private int transactionCount;
		protected string serverVersion;
		private short packetSize;
		private short dialect;
		private int eventsId;
		private int operation;
		private bool disposed;
		private XdrStream outputStream;
		private XdrStream inputStream;
		private object syncObject;

		#endregion

		#region Properties

		public int Handle
		{
			get { return this.handle; }
			protected set { this.handle = value; }
		}

		public int TransactionCount
		{
			get { return this.transactionCount; }
			set { this.transactionCount = value; }
		}

		public string ServerVersion
		{
			get { return this.serverVersion; }
			protected set { this.serverVersion = value; }
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

		public GdsDatabase(GdsConnection connection)
		{
			this.connection = connection;
			this.charset = Charset.DefaultCharset;
			this.dialect = 3;
			this.packetSize = 8192;
			this.inputStream = this.connection.CreateXdrStream();
			this.outputStream = this.connection.CreateXdrStream();

			GC.SuppressFinalize(this);
		}

		#endregion

		#region Finalizer

		~GdsDatabase()
		{
			this.Dispose(false);
		}

		#endregion

		#region IDisposable methods

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			lock (this.SyncObject)
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
							this.connection = null;
							this.charset = null;
							this.eventManager = null;
							this.serverVersion = null;
							this.dialect = 0;
							this.eventsId = 0;
							this.handle = 0;
							this.packetSize = 0;
							this.warningMessage = null;
							this.transactionCount = 0;
						}

						this.disposed = true;
					}
				}
			}
		}

		#endregion

		#region Attach/Detach Methods

		public virtual void Attach(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			lock (this.SyncObject)
			{
				try
				{
					SendAttachToBuffer(dpb, database);
					this.Flush();
					ProcessAttachResponse(this.ReadGenericResponse());
				}
				catch (IscException)
				{
					SafelyDetach();
					throw;
				}
				catch (IOException)
				{
					SafelyDetach();
					throw new IscException(IscCodes.isc_net_write_err);
				}

				AfterAttachActions();
			}
		}

		protected virtual void SendAttachToBuffer(DatabaseParameterBuffer dpb, string database)
		{
			// Attach to the database
			this.Write(IscCodes.op_attach);
			this.Write(0);				    	// Database	object ID
			this.WriteBuffer(Encoding.Default.GetBytes(database));				// Database	PATH
			this.WriteBuffer(dpb.ToArray());	// DPB Parameter buffer
		}

		protected virtual void ProcessAttachResponse(GenericResponse response)
		{
			// Save the database connection handle
			this.handle = response.ObjectHandle;
		}

		protected void AfterAttachActions()
		{
			// Get server version
			this.serverVersion = this.GetServerVersion();
		}

		public virtual void AttachWithTrustedAuth(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			throw new NotSupportedException("Trusted Auth isn't supported on < FB2.1.");
		}

		public virtual void Detach()
		{
			lock (this.SyncObject)
			{
				if (this.TransactionCount > 0)
				{
					throw new IscException(IscCodes.isc_open_trans, this.TransactionCount);
				}

				try
				{
					if (this.handle != 0)
					{
						this.Write(IscCodes.op_detach);
						this.Write(this.handle);
					}
					this.Write(IscCodes.op_disconnect);
					this.Flush();

					// Close the Event Manager
					this.CloseEventManager();

					// Disconnect
					this.CloseConnection();

					// Close Input and Output streams
					if (this.inputStream != null)
					{
						this.inputStream.Close();
					}
					if (this.outputStream != null)
					{
						this.outputStream.Close();
					}

					// Clear members
					this.transactionCount = 0;
					this.handle = 0;
					this.dialect = 0;
					this.packetSize = 0;
					this.operation = 0;
					this.outputStream = null;
					this.inputStream = null;
					this.charset = null;
					this.connection = null;
					this.serverVersion = null;
				}
				catch (IOException)
				{
					try
					{
						this.CloseConnection();
					}
					catch (IOException)
					{
						throw new IscException(IscCodes.isc_network_error);
					}

					throw new IscException(IscCodes.isc_network_error);
				}
			}
		}

		protected void SafelyDetach()
		{
			try
			{
				this.Detach();
			}
			catch
			{ }
		}

		#endregion

		#region Database Methods

		public virtual void CreateDatabase(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			lock (this.SyncObject)
			{
				try
				{
					SendCreateToBuffer(dpb, database);
					this.Flush();

					try
					{
						ProcessCreateResponse(this.ReadGenericResponse());

						this.Detach();
					}
					catch (IscException)
					{
						try
						{
							this.CloseConnection();
						}
						catch
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

		protected virtual void SendCreateToBuffer(DatabaseParameterBuffer dpb, string database)
		{
			this.Write(IscCodes.op_create);
			this.Write(0);
			this.WriteBuffer(Encoding.Default.GetBytes(database));
			this.WriteBuffer(dpb.ToArray());
		}

		protected void ProcessCreateResponse(GenericResponse response)
		{
			this.handle = response.ObjectHandle;
		}

		public virtual void DropDatabase()
		{
			lock (this.SyncObject)
			{
				try
				{
					this.Write(IscCodes.op_drop_database);
					this.Write(this.handle);
					this.Flush();

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
						this.CloseConnection();
					}
					catch
					{
					}
				}
			}
		}

		#endregion

		#region Auxiliary Connection Methods

		public virtual void ConnectionRequest(out int auxHandle, out string ipAddress, out int portNumber)
		{
			lock (this.SyncObject)
			{
				try
				{
					this.Write(IscCodes.op_connect_request);
					this.Write(IscCodes.P_REQ_async);	// Connection type
					this.Write(this.handle);			// Related object
					this.Write(0);						// Partner identification

					this.Flush();

					this.ReadOperation();

					auxHandle = this.ReadInt32();

					// garbage
					this.ReadBytes(8);

					int respLen = this.ReadInt32();
					respLen += respLen % 4;

					// sin_family
					this.ReadBytes(2);
					respLen -= 2;

					// sin_port
					byte[] buffer = this.ReadBytes(2);
					portNumber = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 0));
					respLen -= 2;

					// * The address returned by the server may be incorrect if it is behind a NAT box
					// * so we must use the address that was used to connect the main socket, not the
					// * address reported by the server.
					// sin_addr
					buffer = this.ReadBytes(4);
					//ipAddress = string.Format(
					//    CultureInfo.InvariantCulture,
					//    "{0}.{1}.{2}.{3}",
					//    buffer[0], buffer[1], buffer[2], buffer[3]);
					ipAddress = this.connection.IPAddress.ToString();
					respLen -= 4;

					// garbage
					this.ReadBytes(respLen);

					// Read	Status Vector
					this.ReadStatusVector();
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		#endregion

		#region Connection Methods
		public void CloseConnection()
		{
			this.connection.Disconnect();
		}
		#endregion

		#region Remote Events Methods

		public void CloseEventManager()
		{
			lock (this.SyncObject)
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
				string ipAddress = string.Empty;
				int portNumber = 0;
				int auxHandle = 0;

				this.ConnectionRequest(out auxHandle, out ipAddress, out portNumber);

				this.eventManager = new GdsEventManager(auxHandle, ipAddress, portNumber);
			}

			lock (this.SyncObject)
			{
				try
				{
					events.LocalId = Interlocked.Increment(ref this.eventsId);
					
					// Enqueue events in the event manager
					this.eventManager.QueueEvents(events);

					EventParameterBuffer epb = events.ToEpb();

					this.Write(IscCodes.op_que_events); // Op codes
					this.Write(this.handle);			// Database	object id
					this.WriteBuffer(epb.ToArray());	// Event description block
					this.Write(0);						// Address of ast routine
					this.Write(0);						// Argument	to ast routine						
					this.Write(events.LocalId);		    // Client side id of remote	event

					this.Flush();

					GenericResponse response = (GenericResponse)this.ReadResponse();

					// Update event	Remote event ID
					events.RemoteId = response.ObjectHandle;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public void CancelEvents(RemoteEvent events)
		{
			lock (this.SyncObject)
			{
				try
				{
					this.Write(IscCodes.op_cancel_events);	// Op code
					this.Write(this.handle);				// Database	object id
					this.Write(events.LocalId);			    // Event ID

					this.Flush();

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

		#region Transaction Methods

		public virtual ITransaction BeginTransaction(TransactionParameterBuffer tpb)
		{
			GdsTransaction transaction = new GdsTransaction(this);

			transaction.BeginTransaction(tpb);

			return transaction;
		}

		#endregion

		#region Cancel Methods

		public virtual void CancelOperation(int kind)
		{
			throw new NotSupportedException("Cancel Operation isn't supported on < FB2.5.");
		}

		#endregion

		#region Statement Creation Methods

		public virtual StatementBase CreateStatement()
		{
			return new GdsStatement(this);
		}

		public virtual StatementBase CreateStatement(ITransaction transaction)
		{
			return new GdsStatement(this, transaction);
		}

		#endregion

		#region Database Information Methods

		public virtual string GetServerVersion()
		{
			byte[] items = new byte[]
			{
				IscCodes.isc_info_firebird_version,
				IscCodes.isc_info_end
			};

			return this.GetDatabaseInfo(items, IscCodes.BUFFER_SIZE_256)[0].ToString();
		}

		public virtual ArrayList GetDatabaseInfo(byte[] items)
		{
			return this.GetDatabaseInfo(items, IscCodes.DEFAULT_MAX_BUFFER_SIZE);
		}

		public virtual ArrayList GetDatabaseInfo(byte[] items, int bufferLength)
		{
			byte[] buffer = new byte[bufferLength];

			this.DatabaseInfo(items, buffer, buffer.Length);

			return IscHelper.ParseDatabaseInfo(buffer);
		}

		#endregion

		#region Trigger Context Methods

		public virtual ITriggerContext GetTriggerContext()
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Response Methods

		protected void ProcessResponse(IResponse response)
		{
			if (response != null && response is GenericResponse)
			{
				if (((GenericResponse)response).Exception != null && !((GenericResponse)response).Exception.IsWarning)
				{
					throw ((GenericResponse)response).Exception;
				}
			}
		}

		protected void ProcessResponseWarnings(IResponse response)
		{
			if (response is GenericResponse)
			{
				if (((GenericResponse)response).Exception != null &&
					((GenericResponse)response).Exception.IsWarning &&
					this.warningMessage != null)
				{
					this.warningMessage(((GenericResponse)response).Exception);
				}
			}
		}

		public virtual int ReadOperation()
		{
			return this.inputStream.ReadOperation();
		}

		public virtual int NextOperation()
		{
			return this.inputStream.ReadNextOperation();
		}

		public virtual IResponse ReadResponse()
		{
			IResponse response = this.ReadSingleResponse();

			if (response is GenericResponse)
			{
				this.ProcessResponse(response);
			}

			return response;
		}

		public virtual GenericResponse ReadGenericResponse()
		{
			return (GenericResponse)this.ReadResponse();
		}

		public virtual SqlResponse ReadSqlResponse()
		{
			return (SqlResponse)this.ReadResponse();
		}

		public virtual IscException ReadStatusVector()
		{
			IscException exception = null;
			bool eof = false;

			while (!eof)
			{
				int arg = this.ReadInt32();

				switch (arg)
				{
					case IscCodes.isc_arg_gds:
						int er = this.ReadInt32();
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
							exception.BuildExceptionData();
						}
						eof = true;
						break;

					case IscCodes.isc_arg_interpreted:
					case IscCodes.isc_arg_string:
						exception.Errors.Add(new IscError(arg, this.ReadString()));
						break;

					case IscCodes.isc_arg_number:
						exception.Errors.Add(new IscError(arg, this.ReadInt32()));
						break;

					case IscCodes.isc_arg_sql_state:
						exception.Errors.Add(new IscError(arg, this.ReadString()));
						break;

					default:
						int e = this.ReadInt32();
						if (e != 0)
						{
							if (exception == null)
							{
								exception = new IscException();
							}
							exception.Errors.Add(new IscError(arg, e));
						}
						break;
				}
			}

			return exception;
		}

		public virtual void SetOperation(int operation)
		{
			this.operation = operation;
		}

		public virtual void ReleaseObject(int op, int id)
		{
			lock (this.SyncObject)
			{
				try
				{
					DoReleaseObjectPacket(op, id);
					this.Flush();
					ProcessReleaseObjectResponse(this.ReadResponse());
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		#endregion

		#region Protected Methods

		protected virtual IResponse ReadSingleResponse()
		{
			int operation = this.ReadOperation();

			IResponse response = ProcessOperation(operation);

			ProcessResponseWarnings(response);

			return response;
		}

		protected virtual IResponse ProcessOperation(int operation)
		{
			switch (operation)
			{
				case IscCodes.op_response:
					return new GenericResponse(
						this.ReadInt32(),
						this.ReadInt64(),
						this.ReadBuffer(),
						this.ReadStatusVector());

				case IscCodes.op_fetch_response:
					return new FetchResponse(this.ReadInt32(), this.ReadInt32());

				case IscCodes.op_sql_response:
					return new SqlResponse(this.ReadInt32());

				default:
					return null;
			}
		}

		/// <summary>
		/// isc_database_info
		/// </summary>
		private void DatabaseInfo(byte[] items, byte[] buffer, int bufferLength)
		{
			lock (this.SyncObject)
			{
				try
				{
					// see src/remote/protocol.h for packet	definition (p_info struct)					
					this.Write(IscCodes.op_info_database);	//	operation
					this.Write(this.handle);				//	db_handle
					this.Write(0);							//	incarnation
					this.WriteBuffer(items, items.Length);	//	items
					this.Write(bufferLength);				//	result buffer length

					this.Flush();

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

		protected void DoReleaseObjectPacket(int op, int id)
		{
			this.Write(op);
			this.Write(id);
		}

		protected void ProcessReleaseObjectResponse(IResponse response)
		{ }

		#endregion

		#region Read Members

		public byte[] ReadBytes(int count)
		{
			return this.inputStream.ReadBytes(count);
		}

		public byte[] ReadOpaque(int length)
		{
			return this.inputStream.ReadOpaque(length);
		}

		public byte[] ReadBuffer()
		{
			return this.inputStream.ReadBuffer();
		}

		public string ReadString()
		{
			return this.inputStream.ReadString();
		}

		public string ReadString(int length)
		{
			return this.inputStream.ReadString(length);
		}

		public string ReadString(Charset charset)
		{
			return this.inputStream.ReadString(charset);
		}

		public string ReadString(Charset charset, int length)
		{
			return this.inputStream.ReadString(charset, length);
		}

		public short ReadInt16()
		{
			return this.inputStream.ReadInt16();
		}

		public int ReadInt32()
		{
			return this.inputStream.ReadInt32();
		}

		public long ReadInt64()
		{
			return this.inputStream.ReadInt64();
		}

		public Guid ReadGuid(int length)
		{
			return this.inputStream.ReadGuid(length);
		}

		public float ReadSingle()
		{
			return this.inputStream.ReadSingle();
		}

		public double ReadDouble()
		{
			return this.inputStream.ReadDouble();
		}

		public DateTime ReadDateTime()
		{
			return this.inputStream.ReadDateTime();
		}

		public DateTime ReadDate()
		{
			return this.inputStream.ReadDate();
		}

		public TimeSpan ReadTime()
		{
			return this.inputStream.ReadTime();
		}

		public decimal ReadDecimal(int type, int scale)
		{
			return this.inputStream.ReadDecimal(type, scale);
		}

		public object ReadValue(DbField field)
		{
			return this.inputStream.ReadValue(field);
		}

		#endregion

		#region Write Methods

		public void WriteOpaque(byte[] buffer)
		{
			this.outputStream.WriteOpaque(buffer);
		}

		public void WriteOpaque(byte[] buffer, int length)
		{
			this.outputStream.WriteOpaque(buffer, length);
		}

		public void WriteBuffer(byte[] buffer)
		{
			this.outputStream.WriteBuffer(buffer);
		}

		public void WriteBuffer(byte[] buffer, int length)
		{
			this.outputStream.WriteBuffer(buffer, length);
		}

		public void WriteBlobBuffer(byte[] buffer)
		{
			this.outputStream.WriteBlobBuffer(buffer);
		}

		public void WriteTyped(int type, byte[] buffer)
		{
			this.outputStream.WriteTyped(type, buffer);
		}

		public void Write(string value)
		{
			this.outputStream.Write(value);
		}

		public void Write(short value)
		{
			this.outputStream.Write(value);
		}

		public void Write(int value)
		{
			this.outputStream.Write(value);
		}

		public void Write(long value)
		{
			this.outputStream.Write(value);
		}

		public void Write(float value)
		{
			this.outputStream.Write(value);
		}

		public void Write(double value)
		{
			this.outputStream.Write(value);
		}

		public void Write(decimal value, int type, int scale)
		{
			this.outputStream.Write(value, type, scale);
		}

		public void Write(bool value)
		{
			this.outputStream.Write(value);
		}

		public void Write(DateTime value)
		{
			this.outputStream.Write(value);
		}

		public void WriteDate(DateTime value)
		{
			this.outputStream.Write(value);
		}

		public void WriteTime(DateTime value)
		{
			this.outputStream.Write(value);
		}

		public void Write(Descriptor value)
		{
			this.outputStream.Write(value);
		}

		public void Write(DbField value)
		{
			this.outputStream.Write(value);
		}

		public void Write(byte[] buffer, int offset, int count)
		{
			this.outputStream.Write(buffer, offset, count);
		}

		public void Flush()
		{
			this.outputStream.Flush();
		}

		#endregion
	}
}
