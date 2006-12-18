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

namespace FirebirdSql.Logging
{	
	/// <summary>
	/// Describe class <code>Log4jLogger</code> here. 
	/// </summary>
	internal class Log4jLogger : Logger
	{		
		#region FIELDS

		private static bool loggingAvailable = true;
		// private sealed Category log;
		private Log4CSharp log;
	
		#endregion

		#region CONSTRUCTORS

		public Log4jLogger(string name) 
		{
			if (loggingAvailable) 
			{
				// Category myLog = null;
				Log4CSharp myLog = null;
				try 
				{
					// myLog = Category.getInstance(name);
					myLog = new Log4CSharp(GetType(), null, Mode.APPEND);
				}
				catch (Exception) 
				{
					loggingAvailable = false;
				}
				log = myLog;
			}
			else 
			{
				log = null;
			}
		}

		#endregion

		#region METHODS
		
		public override bool isDebugEnabled() 
		{
			return loggingAvailable && log.IsEnabledFor(Priority.DEBUG);
		}
	
		public override void debug(object message)
		{
			if (isDebugEnabled()) 
			{
				log.Log(Priority.DEBUG, message);
			}
		}
	
		public override void debug(object message, Exception ex) 
		{
			if (isDebugEnabled()) 
			{
				log.Log(Priority.DEBUG, message, ex);
			}
		}
	
		public override bool isInfoEnabled() 
		{
			return loggingAvailable && log.IsEnabledFor(Priority.INFO);
		}
	
		public override void info(object message) 
		{
			if (isInfoEnabled()) 
			{
				log.Log(Priority.INFO, message);
			}
		}
	
		public override void info(object message, Exception ex) 
		{
			if (isInfoEnabled()) 
			{
				log.Log(Priority.INFO, message, ex);
			}
		}
	
		public override bool isWarnEnabled() 
		{
			return loggingAvailable && log.IsEnabledFor(Priority.WARN);
		}
	
		public override void warn(object message) 
		{
			if (isWarnEnabled()) 
			{
				log.Log(Priority.WARN, message);
			}
		}
	
		public override void warn(object message, Exception ex) 
		{
			if (isWarnEnabled()) 
			{
				log.Log(Priority.WARN, message, ex);
			}
		}
	
		public override bool isErrorEnabled() 
		{
			return loggingAvailable && log.IsEnabledFor(Priority.ERROR);
		}
	
		public override void error(object message) 
		{
			if (isErrorEnabled()) 
			{
				log.Log(Priority.ERROR, message);
			}
		}
	
		public override void error(object message, Exception ex) 
		{
			if (isErrorEnabled()) 
			{
				log.Log(Priority.ERROR, message, ex);
			}
		}
	
		public override bool isFatalEnabled() 
		{
			return loggingAvailable && log.IsEnabledFor(Priority.FATAL);
		}
	
		public override void fatal(object message) 
		{
			if (isFatalEnabled()) 
			{
				log.Log(Priority.FATAL, message);
			}
		}
	
		public override void fatal(object message, Exception ex) 
		{
			if (isFatalEnabled()) 
			{
				log.Log(Priority.FATAL, message, ex);
			}
		}	

		#endregion
	}
}
