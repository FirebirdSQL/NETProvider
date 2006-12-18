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
using System.Net;
using System.Text;
using FirebirdSql.Data.Firebird;
using FirebirdSql.Data.Firebird.Gds;

namespace FirebirdSql.Data.Firebird.Events
{
	/// <include file='Doc/en_EN/FbEvent.xml' path='doc/class[@name="FbEvent"]/overview/*'/>
	public sealed class FbEvent
	{
		#region EVENTS

		/// <include file='Doc/en_EN/FbEvent.xml' path='doc/class[@name="FbEvent"]/event[@name="EventAlert"]/*'/>
		public event FbEventAlertEventHandler EventAlert;

		#endregion

		#region FIELDS

		private FbConnection				connection;
		private string[]					events;
		private	GdsEvent					gdsEvent;
		private EventRequestEventHandler	eventRequestHandler;
		private int[]						initialCounts;

		#endregion

		#region PROPERTIES

		/// <include file='Doc/en_EN/FbEvent.xml' path='doc/class[@name="FbEvent"]/property[@name="Connection"]/*'/>
		public FbConnection Connection
		{
			get { return connection; }
			set { connection = value; }
		}

		#endregion

		#region CONSTRUCTORS
		
		/// <include file='Doc/en_EN/FbEvent.xml' path='doc/class[@name="FbEvent"]/constructor[@name="ctor"]/*'/>
		public FbEvent()
		{
			eventRequestHandler = new EventRequestEventHandler(receiveEventNotification);			

			gdsEvent = new GdsEvent();
			gdsEvent.OnEventRequest += eventRequestHandler;
		}

		/// <include file='Doc/en_EN/FbEvent.xml' path='doc/class[@name="FbEvent"]/constructor[@name="ctor(FbConnection)"]/*'/>
		public FbEvent(FbConnection connection) : this()
		{
			this.connection = connection;
		}

		/// <include file='Doc/en_EN/FbEvent.xml' path='doc/class[@name="FbEvent"]/constructor[@name="ctor(FbConnection,System.Array)"]/*'/>
		public FbEvent(FbConnection connection, params string[] events) : this()
		{
			this.connection = connection;
			this.events		= events;
		}

		#endregion

		#region METHODS

		/// <include file='Doc/en_EN/FbEvent.xml' path='doc/class[@name="FbEvent"]/method[@name="RegisterEvents"]/*'/>
		public void RegisterEvents(params string[] events)
		{
			if (events.Length > 15)
			{
				throw new ArgumentException("The number of events is greater than 15.");
			}

			this.events = events;
		}

		/// <include file='Doc/en_EN/FbEvent.xml' path='doc/class[@name="FbEvent"]/method[@name="QueEvents"]/*'/>
		public void QueEvents()
		{
			if (connection == null || connection.State != ConnectionState.Open)
			{
				throw new InvalidOperationException("Connection must valid and open.");
			}

			GdsEpbBuffer epb = buildEpb();

			try
			{
				lock (gdsEvent)
				{
					connection.DbConnection.DB.QueEvents(
						gdsEvent					,
						(short)events.Length		,							
						epb);					
				}
			}
			catch (GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbEvent.xml' path='doc/class[@name="FbEvent"]/method[@name="CanceEvents"]/*'/>
		public void CancelEvents()
		{
			if (connection == null || connection.State != ConnectionState.Open)
			{
				throw new InvalidOperationException("Connection must valid and open.");
			}
			if (gdsEvent == null)
			{
				throw new InvalidOperationException("There are no events to cancel.");
			}

			try
			{				
				lock (gdsEvent)
				{
					connection.DbConnection.DB.CancelEvents(gdsEvent);

					gdsEvent.OnEventRequest -= eventRequestHandler;

					gdsEvent			= null;
					eventRequestHandler	= null;
					initialCounts		= null;
				}
			}
			catch (GdsException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		#endregion

		#region PRIVATE_METHODS

		private void receiveEventNotification(object sender, EventRequestEventArgs e)
		{
			lock (gdsEvent)
			{
				int[] counts = getCounts(e.ResultBuffer);
		
				if (EventAlert != null)
				{
					EventAlert(this, new FbEventAlertEventArgs(counts));
				}
			}
		}

		private int[] getCounts(byte[] resultBuffer)
		{
			int[]	counts	= new int[events.Length];
			int		pos		= 1;

			if (resultBuffer != null)
			{
				for (int i = 0; i < events.Length; i++)
				{
					int		length		= resultBuffer[pos++];
					string	eventName	= connection.Encoding.GetString(resultBuffer, pos, length);

					pos += length;
				
					counts[i] = BitConverter.ToInt32(resultBuffer, pos);

					pos += 4;
				}

				if (initialCounts == null)
				{
					initialCounts = counts;
				}
				else
				{
					for (int i = 0; i < counts.Length; i++)
					{
						counts[i] -= initialCounts[i];
					}
				}
			}

			return counts;
		}

		private GdsEpbBuffer buildEpb()
		{
			GdsEpbBuffer epb = new GdsEpbBuffer();
			
			for (int i = 0; i < events.Length; i++)
			{
				epb.Append(events[i]);
			}

			return epb;
		}

		#endregion
	}
}
