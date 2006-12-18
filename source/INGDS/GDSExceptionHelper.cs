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
using System.Globalization;
using System.Resources;
using System.Text;
using System.Reflection;
using System.Collections;

namespace FirebirdSql.Data.INGDS
{
	/// <summary>
	/// This class is supposed to return messages for the specified error code.
	/// It loads all messages during the class initialization and keeps messages
	/// in the static ResourceManager variable.
	/// </summary>
	internal class GDSExceptionHelper 
	{
		#region FIELDS
		
		private static string MESSAGES = "FirebirdSql.Data.Firebird.Resources.GDS.isc_error_msg";
		private static ResourceManager rm;

		private static bool initialized = false;

		#endregion
	
		#region METHODS

		/// <summary>
		/// This method initializes the messages map.
		/// </summary>
		private static void Init()
		{			
			try 
			{
				rm = new ResourceManager(MESSAGES, Assembly.GetExecutingAssembly());
			} 
			catch (Exception)
			{				
			} 
			finally 
			{
				initialized = true;
			}
		}
		
		/// <summary>
		/// This method returns a message for the specified error code.
		/// </summary>
		/// <param name="code">Firebird error code</param>
		/// <returns>instance of <code>GDSExceptionHelper.GDSMesssage</code> class</returns>
		public static GDSMessage GetMessage(int code)
		{
			string message = null;

			if (!initialized) 
			{
				Init();
			}

			try
			{
				message = rm.GetString(code.ToString(), CultureInfo.InvariantCulture);
			}
			catch(Exception)
			{		
			}
			finally
			{
				if (message==null)
				{
					message = "No message for code " + code.ToString() + " found.";
				}
			}

			return new GDSMessage(message);
		}
		#endregion
	}
	
	/// <summary>
	/// This class wraps message template obtained from isc_error_msg.properties
	/// file and allows to set parameters to the message.
	/// </summary>
	internal class GDSMessage 
	{
		#region FIELDS

		private string format;
		private ArrayList parameters = new ArrayList();

		#endregion

		#region METHODS

		/// <summary>
		/// Constructs an instance of GDSMessage for the specified template.
		/// </summary>
		/// <param name="format">Formato del mensaje</param>
		public GDSMessage(string format)
		{
			this.format = format;
		}

		/// <summary>
		/// Returns the number of parameters for the message template.
		/// </summary>
		/// <returns></returns>
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
			
		/// <summary>
		/// Sets the parameter value
		/// </summary>
		/// <param name="position">the parameter number, 0 - first parameter.</param>
		/// <param name="text">value of parameter</param>
		public void SetParameter(int position, string text) 
		{
			parameters.Add(text);
		}
			
		/// <summary>
		/// Substs the parameters in the format string and retuns the message as a string.
		/// </summary>
		/// <returns>The message as a string</returns>
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
