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

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsValue
	{
		#region FIELDS

		private GdsStatement	statement;
		private GdsField		field;
		private object			value;

		#endregion

		#region PROPERTIES

		public GdsField Field
		{
			get { return field; }
		}

		public object Value
		{
			get { return getValue(); }
		}

		#endregion
		
		#region CONSTRUCTOR

		public GdsValue(GdsStatement statement, GdsField field, object value)
		{
			this.statement	= statement;
			this.field		= field;
			this.value		= value;			
		}

		#endregion

		#region PRIVATE_METHODS

		private object getValue()
		{
			if (value == null)
			{
				return System.DBNull.Value;
			}

			switch (field.SqlType & ~1)
			{
				case GdsCodes.SQL_TEXT:
				case GdsCodes.SQL_VARYING:
					// Char
					// Varchar
					return Convert.ToString(value);

				case GdsCodes.SQL_SHORT:
					// Short/Smallint
					if (field.SqlScale < 0)
					{
						return Convert.ToDecimal(value);
					}
					else
					{
						return Convert.ToInt16(value);
					}

				case GdsCodes.SQL_LONG:
					// Long
					if (field.SqlScale < 0)
					{
						return Convert.ToDecimal(value);
					}
					else
					{
						return Convert.ToInt32(value);
					}
				
				case GdsCodes.SQL_FLOAT:
					// Float
					return Convert.ToSingle(value);
									
				case GdsCodes.SQL_DOUBLE:
				case GdsCodes.SQL_D_FLOAT:
					// Doble
					return Convert.ToDouble(value);
				
				case GdsCodes.SQL_QUAD:
				case GdsCodes.SQL_INT64:
					if (field.SqlScale < 0)
					{
						return Convert.ToDecimal(value);
					}
					else
					{
						return Convert.ToInt64(value);
					}
									
				case GdsCodes.SQL_TIMESTAMP:
				case GdsCodes.SQL_TYPE_TIME:
				case GdsCodes.SQL_TYPE_DATE:
					// Timestamp, Time and Date
					return Convert.ToDateTime(value);
				
				case GdsCodes.SQL_BLOB:
					if (field.SqlSubType == 1)
					{
						// SUB_TYPE TEXT
						if (value is long)
						{
							value = getClobData((long)value);
						}
						return value;
					}
					else
					{
						// SUB_TYPE BINARY
						if (value is long)
						{
							value = getBlobData((long)value);
						}
						return value;
					}
								
				case GdsCodes.SQL_ARRAY:
					if (value is long)
					{
						value = getArrayData((long)value);
					}
					return value;
										
				default:
					throw new NotSupportedException("Unknown data type");
			}
		}

		private string getClobData(long handle)
		{
			GdsAsciiBlob clob = new GdsAsciiBlob(
				statement.DB, 
				statement.Transaction, 
				handle);
			string contents = clob.Read();
			clob.Close();
			
			return contents;
		}

		private byte[] getBlobData(long handle)
		{
			GdsBinaryBlob blob = new GdsBinaryBlob(
				statement.DB, 
				statement.Transaction,
				handle);
			byte[] contents = blob.Read();
			blob.Close();
			
			return contents;
		}

		private object getArrayData(long handle)
		{
			GdsArray gdsArray = new GdsArray(
				statement.DB, 
				statement.Transaction,
				handle,
				field.RelName,
				field.SqlName);
			
			return gdsArray.Read();
		}

		#endregion
	}
}