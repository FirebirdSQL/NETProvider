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
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests.Query;

public class ElementaryTests : EntityFrameworkCoreTestsBase
{
	[Test]
	public async Task SimpleSelect()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			var data = await db.Set<MonAttachment>().ToListAsync();
			Assert.IsNotEmpty(data);
		}
	}

	[Test]
	public async Task SelectWithWhere()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			var query = db.Set<MonAttachment>()
				.Where(x => x.AttachmentName.Trim() != string.Empty);
			Assert.DoesNotThrowAsync(() => query.LoadAsync());
			var sql = db.LastCommandText;
			StringAssert.Contains("TRIM(", sql);
		}
	}

	[Test]
	public async Task SelectWithWhereExtract()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			var query = db.Set<MonAttachment>()
				.Where(x => x.Timestamp.Second > -1 && x.Timestamp.DayOfYear == 1);
			Assert.DoesNotThrowAsync(() => query.LoadAsync());
			var sql = db.LastCommandText;
		}
	}

	[Test]
	public async Task SelectWithWhereSubstring()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			var query = db.Set<MonAttachment>()
				.Where(x => x.AttachmentName.Substring(1) == string.Empty && x.AttachmentName.Substring(1, 1) == string.Empty || x.AttachmentName.Substring(x.AttachmentId) != string.Empty || x.AttachmentName.Substring(x.AttachmentId, x.AttachmentId) != string.Empty);
			Assert.DoesNotThrowAsync(() => query.LoadAsync());
			var sql = db.LastCommandText;
		}
	}

	[Test]
	public async Task SelectWithWhereDateMember()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			var query = db.Set<MonAttachment>()
				.Where(x => x.Timestamp.Date == DateTime.Now.Date);
			Assert.DoesNotThrowAsync(() => query.LoadAsync());
			var sql = db.LastCommandText;
		}
	}

	[Test]
	public async Task SelectWithNewGuid()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			var query = db.Set<MonAttachment>()
				.Where(x => x.AttachmentName == Guid.NewGuid().ToString());
			Assert.DoesNotThrowAsync(() => query.LoadAsync());
			var sql = db.LastCommandText;
		}
	}

	[Test]
	public async Task SelectTake()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			var query = db.Set<MonAttachment>()
				.Take(3);
			Assert.DoesNotThrowAsync(() => query.LoadAsync());
			var sql = db.LastCommandText;
			StringAssert.IsMatch(@"ROWS \(.+\)", sql);
			StringAssert.DoesNotMatch(@" TO \(", sql);
		}
	}

	[Test]
	public async Task SelectSkipTake()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			var query = db.Set<MonAttachment>()
				.Skip(1)
				.Take(3);
			Assert.DoesNotThrowAsync(() => query.LoadAsync());
			var sql = db.LastCommandText;
			StringAssert.IsMatch(@"ROWS \((.+) \+ 1\) TO \(\1 \+ .+\)", sql);
		}
	}

	[Test]
	public async Task SelectSkip()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			var query = db.Set<MonAttachment>()
				.Skip(1);
			Assert.DoesNotThrowAsync(() => query.LoadAsync());
			var sql = db.LastCommandText;
			StringAssert.IsMatch(@"ROWS \(.+ \+ 1\) TO \(9223372036854775807\)", sql);
		}
	}

	[Test]
	public async Task SelectTopLevelAny()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			Assert.DoesNotThrowAsync(() => db.Set<MonAttachment>().AnyAsync(x => x.AttachmentId != 0));
		}
	}

	[Test]
	public async Task SelectableProcedureSimple()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			db.CreateProcedures();
			var query = db.Set<SelectableProcedure>();
			Assert.DoesNotThrowAsync(() => query.LoadAsync());
		}
	}

	[Test]
	public async Task SelectableProcedureWithTable()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			db.CreateProcedures();
			var query = db.Set<MonAttachment>()
				.Where(x => db.Set<SelectableProcedure>().Select(y => y.Value).Contains(x.AttachmentId));
			Assert.DoesNotThrowAsync(() => query.LoadAsync());
		}
	}

	[Test]
	public async Task SelectableProcedureWithParam()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			db.CreateProcedures();
			var query = db.SelectableProcedureWithParam(10).Where(x => x.Value > 10);
			Assert.DoesNotThrowAsync(() => query.LoadAsync());
		}
	}

	[Test]
	public async Task SelectStartsWithConstant()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			var query = db.Set<MonAttachment>()
				.Where(x => x.AttachmentName.StartsWith("Jiri"));
			Assert.DoesNotThrowAsync(() => query.LoadAsync());
			var sql = db.LastCommandText;
			StringAssert.Contains("'Jiri%'", sql);
		}
	}

	[Test]
	public async Task SelectWithCollate()
	{
		await using (var db = await GetDbContext<SelectContext>())
		{
			var query = db.Set<MonAttachment>()
				.Where(x => x.AttachmentName == EF.Functions.Collate("test", "UNICODE_CI_AI"));
			Assert.DoesNotThrowAsync(() => query.LoadAsync());
			var sql = db.LastCommandText;
			StringAssert.Contains(@"CAST(_UTF8'test' AS VARCHAR(4) CHARACTER SET UTF8) COLLATE UNICODE_CI_AI", sql);
		}
	}
}

class SelectContext : FbTestDbContext
{
	class LastCommandTextCommandInterceptor : DbCommandInterceptor
	{
		public string LastCommandText { get; private set; }

		public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
		{
			LastCommandText = command.CommandText;
			return base.NonQueryExecuted(command, eventData, result);
		}

		public override ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
		{
			LastCommandText = command.CommandText;
			return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
		}

		public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
		{
			LastCommandText = command.CommandText;
			return base.ReaderExecuted(command, eventData, result);
		}

		public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
		{
			LastCommandText = command.CommandText;
			return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
		}

		public override object ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object result)
		{
			LastCommandText = command.CommandText;
			return base.ScalarExecuted(command, eventData, result);
		}

		public override ValueTask<object> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object result, CancellationToken cancellationToken = default)
		{
			LastCommandText = command.CommandText;
			return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
		}
	}

	LastCommandTextCommandInterceptor _lastCommandTextInterceptor;

	public SelectContext(string connectionString)
		: base(connectionString)
	{
		_lastCommandTextInterceptor = new LastCommandTextCommandInterceptor();
	}

	protected override void OnTestModelCreating(ModelBuilder modelBuilder)
	{
		base.OnTestModelCreating(modelBuilder);

		var monAttachmentConf = modelBuilder.Entity<MonAttachment>();
		monAttachmentConf.HasKey(x => x.AttachmentId);
		monAttachmentConf.Property(x => x.AttachmentId).HasColumnName("MON$ATTACHMENT_ID");
		monAttachmentConf.Property(x => x.AttachmentName).HasColumnName("MON$ATTACHMENT_NAME");
		monAttachmentConf.Property(x => x.Timestamp).HasColumnName("MON$TIMESTAMP");
		monAttachmentConf.ToTable("MON$ATTACHMENTS");

		var selectableProcedureConf = modelBuilder.Entity<SelectableProcedure>();
		selectableProcedureConf.HasNoKey();
		selectableProcedureConf.Property(x => x.Value).HasColumnName("VAL");
		selectableProcedureConf.ToFunction("SELECTABLE_PROCEDURE");

		var selectableProcedureWithParamConf = modelBuilder.Entity<SelectableProcedureWithParam>();
		selectableProcedureWithParamConf.HasNoKey();
		selectableProcedureWithParamConf.Property(x => x.Value).HasColumnName("VAL");
		modelBuilder.HasDbFunction(typeof(SelectContext).GetMethod(nameof(SelectableProcedureWithParam)),
			c => c.HasName("SELECTABLE_PROCEDURE"));

	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);


		optionsBuilder.AddInterceptors(_lastCommandTextInterceptor);
	}

	public string LastCommandText => _lastCommandTextInterceptor.LastCommandText;

	public IQueryable<SelectableProcedureWithParam> SelectableProcedureWithParam(int i) => FromExpression(() => SelectableProcedureWithParam(i));

	public void CreateProcedures()
	{
		Database.ExecuteSqlRaw(
@"recreate procedure selectable_procedure (i int = 6)
returns (val int)
as
begin
	val = i;
	suspend;
	val = i + 1;
	suspend;
end");
	}
}

class MonAttachment
{
	public int AttachmentId { get; set; }
	public string AttachmentName { get; set; }
	public DateTime Timestamp { get; set; }
}

class SelectableProcedure
{
	public int Value { get; set; }
}
class SelectableProcedureWithParam
{
	public int Value { get; set; }
}
