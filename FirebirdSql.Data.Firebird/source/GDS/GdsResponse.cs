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

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsResponse
	{
		#region FIELDS

		private int		objectHandle;
		private long	blobHandle;
		private byte[]	data;

		#endregion

		#region PROPERTIES

		public int ObjectHandle
		{
			get { return objectHandle; }
		}

		public long BlobHandle
		{
			get { return blobHandle; }
		}

		public byte[] Data
		{
			get { return data; }
		}

		#endregion

		#region CONSTRUCTORS

		public GdsResponse()
		{
		}
		
		public GdsResponse(int objectHandle, long blobHandle, byte[] data)
		{
			this.objectHandle	= objectHandle;
			this.blobHandle		= blobHandle;
			this.data			= data;
		}

		#endregion
	}
}
