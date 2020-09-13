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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net), Vladimir Bodecek

using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;
using System.Collections.Generic;

using FirebirdSql.Data.Common;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Client.Managed.Version11
{
	internal class GdsDatabase : Version10.GdsDatabase
	{
		public GdsDatabase(GdsConnection connection)
			: base(connection)
		{
			DeferredPackets = new Queue<Action<IResponse>>();
		}

		public Queue<Action<IResponse>> DeferredPackets { get; private set; }

		public override StatementBase CreateStatement()
		{
			return new GdsStatement(this);
		}

		public override StatementBase CreateStatement(TransactionBase transaction)
		{
			return new GdsStatement(this, transaction);
		}

		public override void AttachWithTrustedAuth(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey)
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
					ProcessTrustedAuthResponse(sspiHelper, ref response);
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
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}

			AfterAttachActions();
		}

		protected virtual void SendTrustedAuthToBuffer(DatabaseParameterBufferBase dpb, byte[] authData)
		{
			dpb.Append(IscCodes.isc_dpb_trusted_auth, authData);
		}

		protected void ProcessTrustedAuthResponse(SspiHelper sspiHelper, ref IResponse response)
		{
			while (response is AuthResponse)
			{
				var authData = sspiHelper.GetClientSecurity(((AuthResponse)response).Data);
				Xdr.Write(IscCodes.op_trusted_auth);
				Xdr.WriteBuffer(authData);
				Xdr.Flush();
				response = ReadResponse();
			}
		}

		public override void CreateDatabaseWithTrustedAuth(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey)
		{
			using (var sspiHelper = new SspiHelper())
			{
				var authData = sspiHelper.InitializeClientSecurity();
				SendTrustedAuthToBuffer(dpb, authData);
				SendCreateToBuffer(dpb, database);
				Xdr.Flush();

				var response = ReadResponse();
				ProcessTrustedAuthResponse(sspiHelper, ref response);
				ProcessCreateResponse((GenericResponse)response);
			}
		}

		public override void ReleaseObject(int op, int id)
		{
			try
			{
				DoReleaseObjectPacket(op, id);
				DeferredPackets.Enqueue(ProcessReleaseObjectResponse);
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		public override int ReadOperation()
		{
			ProcessDeferredPackets();
			return base.ReadOperation();
		}
		public override Task<int> ReadOperationAsync()
		{
			ProcessDeferredPackets();
			return base.ReadOperationAsync();
		}

		private void ProcessDeferredPackets()
		{
			if (DeferredPackets.Count > 0)
			{
				// copy it to local collection and clear to not get same processing when the method is hit again from ReadSingleResponse
				var methods = DeferredPackets.ToArray();
				DeferredPackets.Clear();
				foreach (var method in methods)
				{
					method(ReadSingleResponse());
				}
			}
		}
	}
}
