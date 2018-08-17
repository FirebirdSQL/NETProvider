/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Runtime.InteropServices;
using System.Text;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native
{
	internal sealed class FesConnection
	{
		#region Static Methods

		public static IscException ParseStatusVector(IntPtr[] statusVector, Charset charset)
		{
			IscException exception = null;
			var eof = false;

			for (var i = 0; i < statusVector.Length;)
			{
				var arg = statusVector[i++];

				switch (arg.AsInt())
				{
					case IscCodes.isc_arg_gds:
					default:
						var er = statusVector[i++];
						if (er != IntPtr.Zero)
						{
							if (exception == null)
							{
								exception = IscException.ForBuilding();
							}
							exception.Errors.Add(new IscError(arg.AsInt(), er.AsInt()));
						}
						break;

					case IscCodes.isc_arg_end:
						exception?.BuildExceptionData();
						eof = true;
						break;

					case IscCodes.isc_arg_interpreted:
					case IscCodes.isc_arg_string:
						{
							var ptr = statusVector[i++];
							var s = Marshal.PtrToStringAnsi(ptr);
							var arg_value = charset.GetString(Encoding2.Default.GetBytes(s));
							exception.Errors.Add(new IscError(arg.AsInt(), arg_value));
						}
						break;

					case IscCodes.isc_arg_cstring:
						{
							i++;

							var ptr = statusVector[i++];
							var s = Marshal.PtrToStringAnsi(ptr);
							var arg_value = charset.GetString(Encoding2.Default.GetBytes(s));
							exception.Errors.Add(new IscError(arg.AsInt(), arg_value));
						}
						break;

					case IscCodes.isc_arg_win32:
					case IscCodes.isc_arg_number:
						exception.Errors.Add(new IscError(arg.AsInt(), statusVector[i++].AsInt()));
						break;
					case IscCodes.isc_arg_sql_state:
						{
							var ptr = statusVector[i++];
							var s = Marshal.PtrToStringAnsi(ptr);
							var arg_value = charset.GetString(Encoding2.Default.GetBytes(s));
							exception.Errors.Add(new IscError(arg.AsInt(), arg_value));
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
