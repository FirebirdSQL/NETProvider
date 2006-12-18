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
using System.Text;
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsField
	{
		#region FIELDS

		private int			sqlType;	
		private int			sqlScale;
		private int			sqlSubType;	
		private int			sqlLen;	
		private object		sqlData;
		private int			sqlInd;
		private string		sqlName;
		private string		relName;
		private string		ownerName;
		private string		aliasName;
		private GdsCharset	charset;
		private	int			charCount;
		private GdsArray	arrayHandle;

		#endregion

		#region Properties

		public int SqlType
		{
			get { return this.sqlType; }
			set { this.sqlType = value; }
		}

		public int SqlScale
		{
			get { return this.sqlScale; }
			set { this.sqlScale = value; }
		}

		public int SqlSubType
		{
			get { return this.sqlSubType; }
			set 
			{ 
				if (IsCharacter())
				{
					// Bits 0-7 of sqlsubtype is charset_id (127 is a special value -
					// current attachment charset).
					// Bits 8-17 hold collation_id for this value.
					byte[] cs = BitConverter.GetBytes(value);

					int index = GdsDbAttachment.CharSets.IndexOf(cs[0]);
					if (index != -1)
					{
						this.charset = GdsDbAttachment.CharSets[index];
					}
					else
					{
						this.charset = GdsDbAttachment.CharSets[0];
					}
				}
								
				this.sqlSubType = value;
			}
		}

		public int SqlLen
		{
			get { return this.sqlLen; }
			set 
			{ 
				this.sqlLen = value; 
				if (this.IsCharacter())
				{
					this.charCount	= this.sqlLen / this.charset.BytesPerCharacter;
				}
			}
		}

		public object SqlData
		{
			get { return this.sqlData; }
			set { this.sqlData = value; }
		}

		public int SqlInd
		{
			get { return this.sqlInd; }
			set { this.sqlInd = value; }
		}

		public string SqlName
		{
			get { return this.sqlName; }
			set { this.sqlName = value; }
		}

		public string RelName
		{
			get { return this.relName; }
			set { this.relName = value; }
		}

		public string OwnerName
		{
			get { return this.ownerName; }
			set { this.ownerName = value; }
		}

		public string AliasName
		{
			get { return this.aliasName; }
			set { this.aliasName = value; }
		}

		public GdsCharset Charset
		{
			get { return this.charset; }
		}

		public int CharCount
		{
			get { return this.charCount; }
			set { this.charCount = value; }
		}

		public GdsArray ArrayHandle
		{
			get
			{
				if (this.IsArray())
				{
					return this.arrayHandle;
				}
				else
				{
					throw new GdsException("Field is not an array type");
				}
			}

			set
			{
				if (this.IsArray())
				{
					this.arrayHandle = value;
				}
				else
				{
					throw new GdsException("Field is not an array type");
				}
			}
		}

		#endregion

		#region Constructors
		
		public GdsField() 
		{
			this.charCount = -1;
		}

		/*
		public GdsField(object sqlData) : this()
		{
			this.sqlData = sqlData;
		}
		*/

		#endregion

		#region STATIC_METHODS

		public static FbDbType GetFbTypeFromBlr(int blrType, int subType, int scale)
		{
			switch (blrType)
			{
				case GdsCodes.blr_varying:
				case GdsCodes.blr_varying2:
					return FbDbType.VarChar;

				case GdsCodes.blr_text:
				case GdsCodes.blr_text2:
					return FbDbType.Char;

				case GdsCodes.blr_cstring:
				case GdsCodes.blr_cstring2:
					return FbDbType.Text;

				case GdsCodes.blr_short:
					if (scale < 0)
					{
						return FbDbType.Decimal;
					}
					else
					{
						return FbDbType.SmallInt;
					}					

				case GdsCodes.blr_long:
					if (scale < 0)
					{
						return FbDbType.Decimal;
					}
					else
					{
						return FbDbType.Integer;
					}

				case GdsCodes.blr_quad:
				case GdsCodes.blr_int64:
				case GdsCodes.blr_blob_id:
					if (scale < 0)
					{
						return FbDbType.Decimal;
					}
					else
					{
						return FbDbType.BigInt;
					}

				case GdsCodes.blr_double:
				case GdsCodes.blr_d_float:
					return FbDbType.Double;

				case GdsCodes.blr_float:
					return FbDbType.Float;

				case GdsCodes.blr_sql_date:
					return FbDbType.Date;

				case GdsCodes.blr_sql_time:
					return FbDbType.Time;

				case GdsCodes.blr_timestamp:
					return FbDbType.TimeStamp;
				
				case GdsCodes.blr_blob:
					if (subType == 1)
					{
						return FbDbType.Text;
					}
					else
					{
						return FbDbType.Binary;
					}

				default:
					throw new SystemException("Invalid data type");
			}
		}

		#endregion

		#region METHODS

		public string GetDataTypeName()
		{			 
			switch (this.sqlType & ~1)
			{
				case GdsCodes.SQL_TEXT:
					// Char
					return "CHAR";
				
				case GdsCodes.SQL_VARYING:
					// Varchar
					return "VARCHAR";
				
				case GdsCodes.SQL_SHORT:
					// Short/Smallint
					if (this.sqlScale < 0)
					{
						if (this.sqlSubType == 2)
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
					if (this.sqlScale < 0)
					{
						if (this.sqlSubType == 2)
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
					if (this.sqlScale < 0)
					{
						if (this.sqlSubType == 2)
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
					if (this.sqlSubType == 1)
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
					if (this.sqlScale < 0)
					{
						if (this.sqlSubType == 2)
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

		public Type GetSystemType()
		{
			switch (this.sqlType & ~1)
			{
				case GdsCodes.SQL_TEXT:
					// Char
					return Type.GetType("System.String");
				
				case GdsCodes.SQL_VARYING:
					// Varchar
					return Type.GetType("System.String");
				
				case GdsCodes.SQL_SHORT:
					// Short/Smallint
					if (this.sqlScale < 0)
					{												
						return Type.GetType("System.Decimal");												
					}
					else
					{
						return Type.GetType("System.Int16");
					}

				case GdsCodes.SQL_LONG:
					// Long
					if (this.sqlScale < 0)
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
					if (this.sqlScale < 0)
					{
						return Type.GetType("System.Decimal");
					}
					else
					{
						return Type.GetType("System.Double");
					}
								
				case GdsCodes.SQL_BLOB:
					// Blob binary
					if (this.sqlSubType == 1)
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
					if (this.sqlScale < 0)
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
					return Type.GetType("System.DateTime");

				default:
					throw new SystemException("Invalid data type");
			}
		}

		public FbDbType GetFbDbType()
		{
			switch (sqlType & ~1)
			{
				case GdsCodes.SQL_TEXT:
					// Char
					return FbDbType.Char;
				
				case GdsCodes.SQL_VARYING:
					// Varchar
					return FbDbType.VarChar;
				
				case GdsCodes.SQL_SHORT:
					// Short/Smallint
					if (this.sqlScale < 0)
					{
						if (sqlSubType == 2)
						{
							return FbDbType.Decimal;
						}
						else
						{
							return FbDbType.Numeric;
						}
					}
					else
					{
						return FbDbType.SmallInt;
					}

				case GdsCodes.SQL_LONG:
					// Long
					if (this.sqlScale < 0)
					{
						if (this.sqlSubType == 2)
						{
							return FbDbType.Decimal;
						}
						else
						{
							return FbDbType.Numeric;
						}
					}
					else
					{
						return FbDbType.Integer;
					}
				
				case GdsCodes.SQL_FLOAT:
					// Float
					return FbDbType.Float;
				
				case GdsCodes.SQL_DOUBLE:
				case GdsCodes.SQL_D_FLOAT:
					// Doble
					if (this.sqlScale < 0)
					{
						if (this.sqlSubType == 2)
						{
							return FbDbType.Decimal;
						}
						else
						{
							return FbDbType.Numeric;
						}
					}
					else
					{
						return FbDbType.Double;
					}
								
				case GdsCodes.SQL_BLOB:
					// Blob type text / Blob binary
					if (this.sqlSubType == 1)
					{
						return FbDbType.Text;
					}
					else
					{
						return FbDbType.LongVarBinary;
					}
				
				case GdsCodes.SQL_QUAD:
				case GdsCodes.SQL_INT64:
					if (this.sqlScale < 0)
					{
						if (this.sqlSubType == 2)
						{
							return FbDbType.Decimal;
						}
						else
						{
							return FbDbType.Numeric;
						}
					}
					else
					{
						return FbDbType.BigInt;
					}
				
				case GdsCodes.SQL_TIMESTAMP:
					// Timestamp
					return FbDbType.TimeStamp;

				case GdsCodes.SQL_TYPE_TIME:			
					// Hora
					return FbDbType.Time;

				case GdsCodes.SQL_TYPE_DATE:
					// Date
					return FbDbType.Date;

				case GdsCodes.SQL_ARRAY:
					// Array
					return FbDbType.Array;

				default:
					throw new SystemException("Invalid data type");
			}
		}

		public bool IsNumeric()
		{
			switch (this.sqlType & ~1)
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

		public bool IsLong()
		{
			switch (this.sqlType & ~1)
			{
				case GdsCodes.SQL_BLOB:
					return true;

				case GdsCodes.SQL_ARRAY:
				case GdsCodes.SQL_TEXT:
				case GdsCodes.SQL_VARYING:
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

		public bool IsCharacter()
		{
			switch (sqlType & ~1)
			{
				case GdsCodes.SQL_TEXT:
				case GdsCodes.SQL_VARYING:
					return true;

				case GdsCodes.SQL_ARRAY:
				case GdsCodes.SQL_BLOB:
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

		public bool IsArray()
		{
			switch (this.sqlType & ~1)
			{
				case GdsCodes.SQL_ARRAY:
					return true;

				case GdsCodes.SQL_TEXT:
				case GdsCodes.SQL_VARYING:
				case GdsCodes.SQL_BLOB:
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
