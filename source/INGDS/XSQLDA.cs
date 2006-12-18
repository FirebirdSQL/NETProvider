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
using FirebirdSql.Data.INGDS;

namespace FirebirdSql.Data.INGDS
{
	/// <include file='xmldoc/xsqlda.xml' path='doc/member[@name="T:XSQLDA"]/*'/>
	internal class XSQLDA 
	{
		#region FIELDS

		/// <include file='xmldoc/xsqlda.xml' path='doc/member[@name="P:version"]/*'/>
		public int version;

		/// <include file='xmldoc/xsqlda.xml' path='doc/member[@name="P:sqln"]/*'/>
		public int sqln;

		/// <include file='xmldoc/xsqlda.xml' path='doc/member[@name="P:sqld"]/*'/>
		public int sqld;

		/// <include file='xmldoc/xsqlda.xml' path='doc/member[@name="P:sqln"]/*'/>
		public XSQLVAR[] sqlvar;
		
		#endregion

		#region CONSTRUCTORS

		/// <include file='xmldoc/xsqlda.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public XSQLDA() 
		{
			version = GdsCodes.SQLDA_VERSION1;
		}

		/// <include file='xmldoc/xsqlda.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public XSQLDA(int n) 
		{
			version = GdsCodes.SQLDA_VERSION1;
			sqln	= n;
			sqld	= n;
			sqlvar	= new XSQLVAR[n];
		}

		#endregion
	}
}
