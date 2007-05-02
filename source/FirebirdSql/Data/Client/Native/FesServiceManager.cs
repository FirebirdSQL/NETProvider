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
 */

using System;
using System.IO;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Embedded
{
	internal sealed class FesServiceManager : IServiceManager
	{
		#region · Fields ·

		private IFbClient   fbClient;
		private int         handle;
        private int[]       statusVector;

		#endregion

		#region · Properties ·

		public int Handle
		{
			get { return this.handle; }
		}

		public bool IsLittleEndian
		{
			get { return BitConverter.IsLittleEndian; }
		}

		#endregion

		#region · Constructors ·

		public FesServiceManager()
			: this(null)
		{
		}

		public FesServiceManager(string dllName)
		{
			this.fbClient       = FbClientFactory.GetFbClient(dllName);
            this.statusVector   = new int[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region · Methods ·

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

			int svcHandle   = this.Handle;
			int reserved    = 0;

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
			ServiceParameterBuffer  spb,
			int                     requestLength,
			byte[]                  requestBuffer,
		    int                     bufferLength,
			byte[]                  buffer)
		{
            // Clear the status vector
            this.ClearStatusVector();

            int svcHandle   = this.Handle;
			int reserved    = 0;

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

		#region · Buffer Creation Methods ·

		public ServiceParameterBuffer CreateParameterBuffer()
		{
			return new ServiceParameterBuffer();
		}

		#endregion

		#region · Private Methods ·

        private void ClearStatusVector()
        {
            Array.Clear(this.statusVector, 0, this.statusVector.Length);
        }

		private void ParseStatusVector(int[] tatusVector)
		{
			IscException ex = FesConnection.ParseStatusVector(this.statusVector);

			if (ex != null && !ex.IsWarning)
			{
				throw ex;
			}
		}

		#endregion
	}
}
