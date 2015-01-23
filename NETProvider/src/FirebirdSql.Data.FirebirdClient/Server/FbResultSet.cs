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
 *  Copyright (c) 2005-2006 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Server
{
	public sealed class FbResultSet : IResultSet
	{
		#region Fields

		private DbDataReader reader;

		#endregion

		#region Constructors

		public FbResultSet()
		{
		}

		public FbResultSet(DbDataReader reader)
		{
			this.reader = reader;
		}

		#endregion

		#region Methods

		public void Close()
		{
			if (this.reader != null)
			{
				this.reader.Close();
				this.reader = null;
			}
		}

		public bool Read()
		{
			if (this.reader == null)
			{
				return false;
			}

			return this.reader.Read();
		}

		public object GetValue(int i)
		{
			if (this.reader == null)
			{
				return null;
			}

			// If the field is a blob or a clob we will be returning the Blob ID
			if (this.reader.GetDataTypeName(i).ToLower().StartsWith("blob"))
			{
				return this.reader.GetInt64(i);
			}

			return this.reader.GetValue(i);
		}

		/*
		public bool GetBoolean(int i)
		{
			return this.reader.GetBoolean(i);
		}

		public byte GetByte(int i)
		{
			return this.reader.GetByte(i);
		}

		public char GetChar(int i)
		{
			return this.reader.GetChar(i);
		}
	
		public Guid GetGuid(int i)
		{
			return this.reader.GetGuid(i);
		}

		public Int16 GetInt16(int i)
		{
			return this.reader.GetInt16(i);
		}

		public Int32 GetInt32(int i)
		{
			return this.reader.GetInt32(i);
		}

		public Int64 GetInt64(int i)
		{
			return this.reader.GetInt64(i);
		}

		public float GetFloat(int i)
		{
			return this.reader.GetFloat(i);
		}

		public double GetDouble(int i)
		{
			return this.reader.GetDouble(i);
		}

		public string GetString(int i)
		{
			return this.reader.GetString(i);
		}

		public Decimal GetDecimal(int i)
		{
			return this.reader.GetDecimal(i);
		}

		public DateTime GetDateTime(int i)
		{
			return this.reader.GetDateTime(i);
		}

		public bool IsDBNull(int i)
		{
			return this.reader.IsDBNull(i);
		}
		*/

		#endregion
	}
}
