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
 *	Copyright (c) 2016 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

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

		protected override byte[] WriteParameters()
		{
			if (_parameters == null)
				return null;

			using (var xdr = new XdrStream(_database.Charset))
			{
				try
				{
					var bits = new BitArray(_parameters.Count);
					for (int i = 0; i < _parameters.Count; i++)
					{
						var field = _parameters[i];
						bits.Set(i, field.DbValue.IsDBNull());
					}
					var buffer = new byte[(int)Math.Ceiling(_parameters.Count / 8d)];
					for (int i = 0; i < buffer.Length * 8; i++)
					{
						var index = i / 8;
						// LSB
						buffer[index] = (byte)((buffer[index] >> 1) | (bits.Length > i && bits[i] ? 1 << 7 : 0));
					}
					xdr.WriteOpaque(buffer);

					for (var i = 0; i < _parameters.Count; i++)
					{
						var field = _parameters[i];
						if (field.DbValue.IsDBNull())
						{
							continue;
						}
						WriteRawParameter(xdr, field);
					}

					return xdr.ToArray();
				}
				catch (IOException ex)
				{
					throw IscException.ForErrorCode(IscCodes.isc_net_write_err, ex);
				}

			}
		}

		protected override DbValue[] ReadRow()
		{
			DbValue[] row = new DbValue[_fields.Count];
			try
			{
				var nullBytes = _database.XdrStream.ReadOpaque((int)Math.Ceiling(_fields.Count / 8d));
				var nullBits = new BitArray(nullBytes);
				for (int i = 0; i < _fields.Count; i++)
				{
					if (nullBits.Get(i))
					{
						row[i] = new DbValue(this, _fields[i], null);
					}
					else
					{
						var value = ReadRawValue(_fields[i]);
						row[i] = new DbValue(this, _fields[i], value);
					}
				}

				return row;
			}
			catch (IOException ex)
			{
				throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
			}
		}

		#endregion
	}
}
