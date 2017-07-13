/*
 *  Firebird ADO.NET Data provider for .NET and Mono
 *
 *     The contents of this file are subject to the Initial
 *     Developer's Public License Version 1.0 (the "License");
 *     you may not use this file except in compliance with the
 *     License. You may obtain a copy of the License at
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *     express or implied.  See the License for the specific
 *     language governing rights and limitations under the License.
 *
 *  Copyright (c) 2016 Hajime Nakagami
 *	Copyright (c) 2016 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
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

namespace FirebirdSql.Data.Client.Managed.Version13
{
	internal class GdsDatabase : Version12.GdsDatabase
	{
#warning Refactoring op_attach and op_create.
		public GdsDatabase(GdsConnection connection)
			: base(connection)
		{ }

		public override void Attach(DatabaseParameterBuffer dpb, string dataSource, int port, string database, byte[] cryptKey)
		{
			try
			{
				SendAttachToBuffer(dpb, database);
				XdrStream.Flush();
				var response = ReadResponse();
				while (response is CryptKeyCallbackReponse cryptResponse)
				{
					XdrStream.Write(IscCodes.op_crypt_key_callback);
					XdrStream.WriteBuffer(cryptKey);
					XdrStream.Flush();
					response = ReadResponse();
				}
				ProcessAttachResponse(response as GenericResponse);
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

		protected override void SendAttachToBuffer(DatabaseParameterBuffer dpb, string database)
		{
			XdrStream.Write(IscCodes.op_attach);
			XdrStream.Write(0);
			if (AuthData != null)
			{
				dpb.Append(IscCodes.isc_dpb_specific_auth_data, AuthData);
			}
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
			XdrStream.WriteBuffer(Encoding.UTF8.GetBytes(database));
			XdrStream.WriteBuffer(dpb.ToArray());
		}

		protected override void SendCreateToBuffer(DatabaseParameterBuffer dpb, string database)
		{
			XdrStream.Write(IscCodes.op_create);
			XdrStream.Write(0);
			if (AuthData != null)
			{
				dpb.Append(IscCodes.isc_dpb_specific_auth_data, AuthData);
			}
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
			XdrStream.WriteBuffer(Encoding.UTF8.GetBytes(database));
			XdrStream.WriteBuffer(dpb.ToArray());
		}

		public override void AttachWithTrustedAuth(DatabaseParameterBuffer dpb, string dataSource, int port, string database, byte[] cryptKey)
		{
			Attach(dpb, dataSource, port, database, cryptKey);
		}

		#region Override Statement Creation Methods

		public override StatementBase CreateStatement()
		{
			return new GdsStatement(this);
		}

		public override StatementBase CreateStatement(TransactionBase transaction)
		{
			return new GdsStatement(this, transaction);
		}

		#endregion
	}
}
