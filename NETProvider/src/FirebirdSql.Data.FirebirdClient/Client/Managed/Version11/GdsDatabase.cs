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
 *	 
 *  Contributors:
 *      Vladimir Bodecek
 *      
 *	All Rights Reserved.
 */

using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;
using System.Collections.Generic;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version11
{
	internal class GdsDatabase : Version10.GdsDatabase
	{
		#region Constructors

		public GdsDatabase(Version10.GdsConnection connection)
			: base(connection)
		{
			this.DeferredPackets = new Queue<Action<IResponse>>();
		}

		#endregion

		#region Properties
		public Queue<Action<IResponse>> DeferredPackets { get; private set; }
		#endregion

		#region Override Statement Creation Methods

		public override StatementBase CreateStatement()
		{
			return new GdsStatement(this);
		}

		public override StatementBase CreateStatement(ITransaction transaction)
		{
			return new GdsStatement(this, transaction);
		}

		#endregion

		#region Trusted Auth
		public override void AttachWithTrustedAuth(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
#if (!LINUX)
			lock (this.SyncObject)
			{
				try
				{
					using (SSPIHelper sspiHelper = new SSPIHelper())
					{
						byte[] authData = sspiHelper.InitializeClientSecurity();
						SendTrustedAuthToBuffer(dpb, authData);
						SendAttachToBuffer(dpb, database);
						this.Flush();

						IResponse response = this.ReadResponse();
						ProcessTrustedAuthResponse(sspiHelper, ref response);
						ProcessAttachResponse((GenericResponse)response);
					}
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
#else            
			throw new NotSupportedException();
#endif
		}

#if (!LINUX)
		protected virtual void SendTrustedAuthToBuffer(DatabaseParameterBuffer dpb, byte[] authData)
		{
			dpb.Append(IscCodes.isc_dpb_trusted_auth, authData);
		}

		protected void ProcessTrustedAuthResponse(SSPIHelper sspiHelper, ref IResponse response)
		{
			while (response is AuthResponse)
			{
				byte[] authData = sspiHelper.GetClientSecurity(((AuthResponse)response).Data);
				this.Write(IscCodes.op_trusted_auth);
				this.WriteBuffer(authData);
				this.Flush();
				response = this.ReadResponse();
			}
		}
#endif
		#endregion

		#region Public methods
		public override void ReleaseObject(int op, int id)
		{
			lock (this.SyncObject)
			{
				try
				{
					DoReleaseObjectPacket(op, id);
					this.DeferredPackets.Enqueue(ProcessReleaseObjectResponse);
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public override int ReadOperation()
		{
			ProcessDeferredPackets();
			return base.ReadOperation();
		}

		public override int NextOperation()
		{
			ProcessDeferredPackets();
			return base.NextOperation();
		}
		#endregion

		#region Protected methods
		protected override IResponse ProcessOperation(int operation)
		{
			switch (operation)
			{
				case IscCodes.op_trusted_auth:
					return new AuthResponse(this.ReadBuffer());

				default:
					return base.ProcessOperation(operation);
			}
		}
		#endregion

		#region Private methods
		private void ProcessDeferredPackets()
		{
			if (DeferredPackets.Count > 0)
			{
				// copy it to local collection and clear to not get same processing when the method is hit again from ReadSingleResponse
				Action<IResponse>[] methods = DeferredPackets.ToArray();
				DeferredPackets.Clear();
				foreach (Action<IResponse> method in methods)
				{
					method(ReadSingleResponse());
				}
			}
		}
		#endregion
	}
}
