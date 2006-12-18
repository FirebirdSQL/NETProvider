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
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Embedded
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class FesDbAttachment : FesAttachment, IDbAttachment
	{
		#region Events

		public event WarningMessageEventHandler DbWarningMessage;

		#endregion

		#region Fields
		
		private int		transactionCount;
		private string	serverVersion;

		#endregion

		#region Properties

		public int TransactionCount
		{
			get { return this.transactionCount; }
			set { this.transactionCount = value; }
		}

		public string ServerVersion
		{
			get { return this.serverVersion; }
		}

		#endregion

		#region Constructors

		public FesDbAttachment(AttachmentParams parameters) : base(parameters)
		{
		}

		#endregion

		#region Database Methods

		public void CreateDatabase(AttachmentParams parameters, DpbBuffer c)
		{
			lock (this)
			{
				string	connectionString	= this.formatConnectionString();
				int[]	statusVector		= FesAttachment.GetNewStatusVector();
				int		dbHandle			= this.Handle;

				int status = FbClient.isc_create_database(
					statusVector,
					(short)connectionString.Length,
					connectionString,
					ref dbHandle,
					(short)c.Length,
					c.ToArray(),
					0);

				this.Handle = dbHandle;

				this.Detach();
			}
		}

		public void DropDatabase()
		{
			lock (this)
			{
				int[]	statusVector	= FesAttachment.GetNewStatusVector();
				int		dbHandle		= this.Handle;

				int status = FbClient.isc_drop_database(
					statusVector,
					ref dbHandle);

				this.ParseStatusVector(statusVector);
			}
		}

		#endregion

		#region Methods

		public void Attach()
		{
			lock (this)
			{
				string	connectionString	= this.formatConnectionString();
				int[]	statusVector		= FesAttachment.GetNewStatusVector();
				int		dbHandle			= 0;
				
				DpbBuffer dpb = this.Parameters.BuildDpb(this.IsLittleEndian);

				int status = FbClient.isc_attach_database(
					statusVector,
					(short)connectionString.Length,
					connectionString,
					ref dbHandle,
					(short)dpb.Length,
					dpb.ToArray());

				this.Handle = dbHandle;
					
				this.ParseStatusVector(statusVector);

				// Get server version
				this.serverVersion = this.GetServerVersion();
			}			
		}

		public void Detach()
		{
			lock (this) 
			{
				if (this.TransactionCount > 0) 
				{
					throw new IscException(
						IscCodes.isc_open_trans, this.TransactionCount);
				}

				int[]	statusVector	= FesAttachment.GetNewStatusVector();
				int		dbHandle		= this.Handle;

				int status = FbClient.isc_detach_database(
					statusVector, ref dbHandle);

				this.Handle = dbHandle;
					
				this.ParseStatusVector(statusVector);	        
			}			
		}

		public ITransaction BeginTransaction(DpbBuffer tpb)
		{
			FesTransaction transaction = new FesTransaction(this);
			
			transaction.BeginTransaction(tpb);

			return transaction;
		}

		public StatementBase CreateStatement()
		{
			return new FesStatement(this);
		}

		public StatementBase CreateStatement(ITransaction transaction)
		{
			return new FesStatement(this, transaction as FesTransaction);
		}

		public ArrayList GetDatabaseInfo(byte[] items)
		{
			byte[] buffer = new byte[1024];

			return this.GetDatabaseInfo(items, buffer);
		}

		public ArrayList GetDatabaseInfo(byte[] items, byte[] buffer)
		{
			ArrayList information = new ArrayList();

			this.databaseInfo(items, buffer, buffer.Length);

			return IscHelper.ParseDatabaseInfo(buffer);
		}

		public string GetServerVersion()
		{
			byte[] items = new byte[]
			{
				IscCodes.isc_info_isc_version,
				IscCodes.isc_info_end
			};

			return this.GetDatabaseInfo(items)[0].ToString();
		}

		public override void SendWarning(IscException warning) 
		{
			if (this.DbWarningMessage != null)
			{
				this.DbWarningMessage(this, new WarningMessageEventArgs(warning));
			}
		}

		#endregion

		#region Private Methods

		private string formatConnectionString()
		{
			string connectionString = String.Empty;

#if (FBCLIENT || GDS32)
			StringBuilder b = new StringBuilder();
			b.AppendFormat(
					"{0}/{1}:{2}",
					this.Parameters.DataSource,
					this.Parameters.Port,
					this.Parameters.Database);
				
			connectionString = b.ToString();
#else
			connectionString = this.Parameters.Database;
#endif

			return connectionString;
		}

		/// <summary>
		/// isc_database_info
		/// </summary>
		private void databaseInfo(byte[] items, byte[] buffer, int bufferLength)
		{		
			lock (this) 
			{			
				int[]	statusVector	= FesAttachment.GetNewStatusVector();
				int		dbHandle		= this.Handle;

				int status = FbClient.isc_database_info(
					statusVector,
					ref dbHandle,
					(short)items.Length,
					items,
					(short)bufferLength,
					buffer);
					
				this.ParseStatusVector(statusVector);
			}
		}

		#endregion
	}
}