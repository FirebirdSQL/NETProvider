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
using System.Reflection;

namespace FirebirdSql.Logging
{
	/// <summary>
	/// Describe class <code>LoggerFactory</code> here.
	/// </summary>
	internal class LoggerFactory
	{
		#region FIELDS

		private static bool _checked = false;
		private static bool log4j = false;

		#endregion
    
		#region METHODS

		public static Logger GetLogger(string name, bool def) 
		{
			if (!_checked)
			{
				try 
				{
					Type verify = Type.GetType("Firebird.Log4CSharp");
					log4j = true;
				}
				catch (TypeLoadException)
				{
					log4j = false;
				}
				_checked = true;
			}
			
			if (log4j)
				return new Log4jLogger(name);
			else
				return null;
		}		
    
		public static Logger GetLogger(System.Type clazz, bool def) 
		{
			return GetLogger(clazz.Name, def);
		}

		#endregion
	}
}
