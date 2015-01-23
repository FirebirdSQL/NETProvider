/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace FirebirdSql.Data.FirebirdClient
{
	[Serializable]
	public sealed class FirebirdClientPermission : DBDataPermission 
	{
		#region Constructors

		[Obsolete("FbPermission() is obsolte call FbPermission(PermissionState.None)", true)]
		public FirebirdClientPermission() : this(PermissionState.None)
		{
		}

		public FirebirdClientPermission(PermissionState state) : base(state)
		{
		}

		[Obsolete("FbPermission(PermissionState, bool) is obsolete", true)]
		public FirebirdClientPermission(PermissionState state, bool allowBlankPassword) : base(state, allowBlankPassword)
		{
		}
		
		#endregion

		#region Internal Constructors

		internal FirebirdClientPermission(string connectionString) : base(PermissionState.None)
		{
			this.Add(connectionString, string.Empty, KeyRestrictionBehavior.AllowOnly);
		}

		internal FirebirdClientPermission(DBDataPermission permission) : base (permission)
		{
		}

		internal FirebirdClientPermission(DBDataPermissionAttribute permissionAttribute) 
			: base (permissionAttribute)
		{
		}

		#endregion

		#region Methods

		public override IPermission Copy()
		{
			return new FirebirdClientPermission(this);
		}

		public override void Add(string connectionString, string restrictions, KeyRestrictionBehavior behavior)
		{
			base.Add(connectionString, restrictions, behavior);
		}

		#endregion
	}
}
