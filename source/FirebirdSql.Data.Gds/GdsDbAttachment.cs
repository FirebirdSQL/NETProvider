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
using System.IO;
using System.Text;
using System.Collections;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Gds
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class GdsDbAttachment : GdsAttachment, IDbAttachment
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

		public GdsDbAttachment(AttachmentParams parameters) : base(parameters)
		{
		}

		#endregion

		#region Database Methods

		public void CreateDatabase(AttachmentParams parameters, DpbBuffer c)
		{
			lock (this) 
			{
				try 
				{
					this.Connect();
					this.Send.Write(IscCodes.op_create);
					this.Send.Write(0);		// packet->p_atch->p_atch_database
					this.Send.Write(this.Parameters.Database);
					this.Send.WriteBuffer(c.ToArray());
					this.Send.Flush();

					try 
					{
						GdsResponse r = this.ReceiveResponse();
						this.Handle = r.ObjectHandle;

						this.Detach();
					} 
					catch (IscException g) 
					{
						try
						{
							this.Disconnect();
						}
						catch (Exception)
						{
						}

						throw g;
					}
				} 
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_net_write_err);
				}
			}
		}

		public void DropDatabase()
		{
			lock (this) 
			{				
				try 
				{
					this.Send.Write(IscCodes.op_drop_database);
					this.Send.Write(this.Handle);
					this.Send.Flush();

					GdsResponse r = this.ReceiveResponse();

					this.Disconnect();
				} 
				catch (IOException) 
				{
					try
					{
						this.Detach();
					}
					catch {}

					throw new IscException(IscCodes.isc_network_error);
				}
			}
		}

		#endregion

		#region Methods

		public void Attach()
		{
			lock (this)
			{
				this.Connect();

				this.identify();
				try
				{
					DpbBuffer dpb = this.Parameters.BuildDpb(
						this.IsLittleEndian);

					this.Send.Write(IscCodes.op_attach);
					this.Send.Write(0);
					this.Send.Write(this.Parameters.Database);
					this.Send.WriteBuffer(dpb.ToArray());
					this.Send.Flush();
					
					try 
					{
						GdsResponse r = this.ReceiveResponse();
						this.Handle = r.ObjectHandle;
					}
					catch (IscException ge) 
					{
						try
						{
							this.Disconnect();
						}
						catch (Exception)
						{
						}
						throw ge;
					}
				} 
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_write_err);
				}

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
	        
				try 
				{
					this.Send.Write(IscCodes.op_detach);
					this.Send.Write(this.Handle);
					this.Send.Flush();

					GdsResponse r = this.ReceiveResponse();

					this.transactionCount = 0;
				} 
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_network_error);
				}
				finally
				{
					try 
					{
						this.Disconnect();
					}
					catch (IOException) 
					{
						throw new IscException(IscCodes.isc_network_error);
					} 
				}
			}
		}

		public ITransaction BeginTransaction(DpbBuffer tpb)
		{
			GdsTransaction transaction = new GdsTransaction(this);
			
			transaction.BeginTransaction(tpb);

			return transaction;
		}

		public StatementBase CreateStatement()
		{
			return new GdsStatement(this);
		}

		public StatementBase CreateStatement(ITransaction transaction)
		{
			return new GdsStatement(this, transaction as GdsTransaction);
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

		private void identify()
		{
			try
			{
				// Here we identify the user to the engine.  
				// This may or may not be used as login info to a database.				
				byte[] user = Encoding.Default.GetBytes(System.Environment.UserName);
				byte[] host = Encoding.Default.GetBytes(System.Net.Dns.GetHostName());
				
				MemoryStream user_id = new MemoryStream();

				/* User Name */
				user_id.WriteByte(1);
				user_id.WriteByte((byte)user.Length);
				user_id.Write(user, 0, user.Length);
				/* Host name */
				user_id.WriteByte(4);
				user_id.WriteByte((byte)host.Length);
				user_id.Write(host, 0, host.Length);
				/* Attach/create using this connection 
				 * will use user verification
				 */
				user_id.WriteByte(6);
				user_id.WriteByte(0);

				this.Send.Write(IscCodes.op_connect);
				this.Send.Write(IscCodes.op_attach);
				this.Send.Write(IscCodes.CONNECT_VERSION2);	// CONNECT_VERSION2
				this.Send.Write(1);							// Architecture of client - Generic

				this.Send.Write(this.Parameters.Database);	// Database path
				this.Send.Write(1);							// Protocol versions understood
				this.Send.WriteBuffer(user_id.ToArray());	// User identification stuff
				
				this.Send.Write(IscCodes.PROTOCOL_VERSION10);// Protocol version
				this.Send.Write(1);							// Architecture of client - Generic
				this.Send.Write(2);							// Minumum type
				this.Send.Write(3);							// Maximum type
				this.Send.Write(2);							// Preference weight
			
				this.Send.Flush();
				
				if (this.ReadOperation() == IscCodes.op_accept) 
				{
					this.Receive.ReadInt32();	// Protocol version
					this.Receive.ReadInt32();	// Architecture for protocol
					this.Receive.ReadInt32();	// Minimum type
				} 
				else 
				{
					try
					{					
						this.Detach();
					}
					catch (Exception)
					{
					}
					finally
					{
						throw new IscException(IscCodes.isc_connect_reject);
					}
				}
			} 
			catch (IOException)
			{
				throw new IscException(IscCodes.isc_arg_gds, IscCodes.isc_network_error, Parameters.DataSource);
			}
		}

		/// <summary>
		/// isc_database_info
		/// </summary>
		private void databaseInfo(byte[] items, byte[] buffer, int bufferLength)
		{		
			lock (this) 
			{			
				try 
				{					
					// see src/remote/protocol.h for packet definition (p_info struct)					
					this.Send.Write(IscCodes.op_info_database);	//	operation
					this.Send.Write(this.Handle);				//	db_handle
					this.Send.Write(0);							//	incarnation
					this.Send.WriteBuffer(items, items.Length);	//	items
					this.Send.Write(bufferLength);				//	result buffer length

					this.Send.Flush();

					GdsResponse r = this.ReceiveResponse();

					Buffer.BlockCopy(r.Data, 0, buffer, 0, bufferLength);
				}
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_network_error);
				}
			}
		}

		#endregion
	}
}