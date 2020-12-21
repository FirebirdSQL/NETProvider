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
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version13
{
	internal class GdsStatement : Version12.GdsStatement
	{
		#region Constructors

		public GdsStatement(IDatabase db)
			: base(db)
		{ }

		public GdsStatement(IDatabase db, TransactionBase transaction)
			: base(db, transaction)
		{ }

		#endregion

		#region Overriden Methods

		protected override async Task<byte[]> WriteParameters(AsyncWrappingCommonArgs async)
		{
			if (_parameters == null)
				return null;

			using (var ms = new MemoryStream())
			{
				try
				{
					var xdr = new XdrReaderWriter(ms, _database.Charset);

					var bits = new BitArray(_parameters.Count);
					for (var i = 0; i < _parameters.Count; i++)
					{
						var field = _parameters[i];
						bits.Set(i, await field.DbValue.IsDBNull(async).ConfigureAwait(false));
					}
					var buffer = new byte[(int)Math.Ceiling(_parameters.Count / 8d)];
					for (var i = 0; i < buffer.Length * 8; i++)
					{
						var index = i / 8;
						// LSB
						buffer[index] = (byte)((buffer[index] >> 1) | (bits.Length > i && bits[i] ? 1 << 7 : 0));
					}
					await xdr.WriteOpaque(buffer, async).ConfigureAwait(false);

					for (var i = 0; i < _parameters.Count; i++)
					{
						var field = _parameters[i];
						if (await field.DbValue.IsDBNull(async).ConfigureAwait(false))
						{
							continue;
						}
						await WriteRawParameter(xdr, field, async).ConfigureAwait(false);
					}

					await xdr.Flush(async).ConfigureAwait(false);
					return ms.ToArray();
				}
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
				}
			}
		}

		protected override async Task<DbValue[]> ReadRow(AsyncWrappingCommonArgs async)
		{
			var row = new DbValue[_fields.Count];
			try
			{
				if (_fields.Count > 0)
				{
					var nullBytes = await _database.Xdr.ReadOpaque((int)Math.Ceiling(_fields.Count / 8d), async).ConfigureAwait(false);
					var nullBits = new BitArray(nullBytes);
					for (var i = 0; i < _fields.Count; i++)
					{
						if (nullBits.Get(i))
						{
							row[i] = new DbValue(this, _fields[i], null);
						}
						else
						{
							var value = await ReadRawValue(_database.Xdr, _fields[i], async).ConfigureAwait(false);
							row[i] = new DbValue(this, _fields[i], value);
						}
					}
				}
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_network_error, ex);
			}
			return row;
		}

		#endregion
	}
}
