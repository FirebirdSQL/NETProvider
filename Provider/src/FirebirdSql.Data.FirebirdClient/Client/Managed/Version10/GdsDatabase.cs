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
	internal class GdsDatabase : IDatabase
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

		public XdrStream XdrStream
		{
			get { return _xdrStream; }
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

		public string Password
		{
			get { return _connection.Password; }
		}

		public byte[] AuthData
		{
			get { return _connection.AuthData; }
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
					finally
					{
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
					XdrStream.Flush();
					ProcessAttachResponse(ReadGenericResponse());
				}
				catch (IscException)
				{
					SafelyDetach();
					throw;
				}
				catch (IOException ex)
				{
					SafelyDetach();
					throw IscException.ForErrorCode(IscCodes.isc_net_write_err, ex);
				}

				AfterAttachActions();
			}
		}

		protected virtual void SendAttachToBuffer(DatabaseParameterBuffer dpb, string database)
		{
			XdrStream.Write(IscCodes.op_attach);
			XdrStream.Write(0);
			if (!string.IsNullOrEmpty(Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, Password);
			}
			XdrStream.WriteBuffer(Encoding.Default.GetBytes(database));
			XdrStream.WriteBuffer(dpb.ToArray());
		}

		protected virtual void ProcessAttachResponse(GenericResponse response)
		{
			_handle = response.ObjectHandle;
		}

		protected void AfterAttachActions()
		{
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
					throw IscException.ForErrorCodeIntParam(IscCodes.isc_open_trans, TransactionCount);
				}

				try
				{
					if (_handle != 0)
					{
						XdrStream.Write(IscCodes.op_detach);
						XdrStream.Write(_handle);
					}
					XdrStream.Write(IscCodes.op_disconnect);
					XdrStream.Flush();

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
					_xdrStream = null;
					_charset = null;
					_connection = null;
					_serverVersion = null;
				}
				catch (IOException ex)
				{
					try
					{
						CloseConnection();
					}
					catch (IOException ex2)
					{
						throw IscException.ForErrorCode(IscCodes.isc_network_error, ex2);
					}

					throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
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
					XdrStream.Flush();

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
						{ }
						throw;
					}
				}
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_net_write_err, ex);
				}
			}
		}

		protected virtual void SendCreateToBuffer(DatabaseParameterBuffer dpb, string database)
		{
			XdrStream.Write(IscCodes.op_create);
#warning Some constant for default database object ID
			XdrStream.Write(0);
			if (!string.IsNullOrEmpty(Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, Password);
			}
			XdrStream.WriteBuffer(Encoding.Default.GetBytes(database));
			XdrStream.WriteBuffer(dpb.ToArray());
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
					XdrStream.Write(IscCodes.op_drop_database);
					XdrStream.Write(_handle);
					XdrStream.Flush();

					ReadResponse();

					_handle = 0;
				}
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
				}
				finally
				{
					try
					{
						CloseConnection();
					}
					catch
					{ }
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
					XdrStream.Write(IscCodes.op_connect_request);
					XdrStream.Write(IscCodes.P_REQ_async);    // Connection type
					XdrStream.Write(_handle);                 // Related object
					XdrStream.Write(0);                       // Partner identification

					XdrStream.Flush();

					ReadOperation();

					auxHandle = XdrStream.ReadInt32();

					// garbage
					XdrStream.ReadBytes(8);

					int respLen = XdrStream.ReadInt32();
					respLen += respLen % 4;

					// sin_family
					XdrStream.ReadBytes(2);
					respLen -= 2;

					// sin_port
					byte[] buffer = XdrStream.ReadBytes(2);
					portNumber = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 0));
					respLen -= 2;

					// * The address returned by the server may be incorrect if it is behind a NAT box
					// * so we must use the address that was used to connect the main socket, not the
					// * address reported by the server.
					// sin_addr
					buffer = XdrStream.ReadBytes(4);
					//ipAddress = string.Format(
					//    CultureInfo.InvariantCulture,
					//    "{0}.{1}.{2}.{3}",
					//    buffer[0], buffer[1], buffer[2], buffer[3]);
					ipAddress = _connection.IPAddress.ToString();
					respLen -= 4;

					// garbage
					XdrStream.ReadBytes(respLen);

					// Read Status Vector
					XdrStream.ReadStatusVector();
				}
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
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

					XdrStream.Write(IscCodes.op_que_events);
					XdrStream.Write(_handle);                 // Database object id
					XdrStream.WriteBuffer(epb.ToArray());     // Event description block
					XdrStream.Write(0);                       // Address of ast routine
					XdrStream.Write(0);                       // Argument to ast routine
					XdrStream.Write(events.LocalId);          // Client side id of remote event

					XdrStream.Flush();

					GenericResponse response = (GenericResponse)ReadResponse();

					// Update event	Remote event ID
					events.RemoteId = response.ObjectHandle;
				}
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
				}
			}
		}

		public void CancelEvents(RemoteEvent events)
		{
			lock (SyncObject)
			{
				try
				{
					XdrStream.Write(IscCodes.op_cancel_events);
					XdrStream.Write(_handle);             // Database object id
					XdrStream.Write(events.LocalId);      // Event ID

					XdrStream.Flush();

					ReadResponse();

					_eventManager.CancelEvents(events);
				}
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
				}
			}
		}

		#endregion

		#region Transaction Methods

		public virtual TransactionBase BeginTransaction(TransactionParameterBuffer tpb)
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

		public virtual StatementBase CreateStatement(TransactionBase transaction)
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
				if (((GenericResponse)response).Exception != null && ((GenericResponse)response).Exception.IsWarning)
				{
					_warningMessage?.Invoke(((GenericResponse)response).Exception);
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

		public virtual void SetOperation(int operation)
		{
			_xdrStream.SetOperation(operation);
		}

		public virtual void ReleaseObject(int op, int id)
		{
			lock (SyncObject)
			{
				try
				{
					DoReleaseObjectPacket(op, id);
					XdrStream.Flush();
					ProcessReleaseObjectResponse(ReadResponse());
				}
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
				}
			}
		}

		#endregion

		#region Protected Methods

		protected virtual IResponse ReadSingleResponse()
		{
			int operation = ReadOperation();

			IResponse response = GdsConnection.ProcessOperation(operation, XdrStream);

			ProcessResponseWarnings(response);

			return response;
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
					XdrStream.Write(IscCodes.op_info_database);   //	operation
					XdrStream.Write(_handle);             //	db_handle
					XdrStream.Write(0);                           //	incarnation
					XdrStream.WriteBuffer(items, items.Length);   //	items
					XdrStream.Write(bufferLength);                //	result buffer length

					XdrStream.Flush();

					GenericResponse response = (GenericResponse)ReadResponse();

					int responseLength = bufferLength;

					if (response.Data.Length < bufferLength)
					{
						responseLength = response.Data.Length;
					}

					Buffer.BlockCopy(response.Data, 0, buffer, 0, responseLength);
				}
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
				}
			}
		}

		protected void DoReleaseObjectPacket(int op, int id)
		{
			XdrStream.Write(op);
			XdrStream.Write(id);
		}

		protected void ProcessReleaseObjectResponse(IResponse response)
		{ }

		#endregion
	}
}
