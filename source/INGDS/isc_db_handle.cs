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
using System.Collections;
using System.Security.Principal;

namespace FirebirdSql.Data.INGDS
{
	internal class DbWarningMessageEventArgs : EventArgs
	{
		private GDSException exception;
		private string		 message = String.Empty;

		public string Message
		{
			get { return message; }
		}

		public GDSException Exception
		{
			get { return exception; }
		}

		public DbWarningMessageEventArgs(GDSException ex)
		{
			message		= ex.ToString();
			exception	= ex;			
		}
	}

	internal delegate void DbWarningMessageEventHandler(object sender, DbWarningMessageEventArgs e);

	/// <include file='xmldoc/isc_db_handle.xml' path='doc/member[@name="T:isc_db_handle"]/*'/>
	internal interface isc_db_handle 
	{
		int PacketSize
		{
			get;
			set;
		}

		/// <include file='xmldoc/isc_db_handle.xml' path='doc/member[@name="E:DbWarningMessage"]/*'/>
		event DbWarningMessageEventHandler DbWarningMessage;

		/// <include file='xmldoc/isc_db_handle.xml' path='doc/member[@name="M:HasTransactions"]/*'/>
		bool HasTransactions();		

		/// <include file='xmldoc/isc_db_handle.xml' path='doc/member[@name="M:TransactionCount"]/*'/>
		int TransactionCount();

		/// <include file='xmldoc/isc_db_handle.xml' path='doc/member[@name="M:GetWarnings"]/*'/>
		ArrayList GetWarnings();

		/// <include file='xmldoc/isc_db_handle.xml' path='doc/member[@name="M:ClearWarnings"]/*'/>
		void ClearWarnings();
	}
}
