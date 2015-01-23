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
 *      
 */

using System;
using System.Runtime.InteropServices;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native
{
	internal sealed class FesConnection
	{
		#region Static Methods

		public static IscException ParseStatusVector(IntPtr[] statusVector, Charset charset)
		{
			IscException exception = null;
			bool eof = false;

			for (int i = 0; i < statusVector.Length; )
			{
				IntPtr arg = statusVector[i++];

				switch (arg.AsInt())
				{
					case IscCodes.isc_arg_gds:
						IntPtr er = statusVector[i++];
						if (er != IntPtr.Zero)
						{
							if (exception == null)
							{
								exception = new IscException();
							}
							exception.Errors.Add(new IscError(arg.AsInt(), er.AsInt()));
						}
						break;

					case IscCodes.isc_arg_end:
						if (exception != null && exception.Errors.Count != 0)
						{
							exception.BuildExceptionData();
						}
						eof = true;
						break;

					case IscCodes.isc_arg_interpreted:
					case IscCodes.isc_arg_string:
						{
							IntPtr ptr = statusVector[i++];
							string s = Marshal.PtrToStringAnsi(ptr);
							string arg_value = charset.GetString(
								System.Text.Encoding.Default.GetBytes(s));

							exception.Errors.Add(new IscError(arg.AsInt(), arg_value));
						}
						break;

					case IscCodes.isc_arg_cstring:
						{
							i++;

							IntPtr ptr = statusVector[i++];
							string s = Marshal.PtrToStringAnsi(ptr);
							string arg_value = charset.GetString(
								System.Text.Encoding.Default.GetBytes(s));

							exception.Errors.Add(new IscError(arg.AsInt(), arg_value));
						}
						break;

					case IscCodes.isc_arg_win32:
					case IscCodes.isc_arg_number:
						exception.Errors.Add(new IscError(arg.AsInt(), statusVector[i++].AsInt()));
						break;

					default:
						IntPtr e = statusVector[i++];
						if (e != IntPtr.Zero)
						{
							if (exception == null)
							{
								exception = new IscException();
							}
							exception.Errors.Add(new IscError(arg.AsInt(), e.AsInt()));
						}
						break;
				}

				if (eof)
				{
					break;
				}
			}

			return exception;
		}

		#endregion

		#region Constructors

		private FesConnection()
		{ }

		#endregion
	}
}
