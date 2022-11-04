/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Net;

namespace FirebirdSql.Data.Common;

internal abstract class ParameterBuffer
{
	private readonly List<byte> _data;

	public short Length => (short)_data.Count;

	protected ParameterBuffer()
	{
		_data = new List<byte>();
	}

	public byte[] ToArray()
	{
		return _data.ToArray();
	}

	public void Append(int type)
	{
		WriteByte(type);
	}

	protected void WriteByte(int value)
	{
		WriteByte((byte)value);
	}

	protected void WriteByte(byte value)
	{
		_data.Add(value);
	}

	protected void Write(byte value)
	{
		WriteByte(value);
	}

	protected void Write(short value)
	{
		if (!BitConverter.IsLittleEndian)
		{
			value = IPAddress.NetworkToHostOrder(value);
		}
		var buffer = BitConverter.GetBytes(value);
		Write(buffer);
	}

	protected void Write(int value)
	{
		if (!BitConverter.IsLittleEndian)
		{
			value = IPAddress.NetworkToHostOrder(value);
		}
		var buffer = BitConverter.GetBytes(value);
		Write(buffer);
	}

	protected void Write(long value)
	{
		if (!BitConverter.IsLittleEndian)
		{
			value = IPAddress.NetworkToHostOrder(value);
		}
		var buffer = BitConverter.GetBytes(value);
		Write(buffer);
	}

	protected void Write(byte[] buffer)
	{
		Write(buffer, 0, buffer.Length);
	}

	protected void Write(byte[] buffer, int offset, int count)
	{
		_data.AddRange(new ArraySegment<byte>(buffer, offset, count));
	}
}
