/*
 *  Firebird BDP - Borland Data provider Firebird
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
 *  Copyright (c) 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Text;

using Borland.Data.Common;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Bdp
{
	public class FbCursor : ISQLCursor
	{
		#region Fields

		private FbCommand		command;
		private Descriptor		fields;
		private DbValue[]		row;
		private bool			isReleased;
		private bool			eof;
		private IscException	lastError;
				
		#endregion

		#region Constructors

		public FbCursor(FbCommand command)
		{
			this.command	= command;
			this.fields		= this.command.GetFieldsDescriptor();
		}

		#endregion

		#region ISQLCursor

#if (DIAMONDBACK)

		public void GetProperty(CursorProps property, out object value)
		{
			value = null;
		}
		
		public void SetProperty(CursorProps property, object value)
		{
		}

#endif

		public short GetColumnCount()
		{
			this.CheckState();

			return this.fields.Count;
		}

		public int GetColumnLength(short index, ref int length)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			length = this.fields[index].Length;

			return 0;
		}

		public int GetColumnName(short index, ref string colName)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			if (this.fields[index].Alias.Length > 0)
			{
				colName = this.fields[index].Alias;
			}
			else
			{
				colName = this.fields[index].Name;
			}

			return 0;
		}

		public int GetColumnType(short index, ref short type, ref short subType)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			type	= (short)BdpTypeHelper.GetBdpType(fields[index].DbDataType);
			subType = (short)BdpTypeHelper.GetBdpSubType(fields[index].DbDataType);

			return 0;
		}

		public int GetColumnTypeName(short index, ref string typeName)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			typeName = TypeHelper.GetDataTypeName(this.fields[index].DbDataType);

			return 0;
		}

		public int GetBlobSize(short index, ref long blobSize, ref int nullInd)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			this.GetIsNull(index, ref nullInd);
			if (nullInd == 0)
			{
				blobSize = this.row[index].GetBinary().Length;
			}

			return 0;
		}

		public int GetBlob(
			short index, ref byte[] buffer, ref int nullInd, int length)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			this.GetIsNull(index, ref nullInd);
			if (nullInd == 0)
			{	
				int		realLength = length;
				byte[]	byteArray  = this.row[index].GetBinary();

				if (length > byteArray.Length)
				{
					realLength = byteArray.Length;
				}
            					
				Buffer.BlockCopy(byteArray, 0, buffer, 0, realLength);
			}

			return 0;
		}

		public int GetVarBytes(
			short index, ref byte[] buffer, ref int nullInd, int length)
		{
			return this.GetBlob(index, ref buffer, ref nullInd, length);
		}

		public int GetByte(short index, ref byte data, ref int nullInd)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			this.GetIsNull(index, ref nullInd);
			if (nullInd == 0)
			{
				data = this.row[index].GetByte();
			}
			
			return 0;
		}

		public int GetChar(short index, ref char data, ref int nullInd)
		{
			this.CheckIndex(index);

			this.GetIsNull(index, ref nullInd);
			if (nullInd == 0)
			{
				data = this.row[index].GetChar();
			}

			return 0;
		}

		public int GetDecimalAsString(
			short index, StringBuilder data, ref int nullInd)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			this.GetIsNull(index, ref nullInd);
			if (nullInd == 0)
			{
				data.Append(this.row[index].GetDecimal().ToString());
			}

			return 0;
		}

		public int GetDouble(short index, ref double data, ref int nullInd)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			this.GetIsNull(index, ref nullInd);
			if (nullInd == 0)
			{
				data = this.row[index].GetDouble();
			}

			return 0;
		}

		public int GetFloat(short index, ref float data, ref int nullInd)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			this.GetIsNull(index, ref nullInd);
			if (nullInd == 0)
			{
				data = this.row[index].GetFloat();
			}

			return 0;
		}

		public int GetShort(short index, ref short data, ref int nullInd)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			this.GetIsNull(index, ref nullInd);
			if (nullInd == 0)
			{
				data = this.row[index].GetInt16();
			}

			return 0;
		}

		public int GetLong(short index, ref int data, ref int nullInd)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			this.GetIsNull(index, ref nullInd);
			if (nullInd == 0)
			{
				data = this.row[index].GetInt32();
			}

			return 0;
		}

		public int GetInt64(short index, ref long data, ref int nullInd)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			this.GetIsNull(index, ref nullInd);
			if (nullInd == 0)
			{
				data = this.row[index].GetInt64();
			}

			return 0;
		}

		public int GetIsNull(short index, ref int nullInd)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			nullInd = this.row[index].IsDBNull() ? 1 : 0;

			return 0;
		}

		public int GetString(short index, ref StringBuilder data, ref int nullInd)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			this.GetIsNull(index, ref nullInd);
			if (nullInd == 0)
			{
				data.Append(this.row[index].GetString());
			}

			return 0;
		}

		public int GetTimeStamp(short index, ref DateTime data, ref int nullInd)
		{
			this.CheckPosition();
			this.CheckIndex(index);

			this.GetIsNull(index, ref nullInd);
			if (nullInd == 0)
			{
				data = this.row[index].GetDateTime();
			}

			return 0;
		}

		public int GetGuid(short index, ref Guid data, ref int nullInd)
		{
			throw new NotSupportedException();
		}

		public int Next()
		{
			this.CheckState();
			this.CheckPosition();

			int result = 0;

			try
			{
				row = this.command.Fetch();
				if (row == null)
				{
					result		= -1;
					this.eof	= true;
				}
			}
			catch (IscException e)
			{
				this.lastError = e;
			}

			return (this.GetErrorCode() != 0 ? this.GetErrorCode() : result);
		}

		public int GetNextResult(out ISQLCursor cursor, ref short resultCols)
		{
			return this.command.GetNextResult(out cursor, ref resultCols);
		}

		public int GetRowsAffected(ref int rowsAffected)
		{
			rowsAffected = this.command.RowsAffected;

			return 0;
		}

		public int Release()
		{
			this.command.CommitImplicitTransaction();
			this.command.Close();
			this.fields		= null;
			this.row		= null;
			this.lastError	= null;
			this.isReleased	= true;
			this.eof		= true;

			return 0;
		}

		public int GetErrorMessage(ref StringBuilder errorMessage)
		{
			if (this.lastError != null)
			{
				errorMessage.Append(this.lastError.Message);

				this.lastError = null;
			}

			return 0;
		}

		#endregion

		#region Internal Methods

		internal object GetValue(int index)
		{
			this.CheckState();
			this.CheckPosition();
			
			return this.row[index].Value;
		}

		internal int GetValues(object[] values)
		{
			this.CheckState();
			this.CheckPosition();
			
			for (int i = 0; i < this.fields.Count; i++)
			{
				values[i] = this.row[i].Value;
			}

			return values.Length;
		}

		#endregion

		#region Private Methods

		private void CheckState()
		{
			if (this.isReleased)
			{
				throw new InvalidOperationException("The cursor is released.");
			}
		}

		private void CheckPosition()
		{
			if (this.eof)
			{
				throw new InvalidOperationException("The are no data to read.");
			}
		}

		private void CheckIndex(int i)
		{
			if (i < 0 || i >= this.fields.Count)
			{
				throw new IndexOutOfRangeException("Could not find specified column in results.");
			}
		}

		private int GetErrorCode()
		{
			return (this.lastError != null ? this.lastError.ErrorCode : 0);
		}

		#endregion
	}
}
