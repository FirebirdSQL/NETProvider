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
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed
{
	internal sealed class GenericResponse : IResponse
	{
		#region Fields

		private int			    _objectHandle;
		private long		    _blobId;
		private byte[]		    _data;
		private IscException    _exception;

		#endregion

		#region Properties

		public int ObjectHandle
		{
			get { return _objectHandle; }
		}

		public long BlobId
		{
			get { return _blobId; }
		}

		public byte[] Data
		{
			get { return _data; }
		}

		public IscException Exception
		{
			get { return _exception; }
		}

		#endregion

		#region Constructors

		public GenericResponse(int objectHandle, long blobId, byte[] data, IscException exception)
		{
			_objectHandle = objectHandle;
			_blobId = blobId;
			_data = data;
			_exception = exception;
		}

		#endregion
	}
}
