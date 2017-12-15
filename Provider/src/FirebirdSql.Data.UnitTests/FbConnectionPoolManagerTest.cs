using System;
using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture()]
	public class ConnectionPoolLifetimeHelperTest
	{
		[Test]
		public void IsAliveTrueIfLifetimeNotExceed()
		{
			var timeAgo = Environment.TickCount - (10 * 1000); //10 seconds
			var now = Environment.TickCount;
			var isAlive = ConnectionPoolLifetimeHelper.IsAlive(20, timeAgo, now);
			Assert.IsTrue(isAlive);
		}

		[Test]
		public void IsAliveFalseIfLifetimeIsExceed()
		{
			var timeAgo = Environment.TickCount  - (30 * 1000); //30 seconds
			var now = Environment.TickCount;
			var isAlive = ConnectionPoolLifetimeHelper.IsAlive(20, timeAgo, now);
			Assert.IsFalse(isAlive);
		}
	}
}