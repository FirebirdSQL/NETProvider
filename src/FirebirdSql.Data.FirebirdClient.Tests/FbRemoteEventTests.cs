/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	public class FbRemoteEventTests : FbTestsBase
	{
		public FbRemoteEventTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[Test]
		public async Task EventSimplyComesBackTest()
		{
			var exception = (Exception)null;
			var triggered = false;
			await using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				@event.RemoteEventError += (sender, e) =>
				{
					exception = e.Error;
				};
				@event.RemoteEventCounts += (sender, e) =>
				{
					triggered = e.Name == "test" && e.Counts == 1;
				};
				await @event.OpenAsync();
				await @event.QueueEventsAsync(new[] { "test" });
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "execute block as begin post_event 'test'; end";
					await cmd.ExecuteNonQueryAsync();
					Thread.Sleep(2000);
				}
				Assert.IsNull(exception);
				Assert.IsTrue(triggered);
			}
		}

		[Test]
		public async Task ProperCountsSingleTest()
		{
			var exception = (Exception)null;
			var triggered = false;
			await using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				@event.RemoteEventError += (sender, e) =>
				{
					exception = e.Error;
				};
				@event.RemoteEventCounts += (sender, e) =>
				{
					triggered = e.Name == "test" && e.Counts == 5;
				};
				await @event.OpenAsync();
				await @event.QueueEventsAsync(new[] { "test" });
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "execute block as begin post_event 'test'; post_event 'test'; post_event 'test'; post_event 'test'; post_event 'test'; end";
					await cmd.ExecuteNonQueryAsync();
					Thread.Sleep(2000);
				}
				Assert.IsNull(exception);
				Assert.IsTrue(triggered);
			}
		}

		[Test]
		public async Task EventNameSeparateSelectionTest()
		{
			var exception = (Exception)null;
			var triggeredA = false;
			var triggeredB = false;
			await using (var @event = new FbRemoteEvent(Connection.ConnectionString))
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
				await @event.OpenAsync();
				await @event.QueueEventsAsync(new[] { "a", "b" });
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "execute block as begin post_event 'b'; end";
					await cmd.ExecuteNonQueryAsync();
					cmd.CommandText = "execute block as begin post_event 'a'; end";
					await cmd.ExecuteNonQueryAsync();
					Thread.Sleep(2000);
				}
				Assert.IsNull(exception);
				Assert.IsTrue(triggeredA);
				Assert.IsTrue(triggeredB);
			}
		}

		[Test]
		public async Task EventNameTogetherSelectionTest()
		{
			var exception = (Exception)null;
			var triggeredA = false;
			var triggeredB = false;
			await using (var @event = new FbRemoteEvent(Connection.ConnectionString))
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
				await @event.OpenAsync();
				await @event.QueueEventsAsync(new[] { "a", "b" });
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "execute block as begin post_event 'b'; post_event 'a'; end";
					await cmd.ExecuteNonQueryAsync();
					Thread.Sleep(2000);
				}
				Assert.IsNull(exception);
				Assert.IsTrue(triggeredA);
				Assert.IsTrue(triggeredB);
			}
		}

		[Test]
		public async Task CancelTest()
		{
			var exception = (Exception)null;
			var triggered = 0;
			await using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				@event.RemoteEventError += (sender, e) =>
				{
					exception = e.Error;
				};
				@event.RemoteEventCounts += (sender, e) =>
				{
					triggered++;
				};
				await @event.OpenAsync();
				await @event.QueueEventsAsync(new[] { "test" });
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "execute block as begin post_event 'test'; end";
					await cmd.ExecuteNonQueryAsync();
					Thread.Sleep(2000);
				}
				await @event.CancelEventsAsync();
				await using (var cmd = Connection.CreateCommand())
				{
					cmd.CommandText = "execute block as begin post_event 'test'; end";
					await cmd.ExecuteNonQueryAsync();
					Thread.Sleep(2000);
				}
				Assert.IsNull(exception);
				Assert.AreEqual(1, triggered);
			}
		}

		[Test]
		public async Task DoubleQueueingTest()
		{
			await using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				await @event.OpenAsync();
				Assert.DoesNotThrowAsync(() => @event.QueueEventsAsync(new[] { "test" }));
				Assert.ThrowsAsync<InvalidOperationException>(() => @event.QueueEventsAsync(new[] { "test" }));
			}
		}

		[Test]
		public async Task NoEventsAfterDispose()
		{
			var triggered = 0;
			await using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				@event.RemoteEventCounts += (sender, e) =>
				{
					triggered++;
				};
				await @event.OpenAsync();
				await @event.QueueEventsAsync(new[] { "test" });
				Thread.Sleep(2000);
			}
			Thread.Sleep(2000);
			await using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "execute block as begin post_event 'test'; end";
				await cmd.ExecuteNonQueryAsync();
				Thread.Sleep(2000);
			}
			Assert.AreEqual(0, triggered);
		}

		[Test]
		public async Task NoExceptionWithDispose()
		{
			var exception = (Exception)null;
			await using (var @event = new FbRemoteEvent(Connection.ConnectionString))
			{
				@event.RemoteEventError += (sender, e) =>
				{
					exception = e.Error;
				};
				await @event.OpenAsync();
				await @event.QueueEventsAsync(new[] { "test" });
				Thread.Sleep(2000);
			}
			Thread.Sleep(2000);
			Assert.IsNull(exception);
		}
	}
}
