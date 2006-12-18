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

namespace FirebirdSql.Data.INGDS
{
	/// <include file='xmldoc/gdserror.xml' path='doc/member[@name="T:GDSError"]/*'/>
	internal sealed class GDSError
	{
		#region FIELDS

		private string	message;
		private int		type;
		private int		errorCode;
		private string	strParam;
		private bool	isFatal;
		
		#endregion

		#region PROPERTIES
		
		/// <include file='xmldoc/gdserror.xml' path='doc/member[@name="P:Message"]/*'/>
		public string Message
		{
			get { return message; }
			set { message = value; }
		}

		/// <include file='xmldoc/gdserror.xml' path='doc/member[@name="P:ErrorCode"]/*'/>
		public int ErrorCode
		{
			get { return errorCode; }
		}

		/// <include file='xmldoc/gdserror.xml' path='doc/member[@name="P:StrParam"]/*'/>
		public string StrParam
		{
			get
			{
				if ((type == GdsCodes.isc_arg_interpreted) || (type == GdsCodes.isc_arg_string))
				{
					return strParam;
				}
				else
				{
					if (type == GdsCodes.isc_arg_number)
					{
						return errorCode.ToString();
					}				
					else
					{
						return String.Empty;
					}
				}
			}
		}

		/// <include file='xmldoc/gdserror.xml' path='doc/member[@name="P:Type"]/*'/>
		public int Type
		{
			get { return type; }
		}

		public bool IsFatal
		{
			get { return isFatal; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='xmldoc/gdserror.xml' path='doc/member[@name="M:#ctor(System.Int32)"]/*'/>
		internal GDSError(int errorCode)
		{						
			this.errorCode	= errorCode;
			this.isFatal = CheckIfIsFatal();
		}

		/// <include file='xmldoc/gdserror.xml' path='doc/member[@name="M:#ctor(System.String)"]/*'/>
		internal GDSError(string strParam)
		{
			this.strParam	= strParam;
		}

		/// <include file='xmldoc/gdserror.xml' path='doc/member[@name="M:#ctor(System.Int32,System.String)"]/*'/>
		internal GDSError(int type, string strParam)
		{
			this.type		= type;
			this.strParam	= strParam;
		}

		/// <include file='xmldoc/gdserror.xml' path='doc/member[@name="M:#ctor(System.Int32,System.Int32)"]/*'/>
		internal GDSError(int type, int errorCode)
		{
			this.type		= type;
			this.errorCode	= errorCode;

			if (type == GdsCodes.isc_arg_number)
			{
				this.isFatal = CheckIfIsFatal();
			}
		}

		/// <include file='xmldoc/gdserror.xml' path='doc/member[@name="M:#ctor(System.Int32,System.Int32,System.String)"]/*'/>
		internal GDSError(int type, int errorCode, string strParam)
		{
			this.type		= type;
			this.errorCode	= errorCode;
			this.strParam	= strParam;

			if (type == GdsCodes.isc_arg_number)
			{
				this.isFatal = CheckIfIsFatal();
			}
		}

		#endregion

		#region METHODS

		public bool IsWarning() 
		{
			return (Type == GdsCodes.isc_arg_warning);
		}

		private bool CheckIfIsFatal()
		{
			for (int i = 0; i < GdsCodes.FATAL_ERRORS.Length 
				&& ErrorCode >= GdsCodes.FATAL_ERRORS[i]; i++)
			{
				if (ErrorCode == GdsCodes.FATAL_ERRORS[i]) 
				{
					return true;
				}
            
			}
			return false;
		}

		#endregion
	}
}
