/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;

namespace FirebirdSql.Data.Common
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class RowDescriptor : ICloneable
	{
		#region Fields

		private short		version;
		private short		count;
		private short		actualCount;
		private DbField[]	fields;
		
		#endregion

		#region Properties

		public short Version
		{
			get { return this.version; }
			set { this.version = value; }
		}

		public short Count
		{
			get { return this.count; }
		}

		public short ActualCount
		{
			get { return this.actualCount; }
			set { this.actualCount = value; }
		}

		#endregion

		#region Indexers

		public DbField this[int index]
		{
			get { return this.fields[index]; }
			set { this.fields[index] = value; }
		}

		#endregion

		#region Constructors

		public RowDescriptor(short n)
		{
			this.version		= IscCodes.SQLDA_VERSION1;
			this.count			= n;
			this.actualCount	= n;
			this.fields			= new DbField[n];
		
			for (int i = 0; i < n; i++)
			{
				this.fields[i] = new DbField();
			}
		}

		#endregion

		#region ICloneable Methods

		public object Clone()
		{
			RowDescriptor descriptor	= new RowDescriptor(this.Count);
			descriptor.Version			= this.version;

			for (int i = 0; i < descriptor.Count; i++ )
			{
				descriptor[i].DataType		= this.fields[i].DataType;
				descriptor[i].NumericScale	= this.fields[i].NumericScale;
				descriptor[i].SubType		= this.fields[i].SubType;
				descriptor[i].Length		= this.fields[i].Length;
				descriptor[i].Value			= this.fields[i].Value;
				descriptor[i].NullFlag		= this.fields[i].NullFlag;
				descriptor[i].Name			= this.fields[i].Name;
				descriptor[i].Relation		= this.fields[i].Relation;
				descriptor[i].Owner			= this.fields[i].Owner;
				descriptor[i].Alias			= this.fields[i].Alias;
			}

			return descriptor;
		}

		#endregion

		#region Methods

		public void ResetValues()
		{
			for (int i = 0; i < this.fields.Length; i++)
			{
				this.fields[i].Value = null;
			}
		}

		public byte[] ToBlrArray()
		{
			int		blr_len = 0;
			byte[]	blr		= null;

			// Determine the BLR length
			blr_len = 8;
			int par_count = 0;

			for (int i = 0; i < this.fields.Length; i++) 
			{
				int dtype = this.fields[i].SqlType;
				switch (dtype)
				{
					case IscCodes.SQL_VARYING:
					case IscCodes.SQL_TEXT:
						blr_len += 3;
						break;

					case IscCodes.SQL_SHORT:
					case IscCodes.SQL_LONG:
					case IscCodes.SQL_INT64:
					case IscCodes.SQL_QUAD:
					case IscCodes.SQL_BLOB:
					case IscCodes.SQL_ARRAY:
						blr_len += 2;
						break;

					default:
						blr_len++;
						break;
				}

				blr_len		+= 2;
				par_count	+= 2;
			}

			blr = new byte[blr_len];

			int n = 0;
			blr[n++] = IscCodes.blr_version5;
			blr[n++] = IscCodes.blr_begin;
			blr[n++] = IscCodes.blr_message;
			blr[n++] = 0;

			blr[n++] = (byte) (par_count & 255);
			blr[n++] = (byte) (par_count >> 8);

			for (int i = 0; i < this.fields.Length; i++) 
			{
				int dtype	= this.fields[i].SqlType;
				int len		= this.fields[i].Length;

				switch (dtype)
				{
					case IscCodes.SQL_VARYING:
						blr[n++] = IscCodes.blr_varying;
						blr[n++] = (byte) (len & 255);
						blr[n++] = (byte) (len >> 8);
						break;

					case IscCodes.SQL_TEXT:
						blr[n++] = IscCodes.blr_text;
						blr[n++] = (byte) (len & 255);
						blr[n++] = (byte) (len >> 8);
						break;

					case IscCodes.SQL_DOUBLE:
						blr[n++] = IscCodes.blr_double;
						break;

					case IscCodes.SQL_FLOAT:
						blr[n++] = IscCodes.blr_float;
						break;

					case IscCodes.SQL_D_FLOAT:
						blr[n++] = IscCodes.blr_d_float;
						break;

					case IscCodes.SQL_TYPE_DATE:
						blr[n++] = IscCodes.blr_sql_date;
						break;

					case IscCodes.SQL_TYPE_TIME:
						blr[n++] = IscCodes.blr_sql_time;
						break;

					case IscCodes.SQL_TIMESTAMP:
						blr[n++] = IscCodes.blr_timestamp;
						break;

					case IscCodes.SQL_BLOB:
						blr[n++] = IscCodes.blr_quad;
						blr[n++] = 0;
						break;

					case IscCodes.SQL_ARRAY:
						blr[n++] = IscCodes.blr_quad;
						blr[n++] = 0;
						break;

					case IscCodes.SQL_LONG:
						blr[n++] = IscCodes.blr_long;
						blr[n++] = (byte)this.fields[i].NumericScale;
						break;

					case IscCodes.SQL_SHORT:
						blr[n++] = IscCodes.blr_short;
						blr[n++] = (byte) this.fields[i].NumericScale;
						break;

					case IscCodes.SQL_INT64:
						blr[n++] = IscCodes.blr_int64;
						blr[n++] = (byte)this.fields[i].NumericScale;
						break;

					case IscCodes.SQL_QUAD:
						blr[n++] = IscCodes.blr_quad;
						blr[n++] = (byte)this.fields[i].NumericScale;
						break;
				}

				blr[n++] = IscCodes.blr_short;
				blr[n++] = 0;
			}

			blr[n++] = IscCodes.blr_end;
			blr[n++] = IscCodes.blr_eoc;

			return blr;
		}

		#endregion
	}
}