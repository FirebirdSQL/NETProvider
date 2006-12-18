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
	/// <include file='xmldoc/isc_blob_handle.xml' path='doc/member[@name="T:isc_blob_handle"]/*'/>
	internal interface isc_blob_handle 
	{
		/// <include file='xmldoc/isc_blob_handle.xml' path='doc/member[@name="P:BlobId"]/*'/>
		long BlobId
		{
			get;
			set;				
		}

		/// <include file='xmldoc/isc_blob_handle.xml' path='doc/member[@name="M:IsEof"]/*'/>
		bool IsEof();
	}
}
