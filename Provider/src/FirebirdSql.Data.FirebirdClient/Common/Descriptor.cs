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
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *
 *  Contributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.IO;

namespace FirebirdSql.Data.Common
{
	/// <summary>
	/// Descriptor of query input and output parameters.
	/// </summary>
	/// <remarks>
	/// This is similar to the XSQLDA structure described
	/// in the Interbase 6.0 API docs.
	/// </remarks>
	internal sealed class Descriptor
	{
		#region Fields

		private short _version;
		private short _count;
		private short _actualCount;
		private DbField[] _fields;

		#endregion

		#region Properties

		public short Version
		{
			get
			{
				return _version;
			}
			set
			{
				_version = value;
			}
		}

		public short Count
		{
			get { return _count; }
		}

		public short ActualCount
		{
			get { return _actualCount; }
			set { _actualCount = value; }
		}

		#endregion

		#region Indexers

		public DbField this[int index]
		{
			get { return _fields[index]; }
		}

		#endregion

		#region Constructors

		public Descriptor(short n)
		{
			_version = IscCodes.SQLDA_VERSION1;
			_count = n;
			_actualCount = n;
			_fields = new DbField[n];

			for (int i = 0; i < n; i++)
			{
				_fields[i] = new DbField();
			}
		}

		#endregion

		#region Methods

		public void ResetValues()
		{
			for (int i = 0; i < _fields.Length; i++)
			{
				_fields[i].Value = null;
			}
		}

		public byte[] ToBlrArray()
		{
			using (MemoryStream blr = new MemoryStream())
			{
				int par_count = Count * 2;

				blr.WriteByte(IscCodes.blr_version5);
				blr.WriteByte(IscCodes.blr_begin);
				blr.WriteByte(IscCodes.blr_message);
				blr.WriteByte(0);
				blr.WriteByte((byte)(par_count & 255));
				blr.WriteByte((byte)(par_count >> 8));

				for (int i = 0; i < _fields.Length; i++)
				{
					int dtype = _fields[i].SqlType;
					int len = _fields[i].Length;

					switch (dtype)
					{
						case IscCodes.SQL_VARYING:
							blr.WriteByte(IscCodes.blr_varying);
							blr.WriteByte((byte)(len & 255));
							blr.WriteByte((byte)(len >> 8));
							break;

						case IscCodes.SQL_TEXT:
							blr.WriteByte(IscCodes.blr_text);
							blr.WriteByte((byte)(len & 255));
							blr.WriteByte((byte)(len >> 8));
							break;

						case IscCodes.SQL_DOUBLE:
							blr.WriteByte(IscCodes.blr_double);
							break;

						case IscCodes.SQL_FLOAT:
							blr.WriteByte(IscCodes.blr_float);
							break;

						case IscCodes.SQL_D_FLOAT:
							blr.WriteByte(IscCodes.blr_d_float);
							break;

						case IscCodes.SQL_TYPE_DATE:
							blr.WriteByte(IscCodes.blr_sql_date);
							break;

						case IscCodes.SQL_TYPE_TIME:
							blr.WriteByte(IscCodes.blr_sql_time);
							break;

						case IscCodes.SQL_TIMESTAMP:
							blr.WriteByte(IscCodes.blr_timestamp);
							break;

						case IscCodes.SQL_BLOB:
							blr.WriteByte(IscCodes.blr_quad);
							blr.WriteByte(0);
							break;

						case IscCodes.SQL_ARRAY:
							blr.WriteByte(IscCodes.blr_quad);
							blr.WriteByte(0);
							break;

						case IscCodes.SQL_LONG:
							blr.WriteByte(IscCodes.blr_long);
							blr.WriteByte((byte)_fields[i].NumericScale);
							break;

						case IscCodes.SQL_SHORT:
							blr.WriteByte(IscCodes.blr_short);
							blr.WriteByte((byte)_fields[i].NumericScale);
							break;

						case IscCodes.SQL_INT64:
							blr.WriteByte(IscCodes.blr_int64);
							blr.WriteByte((byte)_fields[i].NumericScale);
							break;

						case IscCodes.SQL_QUAD:
							blr.WriteByte(IscCodes.blr_quad);
							blr.WriteByte((byte)_fields[i].NumericScale);
							break;

						case IscCodes.SQL_BOOLEAN:
							blr.WriteByte(IscCodes.blr_bool);
							break;

						case IscCodes.SQL_NULL:
							blr.WriteByte(IscCodes.blr_text);
							blr.WriteByte((byte)(len & 255));
							blr.WriteByte((byte)(len >> 8));
							break;
					}

					blr.WriteByte(IscCodes.blr_short);
					blr.WriteByte(0);
				}

				blr.WriteByte(IscCodes.blr_end);
				blr.WriteByte(IscCodes.blr_eoc);

				return blr.ToArray();
			}
		}

		#endregion
	}
}
