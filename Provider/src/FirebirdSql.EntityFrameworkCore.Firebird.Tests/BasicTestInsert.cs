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

//$Authors = Jiri Cincura (jiri@cincura.net), Rafael Almeida (ralms@ralms.net)

using System;
using System.Linq;
using FirebirdSql.EntityFrameworkCore.Firebird.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests
{
	[TestFixture]
	public class BasicTestInsert
	{
		[Test]
		public void Insert_data_basic()
		{
			using (var context = GetDbContext())
			{
				context.Database.EnsureDeleted();
				context.Database.EnsureCreated();
			}

			using (var context = GetDbContext())
			{
				for (var i = 1; i <= 500; i++)
				{
					context.Author.Add(new Author
					{
						TestString = "Rafael",
						TestInt = i,
						TestDate = DateTime.Now.AddMilliseconds(1),
						TestGuid = Guid.NewGuid(),
						TestBytes = Encoding.UTF8.GetBytes("RAFAEL ALMEIDA"),
						TestDecimal = i,
						TestDouble = i
					});
				}

				Assert.AreEqual(500, context.SaveChanges());
			}

			using (var context = GetDbContext())
			{
				for (var i = 1; i <= 500; i++)
				{
					context.Person.Add(new Person
					{
						Name = "Rafael",
						LastName = $"Almeida {i}"
					});
				}

				Assert.AreEqual(500, context.SaveChanges());
			}

			using (var context = GetDbContext())
			{
				for (var i = 1; i <= 500; i++)
				{
					context.Book.Add(new Book
					{
						AuthorId = i,
						Title = $"Test Insert Book {i}"
					});
				}

				Assert.AreEqual(500, context.SaveChanges());

				for (var i = 1; i <= 50; i++)
				{
					context.Author.Remove(context.Author.Find(i));
				}
				Assert.AreEqual(50, context.SaveChanges());
			}
		}

		static TestInsertDbContext GetDbContext()
		{
			var result = new TestInsertDbContext();
			var loggerFactory = result.GetService<ILoggerFactory>();
			loggerFactory.AddConsole();
			return result;
		}
	}

	class TestInsertDbContext : DbContext
	{
		public DbSet<Author> Author { get; set; }
		public DbSet<Book> Book { get; set; }
		public DbSet<Person> Person { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseFirebird(@"database=localhost:test.fdb;user=sysdba;password=masterkey");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Author>()
				.Property(x => x.AuthorId).UseFirebirdSequenceTrigger();

			modelBuilder.Entity<Book>()
				.Property(x => x.BookId).UseFirebirdSequenceTrigger();

			modelBuilder.Entity<Person>()
				.HasKey(person => new { person.Name, person.LastName });
		}
	}

	class Author
	{
		[Key]
		public int AuthorId { get; set; }
		[StringLength(100)]
		public string TestString { get; set; }
		public DateTime TestDate { get; set; }
		public Guid TestGuid { get; set; }
		public byte[] TestBytes { get; set; }
		public int TestInt { get; set; }
		public decimal TestDecimal { get; set; }
		public double TestDouble { get; set; }
		public virtual ICollection<Book> Books { get; set; } = new List<Book>();
	}

	class Book
	{
		[Key]
		public int BookId { get; set; }
		[StringLength(100)]
		public string Title { get; set; }
		public long AuthorId { get; set; }
		public virtual Author Author { get; set; }
	}

	class Person
	{
		[StringLength(100)]
		public string Name { get; set; }
		[StringLength(100)]
		public string LastName { get; set; }
	}
}
