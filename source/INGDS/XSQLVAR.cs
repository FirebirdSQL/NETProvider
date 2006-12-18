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

namespace FirebirdSql.Data.INGDS
{
	/// <include file='xmldoc/xsqlvar.xml' path='doc/member[@name="T:XSQLVAR"]/*'/>
	internal class XSQLVAR 
	{
		#region FIELDS

		/// <include file='xmldoc/xsqlvar.xml' path='doc/member[@name="P:sqltype"]/*'/>
		public int sqltype;
		
		/// <include file='xmldoc/xsqlvar.xml' path='doc/member[@name="P:sqlscale"]/*'/>
		public int sqlscale;

		/// <include file='xmldoc/xsqlvar.xml' path='doc/member[@name="P:sqlsubtype"]/*'/>
		public int sqlsubtype;
		
		/// <include file='xmldoc/xsqlvar.xml' path='doc/member[@name="P:sqllen"]/*'/>
		public int sqllen;
		
		/// <include file='xmldoc/xsqlvar.xml' path='doc/member[@name="P:sqldata"]/*'/>
		public object sqldata;

		/// <include file='xmldoc/xsqlvar.xml' path='doc/member[@name="P:sqlind"]/*'/>
		public int sqlind;

		/// <include file='xmldoc/xsqlvar.xml' path='doc/member[@name="P:sqlname"]/*'/>
		public string sqlname;

		/// <include file='xmldoc/xsqlvar.xml' path='doc/member[@name="P:relname"]/*'/>
		public string relname;

		/// <include file='xmldoc/xsqlvar.xml' path='doc/member[@name="P:ownname"]/*'/>
		public string ownname;

		/// <include file='xmldoc/xsqlvar.xml' path='doc/member[@name="P:aliasname"]/*'/>
		public string aliasname;

		#endregion

		#region CONSTRUCTORS
		
		/// <include file='xmldoc/xsqlvar.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public XSQLVAR() 
		{
		}

		/// <include file='xmldoc/xsqlvar.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public XSQLVAR(object sqldata) 
		{
			this.sqldata = sqldata;
		}

		#endregion
	}
}
