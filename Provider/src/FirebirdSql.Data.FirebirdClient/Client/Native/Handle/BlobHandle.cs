/*
 *	Firebird ADO.NET Data provider for .NET and Mono
 *
 *	   The contents of this file are subject to the Initial
 *	   Developer's Public License Version 1.0 (the "License");
 *	   you may not use this file except in compliance with the
 *	   License. You may obtain a copy of the License at
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *	   express or implied. See the License for the specific
 *	   language governing rights and limitations under the License.
 *
 *	Copyright (c) 2015 Hennadii Zabula
 *	All Rights Reserved.
 */

using System;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native.Handle
{
	// public visibility added, because auto-generated assembly can't work with internal types
	public class BlobHandle : FirebirdHandle
	{
		protected override bool ReleaseHandle()
		{
			if (IsClosed)
			{
				return true;
			}

			IntPtr[] statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
			BlobHandle @ref = this;
			FbClient.isc_close_blob(statusVector, ref @ref);
			handle = @ref.handle;
			var exception = FesConnection.ParseStatusVector(statusVector, Charset.DefaultCharset);
			return exception == null || exception.IsWarning;
		}
	}
}
