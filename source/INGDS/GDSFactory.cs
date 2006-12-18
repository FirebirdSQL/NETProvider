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
using FirebirdSql.Data.NGDS;

namespace FirebirdSql.Data.INGDS
{
	/// <include file='xmldoc/gdsfactory.xml' path='doc/member[@name="T:GDSFactory"]/*'/>
	internal class GDSFactory 
	{
		#region METHODS

		/// <include file='xmldoc/gdsfactory.xml' path='doc/member[@name="M:NewGDS"]/*'/>
		public static IGDS NewGDS() 
		{
			return new GDS();
		}
		
		/// <include file='xmldoc/gdsfactory.xml' path='doc/member[@name="M:NewClumplet(System.Int32,System.String)"]/*'/>
		public static IClumplet NewClumplet(int type, string content) 
		{
			return GDS.NewClumplet(type, content);
		}

		/// <include file='xmldoc/gdsfactory.xml' path='doc/member[@name="M:NewClumplet(System.Int32)"]/*'/>
		public static IClumplet NewClumplet(int type)
		{
			return GDS.NewClumplet(type);
		}
		
		/// <include file='xmldoc/gdsfactory.xml' path='doc/member[@name="M:NewClumplet(System.Int32,System.Int32)"]/*'/>
		public static IClumplet NewClumplet(int type, int c)
		{
			return GDS.NewClumplet(type, c);
		}

		/// <include file='xmldoc/gdsfactory.xml' path='doc/member[@name="M:NewClumplet(System.Int32,System.Int16)"]/*'/>
		public static IClumplet NewClumplet(int type, short c)
		{
			return GDS.NewClumplet(type, c);
		}

		/// <include file='xmldoc/gdsfactory.xml' path='doc/member[@name="M:NewClumplet(System.Int32,System.Array)"]/*'/>
		public static IClumplet NewClumplet(int type, byte[] content) 
		{
			return GDS.NewClumplet(type, content);
		}

		/// <include file='xmldoc/gdsfactory.xml' path='doc/member[@name="M:CloneClumplet(FirebirdSql.Data.INGDS.IClumplet)"]/*'/>
		public static IClumplet CloneClumplet(IClumplet c) 
		{
			return GDS.CloneClumplet(c);
		}

		#endregion
	}
}
