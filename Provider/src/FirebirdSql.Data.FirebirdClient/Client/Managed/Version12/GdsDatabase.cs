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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;
using System.Collections.Generic;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version12
{
	internal class GdsDatabase : Version11.GdsDatabase
	{
		public GdsDatabase(GdsConnection connection)
			: base(connection)
		{ }

		protected override void SendAttachToBuffer(DatabaseParameterBuffer dpb, string database)
		{
			XdrStream.Write(IscCodes.op_attach);
			XdrStream.Write(0);
			if (!string.IsNullOrEmpty(Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, Password);
			}
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
			XdrStream.WriteBuffer(Encoding.UTF8.GetBytes(database));
			XdrStream.WriteBuffer(dpb.ToArray());
		}

		protected override void SendCreateToBuffer(DatabaseParameterBuffer dpb, string database)
		{
			XdrStream.Write(IscCodes.op_create);
			XdrStream.Write(0);
			if (!string.IsNullOrEmpty(Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, Password);
			}
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
			XdrStream.WriteBuffer(Encoding.UTF8.GetBytes(database));
			XdrStream.WriteBuffer(dpb.ToArray());
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

		#region Cancel Methods

		public override void CancelOperation(int kind)
		{
			try
			{
				SendCancelOperationToBuffer(kind);
				XdrStream.Flush();
				// no response, this is async
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		protected void SendCancelOperationToBuffer(int kind)
		{
			XdrStream.Write(IscCodes.op_cancel);
			XdrStream.Write(kind);
		}

		#endregion
	}
}
