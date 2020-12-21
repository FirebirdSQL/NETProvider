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
using System.Threading.Tasks;

namespace FirebirdSql.Data.Client.Managed.Version12
{
	internal class GdsDatabase : Version11.GdsDatabase
	{
		public GdsDatabase(GdsConnection connection)
			: base(connection)
		{ }

		protected override async Task SendAttachToBuffer(DatabaseParameterBufferBase dpb, string database, AsyncWrappingCommonArgs async)
		{
			await Xdr.Write(IscCodes.op_attach, async).ConfigureAwait(false);
			await Xdr.Write(0, async).ConfigureAwait(false);
			if (!string.IsNullOrEmpty(Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, Password);
			}
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
			await Xdr.WriteBuffer(Encoding.UTF8.GetBytes(database), async).ConfigureAwait(false);
			await Xdr.WriteBuffer(dpb.ToArray(), async).ConfigureAwait(false);
		}

		protected override async Task SendCreateToBuffer(DatabaseParameterBufferBase dpb, string database, AsyncWrappingCommonArgs async)
		{
			await Xdr.Write(IscCodes.op_create, async).ConfigureAwait(false);
			await Xdr.Write(0, async).ConfigureAwait(false);
			if (!string.IsNullOrEmpty(Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, Password);
			}
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
			await Xdr.WriteBuffer(Encoding.UTF8.GetBytes(database), async).ConfigureAwait(false);
			await Xdr.WriteBuffer(dpb.ToArray(), async).ConfigureAwait(false);
		}

		public override StatementBase CreateStatement()
		{
			return new GdsStatement(this);
		}

		public override StatementBase CreateStatement(TransactionBase transaction)
		{
			return new GdsStatement(this, transaction);
		}

		public override async Task CancelOperation(int kind, AsyncWrappingCommonArgs async)
		{
			try
			{
				await SendCancelOperationToBuffer(kind, async).ConfigureAwait(false);
				await Xdr.Flush(async).ConfigureAwait(false);
				// no response, this is async
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
		}

		protected async Task SendCancelOperationToBuffer(int kind, AsyncWrappingCommonArgs async)
		{
			await Xdr.Write(IscCodes.op_cancel, async).ConfigureAwait(false);
			await Xdr.Write(kind, async).ConfigureAwait(false);
		}
	}
}
