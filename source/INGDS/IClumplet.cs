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
using System.Runtime.Serialization;

namespace FirebirdSql.Data.INGDS
{
	/// <include file='xmldoc/iclumplet.xml' path='doc/member[@name="T:IClumplet"]/*'/>
	internal interface IClumplet 
	{
		/// <include file='xmldoc/iclumplet.xml' path='doc/member[@name="P:Length"]/*'/>
		int Length
		{
			get;
		}
		/// <include file='xmldoc/iclumplet.xml' path='doc/member[@name="M:Append(FirebirdSql.Data.INGDS.IClumplet)"]/*'/>
		void	Append(IClumplet c);
		/// <include file='xmldoc/iclumplet.xml' path='doc/member[@name="M:Find(System.Int32)"]/*'/>
		byte[]	Find(int type);
		/// <include file='xmldoc/iclumplet.xml' path='doc/member[@name="M:Equals(System.Object)"]/*'/>
		bool	Equals(object o);
		/// <include file='xmldoc/iclumplet.xml' path='doc/member[@name="M:GetHashCode"]/*'/>
		int		GetHashCode();
	}
}
