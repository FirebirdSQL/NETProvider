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

namespace FirebirdSql.Logging
{	
	/// <summary>
	/// Describe class <code>Logger</code> here.
	/// </summary>
	internal abstract class Logger
	{	
		abstract public bool isDebugEnabled();
	
		abstract public void debug(object message);
	
		abstract public void debug(object message, Exception ex);
	
		abstract public bool isInfoEnabled();
	
		abstract public void info(object message);
	
		abstract public void info(object message, Exception ex);
	
		abstract public bool isWarnEnabled();
	
		abstract public void warn(object message);
	
		abstract public void warn(object message, Exception ex);
	
		abstract public bool isErrorEnabled();
	
		abstract public void error(object message);
	
		abstract public void error(object message, Exception ex);
	
		abstract public bool isFatalEnabled();
	
		abstract public void fatal(object message);
	
		abstract public void fatal(object message, Exception ex);
	}
}