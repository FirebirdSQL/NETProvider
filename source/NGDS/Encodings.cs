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
using System.Resources;
using System.Text;
using System.Reflection;

namespace FirebirdSql.Data.NGDS
{
	/// <include file='xmldoc/encodings.xml' path='doc/member[@name="T:Encodings"]/*'/>
	internal class Encodings
	{	
		#region FIELDS

		private static string ENCODINGS		= "FirebirdSql.Data.Firebird.Resources.GDS.isc_encodings";
		private static ResourceManager		rm;
		
		private static bool initialized		= false;

		#endregion

		#region METHODS

		/// <include file='xmldoc/encodings.xml' path='doc/member[@name="M:Init"]/*'/>
		private static void Init()
		{			
			try 
			{				
				rm = new ResourceManager(ENCODINGS, Assembly.GetExecutingAssembly());				
			} 
			catch (Exception)
			{
			} 
			finally 
			{
				initialized = true;
			}
		}

		/// <include file='xmldoc/encodings.xml' path='doc/member[@name="M:GetFromFirebirdEncoding(System.String)"]/*'/>
		public static Encoding GetFromFirebirdEncoding(string fbencoding)
		{
			string encoding = null;
			
			if (!initialized) 
			{
				Init();
			}
			
			try
			{
				switch(fbencoding.ToUpper())
				{
					case "NONE":
						return Encoding.Default;						

					default:
						encoding = rm.GetString(fbencoding, CultureInfo.InvariantCulture);
						break;
				}
			}
			catch(Exception)
			{
				encoding = "Default";
			}
			
			return encoding == "Default" ? Encoding.Default : Encoding.GetEncoding(encoding);
		}

		#endregion
	}
}
