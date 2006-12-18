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
 * 
 *  This file was originally ported from JayBird <http://firebird.sourceforge.net/>
 */

using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;

using FirebirdSql.Data.INGDS;

namespace FirebirdSql.Data.NGDS
{
	/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="T:isc_db_handle_impl"]/*'/>
	internal class isc_db_handle_impl : isc_db_handle 
	{
		#region EVENTS 

		public event DbWarningMessageEventHandler DbWarningMessage;

		#endregion

		#region FIELDS

		private bool				invalid;
		private int					rdb_id;
		private ArrayList			rdb_transactions	= new ArrayList();
		private ArrayList 			rdb_warnings 		= new ArrayList();
		
		private ArrayList			rdb_sql_requests	= new ArrayList();				
		
		private int					packetSize			= 8192;
		private Socket				socket				= null;
		private NetworkStream		networkStream		= null;
		private XdrOutputStream		output;
		private XdrInputStream		input;

		private int					op					= -1;
		private int					protocolVersion		= 10;
		private int					protocolArchitecture;		

		#endregion

		#region PROPERTIES

		public int PacketSize
		{
			get { return packetSize; }
			set { packetSize = value; }
		}

		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="P:Rdb_id"]/*'/>
		public int Rdb_id
		{
			get
			{
				CheckValidity();
				return rdb_id;
			}
			set
			{
				CheckValidity();
				rdb_id = value;
			}
		}

		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="P:Transactions"]/*'/>
		public ArrayList Transactions
		{
			get { return rdb_transactions; }
		}

		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="P:Output"]/*'/>
		public XdrOutputStream Output
		{
			get { return output; }
			set { output = value; }
		}

		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="P:Input"]/*'/>
		public XdrInputStream Input
		{
			get { return input; }
			set { input = value; }
		}

		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="P:DbSocket"]/*'/>
		public Socket DbSocket
		{
			get { return socket; }
			set { socket = value; }
		}

		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="P:DbNetworkStream"]/*'/>
		public NetworkStream DbNetworkStream
		{
			get { return networkStream; }
			set { networkStream = value; }
		}	

		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="P:IsValid"]/*'/>
		public bool IsValid
		{
			get { return !invalid; }
		}
		
		public ArrayList SqlRequest
		{
			get { return rdb_sql_requests; }
		}

		public int Op
		{
			get { return op; }
			set { op = value; }
		}

		public int ProtocolVersion
		{
			get { return protocolVersion; }
			set { protocolVersion = value; }
		}

		public int ProtocolArchitecture
		{
			get { return protocolArchitecture; }
			set { protocolArchitecture = value; }
		}
		
		#endregion

		#region METHODS

		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="M:HasTransactions"]/*'/>
		public bool HasTransactions()
		{
			CheckValidity();
			return rdb_transactions.Count == 0 ? false : true;
		}

		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="M:TransactionCount"]/*'/>
		public int TransactionCount()
		{
			CheckValidity();
			return rdb_transactions.Count;
		}

		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="M:AddTransaction(FirebirdSql.Data.NGDS.isc_tr_handle_impl)"]/*'/>
		public void AddTransaction(isc_tr_handle_impl tr)
		{
			CheckValidity();
			rdb_transactions.Add(tr);
		}

		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="M:RemoveTransaction(FirebirdSql.Data.NGDS.isc_tr_handle_impl)"]/*'/>
		public void RemoveTransaction(isc_tr_handle_impl tr)
		{
			CheckValidity();
			rdb_transactions.Remove(tr);
		}
		
		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="M:GetWarnings"]/*'/>
		public ArrayList GetWarnings() 
		{
			CheckValidity();
			lock (rdb_warnings) 
			{
				return new ArrayList(rdb_warnings);
			}
		}
		
		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="M:AddWarning(FirebirdSql.Data.INGDS.GDSException)"]/*'/>
		public void AddWarning(GDSException warning) 
		{
			CheckValidity();
			lock (rdb_warnings) 
			{
				rdb_warnings.Add(warning);
			}

			if (DbWarningMessage != null)
			{
				DbWarningMessage(this, new DbWarningMessageEventArgs(warning));
			}
		}
		
		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="M:ClearWarnings"]/*'/>
		public void ClearWarnings() 
		{
			CheckValidity();
			lock (rdb_warnings) 
			{
				rdb_warnings.Clear();
			}
		}

		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="M:CheckValidity"]/*'/>
		private void CheckValidity() 
		{
			if (invalid)
			{
				throw new InvalidOperationException("This database handle is invalid and cannot be used anymore.");
			}
		}

		/// <include file='xmldoc/isc_db_handle_impl.xml' path='doc/member[@name="M:Invalidate"]/*'/>
		internal void Invalidate()
		{
			try
			{
				input.Close();
				output.Close();				
				networkStream.Close();
				socket.Close();
			     
				input  = null;
				output = null;
				
				socket		  = null;
				networkStream = null;

				invalid = true;
			}
			catch(IOException ex)
			{
				throw ex;
			}
		}

		#endregion
	}
}
