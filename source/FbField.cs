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
using System.Data;
using FirebirdSql.Data.INGDS;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='xmldoc/fbfield.xml' path='doc/member[@name="T:FbField"]/*'/>
	internal class FbField
	{
		#region METHODS

		/// <include file='xmldoc/fbfield.xml' path='doc/member[@name="M:GetFbTypeName(System.Int32,System.Int32,System.Int32)"]/*'/>
		public static string GetFbTypeName(int sqltype, int sqlscale, 
											int sqlsubtype)
		{			 
			switch (sqltype & ~1)
			{
				case GdsCodes.SQL_TEXT:
					// Char
					return "CHAR";
				
				case GdsCodes.SQL_VARYING:
					// Varchar
					return "VARCHAR";
				
				case GdsCodes.SQL_SHORT:
					// Short/Smallint
					if (sqlscale < 0)
					{
						if (sqlsubtype == 2)
						{
							return "DECIMAL";
						}
						else
						{
							return "NUMERIC";
						}
					}
					else
					{
						return "SMALLINT";
					}

				case GdsCodes.SQL_LONG:
					// Long
					if (sqlscale < 0)
					{
						if (sqlsubtype == 2)
						{
							return "DECIMAL";
						}
						else
						{
							return "NUMERIC";
						}
					}
					else
					{
						return "INTEGER";
					}
				
				case GdsCodes.SQL_FLOAT:
					// Float
					return "FLOAT";
				
				case GdsCodes.SQL_DOUBLE:
				case GdsCodes.SQL_D_FLOAT:
					if (sqlscale < 0)
					{
						if (sqlsubtype == 2)
						{
							return "DECIMAL";
						}
						else
						{
							return "NUMERIC";
						}
					}
					else
					{
						return "DOUBLE";
					}
								
				case GdsCodes.SQL_BLOB:
					// Blob type text / Blob binary
					if (sqlsubtype == 1)
					{
						return "BLOB SUB_TYPE 1";
					}
					else
					{
						return "BLOB";
					}
				
				case GdsCodes.SQL_ARRAY:
					// Array
					return "ARRAY";

				case GdsCodes.SQL_QUAD:
				case GdsCodes.SQL_INT64:
					if (sqlscale < 0)
					{
						if (sqlsubtype == 2)
						{
							return "DECIMAL";
						}
						else
						{
							return "NUMERIC";
						}
					}
					else
					{
						return "BIGINT";
					}
				
				case GdsCodes.SQL_TIMESTAMP:
					// Timestamp
					return "TIMESTAMP";

				case GdsCodes.SQL_TYPE_TIME:			
					// Hora
					return "TIME";

				case GdsCodes.SQL_TYPE_DATE:
					// Date
					return "DATE";

				default:
					throw new SystemException("Invalid data type");
			}
		}

		/// <include file='xmldoc/fbfield.xml' path='doc/member[@name="M:GetFbType(System.Int32,System.Int32,System.Int32)"]/*'/>
		public static FbType GetFbType(int sqltype, int sqlscale, int sqlsubtype)
		{
			switch (sqltype & ~1)
			{
				case GdsCodes.SQL_TEXT:
					// Char
					return FbType.Char;
				
				case GdsCodes.SQL_VARYING:
					// Varchar
					return FbType.VarChar;
				
				case GdsCodes.SQL_SHORT:
					// Short/Smallint
					if (sqlscale < 0)
					{
						if (sqlsubtype == 2)
						{
							return FbType.Decimal;
						}
						else
						{
							return FbType.Numeric;
						}
					}
					else
					{
						return FbType.SmallInt;
					}

				case GdsCodes.SQL_LONG:
					// Long
					if (sqlscale < 0)
					{
						if (sqlsubtype == 2)
						{
							return FbType.Decimal;
						}
						else
						{
							return FbType.Numeric;
						}
					}
					else
					{
						return FbType.Integer;
					}
				
				case GdsCodes.SQL_FLOAT:
					// Float
					return FbType.Float;
				
				case GdsCodes.SQL_DOUBLE:
				case GdsCodes.SQL_D_FLOAT:
					// Doble
					if (sqlscale < 0)
					{
						if (sqlsubtype == 2)
						{
							return FbType.Decimal;
						}
						else
						{
							return FbType.Numeric;
						}
					}
					else
					{
						return FbType.Double;
					}
								
				case GdsCodes.SQL_BLOB:
					// Blob type text / Blob binary
					if (sqlsubtype == 1)
					{
						return FbType.Text;
					}
					else
					{
						return FbType.LongVarBinary;
					}
				
				case GdsCodes.SQL_QUAD:
				case GdsCodes.SQL_INT64:
					if (sqlscale < 0)
					{
						if (sqlsubtype == 2)
						{
							return FbType.Decimal;
						}
						else
						{
							return FbType.Numeric;
						}
					}
					else
					{
						return FbType.BigInt;
					}
				
				case GdsCodes.SQL_TIMESTAMP:
					// Timestamp
					return FbType.TimeStamp;

				case GdsCodes.SQL_TYPE_TIME:			
					// Hora
					return FbType.Time;

				case GdsCodes.SQL_TYPE_DATE:
					// Date
					return FbType.Date;

				case GdsCodes.SQL_ARRAY:
					// Array
					return FbType.Array;

				default:
					throw new SystemException("Invalid data type");
			}
		}

		/// <include file='xmldoc/fbfield.xml' path='doc/member[@name="M:GetSystemTypeFromFbType(System.Int32,System.Int32,System.Int32)"]/*'/>
		public static Type GetSystemTypeFromFbType(int sqltype, int sqlscale,
													int sqlsubtype)
		{
			switch (sqltype & ~1)
			{
				case GdsCodes.SQL_TEXT:
					// Char
					return Type.GetType("System.String");
				
				case GdsCodes.SQL_VARYING:
					// Varchar
					return Type.GetType("System.String");
				
				case GdsCodes.SQL_SHORT:
					// Short/Smallint
					if (sqlscale < 0)
					{												
						return Type.GetType("System.Decimal");												
					}
					else
					{
						return Type.GetType("System.Int16");
					}

				case GdsCodes.SQL_LONG:
					// Long
					if (sqlscale < 0)
					{
						return Type.GetType("System.Decimal");
					}
					else
					{
						return Type.GetType("System.Int32");
					}
				
				case GdsCodes.SQL_FLOAT:
					// Float
					return Type.GetType("System.Double");
				
				case GdsCodes.SQL_DOUBLE:
				case GdsCodes.SQL_D_FLOAT:
					// Doble
					if (sqlscale < 0)
					{
						return Type.GetType("System.Decimal");
					}
					else
					{
						return Type.GetType("System.Double");
					}
								
				case GdsCodes.SQL_BLOB:
					// Blob binary
					if (sqlsubtype == 1)
					{
						return Type.GetType("System.String");
					}
					else
					{
						return Type.GetType("System.Object");
					}
				
				case GdsCodes.SQL_ARRAY:
					// Array
					return Type.GetType("System.Array");

				case GdsCodes.SQL_QUAD:
				case GdsCodes.SQL_INT64:
					if (sqlscale < 0)
					{
						return Type.GetType("System.Decimal");
					}
					else
					{
						return Type.GetType("System.Int64");
					}
				
				case GdsCodes.SQL_TIMESTAMP:
				case GdsCodes.SQL_TYPE_TIME:			
				case GdsCodes.SQL_TYPE_DATE:
					//case GdsCodes.SQL_DATE:
					// Timestamp
					// Hora
					// Fecha
					return Type.GetType("System.DateTime");

				default:
					throw new SystemException("Invalid data type");
			}
		}

		/// <include file='xmldoc/fbfield.xml' path='doc/member[@name="M:GetFbTypeFromDbType(System.Data.DbType)"]/*'/>
		public static int GetFbTypeFromDbType(DbType type)
		{
			switch (type)
			{
				case DbType.Binary:
					return GdsCodes.SQL_BLOB;
				
				case DbType.Boolean:
					return GdsCodes.SQL_SHORT;

				case DbType.UInt16:
				case DbType.Int16:
					return GdsCodes.SQL_SHORT;

				case DbType.UInt32:
				case DbType.Int32:
					return GdsCodes.SQL_LONG;
				
				case DbType.UInt64:
				case DbType.Int64:
					return GdsCodes.SQL_INT64;
								
				case DbType.Single:
					return GdsCodes.SQL_FLOAT;

				case DbType.Double:
					return GdsCodes.SQL_DOUBLE;					

				case DbType.Decimal:
					return GdsCodes.SQL_LONG;

				case DbType.Date:
					return GdsCodes.SQL_TYPE_DATE;

				case DbType.Time:
					return GdsCodes.SQL_TYPE_TIME;

				case DbType.DateTime:
					return GdsCodes.SQL_TIMESTAMP;

				case DbType.String:
					return GdsCodes.SQL_TEXT;

				case DbType.AnsiString:
				case DbType.AnsiStringFixedLength:
					return GdsCodes.SQL_TEXT;

				default:
					throw new SystemException("Value is of unknown data type");
			}
		}

		/// <include file='xmldoc/fbfield.xml' path='doc/member[@name="M:IsNumeric(System.Int32)"]/*'/>
		public static bool IsNumeric(int sqltype)
		{
			switch (sqltype & ~1)
			{
				case GdsCodes.SQL_TEXT:				
				case GdsCodes.SQL_VARYING:
				case GdsCodes.SQL_BLOB:				
				case GdsCodes.SQL_ARRAY:
				case GdsCodes.SQL_TIMESTAMP:
				case GdsCodes.SQL_TYPE_TIME:			
				case GdsCodes.SQL_TYPE_DATE:
					return false;
				
				case GdsCodes.SQL_SHORT:
				case GdsCodes.SQL_LONG:
				case GdsCodes.SQL_FLOAT:
				case GdsCodes.SQL_DOUBLE:
				case GdsCodes.SQL_D_FLOAT:
				case GdsCodes.SQL_QUAD:
				case GdsCodes.SQL_INT64:
					return true;

				default:
					throw new SystemException("Invalid data type");
			}
		}

		/// <include file='xmldoc/fbfield.xml' path='doc/member[@name="M:IsLong(System.Int32)"]/*'/>
		public static bool IsLong(int sqltype)
		{
			switch (sqltype & ~1)
			{
				case GdsCodes.SQL_BLOB:
					return true;

				case GdsCodes.SQL_TEXT:
				case GdsCodes.SQL_VARYING:
				case GdsCodes.SQL_ARRAY:
				case GdsCodes.SQL_TIMESTAMP:
				case GdsCodes.SQL_TYPE_TIME:			
				case GdsCodes.SQL_TYPE_DATE:
				case GdsCodes.SQL_SHORT:
				case GdsCodes.SQL_LONG:
				case GdsCodes.SQL_FLOAT:
				case GdsCodes.SQL_DOUBLE:
				case GdsCodes.SQL_D_FLOAT:
				case GdsCodes.SQL_QUAD:
				case GdsCodes.SQL_INT64:
					return false;

				default:
					throw new SystemException("Invalid data type");
			}
		}

		#endregion
	}
}
