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
 *	Copyright (c) 2016 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Runtime.InteropServices;

namespace FirebirdSql.Data.Common
{
	internal static class Marshal2
	{
		public static int SizeOf<T>()
		{
#if NET40
			return Marshal.SizeOf(typeof(T));
#else
			return Marshal.SizeOf<T>();
#endif
		}

		public static T PtrToStructure<T>(IntPtr ptr)
		{
#if NET40
			return (T)Marshal.PtrToStructure(ptr, typeof(T));
#else
			return Marshal.PtrToStructure<T>(ptr);
#endif
		}

		public static void DestroyStructure<T>(IntPtr ptr)
		{
#if NET40
			Marshal.DestroyStructure(ptr, typeof(T));
#else
			Marshal.DestroyStructure<T>(ptr);
#endif
		}
	}
}
