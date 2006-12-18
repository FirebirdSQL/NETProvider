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
 * 
 *  This file was originally ported from JayBird <http://firebird.sourceforge.net/>
 */

using System;
using System.Text;

namespace FirebirdSql.Data.INGDS
{
	/// <include file='xmldoc/gdsexception.xml' path='doc/member[@name="T:GDSException"]/*'/>
	internal class GDSException : Exception
	{	
		#region FIELDS
		
		private GDSErrorCollection	errors;
		private int					errorCode;
		private string				message;

		#endregion

		#region PROPERTIES
		
		/// <include file='xmldoc/gdsexception.xml' path='doc/member[@name="P:Errors"]/*'/>
		public GDSErrorCollection Errors
		{
			get { return errors; }
		}

		/// <include file='xmldoc/gdsexception.xml' path='doc/member[@name="P:Message"]/*'/>
		public new string Message
		{
			get { return message; }
		}

		/// <include file='xmldoc/gdsexception.xml' path='doc/member[@name="P:ErrorCode"]/*'/>
		public int ErrorCode
		{
			get { return errorCode; }			
		}
	    
		#endregion

		#region CONSTRUCTORS

		/// <include file='xmldoc/gdsexception.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public GDSException() : base()
		{
			errors = new GDSErrorCollection();
		}

		/// <include file='xmldoc/gdsexception.xml' path='doc/member[@name="M:#ctor(System.Int32)"]/*'/>
		public GDSException(int errorCode) : this()
		{
			this.Errors.Add(GdsCodes.isc_arg_gds, errorCode);
			BuildExceptionMessage();
		}

		/// <include file='xmldoc/gdsexception.xml' path='doc/member[@name="M:#ctor(System.String)"]/*'/>
		public GDSException(string strParam) : this()
		{			
			this.Errors.Add(GdsCodes.isc_arg_string, strParam);
			BuildExceptionMessage();
		}

		/// <include file='xmldoc/gdsexception.xml' path='doc/member[@name="M:#ctor(System.Int32,System.String)"]/*'/>
		public GDSException(int type, string strParam) : this()
		{
			this.Errors.Add(type, strParam);
			BuildExceptionMessage();
		}

		/// <include file='xmldoc/gdsexception.xml' path='doc/member[@name="M:#ctor(System.Int32,System.Int32)"]/*'/>
		public GDSException(int errorCode, int intparam) : this()
		{
			this.Errors.Add(GdsCodes.isc_arg_gds, errorCode);
			this.Errors.Add(GdsCodes.isc_arg_number, intparam);
			BuildExceptionMessage();
		}
	    
		/// <include file='xmldoc/gdsexception.xml' path='doc/member[@name="M:#ctor(System.Int32,System.Int32,System.String)"]/*'/>
		public GDSException(int type, int errorCode, string strParam) : this()
		{
			this.Errors.Add(type, errorCode);
			this.Errors.Add(GdsCodes.isc_arg_string, strParam);			
			BuildExceptionMessage();
		}
		
		#endregion

		#region METHODS

		/// <include file='xmldoc/gdsexception.xml' path='doc/member[@name="M:IsWarning"]/*'/>
		public bool IsWarning() 
		{
			if (errors.Count > 0)
			{
				return errors[0].IsWarning();
			}
			else
			{
				return false;
			}
		}

		/// <include file='xmldoc/gdsexception.xml' path='doc/member[@name="M:IsFatal"]/*'/>
		public bool IsFatal()
		{
			bool isFatal = false;

			for (int i=0; i < errors.Count; i++)
			{
				if (errors[0].IsFatal)
				{
					isFatal = true;
					break;
				}
			}

			return isFatal;			
		}

		/// <include file='xmldoc/gdsexception.xml' path='doc/member[@name="M:BuildExceptionMessage"]/*'/>
		public void BuildExceptionMessage()
		{
			StringBuilder	message = new StringBuilder();
			GDSMessage		gdsMessage = null;
			
			errorCode = errors.Count != 0 ? errors[0].ErrorCode : 0;

			for(int i = 0; i < errors.Count; i++)
			{				
				if (errors[i].Type == GdsCodes.isc_arg_gds || errors[i].Type == GdsCodes.isc_arg_warning)
				{
					gdsMessage = GDSExceptionHelper.GetMessage(errors[i].ErrorCode);

					// Add params if exist any
					int paramCount = gdsMessage.GetParamCount();
					for(int j = 1; j <= paramCount; j++)
					{
						if (errors[i+j].Type == GdsCodes.isc_arg_string)
						{
							gdsMessage.SetParameter(j, errors[i+j].StrParam);
						}
						else if (errors[i+j].Type == GdsCodes.isc_arg_number)
							{
								gdsMessage.SetParameter(j, errors[i+j].ErrorCode.ToString());
							}
					}

					errors[i].Message = gdsMessage.ToString();
					message.Append(errors[i].Message + "\n");

					i += paramCount;

					gdsMessage = null;
				}
			}

			this.message = message.ToString();
		}

		#endregion
	}
}
