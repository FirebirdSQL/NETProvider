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

namespace FirebirdSql.Data.Firebird
{
	/// <include file='xmldoc/fberror.xml' path='doc/member[@name="T:FbError"]/*'/>	
	[Serializable]
	public sealed class FbError
	{
		#region FIELDS

		private byte	classError;
		private int		lineNumber;
		private string	message;
		private int		number;
		
		#endregion

		#region PROPERTIES
		
		/// <include file='xmldoc/fberror.xml' path='doc/member[@name="P:Class"]/*'/>
		public byte Class
		{
			get { return classError; }
		}
		
		/// <include file='xmldoc/fberror.xml' path='doc/member[@name="P:LineNumber"]/*'/>
		public int LineNumber
		{
			get { return lineNumber; }
		}

		/// <include file='xmldoc/fberror.xml' path='doc/member[@name="P:Message"]/*'/>
		public string Message
		{
			get { return message; }
		}

		/// <include file='xmldoc/fberror.xml' path='doc/member[@name="P:Number"]/*'/>
		public int Number
		{
			get { return number; }
		}

		#endregion

		#region CONSTRUCTORS
		
		/// <include file='xmldoc/fberror.xml' path='doc/member[@name="M:#ctor(System.String,System.Int32)"]/*'/>
		internal FbError(string message,int number)
		{			
			this.number		= number;
			this.message	= message;
		}

		/// <include file='xmldoc/fberror.xml' path='doc/member[@name="M:#ctor(System.Byte,System.String,System.Int32)"]/*'/>
		internal FbError(byte classError, string message, int number)
		{			
			this.classError = classError;
			this.number		= number;
			this.message	= message;
		}

		/// <include file='xmldoc/fberror.xml' path='doc/member[@name="M:#ctor(System.Byte,System.Int32,System.String,System.Int32)"]/*'/>
		internal FbError(byte classError, int line, string message, int number)
		{			
			this.classError = classError;
			this.lineNumber = line;
			this.number		= number;
			this.message	= message;
		}

		#endregion
	}
}
