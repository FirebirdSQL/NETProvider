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
using System.IO;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Embedded
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class FesSvcAttachment : FesAttachment, ISvcAttachment
	{
		#region Constructors

		public FesSvcAttachment(AttachmentParams parameters) : base(parameters)
		{
		}

		#endregion

		#region Methods

		public void Attach(string service, SpbBuffer spb)
		{
			int[]	statusVector	= FesAttachment.GetNewStatusVector();
			int		svcHandle		= this.Handle;

			int status = FbClient.isc_service_attach(
				statusVector,
				(short) service.Length,
				service,
				ref svcHandle,
				(short) spb.Length,
				spb.ToArray());

			// Parse status vector
			this.ParseStatusVector(statusVector);

			// Update status vector
			this.Handle = svcHandle;
		}

		public void Detach()
		{
			int[]	statusVector	= FesAttachment.GetNewStatusVector();
			int		svcHandle		= this.Handle;

			int status = FbClient.isc_service_detach(
				statusVector,
				ref svcHandle);

			// Parse status vector
			this.ParseStatusVector(statusVector);

			// Update status vector
			this.Handle = svcHandle;
		}				

		public void Start(SpbBuffer spb)
		{
			int[]	statusVector	= FesAttachment.GetNewStatusVector();
			int		svcHandle		= this.Handle;
			int		reserved		= 0;

			int status = FbClient.isc_service_start(
				statusVector,
				ref svcHandle,
				ref reserved,
				(short) spb.Length,
				spb.ToArray());

			// Parse status vector
			this.ParseStatusVector(statusVector);
		}

		public void Query(
			SpbBuffer	spb	,
			int 		requestLength	,
			byte[] 		requestBuffer	,
			int 		bufferLength	,
			byte[] 		buffer)
		{
			int[]	statusVector	= FesAttachment.GetNewStatusVector();
			int		svcHandle		= this.Handle;
			int		reserved		= 0;

			int status = FbClient.isc_service_query(
				statusVector,
				ref svcHandle,
				ref reserved,
				(short) spb.Length,
				spb.ToArray(), 
				(short) requestLength,
				requestBuffer,
				(short) buffer.Length,
				buffer);

			// Parse status vector
			this.ParseStatusVector(statusVector);
		}
		
		public override void SendWarning(IscException warning) 
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
