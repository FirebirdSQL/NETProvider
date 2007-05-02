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
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version11
{
    internal class GdsDatabase : FirebirdSql.Data.Client.Managed.Version10.GdsDatabase
	{
        #region · Constructors ·

        public GdsDatabase(FirebirdSql.Data.Client.Managed.Version10.GdsConnection connection)
            : base(connection)
		{
        }

        #endregion

        #region · Override Attach Method ·

        public override void Attach(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
        {
            lock (this.SyncObject)
            {
                try
                {
                    // Attach to the database
                    this.Write(IscCodes.op_attach);
                    this.Write((int)0);				    // Database	object ID
                    this.Write(database);				// Database	PATH
                    this.WriteBuffer(dpb.ToArray());	// DPB Parameter buffer

                    // Server version Request 
                    this.WriteServerVersionRequest();

                    // Send request
                    this.Flush();

                    // Save the database connection handle
                    this.Handle = this.ReadGenericResponse().ObjectHandle;

                    // Obtain the server version
                    GenericResponse response = this.ReadGenericResponse();
#warning Rewrite this
                    byte[]  buffer          = new byte[IscCodes.BUFFER_SIZE_128];
                    int     responseLength  = IscCodes.BUFFER_SIZE_128;

                    if (response.Data.Length < IscCodes.BUFFER_SIZE_128)
                    {
                        responseLength = response.Data.Length;
                    }

                    Buffer.BlockCopy(response.Data, 0, buffer, 0, responseLength);

                    this.ServerVersion = IscHelper.ParseDatabaseInfo(buffer)[0].ToString();
                }
                catch (IOException)
                {
                    try
                    {
                        this.Detach();
                    }
                    catch (Exception ex)
                    {
                    }

                    throw new IscException(IscCodes.isc_net_write_err);
                }
            }
        }

        #endregion

        #region · Override Statement Creation Methods ·

        public override StatementBase CreateStatement()
		{
			return new GdsStatement(this);
		}

        public override StatementBase CreateStatement(ITransaction transaction)
		{
			return new GdsStatement(this, transaction);
		}

		#endregion

        private void WriteServerVersionRequest()
        {
			byte[] items = new byte[]
			{
				IscCodes.isc_info_isc_version,
				IscCodes.isc_info_end
			};

            this.WriteDatabaseInfoRequest(items, IscCodes.BUFFER_SIZE_128);
        }

        private void WriteDatabaseInfoRequest(byte[] items, int bufferLength)
        {
            lock (this.SyncObject)
            {
                try
                {
                    // see src/remote/protocol.h for packet	definition (p_info struct)					
                    this.Write(IscCodes.op_info_database);	//	operation
                    this.Write(this.Handle);				//	db_handle
                    this.Write(0);							//	incarnation
                    this.WriteBuffer(items, items.Length);	//	items
                    this.Write(bufferLength);				//	result buffer length
                }
                catch (IOException)
                {
                    throw new IscException(IscCodes.isc_network_error);
                }
            }
        }
    }
}