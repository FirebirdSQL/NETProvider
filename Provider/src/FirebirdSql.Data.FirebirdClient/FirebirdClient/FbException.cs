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
#if !NETCORE10
using System.Runtime.Serialization;
#endif

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
#if !NETCORE10
	[Serializable]
#endif
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

#if NETCORE10
		public int ErrorCode
#else
		public override int ErrorCode
#endif
		{
			get
			{
				return (InnerException as IscException)?.ErrorCode ?? 0;
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

#if !NETCORE10
		internal FbException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_errors = (FbErrorCollection)info.GetValue("errors", typeof(FbErrorCollection));
		}
#endif

		#endregion

		#region Methods

#if !NETCORE10
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("errors", _errors);
		}
#endif

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
