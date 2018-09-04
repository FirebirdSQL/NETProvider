/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
	internal class GdsDatabase : IDatabase
	{
		const int DatabaseObjectId = 0;
		const int PartnerIdentification = 0;
		const int AddressOfAstRoutine = 0;
		const int ArgumentToAstRoutine = 0;
		internal const int Incarnation = 0;

		#region Callbacks

		public Action<IscException> WarningMessage
		{
			get { return _warningMessage; }
			set { _warningMessage = value; }
		}

		#endregion

		#region Fields

		protected Action<IscException> _warningMessage;

		private GdsConnection _connection;
		private GdsEventManager _eventManager;
		private Charset _charset;
		protected int _handle;
		private int _transactionCount;
		protected string _serverVersion;
		private short _packetSize;
		private short _dialect;
		private bool _disposed;
		private XdrStream _xdrStream;

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

		public string Password
		{
			get { return _connection.Password; }
		}

		public byte[] AuthData
		{
			get { return _connection.AuthData; }
		}

		public bool ConnectionBroken
		{
			get { return _xdrStream.IOFailed; }
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

		#region IDisposable methods

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				Detach();
				_connection = null;
				_charset = null;
				_eventManager = null;
				_serverVersion = null;
				_dialect = 0;
				_handle = 0;
				_packetSize = 0;
				_warningMessage = null;
				_transactionCount = 0;
			}
		}

		#endregion

		#region Attach/Detach Methods

		public virtual void Attach(DatabaseParameterBuffer dpb, string dataSource, int port, string database, byte[] cryptKey)
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

		protected virtual void SendAttachToBuffer(DatabaseParameterBuffer dpb, string database)
		{
			XdrStream.Write(IscCodes.op_attach);
			XdrStream.Write(0);
			if (!string.IsNullOrEmpty(Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, Password);
			}
			XdrStream.WriteBuffer(Encoding2.Default.GetBytes(database));
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

		public virtual void AttachWithTrustedAuth(DatabaseParameterBuffer dpb, string dataSource, int port, string database, byte[] cryptKey)
		{
			throw new NotSupportedException("Trusted Auth isn't supported on < FB2.1.");
		}

		public virtual void Detach()
		{
			if (TransactionCount > 0)
			{
				throw IscException.ForErrorCodeIntParam(IscCodes.isc_open_trans, TransactionCount);
			}

			try
			{
				CloseEventManager();

				if (_handle != 0)
				{
					XdrStream.Write(IscCodes.op_detach);
					XdrStream.Write(_handle);
				}
				XdrStream.Write(IscCodes.op_disconnect);
				XdrStream.Flush();

				CloseConnection();

#warning Here
				_xdrStream?.Dispose();

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

		public virtual void CreateDatabase(DatabaseParameterBuffer dpb, string dataSource, int port, string database, byte[] cryptKey)
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

		protected virtual void SendCreateToBuffer(DatabaseParameterBuffer dpb, string database)
		{
			XdrStream.Write(IscCodes.op_create);
			XdrStream.Write(DatabaseObjectId);
			if (!string.IsNullOrEmpty(Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, Password);
			}
			XdrStream.WriteBuffer(Encoding2.Default.GetBytes(database));
			XdrStream.WriteBuffer(dpb.ToArray());
		}

		protected void ProcessCreateResponse(GenericResponse response)
		{
			_handle = response.ObjectHandle;
		}

		public virtual void DropDatabase()
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

		#endregion

		#region Auxiliary Connection Methods

		public virtual void ConnectionRequest(out int auxHandle, out string ipAddress, out int portNumber)
		{
			try
			{
				XdrStream.Write(IscCodes.op_connect_request);
				XdrStream.Write(IscCodes.P_REQ_async);
				XdrStream.Write(_handle);
				XdrStream.Write(PartnerIdentification);

				XdrStream.Flush();

				ReadOperation();

				auxHandle = XdrStream.ReadInt32();

				var garbage1 = new byte[8];
				XdrStream.ReadBytes(garbage1);

				var respLen = XdrStream.ReadInt32();
				respLen += respLen % 4;

				var sin_family = new byte[2];
				XdrStream.ReadBytes(sin_family);
				respLen -= 2;

				var sin_port = new byte[2];
				XdrStream.ReadBytes(sin_port);
				portNumber = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(sin_port, 0));
				respLen -= 2;

				// * The address returned by the server may be incorrect if it is behind a NAT box
				// * so we must use the address that was used to connect the main socket, not the
				// * address reported by the server.
				var sin_addr = new byte[4];
				XdrStream.ReadBytes(sin_addr);
				//ipAddress = string.Format(
				//    CultureInfo.InvariantCulture,
				//    "{0}.{1}.{2}.{3}",
				//    buffer[0], buffer[1], buffer[2], buffer[3]);
				ipAddress = _connection.IPAddress.ToString();
				respLen -= 4;

				var garbage2 = new byte[respLen];
				XdrStream.ReadBytes(garbage2);

				XdrStream.ReadStatusVector();
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
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
			if (_eventManager != null)
			{
				_eventManager.Dispose();
				_eventManager = null;
			}
		}

		public void QueueEvents(RemoteEvent remoteEvent)
		{
			try
			{
				if (_eventManager == null)
				{
					ConnectionRequest(out var auxHandle, out var ipAddress, out var portNumber);
					_eventManager = new GdsEventManager(auxHandle, ipAddress, portNumber);
					var dummy = _eventManager.WaitForEventsAsync(remoteEvent);
				}

				remoteEvent.LocalId++;

				var epb = remoteEvent.BuildEpb();
				var epbData = epb.ToArray();

				XdrStream.Write(IscCodes.op_que_events);
				XdrStream.Write(_handle);
				XdrStream.WriteBuffer(epbData);
				XdrStream.Write(AddressOfAstRoutine);
				XdrStream.Write(ArgumentToAstRoutine);
				XdrStream.Write(remoteEvent.LocalId);

				XdrStream.Flush();

				var response = (GenericResponse)ReadResponse();

				remoteEvent.RemoteId = response.ObjectHandle;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		public void CancelEvents(RemoteEvent events)
		{
			try
			{
				XdrStream.Write(IscCodes.op_cancel_events);
				XdrStream.Write(_handle);
				XdrStream.Write(events.LocalId);

				XdrStream.Flush();

				ReadResponse();
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		#endregion

		#region Transaction Methods

		public virtual TransactionBase BeginTransaction(TransactionParameterBuffer tpb)
		{
			var transaction = new GdsTransaction(this);

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
			var items = new byte[]
			{
				IscCodes.isc_info_firebird_version,
				IscCodes.isc_info_end
			};
			var info = GetDatabaseInfo(items, IscCodes.BUFFER_SIZE_256);
			return (string)info[info.Count - 1];
		}

		public virtual List<object> GetDatabaseInfo(byte[] items)
		{
			return GetDatabaseInfo(items, IscCodes.DEFAULT_MAX_BUFFER_SIZE);
		}

		public virtual List<object> GetDatabaseInfo(byte[] items, int bufferLength)
		{
			var buffer = new byte[bufferLength];
			DatabaseInfo(items, buffer, buffer.Length);
			return IscHelper.ParseDatabaseInfo(buffer);
		}

		#endregion

		#region Response Methods

		protected void ProcessResponse(IResponse response)
		{
			if (response is GenericResponse genericResponse)
			{
				if (genericResponse.Exception != null && !genericResponse.Exception.IsWarning)
				{
					throw genericResponse.Exception;
				}
			}
		}

		protected void ProcessResponseWarnings(IResponse response)
		{
			if (response is GenericResponse genericResponse)
			{
				if (genericResponse.Exception != null && genericResponse.Exception.IsWarning)
				{
					_warningMessage?.Invoke(genericResponse.Exception);
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
		public virtual Task<int> NextOperationAsync()
		{
			return _xdrStream.ReadNextOperationAsync();
		}

		public virtual IResponse ReadResponse()
		{
			var response = ReadSingleResponse();

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

		#endregion

		#region Protected Methods

		protected virtual IResponse ReadSingleResponse()
		{
			var operation = ReadOperation();

			var response = GdsConnection.ProcessOperation(operation, XdrStream);

			ProcessResponseWarnings(response);

			return response;
		}

		private void DatabaseInfo(byte[] items, byte[] buffer, int bufferLength)
		{
			try
			{
				XdrStream.Write(IscCodes.op_info_database);
				XdrStream.Write(_handle);
				XdrStream.Write(Incarnation);
				XdrStream.WriteBuffer(items, items.Length);
				XdrStream.Write(bufferLength);

				XdrStream.Flush();

				var response = (GenericResponse)ReadResponse();

				var responseLength = bufferLength;

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
