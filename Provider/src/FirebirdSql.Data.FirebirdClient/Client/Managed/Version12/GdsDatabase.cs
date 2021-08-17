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

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version12
{
	internal class GdsDatabase : Version11.GdsDatabase
	{
		public GdsDatabase(GdsConnection connection)
			: base(connection)
		{ }

		protected override void SendAttachToBuffer(DatabaseParameterBufferBase dpb, string database)
		{
			Xdr.Write(IscCodes.op_attach);
			Xdr.Write(0);
			if (!string.IsNullOrEmpty(AuthBlock.Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, AuthBlock.Password);
			}
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
			Xdr.WriteBuffer(Encoding.UTF8.GetBytes(database));
			Xdr.WriteBuffer(dpb.ToArray());
		}
		protected override async ValueTask SendAttachToBufferAsync(DatabaseParameterBufferBase dpb, string database, CancellationToken cancellationToken = default)
		{
			await Xdr.WriteAsync(IscCodes.op_attach, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(0, cancellationToken).ConfigureAwait(false);
			if (!string.IsNullOrEmpty(AuthBlock.Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, AuthBlock.Password);
			}
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
			await Xdr.WriteBufferAsync(Encoding.UTF8.GetBytes(database), cancellationToken).ConfigureAwait(false);
			await Xdr.WriteBufferAsync(dpb.ToArray(), cancellationToken).ConfigureAwait(false);
		}

		protected override void SendCreateToBuffer(DatabaseParameterBufferBase dpb, string database)
		{
			Xdr.Write(IscCodes.op_create);
			Xdr.Write(0);
			if (!string.IsNullOrEmpty(AuthBlock.Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, AuthBlock.Password);
			}
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
			Xdr.WriteBuffer(Encoding.UTF8.GetBytes(database));
			Xdr.WriteBuffer(dpb.ToArray());
		}
		protected override async ValueTask SendCreateToBufferAsync(DatabaseParameterBufferBase dpb, string database, CancellationToken cancellationToken = default)
		{
			await Xdr.WriteAsync(IscCodes.op_create, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(0, cancellationToken).ConfigureAwait(false);
			if (!string.IsNullOrEmpty(AuthBlock.Password))
			{
				dpb.Append(IscCodes.isc_dpb_password, AuthBlock.Password);
			}
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
			await Xdr.WriteBufferAsync(Encoding.UTF8.GetBytes(database), cancellationToken).ConfigureAwait(false);
			await Xdr.WriteBufferAsync(dpb.ToArray(), cancellationToken).ConfigureAwait(false);
		}

		public override StatementBase CreateStatement()
		{
			return new GdsStatement(this);
		}

		public override StatementBase CreateStatement(TransactionBase transaction)
		{
			return new GdsStatement(this, transaction);
		}

		public override void CancelOperation(int kind)
		{
			try
			{
				SendCancelOperationToBuffer(kind);
				Xdr.Flush();
				// no response, this is out-of-band
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}
		public override async ValueTask CancelOperationAsync(int kind, CancellationToken cancellationToken = default)
		{
			try
			{
				await SendCancelOperationToBufferAsync(kind, cancellationToken).ConfigureAwait(false);
				await Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
				// no response, this is out-of-band
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}

		protected void SendCancelOperationToBuffer(int kind)
		{
			Xdr.Write(IscCodes.op_cancel);
			Xdr.Write(kind);
		}
		protected async ValueTask SendCancelOperationToBufferAsync(int kind, CancellationToken cancellationToken = default)
		{
			await Xdr.WriteAsync(IscCodes.op_cancel, cancellationToken).ConfigureAwait(false);
			await Xdr.WriteAsync(kind, cancellationToken).ConfigureAwait(false);
		}
	}
}
