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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FirebirdSql.Data.Common;

internal static class Extensions
{
	extension(IntPtr ptr)
	{
		public int AsInt()
		{
			return (int)ptr.ToInt64();
		}
	}

	extension(BinaryReader binaryReader)
	{
		public IntPtr ReadIntPtr()
		{
			if (IntPtr.Size == sizeof(int))
			{
				return new IntPtr(binaryReader.ReadInt32());
			}
			else if (IntPtr.Size == sizeof(long))
			{
				return new IntPtr(binaryReader.ReadInt64());
			}
			else
			{
				throw new NotSupportedException();
			}
		}
	}

	extension(byte[] b)
	{
		public string ToHexString()
		{
			return Convert.ToHexString(b);
		}
	}

	extension<T>(T[] array)
	{
		public IEnumerable<IEnumerable<T>> Split(int size)
		{
			for (var i = 0; i < (float)array.Length / size; i++)
			{
				yield return array.Skip(i * size).Take(size);
			}
		}
	}

	extension(string s)
	{
		public IEnumerable<char[]> EnumerateRunesToChars()
		{
			if (s == null)
				throw new ArgumentNullException(nameof(s));

			return s.EnumerateRunes().Select(r =>
			{
				var result = new char[r.Utf16SequenceLength];
				r.EncodeToUtf16(result);
				return result;
			});
		}
	}

	extension(Encoding)
	{
		public static Encoding GetANSIEncoding()
		{
			try
			{
				return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
			}
			catch (Exception)
			{
				return Encoding.Default;
			}
		}
	}
}
