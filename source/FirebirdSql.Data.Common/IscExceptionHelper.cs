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
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;

namespace FirebirdSql.Data.Common
{
	internal sealed class IscExceptionHelper
	{
		#region Fields
				
		private static ResourceManager	rm;
		private static bool				initialized = false;

		#endregion
	
		#region Constructors

		private IscExceptionHelper()
		{
		}

		#endregion

		#region Methods

		private static void Init()
		{
			try 
			{
				string resources = "FirebirdSql.Data.Common.Resources.isc_error_msg";

				rm = new ResourceManager(resources, Assembly.GetExecutingAssembly());
			} 
			catch (Exception)
			{				
			} 
			finally 
			{
				initialized = true;
			}
		}
		
		public static GdsMessage GetMessage(int code)
		{
			string message = null;

			if (!initialized) 
			{
				Init();
			}

			try
			{
				message = rm.GetString(code.ToString(CultureInfo.InvariantCulture.NumberFormat));
			}
			catch(Exception)
			{		
			}
			finally
			{
				if (message == null)
				{
					message = String.Format("No message for code {0} found", code);
				}
			}

			return new GdsMessage(message);
		}
		
		#endregion
	}
	
	internal class GdsMessage 
	{
		#region Fields

		private string format;
		private ArrayList parameters = new ArrayList();

		#endregion

		#region Constructors

		public GdsMessage()
		{
		}

		#endregion

		#region Methods

		public GdsMessage(string format)
		{
			this.format = format;
		}

		public int GetParamCount() 
		{
			int count = 0;

			if (format == null)
			{
				format = String.Empty;
			}

			for (int i = 0; i < format.Length; i++)
			{
				if (format[i] == '{') 
				{
					count++;
				}
			}

			return count;
		}
			
		public void SetParameter(int position, string text) 
		{
			parameters.Add(text);
		}
			
		public override string ToString() 
		{
			StringBuilder message = new StringBuilder();
			
			if (parameters.Count == GetParamCount())
			{
				message.AppendFormat(format, parameters.ToArray());
			}
			else
			{
				message.Append(format);
			}

			return message.ToString();
		}

		#endregion
	}
}
