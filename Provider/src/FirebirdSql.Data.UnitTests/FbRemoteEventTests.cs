/*
 *  Firebird ADO.NET Data provider for .NET and Mono
 *
 *     The contents of this file are subject to the Initial
 *     Developer's Public License Version 1.0 (the "License");
 *     you may not use this file except in compliance with the
 *     License. You may obtain a copy of the License at
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *     express or implied.  See the License for the specific
 *     language governing rights and limitations under the License.
 *
 *  Copyright (c) 2017 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Threading;
using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	public class FbRemoteEventTests : TestsBase
	{
		public FbRemoteEventTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{ }

		[Test]
		public void EventSimplyComesBackTest()
		{
			var exception = (Exception)null;
			var triggered = false;
			using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				@event.RemoteEventError += (sender, e) =>
				{
					exception = e.Error;
				};
				@event.RemoteEventCounts += (sender, e) =>
				{
					triggered = e.Name == "test" && e.Counts == 1;
				};
				@event.QueueEvents("test");
				using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "execute block as begin post_event 'test'; end";
					cmd.ExecuteNonQuery();
					Thread.Sleep(2000);
				}
				Assert.IsNull(exception);
				Assert.IsTrue(triggered);
			}
		}

		[Test]
		public void ProperCountsSingleTest()
		{
			var exception = (Exception)null;
			var triggered = false;
			using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				@event.RemoteEventError += (sender, e) =>
				{
					exception = e.Error;
				};
				@event.RemoteEventCounts += (sender, e) =>
				{
					triggered = e.Name == "test" && e.Counts == 5;
				};
				@event.QueueEvents("test");
				using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "execute block as begin post_event 'test'; post_event 'test'; post_event 'test'; post_event 'test'; post_event 'test'; end";
					cmd.ExecuteNonQuery();
					Thread.Sleep(2000);
				}
				Assert.IsNull(exception);
				Assert.IsTrue(triggered);
			}
		}

		[Test]
		public void EventNameSeparateSelectionTest()
		{
			var exception = (Exception)null;
			var triggeredA = false;
			var triggeredB = false;
			using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				@event.RemoteEventError += (sender, e) =>
				{
					exception = e.Error;
				};
				@event.RemoteEventCounts += (sender, e) =>
				{
					switch (e.Name)
					{
						case "a":
							triggeredA = e.Counts == 1;
							break;
						case "b":
							triggeredB = e.Counts == 1;
							break;
					}
				};
				@event.QueueEvents("a", "b");
				using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "execute block as begin post_event 'b'; end";
					cmd.ExecuteNonQuery();
					cmd.CommandText = "execute block as begin post_event 'a'; end";
					cmd.ExecuteNonQuery();
					Thread.Sleep(2000);
				}
				Assert.IsNull(exception);
				Assert.IsTrue(triggeredA);
				Assert.IsTrue(triggeredB);
			}
		}

		[Test]
		public void EventNameTogetherSelectionTest()
		{
			var exception = (Exception)null;
			var triggeredA = false;
			var triggeredB = false;
			using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				@event.RemoteEventError += (sender, e) =>
				{
					exception = e.Error;
				};
				@event.RemoteEventCounts += (sender, e) =>
				{
					switch (e.Name)
					{
						case "a":
							triggeredA = e.Counts == 1;
							break;
						case "b":
							triggeredB = e.Counts == 1;
							break;
					}
				};
				@event.QueueEvents("a", "b");
				using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "execute block as begin post_event 'b'; post_event 'a'; end";
					cmd.ExecuteNonQuery();
					Thread.Sleep(2000);
				}
				Assert.IsNull(exception);
				Assert.IsTrue(triggeredA);
				Assert.IsTrue(triggeredB);
			}
		}

		[Test]
		public void CancelTest()
		{
			var exception = (Exception)null;
			var triggered = 0;
			using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				@event.RemoteEventError += (sender, e) =>
				{
					exception = e.Error;
				};
				@event.RemoteEventCounts += (sender, e) =>
				{
					triggered++;
				};
				@event.QueueEvents("test");
				using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "execute block as begin post_event 'test'; end";
					cmd.ExecuteNonQuery();
					Thread.Sleep(2000);
				}
				@event.CancelEvents();
				using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "execute block as begin post_event 'test'; end";
					cmd.ExecuteNonQuery();
					Thread.Sleep(2000);
				}
				Assert.IsNull(exception);
				Assert.AreEqual(1, triggered);
			}
		}

		[Test]
		public void DoubleQueueingTest()
		{
			using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				Assert.DoesNotThrow(() => @event.QueueEvents("test"));
				Assert.Throws<InvalidOperationException>(() => @event.QueueEvents("test"));
			}
		}

		[Test]
		public void NoEventsAfterDispose()
		{
			var triggered = 0;
			using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				@event.RemoteEventCounts += (sender, e) =>
				{
					triggered++;
				};
				@event.QueueEvents("test");
				Thread.Sleep(2000);
			}
			Thread.Sleep(2000);
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "execute block as begin post_event 'test'; end";
				cmd.ExecuteNonQuery();
				Thread.Sleep(2000);
			}
			Assert.AreEqual(0, triggered);
		}

		[Test]
		public void NoExceptionWithDispose()
		{
			var exception = (Exception)null;
			using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				@event.RemoteEventError += (sender, e) =>
				{
					exception = e.Error;
				};
				@event.QueueEvents("test");
				Thread.Sleep(2000);
			}
			Thread.Sleep(2000);
			Assert.IsNull(exception);
		}
	}
}
