/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10;

internal class GdsServiceManager : ServiceManagerBase
{
	#region Fields

	private GdsConnection _connection;
	private GdsDatabase _database;

	#endregion

	#region Properties

	public override bool UseUtf8ParameterBuffer => false;

	public GdsConnection Connection
	{
		get { return _connection; }
	}

	public GdsDatabase Database
	{
		get { return _database; }
	}

	#endregion

	#region Constructors

	public GdsServiceManager(GdsConnection connection)
		: base(connection.Charset)
	{
		_connection = connection;
		_database = CreateDatabase(_connection);
		RewireWarningMessage();
	}

	#endregion

	#region Methods

	public override void Attach(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey)
	{
		try
		{
			SendAttachToBuffer(spb, service);
			_database.Xdr.Flush();
			ProcessAttachResponse((GenericResponse)_database.ReadResponse());
		}
		catch (IOException ex)
		{
			_database.Detach();
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask AttachAsync(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		try
		{
			await SendAttachToBufferAsync(spb, service, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			await ProcessAttachResponseAsync((GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
		}
		catch (IOException ex)
		{
			await _database.DetachAsync(cancellationToken).ConfigureAwait(false);
			throw IscException.ForIOException(ex);
		}
	}

	protected virtual void SendAttachToBuffer(ServiceParameterBufferBase spb, string service)
	{
		_database.Xdr.Write(IscCodes.op_service_attach);
		_database.Xdr.Write(GdsDatabase.DatabaseObjectId);
		_database.Xdr.Write(service);
		_database.Xdr.WriteBuffer(spb.ToArray());
	}
	protected virtual async ValueTask SendAttachToBufferAsync(ServiceParameterBufferBase spb, string service, CancellationToken cancellationToken = default)
	{
		await _database.Xdr.WriteAsync(IscCodes.op_service_attach, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteAsync(GdsDatabase.DatabaseObjectId, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteAsync(service, cancellationToken).ConfigureAwait(false);
		await _database.Xdr.WriteBufferAsync(spb.ToArray(), cancellationToken).ConfigureAwait(false);
	}

	protected virtual void ProcessAttachResponse(GenericResponse response)
	{
		Handle = response.ObjectHandle;
	}
	protected virtual ValueTask ProcessAttachResponseAsync(GenericResponse response, CancellationToken cancellationToken = default)
	{
		Handle = response.ObjectHandle;
		return ValueTask.CompletedTask;
	}

	public override void Detach()
	{
		try
		{
			_database.Xdr.Write(IscCodes.op_service_detach);
			_database.Xdr.Write(Handle);
			_database.Xdr.Write(IscCodes.op_disconnect);
			_database.Xdr.Flush();

			Handle = 0;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
		finally
		{
			try
			{
				_connection.Disconnect();
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
			finally
			{
				_database = null;
				_connection = null;
			}
		}
	}
	public override async ValueTask DetachAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await _database.Xdr.WriteAsync(IscCodes.op_service_detach, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(Handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(IscCodes.op_disconnect, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			Handle = 0;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
		finally
		{
			try
			{
				await _connection.DisconnectAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
			finally
			{
				_database = null;
				_connection = null;
			}
		}
	}

	public override void Start(ServiceParameterBufferBase spb)
	{
		try
		{
			_database.Xdr.Write(IscCodes.op_service_start);
			_database.Xdr.Write(Handle);
			_database.Xdr.Write(0);
			_database.Xdr.WriteBuffer(spb.ToArray(), spb.Length);
			_database.Xdr.Flush();

			try
			{
				_database.ReadResponse();
			}
			catch (IscException)
			{
				throw;
			}
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask StartAsync(ServiceParameterBufferBase spb, CancellationToken cancellationToken = default)
	{
		try
		{
			await _database.Xdr.WriteAsync(IscCodes.op_service_start, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(Handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(0, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteBufferAsync(spb.ToArray(), spb.Length, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			try
			{
				await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (IscException)
			{
				throw;
			}
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	public override void Query(ServiceParameterBufferBase spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer)
	{
		try
		{
			_database.Xdr.Write(IscCodes.op_service_info);
			_database.Xdr.Write(Handle);
			_database.Xdr.Write(GdsDatabase.Incarnation);
			_database.Xdr.WriteBuffer(spb.ToArray(), spb.Length);
			_database.Xdr.WriteBuffer(requestBuffer, requestLength);
			_database.Xdr.Write(bufferLength);

			_database.Xdr.Flush();

			var response = (GenericResponse)_database.ReadResponse();

			var responseLength = bufferLength;

			if (response.Data.Length < bufferLength)
			{
				responseLength = response.Data.Length;
			}

			Buffer.BlockCopy(response.Data, 0, buffer, 0, responseLength);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask QueryAsync(ServiceParameterBufferBase spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer, CancellationToken cancellationToken = default)
	{
		try
		{
			await _database.Xdr.WriteAsync(IscCodes.op_service_info, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(Handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(GdsDatabase.Incarnation, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteBufferAsync(spb.ToArray(), spb.Length, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteBufferAsync(requestBuffer, requestLength, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(bufferLength, cancellationToken).ConfigureAwait(false);

			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			var response = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			var responseLength = bufferLength;

			if (response.Data.Length < bufferLength)
			{
				responseLength = response.Data.Length;
			}

			Buffer.BlockCopy(response.Data, 0, buffer, 0, responseLength);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	public override ServiceParameterBufferBase CreateServiceParameterBuffer()
	{
		return new ServiceParameterBuffer2(Database.ParameterBufferEncoding);
	}

	protected virtual GdsDatabase CreateDatabase(GdsConnection connection)
	{
		return new GdsDatabase(connection);
	}

	private void RewireWarningMessage()
	{
		_database.WarningMessage = ex => WarningMessage?.Invoke(ex);
	}

	#endregion
}
