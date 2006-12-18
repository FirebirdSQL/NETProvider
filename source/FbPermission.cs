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
using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace FirebirdSql.Data.Firebird
{	
	/// <include file='xmldoc/fbpermission.xml' path='doc/member[@name="T:FbPermission"]/*'/>	
	[Serializable]	
	public sealed class FbPermission : DBDataPermission
	{		
		#region FIELDS

		private PermissionState state;
		private string			connectionString;

		#endregion

		#region CONSTRUCTORS

		/// <include file='xmldoc/fbpermission.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public FbPermission()
		{
			state				= PermissionState.None;
			connectionString	= String.Empty;
		}

		/// <include file='xmldoc/fbpermission.xml' path='doc/member[@name="M:#ctor(System.Security.Permissions.PermissionState)"]/*'/>
		public FbPermission(PermissionState state) : this()
		{			
			this.state = state;
		}

		/// <include file='xmldoc/fbpermission.xml' path='doc/member[@name="M:#ctor(System.Security.Permissions.PermissionState,System.Boolean)"]/*'/>
		public FbPermission(PermissionState state, bool allowBlankPassword) : this()
		{
			this.AllowBlankPassword = allowBlankPassword;
			this.state				= state;
		}

		#endregion

		#region METHODS

		/// <include file='xmldoc/fbpermission.xml' path='doc/member[@name="M:Copy"]/*'/>
		public override IPermission Copy()
		{
			// TODO: implementar
			throw new NotImplementedException ();
		}

		/// <include file='xmldoc/fbpermission.xml' path='doc/member[@name="M:FromXml(System.Security.SecurityElement)"]/*'/>
		public override void FromXml(SecurityElement securityElement)
		{
			// TODO: implementar
			throw new NotImplementedException ();
		}

		/// <include file='xmldoc/fbpermission.xml' path='doc/member[@name="M:Intersect(System.Security.IPermission)"]/*'/>
		public override IPermission Intersect(IPermission target)
		{
			// TODO: implementar
			throw new NotImplementedException ();
		}

		/// <include file='xmldoc/fbpermission.xml' path='doc/member[@name="M:IsSubsetOf(System.Security.IPermission)"]/*'/>
		public override bool IsSubsetOf(IPermission target)
		{
			// TODO: implementar
			// throw new NotImplementedException ();
			return base.IsSubsetOf(target);
		}

		/// <include file='xmldoc/fbpermission.xml' path='doc/member[@name="M:ToString"]/*'/>
		public override string ToString()
		{					
			// TODO: implementar
			throw new NotImplementedException ();
		}

		/// <include file='xmldoc/fbpermission.xml' path='doc/member[@name="M:ToXml"]/*'/>
		public override SecurityElement ToXml()
		{
			throw new NotImplementedException ();
		}

		/// <include file='xmldoc/fbpermission.xml' path='doc/member[@name="M:Union(System.Security.IPermission)"]/*'/>
		public override IPermission Union(IPermission target)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
