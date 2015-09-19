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
			get { return _warningMessage; }
			set { _warningMessage = value; }
		}

		#endregion

		#region Fields

		protected WarningMessageCallback _warningMessage;

		private GdsConnection _connection;
		private GdsEventManager _eventManager;
		private Charset _charset;
		protected int _handle;
		private int _transactionCount;
		protected string _serverVersion;
		private short _packetSize;
		private short _dialect;
		private int _eventsId;
		private int _operation;
		private bool _disposed;
		private XdrStream _xdrStream;
		private object _syncObject;

		#endregion

		#region Properties

		public int Handle
		{
			get { return _handle; }
			protected set { _handle = value; }
		}

		public int TransactionCount
		{
			get { return _transactionCount; }
			set { _transactionCount = value; }
		}

		public string ServerVersion
		{
			get { return _serverVersion; }
			protected set { _serverVersion = value; }
		}

		public Charset Charset
		{
			get { return _charset; }
			set { _charset = value; }
		}

		public short PacketSize
		{
			get { return _packetSize; }
			set { _packetSize = value; }
		}

		public short Dialect
		{
			get { return _dialect; }
			set { _dialect = value; }
		}

		public bool HasRemoteEventSupport
		{
			get { return true; }
		}

		public object SyncObject
		{
			get
			{
				if (_syncObject == null)
				{
					Interlocked.CompareExchange(ref _syncObject, new object(), null);
				}

				return _syncObject;
			}
		}

		#endregion

		#region Constructors

		public GdsDatabase(GdsConnection connection)
		{
			_connection = connection;
			_charset = Charset.DefaultCharset;
			_dialect = 3;
			_packetSize = 8192;
			_xdrStream = _connection.CreateXdrStream();
		}

		#endregion

		#region Finalizer

		~GdsDatabase()
		{
			Dispose(false);
		}

		#endregion

		#region IDisposable methods

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			lock (SyncObject)
			{
				if (!_disposed)
				{
					try
					{
						Detach();
					}
					catch
					{ }

					if (disposing)
					{
						_connection = null;
						_charset = null;
						_eventManager = null;
						_serverVersion = null;
						_dialect = 0;
						_eventsId = 0;
						_handle = 0;
						_packetSize = 0;
						_warningMessage = null;
						_transactionCount = 0;
					}

					_disposed = true;
				}
			}
		}

		#endregion

		#region Attach/Detach Methods

		public virtual void Attach(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			lock (SyncObject)
			{
				try
				{
					SendAttachToBuffer(dpb, database);
					Flush();
					ProcessAttachResponse(ReadGenericResponse());
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
			Write(IscCodes.op_attach);
			Write(0);                                           // Database object ID
			WriteBuffer(Encoding.Default.GetBytes(database));   // Database PATH
			WriteBuffer(dpb.ToArray());                         // DPB Parameter buffer
		}

		protected virtual void ProcessAttachResponse(GenericResponse response)
		{
			// Save the database connection handle
			_handle = response.ObjectHandle;
		}

		protected void AfterAttachActions()
		{
			// Get server version
			_serverVersion = GetServerVersion();
		}

		public virtual void AttachWithTrustedAuth(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			throw new NotSupportedException("Trusted Auth isn't supported on < FB2.1.");
		}

		public virtual void Detach()
		{
			lock (SyncObject)
			{
				if (TransactionCount > 0)
				{
					throw new IscException(IscCodes.isc_open_trans, TransactionCount);
				}

				try
				{
					if (_handle != 0)
					{
						Write(IscCodes.op_detach);
						Write(_handle);
					}
					Write(IscCodes.op_disconnect);
					Flush();

					// Close the Event Manager
					CloseEventManager();

					// Disconnect
					CloseConnection();

#warning Here
					// Close Input and Output streams
					_xdrStream?.Close();

					// Clear members
					_transactionCount = 0;
					_handle = 0;
					_dialect = 0;
					_packetSize = 0;
					_operation = 0;
					_xdrStream = null;
					_charset = null;
					_connection = null;
					_serverVersion = null;
				}
				catch (IOException)
				{
					try
					{
						CloseConnection();
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
				Detach();
			}
			catch
			{ }
		}

		#endregion

		#region Database Methods

		public virtual void CreateDatabase(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			lock (SyncObject)
			{
				try
				{
					SendCreateToBuffer(dpb, database);
					Flush();

					try
					{
						ProcessCreateResponse(ReadGenericResponse());

						Detach();
					}
					catch (IscException)
					{
						try
						{
							CloseConnection();
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
			Write(IscCodes.op_create);
			Write(0);
			WriteBuffer(Encoding.Default.GetBytes(database));
			WriteBuffer(dpb.ToArray());
		}

		protected void ProcessCreateResponse(GenericResponse response)
		{
			_handle = response.ObjectHandle;
		}

		public virtual void DropDatabase()
		{
			lock (SyncObject)
			{
				try
				{
					Write(IscCodes.op_drop_database);
					Write(_handle);
					Flush();

					ReadResponse();

					_handle = 0;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_network_error);
				}
				finally
				{
					try
					{
						CloseConnection();
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
			lock (SyncObject)
			{
				try
				{
					Write(IscCodes.op_connect_request);
					Write(IscCodes.P_REQ_async);    // Connection type
					Write(_handle);                 // Related object
					Write(0);                       // Partner identification

					Flush();

					ReadOperation();

					auxHandle = ReadInt32();

					// garbage
					ReadBytes(8);

					int respLen = ReadInt32();
					respLen += respLen % 4;

					// sin_family
					ReadBytes(2);
					respLen -= 2;

					// sin_port
					byte[] buffer = ReadBytes(2);
					portNumber = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 0));
					respLen -= 2;

					// * The address returned by the server may be incorrect if it is behind a NAT box
					// * so we must use the address that was used to connect the main socket, not the
					// * address reported by the server.
					// sin_addr
					buffer = ReadBytes(4);
					//ipAddress = string.Format(
					//    CultureInfo.InvariantCulture,
					//    "{0}.{1}.{2}.{3}",
					//    buffer[0], buffer[1], buffer[2], buffer[3]);
					ipAddress = _connection.IPAddress.ToString();
					respLen -= 4;

					// garbage
					ReadBytes(respLen);

					// Read Status Vector
					ReadStatusVector();
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
			_connection.Disconnect();
		}
		#endregion

		#region Remote Events Methods

		public void CloseEventManager()
		{
			lock (SyncObject)
			{
				if (_eventManager != null)
				{
					_eventManager.Close();
					_eventManager = null;
				}
			}
		}

		public RemoteEvent CreateEvent()
		{
			return new RemoteEvent(this);
		}

		public void QueueEvents(RemoteEvent events)
		{
			if (_eventManager == null)
			{
				string ipAddress = string.Empty;
				int portNumber = 0;
				int auxHandle = 0;

				ConnectionRequest(out auxHandle, out ipAddress, out portNumber);

				_eventManager = new GdsEventManager(auxHandle, ipAddress, portNumber);
			}

			lock (SyncObject)
			{
				try
				{
					events.LocalId = Interlocked.Increment(ref _eventsId);

					// Enqueue events in the event manager
					_eventManager.QueueEvents(events);

					EventParameterBuffer epb = events.ToEpb();

					Write(IscCodes.op_que_events);
					Write(_handle);                 // Database object id
					WriteBuffer(epb.ToArray());     // Event description block
					Write(0);                       // Address of ast routine
					Write(0);                       // Argument to ast routine
					Write(events.LocalId);          // Client side id of remote event

					Flush();

					GenericResponse response = (GenericResponse)ReadResponse();

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
			lock (SyncObject)
			{
				try
				{
					Write(IscCodes.op_cancel_events);
					Write(_handle);             // Database object id
					Write(events.LocalId);      // Event ID

					Flush();

					ReadResponse();

					_eventManager.CancelEvents(events);
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

			return GetDatabaseInfo(items, IscCodes.BUFFER_SIZE_256)[0].ToString();
		}

		public virtual ArrayList GetDatabaseInfo(byte[] items)
		{
			return GetDatabaseInfo(items, IscCodes.DEFAULT_MAX_BUFFER_SIZE);
		}

		public virtual ArrayList GetDatabaseInfo(byte[] items, int bufferLength)
		{
			byte[] buffer = new byte[bufferLength];

			DatabaseInfo(items, buffer, buffer.Length);

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
					_warningMessage != null)
				{
					_warningMessage(((GenericResponse)response).Exception);
				}
			}
		}

		public virtual int ReadOperation()
		{
			return _xdrStream.ReadOperation();
		}

		public virtual int NextOperation()
		{
			return _xdrStream.ReadNextOperation();
		}

		public virtual IResponse ReadResponse()
		{
			IResponse response = ReadSingleResponse();

			if (response is GenericResponse)
			{
				ProcessResponse(response);
			}

			return response;
		}

		public virtual GenericResponse ReadGenericResponse()
		{
			return (GenericResponse)ReadResponse();
		}

		public virtual SqlResponse ReadSqlResponse()
		{
			return (SqlResponse)ReadResponse();
		}

		public virtual IscException ReadStatusVector()
		{
			IscException exception = null;
			bool eof = false;

			while (!eof)
			{
				int arg = ReadInt32();

				switch (arg)
				{
					case IscCodes.isc_arg_gds:
						int er = ReadInt32();
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
						exception.Errors.Add(new IscError(arg, ReadString()));
						break;

					case IscCodes.isc_arg_number:
						exception.Errors.Add(new IscError(arg, ReadInt32()));
						break;

					case IscCodes.isc_arg_sql_state:
						exception.Errors.Add(new IscError(arg, ReadString()));
						break;

					default:
						int e = ReadInt32();
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
			_operation = operation;
		}

		public virtual void ReleaseObject(int op, int id)
		{
			lock (SyncObject)
			{
				try
				{
					DoReleaseObjectPacket(op, id);
					Flush();
					ProcessReleaseObjectResponse(ReadResponse());
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
			int operation = ReadOperation();

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
						ReadInt32(),
						ReadInt64(),
						ReadBuffer(),
						ReadStatusVector());

				case IscCodes.op_fetch_response:
					return new FetchResponse(ReadInt32(), ReadInt32());

				case IscCodes.op_sql_response:
					return new SqlResponse(ReadInt32());

				default:
					return null;
			}
		}

		/// <summary>
		/// isc_database_info
		/// </summary>
		private void DatabaseInfo(byte[] items, byte[] buffer, int bufferLength)
		{
			lock (SyncObject)
			{
				try
				{
					// see src/remote/protocol.h for packet	definition (p_info struct)
					Write(IscCodes.op_info_database);   //	operation
					Write(_handle);             //	db_handle
					Write(0);                           //	incarnation
					WriteBuffer(items, items.Length);   //	items
					Write(bufferLength);                //	result buffer length

					Flush();

					GenericResponse response = (GenericResponse)ReadResponse();

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
			Write(op);
			Write(id);
		}

		protected void ProcessReleaseObjectResponse(IResponse response)
		{ }

		#endregion

		#region Read Members

		public byte[] ReadBytes(int count)
		{
			return _xdrStream.ReadBytes(count);
		}

		public byte[] ReadOpaque(int length)
		{
			return _xdrStream.ReadOpaque(length);
		}

		public byte[] ReadBuffer()
		{
			return _xdrStream.ReadBuffer();
		}

		public string ReadString()
		{
			return _xdrStream.ReadString();
		}

		public string ReadString(int length)
		{
			return _xdrStream.ReadString(length);
		}

		public string ReadString(Charset charset)
		{
			return _xdrStream.ReadString(charset);
		}

		public string ReadString(Charset charset, int length)
		{
			return _xdrStream.ReadString(charset, length);
		}

		public short ReadInt16()
		{
			return _xdrStream.ReadInt16();
		}

		public int ReadInt32()
		{
			return _xdrStream.ReadInt32();
		}

		public long ReadInt64()
		{
			return _xdrStream.ReadInt64();
		}

		public Guid ReadGuid(int length)
		{
			return _xdrStream.ReadGuid(length);
		}

		public float ReadSingle()
		{
			return _xdrStream.ReadSingle();
		}

		public double ReadDouble()
		{
			return _xdrStream.ReadDouble();
		}

		public DateTime ReadDateTime()
		{
			return _xdrStream.ReadDateTime();
		}

		public DateTime ReadDate()
		{
			return _xdrStream.ReadDate();
		}

		public TimeSpan ReadTime()
		{
			return _xdrStream.ReadTime();
		}

		public decimal ReadDecimal(int type, int scale)
		{
			return _xdrStream.ReadDecimal(type, scale);
		}

		public object ReadValue(DbField field)
		{
			return _xdrStream.ReadValue(field);
		}

		#endregion

		#region Write Methods

		public void WriteOpaque(byte[] buffer)
		{
			_xdrStream.WriteOpaque(buffer);
		}

		public void WriteOpaque(byte[] buffer, int length)
		{
			_xdrStream.WriteOpaque(buffer, length);
		}

		public void WriteBuffer(byte[] buffer)
		{
			_xdrStream.WriteBuffer(buffer);
		}

		public void WriteBuffer(byte[] buffer, int length)
		{
			_xdrStream.WriteBuffer(buffer, length);
		}

		public void WriteBlobBuffer(byte[] buffer)
		{
			_xdrStream.WriteBlobBuffer(buffer);
		}

		public void WriteTyped(int type, byte[] buffer)
		{
			_xdrStream.WriteTyped(type, buffer);
		}

		public void Write(string value)
		{
			_xdrStream.Write(value);
		}

		public void Write(short value)
		{
			_xdrStream.Write(value);
		}

		public void Write(int value)
		{
			_xdrStream.Write(value);
		}

		public void Write(long value)
		{
			_xdrStream.Write(value);
		}

		public void Write(float value)
		{
			_xdrStream.Write(value);
		}

		public void Write(double value)
		{
			_xdrStream.Write(value);
		}

		public void Write(decimal value, int type, int scale)
		{
			_xdrStream.Write(value, type, scale);
		}

		public void Write(bool value)
		{
			_xdrStream.Write(value);
		}

		public void Write(DateTime value)
		{
			_xdrStream.Write(value);
		}

		public void WriteDate(DateTime value)
		{
			_xdrStream.Write(value);
		}

		public void WriteTime(DateTime value)
		{
			_xdrStream.Write(value);
		}

		public void Write(Descriptor value)
		{
			_xdrStream.Write(value);
		}

		public void Write(DbField value)
		{
			_xdrStream.Write(value);
		}

		public void Write(byte[] buffer, int offset, int count)
		{
			_xdrStream.Write(buffer, offset, count);
		}

		public void Flush()
		{
			_xdrStream.Flush();
		}

		#endregion
	}
}
