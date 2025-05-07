using BookShelf.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookShelfEntity = BookShelf.Domain.Entities.BookShelf;

namespace BookShelf.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<BookShelfEntity> Bookshelves { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<ReadingProgress> ReadingProgresses { get; set; } = null!;
    public DbSet<ReadingGoal> ReadingGoals { get; set; } = null!;
    public DbSet<BookClub> BookClubs { get; set; } = null!;
    public DbSet<BookClubMembership> BookClubMemberships { get; set; } = null!;
    public DbSet<Discussion> Discussions { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;

    // Override SaveChanges to ensure all DateTime properties are in UTC
    public override int SaveChanges()
    {
        ConvertDatesToUtc();
        return base.SaveChanges();
    }

    // Override SaveChangesAsync to ensure all DateTime properties are in UTC
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ConvertDatesToUtc();
        return base.SaveChangesAsync(cancellationToken);
    }

    // Private helper method to convert all DateTime properties to UTC
    private void ConvertDatesToUtc()
    {
        var entities = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entityEntry in entities)
        {
            foreach (var property in entityEntry.Properties)
            {
                if (property.Metadata.ClrType == typeof(DateTime))
                {
                    if (property.CurrentValue is DateTime dateTime)
                    {
                        switch (dateTime.Kind)
                        {
                            case DateTimeKind.Local:
                                // Convert local time to UTC
                                property.CurrentValue = dateTime.ToUniversalTime();
                                break;
                            case DateTimeKind.Unspecified:
                                // Assume Unspecified is UTC, just set the Kind
                                property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                                break;
                                // UTC kind needs no conversion
                        }
                    }
                }
                else if (property.Metadata.ClrType == typeof(DateTime?))
                {
                    var nullableDateTime = property.CurrentValue as DateTime?;
                    if (nullableDateTime.HasValue)
                    {
                        var dateTime = nullableDateTime.Value;
                        switch (dateTime.Kind)
                        {
                            case DateTimeKind.Local:
                                // Convert local time to UTC
                                property.CurrentValue = dateTime.ToUniversalTime();
                                break;
                            case DateTimeKind.Unspecified:
                                // Assume Unspecified is UTC, just set the Kind
                                property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                                break;
                                // UTC kind needs no conversion
                        }
                    }
                }
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure many-to-many relationship between Book and BookShelf
        modelBuilder
            .Entity<Book>()
            .HasMany(b => b.Bookshelves)
            .WithMany(s => s.Books)
            .UsingEntity(j => j.ToTable("BookBookShelf"));

        // Configure one-to-many relationship between User and BookShelf
        modelBuilder
            .Entity<BookShelfEntity>()
            .HasOne(bs => bs.User)
            .WithMany(u => u.Bookshelves)
            .HasForeignKey(bs => bs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure one-to-many relationship between Book and Review
        modelBuilder
            .Entity<Review>()
            .HasOne(r => r.Book)
            .WithMany(b => b.Reviews)
            .HasForeignKey(r => r.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure one-to-many relationship between User and Review
        modelBuilder
            .Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure one-to-many relationship between Book and ReadingProgress
        modelBuilder
            .Entity<ReadingProgress>()
            .HasOne(rp => rp.Book)
            .WithMany(b => b.ReadingProgresses)
            .HasForeignKey(rp => rp.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure one-to-many relationship between User and ReadingProgress
        modelBuilder
            .Entity<ReadingProgress>()
            .HasOne(rp => rp.User)
            .WithMany()
            .HasForeignKey(rp => rp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure one-to-many relationship between User and ReadingGoal
        modelBuilder
            .Entity<ReadingGoal>()
            .HasOne(rg => rg.User)
            .WithMany(u => u.ReadingGoals)
            .HasForeignKey(rg => rg.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure recursive relationship for Comment replies
        modelBuilder
            .Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
