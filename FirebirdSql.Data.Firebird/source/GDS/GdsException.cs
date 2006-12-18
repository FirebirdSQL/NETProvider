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
 *  Initial code was ported from Jaybird
 */

using System;
using System.Text;

namespace FirebirdSql.Data.Firebird.Gds
{
	[Serializable]
	internal class GdsException : Exception
	{	
		#region FIELDS
		
		private GdsErrorCollection	errors;
		private int					errorCode;
		private string				message;

		#endregion

		#region PROPERTIES
		
		public GdsErrorCollection Errors
		{
			get { return errors; }
		}

		public new string Message
		{
			get { return message; }
		}

		public int ErrorCode
		{
			get { return errorCode; }			
		}
	    
		#endregion

		#region CONSTRUCTORS

		public GdsException() : base()
		{
			errors = new GdsErrorCollection();
		}

		public GdsException(int errorCode) : this()
		{
			this.Errors.Add(GdsCodes.isc_arg_gds, errorCode);
			this.BuildExceptionMessage();
		}

		public GdsException(string strParam) : this()
		{			
			this.Errors.Add(GdsCodes.isc_arg_string, strParam);
			this.BuildExceptionMessage();
		}

		public GdsException(int type, string strParam) : this()
		{
			this.Errors.Add(type, strParam);
			this.BuildExceptionMessage();
		}

		public GdsException(int errorCode, int intParam) : this()
		{
			this.Errors.Add(GdsCodes.isc_arg_gds, errorCode);
			this.Errors.Add(GdsCodes.isc_arg_number, intParam);
			this.BuildExceptionMessage();
		}
	    
		public GdsException(int type, int errorCode, string strParam) : this()
		{
			this.Errors.Add(type, errorCode);
			this.Errors.Add(GdsCodes.isc_arg_string, strParam);			
			this.BuildExceptionMessage();
		}
		
		public GdsException(
			int type, int errorCode, int intParam, string strParam) : this()
		{
			this.Errors.Add(type, errorCode);
			this.Errors.Add(GdsCodes.isc_arg_string, strParam);
			this.Errors.Add(GdsCodes.isc_arg_number, intParam);
			this.BuildExceptionMessage();
		}

		#endregion

		#region METHODS

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

		public void BuildExceptionMessage()
		{
			StringBuilder	message = new StringBuilder();
			GdsMessage		gdsMessage = null;
			
			errorCode = errors.Count != 0 ? errors[0].ErrorCode : 0;

			for(int i = 0; i < errors.Count; i++)
			{
				if (errors[i].Type == GdsCodes.isc_arg_gds || 
					errors[i].Type == GdsCodes.isc_arg_warning)
				{
					gdsMessage = GdsExceptionHelper.GetMessage(errors[i].ErrorCode);

					// Add params if exist any
					int paramCount = gdsMessage.GetParamCount();
					for(int j = 1; j <= paramCount; j++)
					{
						int index = i + j;

						if (index >= 0 && index < errors.Count)
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
