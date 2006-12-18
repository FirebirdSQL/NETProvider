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

using NUnit.Framework;
using System;
using System.Data;
using FirebirdSql.Data.Firebird;
using FirebirdSql.Data.Firebird.Events;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbEventTest : BaseTest 
	{
		public FbEventTest() : base(false)
		{		
		}

		[Test]
		public void EventsTest()
		{
			Transaction = Connection.BeginTransaction();

			FbEvent FbEvent = new FbEvent(Connection);

			FbEventAlertEventHandler e = new FbEventAlertEventHandler(processEvent);

			FbEvent.EventAlert += e;
			FbEvent.RegisterEvents("new row", "updated row");
			
			FbEvent.QueEvents();

			FbCommand command = new FbCommand("UPDATE TEST SET char_field = 'events test'", Connection , Transaction);

			command.ExecuteNonQuery();
			
			Transaction.Commit();

			FbEvent.QueEvents();
		}

		private void processEvent(object sender, FbEventAlertEventArgs e)
		{
			for (int i = 0; i < e.Counts.Length; i++)
			{
				Console.WriteLine("{0} Event - Counts : {1}", i, e.Counts[i]);
			}
		}
	}
}
