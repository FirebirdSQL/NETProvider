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
	/// <include file='xmldoc/fbpermissionattribute.xml' path='doc/member[@name="T:FbPermissionAttribute"]/*'/>
	[AttributeUsage(AttributeTargets.Assembly    | 
					AttributeTargets.Class 	     | 
					AttributeTargets.Struct      | 
					AttributeTargets.Constructor |
					AttributeTargets.Method)]
	[Serializable]
	public sealed class FbPermissionAttribute : DBDataPermissionAttribute
	{
		#region CONSTRUCTORS

		/// <include file='xmldoc/fbpermissionattribute.xml' path='doc/member[@name="M:#ctor(System.Security.Permissions.SecurityAction)"]/*'/>
		public FbPermissionAttribute(SecurityAction action) : base(action)
		{
		}

		#endregion

		#region METHODS

		/// <include file='xmldoc/fbpermissionattribute.xml' path='doc/member[@name="M:CreatePermission"]/*'/>
		public override IPermission CreatePermission()
		{
			FbPermission permission = null;
			
			permission = new FbPermission(PermissionState.Unrestricted,
											this.AllowBlankPassword);

			return permission;
		}

		#endregion
	}
}
