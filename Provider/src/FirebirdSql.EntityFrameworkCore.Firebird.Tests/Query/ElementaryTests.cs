/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
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
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests.Query
{
	public class ElementaryTests : EntityFrameworkCoreTestsBase
	{
		[Test]
		public void SimpleSelect()
		{
			using (var db = GetDbContext<SelectContext>())
			{
				var data = db.Set<MonAttachment>().ToList();
				Assert.IsNotEmpty(data);
			}
		}

		[Test]
		public void SelectWithWhere()
		{
			using (var db = GetDbContext<SelectContext>())
			{
				var query = db.Set<MonAttachment>()
					.Where(x => x.AttachmentName.Trim() != string.Empty && x.Timestamp.Second > -1);
				Assert.DoesNotThrow(() => query.Load());
				var sql = query.ToSql();
				StringAssert.Contains("TRIM(", sql);
				StringAssert.Contains("EXTRACT(SECOND FROM", sql);
			}
		}

		[Test]
		public void SelectTake()
		{
			using (var db = GetDbContext<SelectContext>())
			{
				var query = db.Set<MonAttachment>()
					.Take(3);
				Assert.DoesNotThrow(() => query.Load());
				var sql = query.ToSql();
				StringAssert.Contains("FIRST 3", sql);
				StringAssert.DoesNotContain("SKIP", sql);
			}
		}

		[Test]
		public void SelectSkipTake()
		{
			using (var db = GetDbContext<SelectContext>())
			{
				var query = db.Set<MonAttachment>()
					.Skip(1)
					.Take(3);
				Assert.DoesNotThrow(() => query.Load());
				var sql = query.ToSql();
				StringAssert.Contains("FIRST 3", sql);
				StringAssert.Contains("SKIP 1", sql);
			}
		}

		[Test]
		public void SelectSkip()
		{
			using (var db = GetDbContext<SelectContext>())
			{
				var query = db.Set<MonAttachment>()
					.Skip(1);
				Assert.DoesNotThrow(() => query.Load());
				var sql = query.ToSql();
				StringAssert.DoesNotContain("FIRST", sql);
				StringAssert.Contains("SKIP 1", sql);
			}
		}
	}

	class SelectContext : FbTestDbContext
	{
		public SelectContext(string connectionString)
			: base(connectionString)
		{ }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			var monAttachmentConf = modelBuilder.Entity<MonAttachment>();
			monAttachmentConf.HasKey(x => x.AttachmentId);
			monAttachmentConf.Property(x => x.AttachmentId).HasColumnName("MON$ATTACHMENT_ID");
			monAttachmentConf.Property(x => x.AttachmentName).HasColumnName("MON$ATTACHMENT_NAME");
			monAttachmentConf.Property(x => x.Timestamp).HasColumnName("MON$TIMESTAMP");
			monAttachmentConf.ToTable("MON$ATTACHMENTS");
		}
	}

	class MonAttachment
	{
		public int AttachmentId { get; set; }
		public string AttachmentName { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
