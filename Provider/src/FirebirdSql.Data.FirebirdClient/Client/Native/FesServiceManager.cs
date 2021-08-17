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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native
{
	internal sealed class FesServiceManager : ServiceManagerBase
	{
		#region Fields

		private IFbClient _fbClient;
		private IntPtr[] _statusVector;
		private Charset _charset;

		#endregion

		#region Properties

		public Charset Charset
		{
			get { return _charset; }
			set { _charset = value; }
		}

		#endregion

		#region Constructors

		public FesServiceManager(string dllName, Charset charset)
		{
			_fbClient = FbClientFactory.Create(dllName);
			_charset = charset ?? Charset.DefaultCharset;
			_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region Methods

		public override void Attach(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey)
		{
			FesDatabase.CheckCryptKeyForSupport(cryptKey);

			ClearStatusVector();

			var svcHandle = Handle;

			_fbClient.isc_service_attach(
				_statusVector,
				(short)service.Length,
				service,
				ref svcHandle,
				spb.Length,
				spb.ToArray());

			ProcessStatusVector(_statusVector);

			Handle = svcHandle;
		}
		public override ValueTask AttachAsync(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey, CancellationToken cancellationToken = default)
		{
			FesDatabase.CheckCryptKeyForSupport(cryptKey);

			ClearStatusVector();

			var svcHandle = Handle;

			_fbClient.isc_service_attach(
				_statusVector,
				(short)service.Length,
				service,
				ref svcHandle,
				spb.Length,
				spb.ToArray());

			ProcessStatusVector(_statusVector);

			Handle = svcHandle;

			return ValueTask2.CompletedTask;
		}

		public override void Detach()
		{
			ClearStatusVector();

			var svcHandle = Handle;

			_fbClient.isc_service_detach(_statusVector, ref svcHandle);

			ProcessStatusVector(_statusVector);

			Handle = svcHandle;
		}
		public override ValueTask DetachAsync(CancellationToken cancellationToken = default)
		{
			ClearStatusVector();

			var svcHandle = Handle;

			_fbClient.isc_service_detach(_statusVector, ref svcHandle);

			ProcessStatusVector(_statusVector);

			Handle = svcHandle;

			return ValueTask2.CompletedTask;
		}

		public override void Start(ServiceParameterBufferBase spb)
		{
			ClearStatusVector();

			var svcHandle = Handle;
			var reserved = 0;

			_fbClient.isc_service_start(
				_statusVector,
				ref svcHandle,
				ref reserved,
				spb.Length,
				spb.ToArray());

			ProcessStatusVector(_statusVector);
		}
		public override ValueTask StartAsync(ServiceParameterBufferBase spb, CancellationToken cancellationToken = default)
		{
			ClearStatusVector();

			var svcHandle = Handle;
			var reserved = 0;

			_fbClient.isc_service_start(
				_statusVector,
				ref svcHandle,
				ref reserved,
				spb.Length,
				spb.ToArray());

			ProcessStatusVector(_statusVector);

			return ValueTask2.CompletedTask;
		}

		public override void Query(ServiceParameterBufferBase spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer)
		{
			ClearStatusVector();

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

			ProcessStatusVector(_statusVector);
		}
		public override ValueTask QueryAsync(ServiceParameterBufferBase spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer, CancellationToken cancellationToken = default)
		{
			ClearStatusVector();

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

			ProcessStatusVector(_statusVector);

			return ValueTask2.CompletedTask;
		}

		public override ServiceParameterBufferBase CreateServiceParameterBuffer()
		{
			return new ServiceParameterBuffer2();
		}

		#endregion

		#region Private Methods

		private void ProcessStatusVector(IntPtr[] statusVector)
		{
			var ex = FesConnection.ParseStatusVector(statusVector, _charset);

			if (ex != null)
			{
				if (ex.IsWarning)
				{
					WarningMessage?.Invoke(ex);
				}
				else
				{
					throw ex;
				}
			}
		}

		private void ClearStatusVector()
		{
			Array.Clear(_statusVector, 0, _statusVector.Length);
		}

		#endregion
	}
}
