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
using System.IO;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native
{
	internal sealed class FesServiceManager : IServiceManager
	{
		#region Fields

		private IFbClient _fbClient;
		private int _handle;
		private IntPtr[] _statusVector;
		private Charset _charset;

		#endregion

		#region Properties

		public int Handle
		{
			get { return _handle; }
		}

		public Charset Charset
		{
			get { return _charset; }
			set { _charset = value; }
		}

		#endregion

		#region Constructors

		public FesServiceManager(string dllName, Charset charset)
		{
			_fbClient = FbClientFactory.GetFbClient(dllName);
			_charset = charset ?? Charset.DefaultCharset;
			_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region Methods

		public void Attach(ServiceParameterBuffer spb, string dataSource, int port, string service, byte[] cryptKey)
		{
			// ICryptKeyCallbackImpl would have to be passed from C# for 'cryptKey' passing
			if (cryptKey?.Length > 0)
				throw new NotSupportedException("Passing Encryption Key isn't, yet, supported on Firebird Embedded.");

			ClearStatusVector();

			int svcHandle = Handle;

			_fbClient.isc_service_attach(
				_statusVector,
				(short)service.Length,
				service,
				ref	svcHandle,
				spb.Length,
				spb.ToArray());

			ProcessStatusVector(_statusVector);

			_handle = svcHandle;
		}

		public void Detach()
		{
			ClearStatusVector();

			int svcHandle = Handle;

			_fbClient.isc_service_detach(_statusVector, ref svcHandle);

			ProcessStatusVector(_statusVector);

			_handle = svcHandle;
		}

		public void Start(ServiceParameterBuffer spb)
		{
			ClearStatusVector();

			int svcHandle = Handle;
			int reserved = 0;

			_fbClient.isc_service_start(
				_statusVector,
				ref	svcHandle,
				ref	reserved,
				spb.Length,
				spb.ToArray());

			ProcessStatusVector(_statusVector);
		}

		public void Query(
			ServiceParameterBuffer spb,
			int requestLength,
			byte[] requestBuffer,
			int bufferLength,
			byte[] buffer)
		{
			ClearStatusVector();

			int svcHandle = Handle;
			int reserved = 0;

			_fbClient.isc_service_query(
				_statusVector,
				ref	svcHandle,
				ref	reserved,
				spb.Length,
				spb.ToArray(),
				(short)requestLength,
				requestBuffer,
				(short)buffer.Length,
				buffer);

			ProcessStatusVector(_statusVector);
		}

		#endregion

		#region Private Methods

		private void ProcessStatusVector(IntPtr[] statusVector)
		{
			IscException ex = FesConnection.ParseStatusVector(statusVector, _charset);

			if (ex != null)
			{
				if (ex.IsWarning)
				{
#warning This is not propagated to FesDatabase's callback as with GdsDatabase
					//_warningMessage?.Invoke(ex);
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
