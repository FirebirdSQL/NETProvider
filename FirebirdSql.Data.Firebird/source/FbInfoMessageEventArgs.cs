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
using FirebirdSql.Data.Firebird.Gds;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbInfoMessageEventArgs.xml' path='doc/delegate[@name="FbInfoMessageEventHandler"]/overview/*'/>
	public delegate void FbInfoMessageEventHandler(object sender, FbInfoMessageEventArgs e);

	/// <include file='Doc/en_EN/FbInfoMessageEventArgs.xml' path='doc/class[@name="FbInfoMessageEventArgs"]/overview/*'/>
	public sealed class FbInfoMessageEventArgs : EventArgs
	{
		#region FIELDS

		private FbErrorCollection errors = new FbErrorCollection();
		private string			  message = String.Empty;

		#endregion

		#region PROPERTIES

		/// <include file='Doc/en_EN/FbInfoMessageEventArgs.xml' path='doc/class[@name="FbInfoMessageEventArgs"]/property[@name="Errors"]/*'/>
		public FbErrorCollection Errors
		{
			get { return errors; }
		}

		/// <include file='Doc/en_EN/FbInfoMessageEventArgs.xml' path='doc/class[@name="FbInfoMessageEventArgs"]/property[@name="Message"]/*'/>
		public string Message
		{
			get { return message; }
		}

		#endregion

		#region CONSTRUCTORS

		internal FbInfoMessageEventArgs(GdsException ex)
		{
			this.message = ex.Message;
			
			foreach (GdsError error in ex.Errors)
			{
				errors.Add(error.Message, error.ErrorCode);
			}
		}

		#endregion
	}
}
