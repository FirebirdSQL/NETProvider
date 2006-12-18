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
using System.Threading;

namespace FirebirdSql.Data.Firebird.Gds
{
	#region GDS_WARNING_EVENTSARGS_CLASS

	internal class GdsWarningMessageEventArgs : EventArgs
	{
		private GdsException exception;
		private string		 message;

		public string Message
		{
			get { return message; }
		}

		public GdsException Exception
		{
			get { return exception; }
		}

		public GdsWarningMessageEventArgs(GdsException ex)
		{
			message		= ex.ToString();
			exception	= ex;			
		}
	}

	#endregion

	#region DELEGATES

	internal delegate void GdsWarningMessageEventHandler(object sender, GdsWarningMessageEventArgs e);

	#endregion

	internal class GdsDbAttachment : GdsAttachment
	{
		#region EVENTS

		public event GdsWarningMessageEventHandler DbWarningMessage;

		#endregion

		#region STATIC_FIELDS

		private static GdsCharsetCollection		charSets;

		#endregion

		#region STATIC_PROPERTIES

		public static GdsCharsetCollection CharSets
		{
			get { return charSets; }
		}

		#endregion

		#region FIELDS

		private int					remoteEventId;
		private GdsEvtAttachment	eventsAtt;
		private int					transactionCount;

		#endregion

		#region PROPERTIES

		public GdsEvtAttachment EventsAtt
		{
			get { return eventsAtt; }
			set { eventsAtt = value; }
		}

		public int TransactionCount
		{
			get { return transactionCount; }
			set { transactionCount = value; }
		}

		#endregion

		#region CONSTRUCTORS

		public GdsDbAttachment(GdsAttachParams parameters) : base(parameters)
		{
		}

		#endregion

		#region DB_HANDLING_METHODS

		public void CreateDatabase(GdsAttachParams parameters, GdsDpbBuffer c)
		{
			lock (this) 
			{
				try 
				{
					Connect();
					Send.WriteInt(GdsCodes.op_create);
					Send.WriteInt(0);					// packet->p_atch->p_atch_database
					Send.WriteString(parameters.Database);
					Send.WriteTyped(GdsCodes.isc_dpb_version1, c.ToArray());
					Send.Flush();

					try 
					{
						GdsResponse r = ReceiveResponse();
						Handle = r.ObjectHandle;
					} 
					catch (GdsException g) 
					{
						try
						{
							Disconnect();
						}
						catch (Exception)
						{
						}

						throw g;
					}
				} 
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_write_err);
				}
			}
		}

		public void DropDatabase()
		{
			lock (this) 
			{				
				try 
				{
					Send.WriteInt(GdsCodes.op_drop_database);
					Send.WriteInt(Handle);
					Send.Flush();

					GdsResponse r = ReceiveResponse();
				} 
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_network_error);
				}
			}
		}

		#endregion

		#region EVENTS_METHODS

		public void ConnectionRequest()
		{
			lock (this)
			{
				try 
				{
					Send.WriteInt(GdsCodes.op_connect_request);
					Send.WriteInt(Handle);
					Send.WriteInt(GdsCodes.P_REQ_async);
					Send.WriteInt(0);

					Send.Flush();

					int op = ReadOperation();

					int rdb_id	= Receive.ReadInt();					
					int	port	= Receive.ReadInt16();
					
					// Skip next two bytes
					int n = Receive.ReadShort();

					byte[] ipBytes	= Receive.ReadBytes(4);
					string ip		= ipBytes[3].ToString() + "." + 
									ipBytes[2].ToString() + "." + 
									ipBytes[1].ToString() + "." + 
									ipBytes[0].ToString();

					// Read Garbage
					Receive.ReadBytes(12);

					// Receive Response
					ReadStatusVector();

					// Make connection
					eventsAtt = new GdsEvtAttachment(this, Parameters);
					eventsAtt.Parameters.Port = port;

					// Set handle and connect
					eventsAtt.Handle = rdb_id;
					eventsAtt.Attach();

					// Start the Async thread
					eventsAtt.StartEventThread();
				}
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
			}
		}

		public void QueEvents(GdsEvent gdsEvent, short length, GdsEpbBuffer epb)
		{
			if (this.eventsAtt == null)
			{
				ConnectionRequest();
			}

			lock (this)
			{
				lock (this.eventsAtt.Events.SyncRoot)
				{
					try 
					{
						Send.WriteInt(GdsCodes.op_que_events);	// Op code
						Send.WriteInt(Handle); 					// Database object id
						Send.WriteTyped(GdsCodes.EPB_version1, 
										epb.ToArray());			// Event description block
						Send.WriteInt(0);						// Address of ast routine
						Send.WriteInt(0);						// Argument to ast routine						
						Send.WriteInt(++remoteEventId);			// Client side id of remote event

						Send.Flush();

						GdsResponse r = ReceiveResponse();

						// Update event LocalID and Handle ID
						gdsEvent.Handle		= r.ObjectHandle;
						gdsEvent.LocalID	= remoteEventId;

						eventsAtt.Events.Add(gdsEvent.LocalID, gdsEvent);
					}
					catch (IOException) 
					{
						throw new GdsException(GdsCodes.isc_net_read_err);
					}
				}
			}
		}

		public void CancelEvents(GdsEvent gdsEvent)
		{
			lock (this)
			{
				lock (this.eventsAtt) 
				{
					try 
					{
						Send.WriteInt(GdsCodes.op_cancel_events);	// Op code
						Send.WriteInt(Handle); 						// Database object id
						Send.WriteInt(gdsEvent.Handle);				// Event ID

						Send.Flush();

						GdsResponse r = ReceiveResponse();

						if (this.eventsAtt != null)
						{
							this.eventsAtt.Events.Remove(gdsEvent.LocalID);
						}
					}
					catch (IOException) 
					{
						throw new GdsException(GdsCodes.isc_net_read_err);
					}
				}
			}
		}

		#endregion

		#region CREATION_METHODS

		public GdsTransaction CreateTransaction(IsolationLevel isolationLevel)
		{
			return new GdsTransaction(this, isolationLevel);
		}
		
		public GdsStatement CreateStatement()
		{
			return new GdsStatement(this);
		}

		public GdsStatement CreateStatement(
			string commandText, GdsTransaction transaction)
		{
			return new GdsStatement(commandText, this, transaction);
		}

		#endregion

		#region METHODS

		public override void Attach()
		{
			lock (this)
			{
				Connect();

				identify();
				try
				{
					GdsDpbBuffer dpb = buildDpb();

					Send.WriteInt(GdsCodes.op_attach);
					Send.WriteInt(0);
					Send.WriteString(Parameters.Database);
					Send.WriteBuffer(dpb.ToArray());
					Send.Flush();
					
					try 
					{
						GdsResponse r = ReceiveResponse();
						Handle = r.ObjectHandle;
					}
					catch (GdsException ge) 
					{
						try
						{
							Disconnect();
						}
						catch (Exception)
						{
						}
						throw ge;
					}
				} 
				catch (IOException)
				{
					throw new GdsException(GdsCodes.isc_net_write_err);
				}
			}			
		}

		public override void Detach()
		{
			lock (this) 
			{
				if (TransactionCount > 0) 
				{
					throw new GdsException(GdsCodes.isc_open_trans, TransactionCount);
				}

				// if is listening for events clos events connection
				if (eventsAtt != null)
				{
					eventsAtt.Detach();
				}
	        
				try 
				{
					Send.WriteInt(GdsCodes.op_detach);
					Send.WriteInt(Handle);
					Send.Flush();

					GdsResponse r = ReceiveResponse();

					transactionCount = 0;
				} 
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_network_error);
				}
				finally
				{
					try 
					{
						Disconnect();
					}
					catch (IOException) 
					{
						throw new GdsException(GdsCodes.isc_network_error);
					} 
				}
			}			
		}

		/// <summary>
		/// isc_database_info
		/// </summary>
		public void GetDatabaseInfo(byte[] items, int buffer_length, byte[] buffer)
		{		
			lock (this) 
			{			
				try 
				{					
					// see src/remote/protocol.h for packet definition (p_info struct)					
					Send.WriteInt(GdsCodes.op_info_database);		//	operation
					Send.WriteInt(Handle);							//	db_handle
					Send.WriteInt(0);								//	incarnation
					Send.WriteBuffer(items, items.Length);			//	items
					Send.WriteInt(buffer_length);					//	result buffer length

					Send.Flush();

					GdsResponse r = ReceiveResponse();

					System.Array.Copy(r.Data, 0, buffer, 0, buffer_length);
				}
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_network_error);
				}
			}
		}

		public override void SendWarning(GdsException warning) 
		{
			if (DbWarningMessage != null)
			{
				DbWarningMessage(this, new GdsWarningMessageEventArgs(warning));
			}
		}

		#endregion

		#region PRIVATE_METHODS

		private GdsDpbBuffer buildDpb()
		{
			GdsDpbBuffer dpb = new GdsDpbBuffer();

			dpb.Append(GdsCodes.isc_dpb_version1);
			dpb.Append(GdsCodes.isc_dpb_dummy_packet_interval, 
				new byte[] {120, 10, 0, 0});
			dpb.Append(GdsCodes.isc_dpb_sql_dialect, 
				new byte[] {Parameters.Dialect, 0, 0, 0});
			dpb.Append(GdsCodes.isc_dpb_lc_ctype, Parameters.Charset.Name);
			if (Parameters.Role != null)
			{
				if (Parameters.Role.Length > 0)
				{
					dpb.Append(GdsCodes.isc_dpb_sql_role_name, Parameters.Role);
				}
			}
			dpb.Append(GdsCodes.isc_dpb_connect_timeout, Parameters.Timeout);			
			dpb.Append(GdsCodes.isc_dpb_user_name, Parameters.UserName);
			dpb.Append(GdsCodes.isc_dpb_password, Parameters.UserPassword);

			return dpb;
		}

		private void identify()
		{
			try
			{
				// Here we identify the user to the engine.  This may or may not be used 
				// as login info to a database.				
				string user = System.Environment.UserName;
				string host = System.Net.Dns.GetHostName();
				
				int n = 0;
				byte[] user_id = new byte[200];
						
				int userLength = Encoding.Default.GetByteCount(user);
				user_id[n++] = 1;		// CNCT_user
				user_id[n++] = (byte)userLength;
				System.Array.Copy(Encoding.Default.GetBytes(user), 0, user_id, n, userLength);
				n += userLength;
						
				int hostLength = Encoding.Default.GetByteCount(host);
				user_id[n++] = 4;		// CNCT_host
				user_id[n++] = (byte)host.Length;
				System.Array.Copy(Encoding.Default.GetBytes(host), 0, user_id, n, hostLength);
				n += hostLength;
			    
				user_id[n++] = 6;		// CNCT_user_verification
				user_id[n++] = 0;

				Send.WriteInt(GdsCodes.op_connect);
				Send.WriteInt(GdsCodes.op_attach);
				Send.WriteInt(GdsCodes.CONNECT_VERSION2);	// CONNECT_VERSION2
				Send.WriteInt(1);							// arch_generic

				Send.WriteString(Parameters.Database);	// p_cnct_file
				Send.WriteInt(1);						// p_cnct_count
				Send.WriteBuffer(user_id, n);			// p_cnct_user_id
				
				Send.WriteInt(GdsCodes.PROTOCOL_VERSION10);
				Send.WriteInt(1);						// arch_generic
				Send.WriteInt(2);						// ptype_rpc
				Send.WriteInt(3);						// ptype_batch_send
				Send.WriteInt(2);
			
				Send.Flush();				
				
				if (ReadOperation() == GdsCodes.op_accept) 
				{
					Receive.ReadInt();
					Receive.ReadInt();
					Receive.ReadInt();
				} 
				else 
				{
					try
					{					
						Detach();
					}
					catch (Exception)
					{
					}
					finally
					{
						throw new GdsException(GdsCodes.isc_connect_reject);
					}
				}
			} 
			catch (IOException)
			{
				throw new GdsException(GdsCodes.isc_arg_gds, GdsCodes.isc_network_error, Parameters.DataSource);
			}
		}

		#endregion

		#region STATIC_METHODS

		public static void InitializeCharSets()
		{
			if (charSets != null)
			{
				return;
			}

			charSets = new GdsCharsetCollection();

			// NONE
			GdsDbAttachment.addCharset(0, "NONE"		, 1, Encoding.Default);
			// American Standard Code for Information Interchange	
			GdsDbAttachment.addCharset(2, "ASCII"		, 1, "ascii");
			// Eight-bit Unicode Transformation Format
			GdsDbAttachment.addCharset(3, "UNICODE_FSS"	, 3, "UTF-8");
			// Shift-JIS, Japanese
			GdsDbAttachment.addCharset(5, "SJIS_0208"	, 2, "shift_jis");
			// JIS X 0201, 0208, 0212, EUC encoding, Japanese
			GdsDbAttachment.addCharset(6, "EUCJ_0208"	, 2, "euc-jp");
			// Windows Japanese	
			GdsDbAttachment.addCharset(7, "ISO2022-JP"	, 2, "iso-2022-jp");
			// MS-DOS United States, Australia, New Zealand, South Africa	
			GdsDbAttachment.addCharset(10, "DOS437"		, 1, "IBM437");
			// MS-DOS Latin-1				
			GdsDbAttachment.addCharset(11, "DOS850"		, 1, "ibm850");
			// MS-DOS Nordic	
			GdsDbAttachment.addCharset(12, "DOS865"		, 1, "IBM865");
			// MS-DOS Portuguese	
			GdsDbAttachment.addCharset(13, "DOS860"		, 1, "IBM860");
			// MS-DOS Canadian French	
			GdsDbAttachment.addCharset(14, "DOS863"		, 1, "IBM863");
			// ISO 8859-1, Latin alphabet No. 1
			GdsDbAttachment.addCharset(21, "ISO8859_1"	, 1, "iso-8859-1");
			// ISO 8859-2, Latin alphabet No. 2
			GdsDbAttachment.addCharset(22, "ISO8859_2"	, 1, "iso-8859-2");		
			// Windows Korean	
			GdsDbAttachment.addCharset(44, "KSC_5601"	, 2, "ks_c_5601-1987");
			// MS-DOS Icelandic	
			GdsDbAttachment.addCharset(47, "DOS861"		, 1, "ibm861");
			// Windows Eastern European
			GdsDbAttachment.addCharset(51, "WIN1250"	, 1, "windows-1250");
			// Windows Cyrillic
			GdsDbAttachment.addCharset(52, "WIN1251"	, 1, "windows-1251");
			// Windows Latin-1
			GdsDbAttachment.addCharset(53, "WIN1252"	, 1, "windows-1252");
			// Windows Greek
			GdsDbAttachment.addCharset(54, "WIN1253"	, 1, "windows-1253");
			// Windows Turkish
			GdsDbAttachment.addCharset(55, "WIN1254"	, 1, "windows-1254");
			// Big5, Traditional Chinese
			GdsDbAttachment.addCharset(56, "BIG_5"		, 2, "big5");
			// GB2312, EUC encoding, Simplified Chinese	
			GdsDbAttachment.addCharset(57, "GB_2312"	, 2, "gb2312");
			// Windows Hebrew
			GdsDbAttachment.addCharset(58, "WIN1255"	, 1, "windows-1255");
			// Windows Arabic	
			GdsDbAttachment.addCharset(59, "WIN1256"	, 1, "windows-1256");
			// Windows Baltic	
			GdsDbAttachment.addCharset(60, "WIN1257"	, 1, "windows-1257");
		}

		private static void addCharset(
			int id, 
			string charset, 
			int bytesPerCharacter, 
			string systemCharset)
		{
			try
			{
				charSets.Add(
					id, 
					charset, 
					bytesPerCharacter, 
					systemCharset);
			}
			catch (Exception)
			{
			}
		}

		private static void addCharset(
			int id, 
			string charset, 
			int bytesPerCharacter,
			Encoding encoding)
		{
			try
			{
				charSets.Add(
					id, 
					charset, 
					bytesPerCharacter,
					encoding);
			}
			catch (Exception)
			{
			}
		}

		#endregion
	}
}