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
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	public sealed class FbInfoMessageEventArgs : EventArgs
	{
		#region Fields

		private FbErrorCollection errors = new FbErrorCollection();
		private string message = string.Empty;

		#endregion

		#region Properties

		public FbErrorCollection Errors
		{
			get { return this.errors; }
		}

		public string Message
		{
			get { return this.message; }
		}

		#endregion

		#region Constructors

		internal FbInfoMessageEventArgs(IscException ex)
		{
			this.message = ex.Message;

			foreach (IscError error in ex.Errors)
			{
				this.errors.Add(error.Message, error.ErrorCode);
			}
		}

		#endregion
	}
}
