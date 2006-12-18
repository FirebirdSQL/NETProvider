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
 *  This file was originally ported from Jaybird
 */


using System;
using System.Globalization;
using System.Text;

namespace FirebirdSql.Data.Common
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class IscException : Exception
	{	
		#region Fields
		
		private IscErrorCollection	errors;
		private int					errorCode;
		private string				message;

		#endregion

		#region Properties
		
		public IscErrorCollection Errors
		{
			get { return this.errors; }
		}

		public new string Message
		{
			get { return this.message; }
		}

		public int ErrorCode
		{
			get { return this.errorCode; }			
		}
	    
		#endregion

		#region Constructors

		public IscException() : base()
		{
			this.errors = new IscErrorCollection();
		}

		public IscException(int errorCode) : this()
		{
			this.Errors.Add(IscCodes.isc_arg_gds, errorCode);
			this.BuildExceptionMessage();
		}

		public IscException(string strParam) : this()
		{			
			this.Errors.Add(IscCodes.isc_arg_string, strParam);
			this.BuildExceptionMessage();
		}

		public IscException(int type, string strParam) : this()
		{
			this.Errors.Add(type, strParam);
			this.BuildExceptionMessage();
		}

		public IscException(int errorCode, int intparam) : this()
		{
			this.Errors.Add(IscCodes.isc_arg_gds, errorCode);
			this.Errors.Add(IscCodes.isc_arg_number, intparam);
			this.BuildExceptionMessage();
		}
	    
		public IscException(int type, int errorCode, string strParam) : this()
		{
			this.Errors.Add(type, errorCode);
			this.Errors.Add(IscCodes.isc_arg_string, strParam);			
			this.BuildExceptionMessage();
		}

		public IscException(
			int type, int errorCode, int intParam, string strParam) : this()
		{
			this.Errors.Add(type, errorCode);
			this.Errors.Add(IscCodes.isc_arg_string, strParam);
			this.Errors.Add(IscCodes.isc_arg_number, intParam);
			this.BuildExceptionMessage();
		}
		
		#endregion

		#region Methods

		public bool IsWarning() 
		{
			if (this.errors.Count > 0)
			{
				return this.errors[0].IsWarning();
			}
			else
			{
				return false;
			}
		}

		public bool IsFatal()
		{
			bool isFatal = false;

			for (int i = 0; i < this.errors.Count; i++)
			{
				if (this.errors[0].IsFatal)
				{
					isFatal = true;
					break;
				}
			}

			return isFatal;			
		}

		public void BuildExceptionMessage()
		{
			StringBuilder	message = new StringBuilder();
			GdsMessage		gdsMessage = null;
			
			errorCode = this.errors.Count != 0 ? this.errors[0].ErrorCode : 0;

			for (int i = 0; i < this.errors.Count; i++)
			{	
				if (this.errors[i].Type == IscCodes.isc_arg_gds || 
					this.errors[i].Type == IscCodes.isc_arg_warning)
				{
					gdsMessage = IscExceptionHelper.GetMessage(
						this.errors[i].ErrorCode);

					// Add params if exist any
					int paramCount = gdsMessage.GetParamCount();

					for (int j = 1; j <= paramCount; j++)
					{
						int index = i + j;
						
						if (index >= 0 && index < errors.Count)
						{
							switch (errors[index].Type)
							{
								case IscCodes.isc_arg_interpreted:
								case IscCodes.isc_arg_string: 
								case IscCodes.isc_arg_cstring:
									gdsMessage.SetParameter(
										j, 
										this.errors[index].StrParam);
									break;

								case IscCodes.isc_arg_number:
									gdsMessage.SetParameter(
										j, 
										this.errors[index].ErrorCode.ToString(CultureInfo.InvariantCulture.NumberFormat));
									break;
							}
						}
					}

					errors[i].Message = gdsMessage.ToString();
					message.Append(this.errors[i].Message + "\n");

					i += paramCount;

					gdsMessage = null;
				}
			}

			this.message = message.ToString();
		}

		#endregion
	}
}
