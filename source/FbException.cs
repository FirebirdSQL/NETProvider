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
using System.Runtime.Serialization;

using FirebirdSql.Data.INGDS;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='xmldoc/fbexception.xml' path='doc/member[@name="T:FbException"]/*'/>	
	[Serializable]
	public sealed class FbException : SystemException
	{
		#region FIELDS
		
		private FbErrorCollection	errors = new FbErrorCollection();
		private int					errorCode;
		
		#endregion

		#region PROPERTIES

		/// <include file='xmldoc/fbexception.xml' path='doc/member[@name="P:Errors"]/*'/>
		public FbErrorCollection Errors
		{
			get { return errors; }
		}

		/// <include file='xmldoc/fbexception.xml' path='doc/member[@name="P:ErrorCode"]/*'/>
		public int ErrorCode
		{
			get { return errorCode; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='xmldoc/fbexception.xml' path='doc/member[@name="M:#ctor"]/*'/>
		internal FbException() : base()
		{
		}

		/// <include file='xmldoc/fbexception.xml' path='doc/member[@name="M:#ctor(System.String)"]/*'/>
		internal FbException(string message) : base(message)
		{
		}

		internal FbException(SerializationInfo info, StreamingContext context) 
								: base(info, context)
		{
		}

		/// <include file='xmldoc/fbexception.xml' path='doc/member[@name="M:#ctor(System.String,FirebirdSql.Data.INGDS.GDSException)"]/*'/>
		internal FbException(string message, GDSException ex) : base(message, ex)
		{
			errorCode = ex.ErrorCode;
			Source	  = ex.Source;

			GetGdsExceptionErrors((GDSException)ex);
		}

		#endregion

		#region METHODS

		internal void GetGdsExceptionErrors(GDSException ex)
		{
			foreach(GDSError error in ex.Errors)
			{
				errors.Add(error.Message, error.ErrorCode);
			}
		}

		#endregion
	}
}
