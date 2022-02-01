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
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace FirebirdSql.Data.Common;

internal static class Extensions
{
	public static int AsInt(this IntPtr ptr)
	{
		return (int)ptr.ToInt64();
	}

	public static IntPtr ReadIntPtr(this BinaryReader self)
	{
		if (IntPtr.Size == sizeof(int))
		{
			return new IntPtr(self.ReadInt32());
		}
		else if (IntPtr.Size == sizeof(long))
		{
			return new IntPtr(self.ReadInt64());
		}
		else
		{
			throw new NotSupportedException();
		}
	}

	public static string ToHexString(this byte[] b)
	{
		return BitConverter.ToString(b).Replace("-", string.Empty);
	}

	public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
	{
		for (var i = 0; i < (float)array.Length / size; i++)
		{
			yield return array.Skip(i * size).Take(size);
		}
	}

#if NETSTANDARD2_0
	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) => new HashSet<T>(source);
#endif
}
