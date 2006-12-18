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
using System.Text;
using System.Collections;
using System.Globalization;

using FirebirdSql.Data.INGDS;
using FirebirdSql.Data.NGDS;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="T:FbResultset"]/*'/>
	internal class FbResultset
	{
		#region FIELDS

		private isc_stmt_handle_impl statement;
		private FbTransaction		 transaction;
		private FbConnection		 connection;

		private object[] row;
		
		#endregion
		
		#region INDEXERS

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="P:Item(System.Int32)"]/*'/>
		public object this[int i]
		{
			get { return GetValue(i); }
		}

		/// <include file='xmldoc/fbdatareader.xml' path='doc/member[@name="P:Item(System.String)"]/*'/>
		public object this[String name]
		{			
			get { return GetValue(GetOrdinal(name)); }
		}

		#endregion

		#region PROPERTIES

		public bool EOF
		{
			get { return row == null ? true : false; }
		}

		#endregion

		#region CONSTRUCTORS

		public FbResultset(FbConnection connection, FbTransaction transaction, 
							isc_stmt_handle_impl statement)
		{
			this.connection		= connection;
			this.transaction	= transaction;
			this.statement		= statement;
		}

		#endregion

		#region METHODS

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="P:FieldCount"]/*'/>
		public int FieldCount
		{
			get { return statement.OutSqlda.sqld; }
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:Fetch"]/*'/>
		public bool Fetch()
		{
			try
			{
				row = connection.IscConnection.GDS.isc_dsql_fetch(
											statement				,
											GdsCodes.SQLDA_VERSION1	,
											statement.OutSqlda);				
			}
			catch(GDSException ex)
			{
				throw new FbException(ex.Message, ex);
			}

			return row == null ? false : true;
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetProviderType(System.Int32)"]/*'/>
		public int GetProviderType(int i)
		{			
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			XSQLVAR sqlvar = statement.OutSqlda.sqlvar[i];

			return (int)FbField.GetFbType(sqlvar.sqltype, sqlvar.sqlscale, 
											sqlvar.sqlsubtype);
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetSubType(System.Int32)"]/*'/>
		public int GetSubtype(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return statement.OutSqlda.sqlvar[i].sqlsubtype;
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetBaseTableName(System.Int32)"]/*'/>
		public string GetBaseTableName(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return statement.OutSqlda.sqlvar[i].relname;
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetName(System.Int32)"]/*'/>
		public string GetName(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			if (statement.OutSqlda.sqlvar[i].aliasname.Length > 0)
			{
				return statement.OutSqlda.sqlvar[i].aliasname;
			}
			else
			{
				return statement.OutSqlda.sqlvar[i].sqlname;
			}
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetBaseColumnName(System.Int32)"]/*'/>
		public String GetBaseColumnName(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return statement.OutSqlda.sqlvar[i].sqlname;
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetSize(System.Int32)"]/*'/>
		public int GetSize(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return statement.OutSqlda.sqlvar[i].sqllen;
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetScale(System.Int32)"]/*'/>
		public int GetScale(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return statement.OutSqlda.sqlvar[i].sqlscale;
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetDataTypeName(System.Int32)"]/*'/>
		public String GetDataTypeName(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return FbField.GetFbTypeName(
					statement.OutSqlda.sqlvar[i].sqltype	,
					statement.OutSqlda.sqlvar[i].sqlscale	,
					statement.OutSqlda.sqlvar[i].sqlsubtype);
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetFieldType(System.Int32)"]/*'/>
		public Type GetFieldType(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return FbField.GetSystemTypeFromFbType(
				statement.OutSqlda.sqlvar[i].sqltype	,
				statement.OutSqlda.sqlvar[i].sqlscale	,
				statement.OutSqlda.sqlvar[i].sqlsubtype);
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetValue(System.Int32)"]/*'/>
		public Object GetValue(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			if (row[i] == null)
			{
				return System.DBNull.Value;
			}

			switch (statement.OutSqlda.sqlvar[i].sqltype & ~1)
			{
				case GdsCodes.SQL_TEXT:
				case GdsCodes.SQL_VARYING:
					// Char
					// Varchar
					return GetString(i);

				case GdsCodes.SQL_SHORT:
					// Short/Smallint
					if (statement.OutSqlda.sqlvar[i].sqlscale < 0)
					{
						return GetDecimal(i);
					}
					else
					{
						return GetInt16(i);
					}

				case GdsCodes.SQL_LONG:
					// Long
					if (statement.OutSqlda.sqlvar[i].sqlscale < 0)
					{
						return GetDecimal(i);
					}
					else
					{
						return GetInt32(i);
					}
				
				case GdsCodes.SQL_FLOAT:
					// Float
					return GetFloat(i);
									
				case GdsCodes.SQL_DOUBLE:
				case GdsCodes.SQL_D_FLOAT:
					// Doble
					return GetDouble(i);
								
				case GdsCodes.SQL_BLOB:
					// Blob binary
					if (statement.OutSqlda.sqlvar[i].sqlsubtype == 1)
					{
						if (row[i] is long)
						{
							row[i] = getClobData((long)row[i]);
						}

						return row[i];
					}
					else
					{
						if (row[i] is long)
						{
							row[i] = getBlobData((long)row[i]);
						}

						return row[i];
					}
				
				case GdsCodes.SQL_QUAD:
				case GdsCodes.SQL_INT64:
					if (statement.OutSqlda.sqlvar[i].sqlscale < 0)
					{
						return GetDecimal(i);
					}
					else
					{
						return GetInt64(i);
					}
									
				case GdsCodes.SQL_TIMESTAMP:
				case GdsCodes.SQL_TYPE_TIME:			
				case GdsCodes.SQL_TYPE_DATE:				
					// Timestamp, Time and Date
					return GetDateTime(i);
				
				case GdsCodes.SQL_ARRAY:	
				default:
					throw new NotSupportedException("Unknown data type");
			}
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetOrdinal(System.String)"]/*'/>
		public int GetOrdinal(string name)
		{
			XSQLDA sqlda = statement.OutSqlda;

			for (int i = 0; i < FieldCount; i++)
			{
				if (0 == _cultureAwareCompare(name, sqlda.sqlvar[i].aliasname))
				{
					return i;
				}
			}
						
			throw new IndexOutOfRangeException("Could not find specified column in results.");
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetBoolean(System.Int32)"]/*'/>
		public bool GetBoolean(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return Convert.ToBoolean(row[i]);
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetByte(System.Int32)"]/*'/>
		public byte GetByte(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return Convert.ToByte(row[i]);
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetBytes(System.Int32,System.Int64,System.Byte[],System.Int32,System.Int32)"]/*'/>
		public long GetBytes(int i, long dataIndex, byte[] buffer, 
								int bufferIndex, int length)
		{
			int bytesRead	= 0;
			int realLength	= length;

			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			if (buffer == null)
			{
				return statement.OutSqlda.sqlvar[i].sqllen;
			}
			
			byte[] byteArray = (byte[])GetValue(i);

			if (length > (byteArray.Length - dataIndex))
			{
				realLength = byteArray.Length - (int)dataIndex;
			}
            					
			System.Array.Copy(byteArray, (int)dataIndex, buffer, 
								bufferIndex, realLength);

			if ((byteArray.Length - dataIndex) < length)
			{
				bytesRead = byteArray.Length - (int)dataIndex;
			}
			else
			{
				bytesRead = length;
			}

			return bytesRead;
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetChar(System.Int32)"]/*'/>
		public char GetChar(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}
			
			return Convert.ToChar(row[i]);
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetChars(System.Int32,System.Int64,System.Char[],System.Int32,System.Int32)"]/*'/>
		public long GetChars(int i, long dataIndex, char[] buffer, 
								int bufferIndex, int length)
		{
			int charsRead	= 0;
			int realLength	= length;

			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			if (buffer == null)
			{
				return statement.OutSqlda.sqlvar[i].sqllen;
			}
			
			char[] charArray = ((string)GetValue(i)).ToCharArray();

			if (length > (charArray.Length - dataIndex))
			{
				realLength = charArray.Length - (int)dataIndex;
			}
            					
			System.Array.Copy(charArray, (int)dataIndex, buffer, 
								bufferIndex, realLength);

			if ( (charArray.Length - dataIndex) < length)
			{
				charsRead = charArray.Length - (int)dataIndex;
			}
			else
			{
				charsRead = length;
			}

			return charsRead;
		}
		
		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetGuid(System.Int32)"]/*'/>
		public Guid GetGuid(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return (Guid)row[i];
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetInt16(System.Int32)"]/*'/>
		public Int16 GetInt16(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return Convert.ToInt16(row[i]);
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetInt32(System.Int32)"]/*'/>
		public Int32 GetInt32(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return Convert.ToInt32(row[i]);
		}
		
		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetInt64(System.Int32)"]/*'/>
		public Int64 GetInt64(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return Convert.ToInt64(row[i]);
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetFloat(System.Int32)"]/*'/>
		public float GetFloat(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return Convert.ToSingle(row[i]);
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetDouble(System.Int32)"]/*'/>
		public double GetDouble(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return Convert.ToDouble(row[i]);
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetString(System.Int32)"]/*'/>
		public String GetString(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			if (!IsDBNull(i))
			{
				return connection.Encoding.GetString((byte[])row[i]);
			}
			else
			{
				return null;
			}
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetDecimal(System.Int32)"]/*'/>
		public Decimal GetDecimal(int i)
		{
			long	divisor = 1;
			decimal returnValue;

			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			if (statement.OutSqlda.sqlvar[i].sqlscale < 0)
			{
				int exp = statement.OutSqlda.sqlvar[i].sqlscale * (-1);
				divisor = (long)System.Math.Pow(10, exp);
			}
			
			switch (statement.OutSqlda.sqlvar[i].sqltype & ~1)
			{
				case GdsCodes.SQL_SHORT:
				case GdsCodes.SQL_LONG:
				case GdsCodes.SQL_QUAD:
				case GdsCodes.SQL_INT64:
					returnValue = Convert.ToDecimal(row[i]) / divisor;
					break;
											
				case GdsCodes.SQL_DOUBLE:
				default:
					returnValue = Convert.ToDecimal(row[i]);
					break;
			}

			return returnValue;
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetDateTime(System.Int32)"]/*'/>
		public DateTime GetDateTime(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return Convert.ToDateTime(row[i]);
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:IsDBNull(System.Int32)"]/*'/>
		public bool IsDBNull(int i)
		{	
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			return statement.OutSqlda.sqlvar[i].sqlind == -1 ? true : false;
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:GetData(System.Int32)"]/*'/>		
		public IDataReader GetData(int i)
		{
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			throw new NotSupportedException("GetChars not supported.");
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:FillOutputParameters(System.Int32)"]/*'/>
		public void FillOutputParameters()
		{
			row = null;
			row = new object[FieldCount];

			for (int i = 0; i < FieldCount; i++)
			{
				row[i] = statement.out_sqlda.sqlvar[i].sqldata;
			}
		}
		
		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:AllowDBNull(System.Int32)"]/*'/>
		public bool AllowDBNull(int i)
		{							
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			XSQLVAR sqlvar = statement.OutSqlda.sqlvar[i];

			return (sqlvar.sqltype & 1) == 1 ? true : false;
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:IsAliased(System.Int32)"]/*'/>
		public bool IsAliased(int i)
		{	
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			XSQLVAR sqlvar = statement.OutSqlda.sqlvar[i];

			return sqlvar.sqlname != sqlvar.aliasname ? true : false;
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:IsExpression(System.Int32)"]/*'/>
		public bool IsExpression(int i)
		{	
			if (i < 0 || i >= FieldCount)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}

			XSQLVAR sqlvar = statement.OutSqlda.sqlvar[i];

			return sqlvar.sqlname == String.Empty ? true : false;
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:getClobData(System.Int64)"]/*'/>
		private string getClobData(long clob_id)
		{
			FbClob clob = new FbClob(connection, transaction, clob_id);

			return clob.Read();
		}

		/// <include file='xmldoc/fbresultset.xml' path='doc/member[@name="M:getBlobData(System.Int64)"]/*'/>
		private byte[] getBlobData(long blob_id)
		{
			FbBlob blob = new FbBlob(connection, transaction, blob_id);

			return blob.Read();
		}

		private int _cultureAwareCompare(string strA, string strB)
		{
			#if (_MONO)
			return strA.ToUpper() == strB.ToUpper() ? 0 : 1;
			#else				
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, 
					CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | 
					CompareOptions.IgnoreCase);
			#endif
		}

		#endregion
	}
}
