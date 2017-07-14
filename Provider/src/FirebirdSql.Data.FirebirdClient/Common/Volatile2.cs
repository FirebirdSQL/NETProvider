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
using System.Threading;

namespace FirebirdSql.Data.Common
{
	internal static class Volatile2
	{
		public static int Read(ref int location)
		{
#if NET40
			return Thread.VolatileRead(ref location);
#else
			return Volatile.Read(ref location);
#endif
		}

		public static void Write(ref int location, int value)
		{
#if NET40
			Thread.VolatileWrite(ref location, value);
#else
			Volatile.Write(ref location, value);
#endif
		}
	}
}
