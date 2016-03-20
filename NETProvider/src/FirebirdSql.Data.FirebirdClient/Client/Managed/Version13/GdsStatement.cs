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
 *	Copyright (c) 2010 Jiri Cincura (jiri@cincura.net)
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
				var bits = new BitArray(_parameters.Count);
				for (int i = 0; i < _parameters.Count; i++)
				{
					var field = _parameters[i];
					bits.Set(i, field.DbValue.IsDBNull());
					bits.Set(i, field.DbDataType == DbDataType.Null);
				}
				var buffer = new byte[(int)Math.Ceiling(_parameters.Count / 8d)];
				bits.CopyTo(buffer, 0);
				xdr.WriteOpaque(buffer);

				for (var i = 0; i < _parameters.Count; i++)
				{
					var field = _parameters[i];
					if (field.DbValue.IsDBNull())
					{
						continue;
					}
					try
					{
						if (field.DbDataType != DbDataType.Null)
						{
							field.FixNull();

							switch (field.DbDataType)
							{
								case DbDataType.Char:
									if (field.Charset.IsOctetsCharset)
									{
										xdr.WriteOpaque(field.DbValue.GetBinary(), field.Length);
									}
									else
									{
										string svalue = field.DbValue.GetString();

										if ((field.Length % field.Charset.BytesPerCharacter) == 0 &&
											svalue.Length > field.CharCount)
										{
											throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
										}

										xdr.WriteOpaque(field.Charset.GetBytes(svalue), field.Length);
									}
									break;

								case DbDataType.VarChar:
									if (field.Charset.IsOctetsCharset)
									{
										xdr.WriteOpaque(field.DbValue.GetBinary(), field.Length);
									}
									else
									{
										string svalue = field.DbValue.GetString();

										if ((field.Length % field.Charset.BytesPerCharacter) == 0 &&
											svalue.Length > field.CharCount)
										{
											throw IscException.ForErrorCodes(new[] { IscCodes.isc_arith_except, IscCodes.isc_string_truncation });
										}

										byte[] data = field.Charset.GetBytes(svalue);

										xdr.WriteBuffer(data, data.Length);
									}
									break;

								case DbDataType.SmallInt:
									xdr.Write(field.DbValue.GetInt16());
									break;

								case DbDataType.Integer:
									xdr.Write(field.DbValue.GetInt32());
									break;

								case DbDataType.BigInt:
								case DbDataType.Array:
								case DbDataType.Binary:
								case DbDataType.Text:
									xdr.Write(field.DbValue.GetInt64());
									break;

								case DbDataType.Decimal:
								case DbDataType.Numeric:
									xdr.Write(field.DbValue.GetDecimal(), field.DataType, field.NumericScale);
									break;

								case DbDataType.Float:
									xdr.Write(field.DbValue.GetFloat());
									break;

								case DbDataType.Guid:
									xdr.WriteOpaque(field.DbValue.GetGuid().ToByteArray());
									break;

								case DbDataType.Double:
									xdr.Write(field.DbValue.GetDouble());
									break;

								case DbDataType.Date:
									xdr.Write(field.DbValue.GetDate());
									break;

								case DbDataType.Time:
									xdr.Write(field.DbValue.GetTime());
									break;

								case DbDataType.TimeStamp:
									xdr.Write(field.DbValue.GetDate());
									xdr.Write(field.DbValue.GetTime());
									break;

								case DbDataType.Boolean:
									xdr.Write(Convert.ToBoolean(field.Value));
									break;

								default:
									throw IscException.ForStrParam($"Unknown SQL data type: {field.DataType}.");
							}
						}
					}
					catch (IOException ex)
					{
						throw IscException.ForErrorCode(IscCodes.isc_net_write_err, ex);
					}
				}
				return xdr.ToArray();
			}
		}

		protected override DbValue[] ReadRow()
		{
			DbValue[] row = new DbValue[_fields.Count];
			lock (_database.SyncObject)
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
						try
						{
							var value = ReadRawValue(_fields[i]);
							row[i] = new DbValue(this, _fields[i], value);
						}
						catch (IOException ex)
						{
							throw IscException.ForErrorCode(IscCodes.isc_net_read_err, ex);
						}
					}
				}
			}
			return row;
		}

		#endregion
	}
}
