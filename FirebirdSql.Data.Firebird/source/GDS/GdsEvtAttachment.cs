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
using System.Collections;
using System.Threading;

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsEvtAttachment : GdsAttachment
	{
		#region FIELDS

		private GdsDbAttachment	db;
		private Thread			eventThread;		
		private Hashtable		events;
		private bool			asyncReads;

		#endregion

		#region PROPERTIES

		public Hashtable Events
		{
			get { return events; }
		}

		#endregion

		#region CONSTRUCTORS

		public GdsEvtAttachment(GdsDbAttachment db, GdsAttachParams parameters) : base(parameters)
		{	
			this.db		= db;
			this.events = new Hashtable();
		}

		#endregion

		#region METHODS

		public override void Attach()
		{
			Connect();
		}

		public override void Detach()
		{
			lock (this)
			{
				lock (this.events.SyncRoot)
				{
					StopEventThread();
					this.events = null;
				}

				Disconnect();
			}
		}

		public void StartEventThread()
		{
			lock (this)
			{
				asyncReads = true;

				eventThread = new Thread(new ThreadStart(eventThreadHandler));
				eventThread.Start();
				eventThread.IsBackground = true;
			}
		}

		public void StopEventThread()
		{
			lock (this)
			{
				if (eventThread != null)
				{
					asyncReads = false;
					if (eventThread.ThreadState == ThreadState.Running)
					{
						eventThread.Abort();
						eventThread.Join();
					}
					eventThread = null;
				}
			}
		}

		public override void SendWarning(GdsException ex)
		{
		}

		private void eventThreadHandler()
		{
			int	eventOP	= -1;

			while (asyncReads)
			{
				try
				{
					eventOP = NextOperation();
					lock (this)
					{						
						switch(eventOP)
						{
							case GdsCodes.op_exit:
							case GdsCodes.op_disconnect:
							{
								Detach();
							}
								break;

							case GdsCodes.op_event:
							{
								lock (events.SyncRoot)
								{
									int		rdb_id		= Receive.ReadInt();							
									byte[]	buffer		= Receive.ReadBuffer();
									byte[]	ast			= Receive.ReadBytes(8);
									int		event_id	= Receive.ReadInt();

									GdsEvent gdsEvent = (GdsEvent)events[event_id];

									// Notify event request
									if (gdsEvent != null)
									{
										gdsEvent.NotifyEvent(buffer);
									}
								}
							}
								break;
						}
					}					
				}
				catch (ThreadAbortException)
				{
					// Finish thread execution
					asyncReads = false;
					break;
				}
				catch (Exception)
				{
					asyncReads = false;
				}
			}
		}

		#endregion
	}
}
