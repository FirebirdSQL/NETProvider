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
 *	Copyright (c) 2017 Gerdus van Zyl
 *	Copyright (c) 2017 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;

namespace FirebirdSql.Data.Common
{
	internal class ReadBuffer
	{
		byte[] _buffer;
		int _readPosition;
		int _writePosition;
		int _length;

		public int Length => _length;

		public ReadBuffer(int bufferSize)
		{
			_buffer = new byte[bufferSize];
			_readPosition = 0;
			_writePosition = 0;
			_length = 0;
		}

		public void AddRange(byte[] source, int count)
		{
			ResizeBufferIfNeeded(count);
			for (var i = 0; i < count; i++)
			{
				if (_writePosition == _buffer.Length)
					_writePosition = 0;
				_buffer[_writePosition] = source[i];
				_writePosition++;
			}
			_length += count;
		}

		public int ReadInto(ref byte[] destination, int offset, int count)
		{
			count = Math.Min(count, _length);
			for (var i = 0; i < count; i++)
			{
				if (_readPosition == _buffer.Length)
					_readPosition = 0;
				destination[offset + i] = _buffer[_readPosition];
				_readPosition++;
			}
			_length -= count;
			return count;
		}

		void ResizeBufferIfNeeded(int count)
		{
			var requiredLength = _length + count;
			if (requiredLength > _buffer.Length)
			{
				var newLength = _buffer.Length * 2;
				while (requiredLength > newLength)
					newLength *= 2;
				var newBuffer = new byte[newLength];
				Array.Copy(_buffer, newBuffer, _buffer.Length);
				_buffer = newBuffer;
			}
		}
	}
}
