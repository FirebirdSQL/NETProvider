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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net), Vladimir Bodecek

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Client.Managed.Sspi;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version11;

internal class GdsDatabase : Version10.GdsDatabase
{
	private readonly Queue<(Action<IResponse>, Func<IResponse, CancellationToken, ValueTask>)> _deferredPackets;

	public GdsDatabase(GdsConnection connection)
		: base(connection)
	{
		_deferredPackets = new Queue<(Action<IResponse>, Func<IResponse, CancellationToken, ValueTask>)>();
	}

	public override StatementBase CreateStatement()
	{
		return new GdsStatement(this);
	}

	public override StatementBase CreateStatement(TransactionBase transaction)
	{
		return new GdsStatement(this, (Version10.GdsTransaction)transaction);
	}

	public override void AttachWithTrustedAuth(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		try
		{
			using (var sspiHelper = new SspiHelper())
			{
				var authData = sspiHelper.InitializeClientSecurity();
				SendTrustedAuthToBuffer(dpb, authData);
				SendAttachToBuffer(dpb, database);
				Xdr.Flush();

				var response = ReadResponse();
				response = ProcessTrustedAuthResponse(sspiHelper, response);
				ProcessAttachResponse((GenericResponse)response);
			}
		}
		catch (IscException)
		{
			SafelyDetach();
			throw;
		}
		catch (IOException ex)
		{
			SafelyDetach();
			throw IscException.ForIOException(ex);
		}

		AfterAttachActions();
	}
	public override async ValueTask AttachWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		try
		{
			using (var sspiHelper = new SspiHelper())
			{
				var authData = sspiHelper.InitializeClientSecurity();
				await SendTrustedAuthToBufferAsync(dpb, authData, cancellationToken).ConfigureAwait(false);
				await SendAttachToBufferAsync(dpb, database, cancellationToken).ConfigureAwait(false);
				await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

				var response = await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
				response = await ProcessTrustedAuthResponseAsync(sspiHelper, response, cancellationToken).ConfigureAwait(false);
				await ProcessAttachResponseAsync((GenericResponse)response, cancellationToken).ConfigureAwait(false);
			}
		}
		catch (IscException)
		{
			await SafelyDetachAsync(cancellationToken).ConfigureAwait(false);
			throw;
		}
		catch (IOException ex)
		{
			await SafelyDetachAsync(cancellationToken).ConfigureAwait(false);
			throw IscException.ForIOException(ex);
		}

		await AfterAttachActionsAsync(cancellationToken).ConfigureAwait(false);
	}

	protected virtual void SendTrustedAuthToBuffer(DatabaseParameterBufferBase dpb, byte[] authData)
	{
		dpb.Append(IscCodes.isc_dpb_trusted_auth, authData);
	}
	protected virtual ValueTask SendTrustedAuthToBufferAsync(DatabaseParameterBufferBase dpb, byte[] authData, CancellationToken cancellationToken = default)
	{
		dpb.Append(IscCodes.isc_dpb_trusted_auth, authData);
		return ValueTask.CompletedTask;
	}

	protected IResponse ProcessTrustedAuthResponse(SspiHelper sspiHelper, IResponse response)
	{
		while (response is AuthResponse authResponse)
		{
			var authData = sspiHelper.GetClientSecurity(authResponse.Data);
			Xdr.Write(IscCodes.op_trusted_auth);
			Xdr.WriteBuffer(authData);
			Xdr.Flush();
			response = ReadResponse();
		}
		return response;
	}
	protected async ValueTask<IResponse> ProcessTrustedAuthResponseAsync(SspiHelper sspiHelper, IResponse response, CancellationToken cancellationToken = default)
	{
		while (response is AuthResponse authResponse)
		{
			var authData = sspiHelper.GetClientSecurity(authResponse.Data);
			await Xdr.WriteAsync(IscCodes.op_trusted_auth, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteBufferAsync(authData, cancellationToken).ConfigureAwait(false);
			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			response = await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
		}
		return response;
	}

	public override void CreateDatabaseWithTrustedAuth(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
	{
		using (var sspiHelper = new SspiHelper())
		{
			var authData = sspiHelper.InitializeClientSecurity();
			SendTrustedAuthToBuffer(dpb, authData);
			SendCreateToBuffer(dpb, database);
			Xdr.Flush();

			var response = ReadResponse();
			response = ProcessTrustedAuthResponse(sspiHelper, response);
			ProcessCreateResponse((GenericResponse)response);
		}
	}
	public override async ValueTask CreateDatabaseWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		using (var sspiHelper = new SspiHelper())
		{
			var authData = sspiHelper.InitializeClientSecurity();
			await SendTrustedAuthToBufferAsync(dpb, authData, cancellationToken).ConfigureAwait(false);
			await SendCreateToBufferAsync(dpb, database, cancellationToken).ConfigureAwait(false);
			await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			var response = await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
			response = await ProcessTrustedAuthResponseAsync(sspiHelper, response, cancellationToken).ConfigureAwait(false);
			await ProcessCreateResponseAsync((GenericResponse)response, cancellationToken).ConfigureAwait(false);
		}
	}

	public override void ReleaseObject(int op, int id)
	{
		try
		{
			SendReleaseObjectToBuffer(op, id);
			AppendDeferredPacket(ProcessReleaseObjectResponse);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask ReleaseObjectAsync(int op, int id, CancellationToken cancellationToken = default)
	{
		try
		{
			await SendReleaseObjectToBufferAsync(op, id, cancellationToken).ConfigureAwait(false);
			AppendDeferredPacket(ProcessReleaseObjectResponseAsync);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	public override async ValueTask<int> ReadOperationAsync(CancellationToken cancellationToken = default)
	{
		await ProcessDeferredPacketsAsync(cancellationToken).ConfigureAwait(false);
		return await base.ReadOperationAsync(cancellationToken).ConfigureAwait(false);
	}
	public override int ReadOperation()
	{
		ProcessDeferredPackets();
		return base.ReadOperation();
	}

	public void AppendDeferredPacket(Action<IResponse> packet)
	{
		_deferredPackets.Enqueue((packet, null));
	}
	public void AppendDeferredPacket(Func<IResponse, CancellationToken, ValueTask> packet)
	{
		_deferredPackets.Enqueue((null, packet));
	}

	private void ProcessDeferredPackets()
	{
		if (_deferredPackets.Count > 0)
		{
			// copy it to local collection and clear to not get same processing when the method is hit again from ReadSingleResponse
			var methods = _deferredPackets.ToArray();
			_deferredPackets.Clear();
			foreach (var (method, methodAsync) in methods)
			{
				var response = ReadSingleResponse();
				if (method != null)
				{
					method(response);
					continue;
				}
				if (methodAsync != null)
				{
					methodAsync(response, CancellationToken.None).GetAwaiter().GetResult();
				}
			}
		}
	}
	private async ValueTask ProcessDeferredPacketsAsync(CancellationToken cancellationToken = default)
	{
		if (_deferredPackets.Count > 0)
		{
			// copy it to local collection and clear to not get same processing when the method is hit again from ReadSingleResponse
			var methods = _deferredPackets.ToArray();
			_deferredPackets.Clear();
			foreach (var (method, methodAsync) in methods)
			{
				var response = await ReadSingleResponseAsync(cancellationToken).ConfigureAwait(false);
				if (method != null)
				{
					method(response);
					continue;
				}
				if (methodAsync != null)
				{
					await methodAsync(response, cancellationToken).ConfigureAwait(false);
				}
			}
		}
	}
}
