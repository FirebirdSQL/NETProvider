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
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 *
 *  Contributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.IO;
using System.Text;
using System.Net;

namespace FirebirdSql.Data.Common
{
	internal abstract class ParameterBuffer
	{
		private MemoryStream _stream;

		public short Length => (short)ToArray().Length;
		protected bool IsLittleEndian { get; }

		protected ParameterBuffer(bool isLittleEndian)
		{
			_stream = new MemoryStream();
			IsLittleEndian = isLittleEndian;
		}

		public virtual void Append(int type)
		{
			WriteByte(type);
		}

		public byte[] ToArray()
		{
			return _stream.ToArray();
		}

		protected void WriteByte(int value)
		{
			WriteByte((byte)value);
		}

		protected void WriteByte(byte value)
		{
			_stream.WriteByte(value);
		}

		protected void Write(byte value)
		{
			WriteByte(value);
		}

		protected void Write(short value)
		{
			if (!IsLittleEndian)
			{
				value = (short)IPAddress.NetworkToHostOrder(value);
			}

			byte[] buffer = BitConverter.GetBytes(value);

			_stream.Write(buffer, 0, buffer.Length);
		}

		protected void Write(int value)
		{
			if (!IsLittleEndian)
			{
				value = (int)IPAddress.NetworkToHostOrder(value);
			}

			byte[] buffer = BitConverter.GetBytes(value);

			_stream.Write(buffer, 0, buffer.Length);
		}

		protected void Write(byte[] buffer)
		{
			Write(buffer, 0, buffer.Length);
		}

		protected void Write(byte[] buffer, int offset, int count)
		{
			_stream.Write(buffer, offset, count);
		}
	}
}
