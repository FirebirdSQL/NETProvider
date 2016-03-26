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
 *
 * Contributors:
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	[Serializable]
	public sealed class FbException : DbException
	{
		#region Fields

		private FbErrorCollection _errors;

		#endregion

		#region Properties

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FbErrorCollection Errors
		{
			get
			{
				return _errors ?? (_errors = new FbErrorCollection());
			}
		}

		public override int ErrorCode
		{
			get
			{
				return (InnerException as IscException)?.ErrorCode ?? base.ErrorCode;
			}
		}

		public string SQLSTATE
		{
			get
			{
				return (InnerException as IscException)?.SQLSTATE;
			}
		}

		#endregion

		#region Constructors

		internal FbException()
			: base()
		{
		}

		internal FbException(string message)
			: base(message)
		{
		}

		internal FbException(string message, Exception innerException)
			: base(message, innerException)
		{
			if (innerException is IscException)
			{
				ProcessIscExceptionErrors((IscException)innerException);
			}
		}

		internal FbException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_errors = (FbErrorCollection)info.GetValue("errors", typeof(FbErrorCollection));
		}

		#endregion

		#region Methods

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		[SecurityCritical]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("errors", _errors);
		}

		#endregion

		#region Internal Methods

		internal void ProcessIscExceptionErrors(IscException innerException)
		{
			foreach (IscError error in innerException.Errors)
			{
				Errors.Add(error.Message, error.ErrorCode);
			}
		}

		#endregion
	}
}
