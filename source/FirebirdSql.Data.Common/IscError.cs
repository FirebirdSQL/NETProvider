/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Globalization;

namespace FirebirdSql.Data.Common
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class IscError
	{
		#region Fields

		private string	message;
		private int		type;
		private int		errorCode;
		private string	strParam;
		private bool	isFatal;
		
		#endregion

		#region Properties
		
		public string Message
		{
			get { return this.message; }
			set { this.message = value; }
		}

		public int ErrorCode
		{
			get { return this.errorCode; }
		}

		public string StrParam
		{
			get
			{
				switch (this.type)
				{
					case IscCodes.isc_arg_interpreted:
					case IscCodes.isc_arg_string:
					case IscCodes.isc_arg_cstring:
						return this.strParam;

					case IscCodes.isc_arg_number:
						return this.errorCode.ToString(CultureInfo.InvariantCulture.NumberFormat);

					default:
						return String.Empty;
				}
			}
		}

		public int Type
		{
			get { return this.type; }
		}

		public bool IsFatal
		{
			get { return this.isFatal; }
		}

		#endregion

		#region Constructors

		internal IscError(int errorCode)
		{						
			this.errorCode	= errorCode;
			this.isFatal	= this.CheckIfIsFatal();
		}

		internal IscError(string strParam)
		{
			this.strParam	= strParam;
		}

		internal IscError(int type, string strParam)
		{
			this.type		= type;
			this.strParam	= strParam;
		}

		internal IscError(int type, int errorCode)
		{
			this.type		= type;
			this.errorCode	= errorCode;

			if (this.type == IscCodes.isc_arg_number)
			{
				this.isFatal = this.CheckIfIsFatal();
			}
		}

		internal IscError(int type, int errorCode, string strParam)
		{
			this.type		= type;
			this.errorCode	= errorCode;
			this.strParam	= strParam;

			if (this.type == IscCodes.isc_arg_number)
			{
				this.isFatal = this.CheckIfIsFatal();
			}
		}

		#endregion

		#region Methods

		public bool IsWarning() 
		{
			return (Type == IscCodes.isc_arg_warning);
		}

		private bool CheckIfIsFatal()
		{
			/*
			for (int i = 0; i < IscCodes.FATAL_ERRORS.Length 
				&& ErrorCode >= IscCodes.FATAL_ERRORS[i]; i++)
			{
				if (ErrorCode == IscCodes.FATAL_ERRORS[i]) 
				{
					return true;
				}
            
			}
			*/
			return false;
		}

		#endregion
	}
}
