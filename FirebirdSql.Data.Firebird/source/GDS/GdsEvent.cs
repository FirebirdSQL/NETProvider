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
	#region EVENTARGS_CLASS

	internal class EventRequestEventArgs : EventArgs
	{
		private byte[] resultBuffer;
 
		public byte[] ResultBuffer
		{
			get { return resultBuffer; } 
			set { resultBuffer = value; } 
		}

		public EventRequestEventArgs(byte[] resultBuffer)
		{
			this.resultBuffer = resultBuffer;
		}
	}

	#endregion

	#region DELEGATES

	internal delegate void EventRequestEventHandler(object sender, EventRequestEventArgs e);

	#endregion

	internal class GdsEvent
	{
		#region EVENTS

		public event EventRequestEventHandler OnEventRequest;

		#endregion

		#region FIELDS

		private int			event_rid;
		private int			local_id;

		#endregion

		#region PROPERTIES

		public int Handle
		{
			get { return event_rid; }
			set	{ event_rid = value; }
		}

		public int LocalID
		{
			get { return local_id; }
			set	{ local_id = value; }
		}

		#endregion

		#region CONSTRUCTORS

		public GdsEvent()
		{
		}

		#endregion

		#region METHODS

		public void NotifyEvent(byte[] resultBuffer)
		{
			if (OnEventRequest != null)
			{
				OnEventRequest(this, new EventRequestEventArgs(resultBuffer));
			}
		}

		#endregion
	}
}
