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

using System.Linq;
using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests.Query;

public class ToSqlQueryFbTest(NonSharedFixture fixture) : ToSqlQueryTestBase(fixture)
{
	protected override ITestStoreFactory TestStoreFactory => FbTestStoreFactory.Instance;

	public override async Task Entity_type_with_navigation_mapped_to_SqlQuery(bool async)
	{
		var contextFactory = await InitializeAsync<Context27629>(
			seed: async c =>
			{
				var author = new Author { Name = "Toast", Posts = { new Post { Title = "Sausages of the world!" } } };
				c.Add(author);
				await c.SaveChangesAsync();

				var postStat = new PostStat { Count = 10, Author = author };
				author.PostStat = postStat;
				c.Add(postStat);
				await c.SaveChangesAsync();
			});

		using var context = contextFactory.CreateContext();

		var authors = await
			(from o in context.Authors
			 select new { Author = o, PostCount = o.PostStat!.Count }).ToListAsync();

		Assert.Single(authors);
		Assert.Equal("Toast", authors[0].Author.Name);
		Assert.Equal(10, authors[0].PostCount);
	}

	protected new class Context27629 : DbContext
	{
		public Context27629(DbContextOptions options)
			: base(options)
		{ }

		public DbSet<Author> Authors
			=> Set<Author>();

		public DbSet<Post> Posts
			=> Set<Post>();

		public DbSet<PostStat> PostStats
			=> Set<PostStat>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Author>(
				builder =>
				{
					builder.ToTable("Authors");
					builder.Property(o => o.Name).HasMaxLength(50);
				});

			modelBuilder.Entity<Post>(
				builder =>
				{
					builder.ToTable("Posts");
					builder.Property(o => o.Title).HasMaxLength(50);
					builder.Property(o => o.Content).HasMaxLength(500);

					builder
						.HasOne(o => o.Author)
						.WithMany(o => o.Posts)
						.HasForeignKey(o => o.AuthorId)
						.OnDelete(DeleteBehavior.ClientCascade);
				});

			modelBuilder.Entity<PostStat>(
				builder =>
				{
					builder
						.ToSqlQuery("SELECT * FROM \"PostStats\"")
						.HasKey(o => o.AuthorId);

					builder
						.HasOne(o => o.Author)
						.WithOne().HasForeignKey<PostStat>(o => o.AuthorId)
						.OnDelete(DeleteBehavior.ClientCascade);
				});
		}
	}
}
