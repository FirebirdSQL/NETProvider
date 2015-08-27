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
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Globalization;

namespace FirebirdSql.Data.Common
{
	[Serializable]
	internal sealed class IscError
	{
		#region Fields

		private string	_message;
		private int		_type;
		private int		_errorCode;
		private string	_strParam;

		#endregion

		#region Properties

		public string Message
		{
			get { return _message; }
			set { _message = value; }
		}

		public int ErrorCode
		{
			get { return _errorCode; }
		}

		public string StrParam
		{
			get
			{
				switch (_type)
				{
					case IscCodes.isc_arg_interpreted:
					case IscCodes.isc_arg_string:
					case IscCodes.isc_arg_cstring:
					case IscCodes.isc_arg_sql_state:
						return _strParam;

					case IscCodes.isc_arg_number:
						return _errorCode.ToString(CultureInfo.InvariantCulture);

					default:
						return string.Empty;
				}
			}
		}

		public int Type
		{
			get { return _type; }
		}

		public bool IsArgument
		{
			get
			{
				switch (_type)
				{
					case IscCodes.isc_arg_interpreted:
					case IscCodes.isc_arg_string:
					case IscCodes.isc_arg_cstring:
					case IscCodes.isc_arg_number:
						return true;

					default:
						return false;
				}
			}
		}

		public bool IsWarning
		{
			get { return _type == IscCodes.isc_arg_warning; }
		}

		#endregion

		#region Constructors

		internal IscError(int errorCode)
		{
			_errorCode = errorCode;
		}

		internal IscError(int type, string strParam)
		{
			_type = type;
			_strParam = strParam;
		}

		internal IscError(int type, int errorCode)
		{
			_type = type;
			_errorCode = errorCode;
		}

		#endregion
	}
}
