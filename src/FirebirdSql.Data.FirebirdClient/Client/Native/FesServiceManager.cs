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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native;

internal sealed class FesServiceManager : ServiceManagerBase
{
	#region Fields

	private static readonly Version Version30 = new Version(3, 0);

	private readonly IFbClient _fbClient;
	private readonly Version _fbClientVersion;
	private IntPtr[] _statusVector;

	#endregion

	#region Properties

	public override bool UseUtf8ParameterBuffer => _fbClientVersion >= Version30;

	#endregion

	#region Constructors

	public FesServiceManager(string dllName, Charset charset)
		: base(charset)
	{
		_fbClient = FbClientFactory.Create(dllName);
		_fbClientVersion = FesConnection.GetClientVersion(_fbClient);
		_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
	}

	#endregion

	#region Methods

	public override void Attach(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey)
	{
		FesDatabase.CheckCryptKeyForSupport(cryptKey);

		StatusVectorHelper.ClearStatusVector(_statusVector);

		var svcHandle = Handle;

		_fbClient.isc_service_attach(
			_statusVector,
			(short)service.Length,
			service,
			ref svcHandle,
			spb.Length,
			spb.ToArray());

		ProcessStatusVector(Charset.DefaultCharset);

		Handle = svcHandle;
	}
	public override ValueTask AttachAsync(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey, CancellationToken cancellationToken = default)
	{
		FesDatabase.CheckCryptKeyForSupport(cryptKey);

		StatusVectorHelper.ClearStatusVector(_statusVector);

		var svcHandle = Handle;

		_fbClient.isc_service_attach(
			_statusVector,
			(short)service.Length,
			service,
			ref svcHandle,
			spb.Length,
			spb.ToArray());

		ProcessStatusVector(Charset.DefaultCharset);

		Handle = svcHandle;

		return ValueTask2.CompletedTask;
	}

	public override void Detach()
	{
		StatusVectorHelper.ClearStatusVector(_statusVector);

		var svcHandle = Handle;

		_fbClient.isc_service_detach(_statusVector, ref svcHandle);

		ProcessStatusVector();

		Handle = svcHandle;
	}
	public override ValueTask DetachAsync(CancellationToken cancellationToken = default)
	{
		StatusVectorHelper.ClearStatusVector(_statusVector);

		var svcHandle = Handle;

		_fbClient.isc_service_detach(_statusVector, ref svcHandle);

		ProcessStatusVector();

		Handle = svcHandle;

		return ValueTask2.CompletedTask;
	}

	public override void Start(ServiceParameterBufferBase spb)
	{
		StatusVectorHelper.ClearStatusVector(_statusVector);

		var svcHandle = Handle;
		var reserved = 0;

		_fbClient.isc_service_start(
			_statusVector,
			ref svcHandle,
			ref reserved,
			spb.Length,
			spb.ToArray());

		ProcessStatusVector();
	}
	public override ValueTask StartAsync(ServiceParameterBufferBase spb, CancellationToken cancellationToken = default)
	{
		StatusVectorHelper.ClearStatusVector(_statusVector);

		var svcHandle = Handle;
		var reserved = 0;

		_fbClient.isc_service_start(
			_statusVector,
			ref svcHandle,
			ref reserved,
			spb.Length,
			spb.ToArray());

		ProcessStatusVector();

		return ValueTask2.CompletedTask;
	}

	public override void Query(ServiceParameterBufferBase spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer)
	{
		StatusVectorHelper.ClearStatusVector(_statusVector);

		var svcHandle = Handle;
		var reserved = 0;

		_fbClient.isc_service_query(
			_statusVector,
			ref svcHandle,
			ref reserved,
			spb.Length,
			spb.ToArray(),
			(short)requestLength,
			requestBuffer,
			(short)buffer.Length,
			buffer);

		ProcessStatusVector();
	}
	public override ValueTask QueryAsync(ServiceParameterBufferBase spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer, CancellationToken cancellationToken = default)
	{
		StatusVectorHelper.ClearStatusVector(_statusVector);

		var svcHandle = Handle;
		var reserved = 0;

		_fbClient.isc_service_query(
			_statusVector,
			ref svcHandle,
			ref reserved,
			spb.Length,
			spb.ToArray(),
			(short)requestLength,
			requestBuffer,
			(short)buffer.Length,
			buffer);

		ProcessStatusVector();

		return ValueTask2.CompletedTask;
	}

	public override ServiceParameterBufferBase CreateServiceParameterBuffer()
	{
		return new ServiceParameterBuffer2(ParameterBufferEncoding);
	}

	#endregion

	#region Private Methods

	private void ProcessStatusVector()
	{
		StatusVectorHelper.ProcessStatusVector(_statusVector, Charset, WarningMessage);
	}

	private void ProcessStatusVector(Charset charset)
	{
		StatusVectorHelper.ProcessStatusVector(_statusVector, charset, WarningMessage);
	}

	#endregion
}
