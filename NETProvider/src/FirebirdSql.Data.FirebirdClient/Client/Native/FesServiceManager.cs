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
 *   
 * Contributors:
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.IO;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native
{
	internal sealed class FesServiceManager : IServiceManager
	{
		#region Fields

		private IFbClient fbClient;
		private int handle;
		private IntPtr[] statusVector;
		private Charset charset;

		#endregion

		#region Properties

		public int Handle
		{
			get { return this.handle; }
		}

		public Charset Charset
		{
			get { return this.charset; }
			set { this.charset = value; }
		}

		#endregion

		#region Constructors

		public FesServiceManager()
			: this(null, null)
		{
		}

		public FesServiceManager(string dllName, Charset charset)
		{
			this.fbClient = FbClientFactory.GetFbClient(dllName);
			this.charset = (charset != null ? charset : Charset.DefaultCharset);
			this.statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region Methods

		public void Attach(ServiceParameterBuffer spb, string dataSource, int port, string service)
		{
			// Clear the status vector
			this.ClearStatusVector();

			int svcHandle = this.Handle;

			fbClient.isc_service_attach(
				this.statusVector,
				(short)service.Length,
				service,
				ref	svcHandle,
				(short)spb.Length,
				spb.ToArray());

			// Parse status	vector
			this.ParseStatusVector(this.statusVector);

			// Update status vector
			this.handle = svcHandle;
		}

		public void Detach()
		{
			// Clear the status vector
			this.ClearStatusVector();

			int svcHandle = this.Handle;

			fbClient.isc_service_detach(this.statusVector, ref svcHandle);

			// Parse status	vector
			this.ParseStatusVector(this.statusVector);

			// Update status vector
			this.handle = svcHandle;
		}

		public void Start(ServiceParameterBuffer spb)
		{
			// Clear the status vector
			this.ClearStatusVector();

			int svcHandle = this.Handle;
			int reserved = 0;

			fbClient.isc_service_start(
				this.statusVector,
				ref	svcHandle,
				ref	reserved,
				(short)spb.Length,
				spb.ToArray());

			// Parse status	vector
			this.ParseStatusVector(this.statusVector);
		}

		public void Query(
			ServiceParameterBuffer spb,
			int requestLength,
			byte[] requestBuffer,
			int bufferLength,
			byte[] buffer)
		{
			// Clear the status vector
			this.ClearStatusVector();

			int svcHandle = this.Handle;
			int reserved = 0;

			fbClient.isc_service_query(
				this.statusVector,
				ref	svcHandle,
				ref	reserved,
				(short)spb.Length,
				spb.ToArray(),
				(short)requestLength,
				requestBuffer,
				(short)buffer.Length,
				buffer);

			// Parse status	vector
			this.ParseStatusVector(this.statusVector);
		}

		#endregion

		#region Private Methods

		private void ClearStatusVector()
		{
			Array.Clear(this.statusVector, 0, this.statusVector.Length);
		}

		private void ParseStatusVector(IntPtr[] statusVector)
		{
			IscException ex = FesConnection.ParseStatusVector(statusVector, this.charset);

			if (ex != null && !ex.IsWarning)
			{
				throw ex;
			}
		}

		#endregion
	}
}
