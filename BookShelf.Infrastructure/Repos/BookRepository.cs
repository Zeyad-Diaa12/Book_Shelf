using System.Linq;
using BookShelf.Domain.Entities;
using BookShelf.Domain.Interfaces;
using BookShelf.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookShelf.Infrastructure.Repos;

public class BookRepository : Repository<Book>, IBookRepository
{
    public BookRepository(ApplicationDbContext context)
        : base(context) { }

    // Override to include related entities
    public override async Task<Book?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(b => b.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Book>> GetByAuthorAsync(string author)
    {
        return await _dbSet.Where(b => b.Author.Contains(author)).ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetByTitleAsync(string title)
    {
        return await _dbSet.Where(b => b.Title.Contains(title)).ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetByISBNAsync(string isbn)
    {
        return await _dbSet.Where(b => b.ISBN == isbn).ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetBooksByUserIdAsync(Guid userId)
    {
        return await _context
            .Bookshelves.Where(bs => bs.UserId == userId)
            .SelectMany(bs => bs.Books)
            .Distinct()
            .ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetRecommendationsAsync(Guid userId, int count = 10)
    {
        var userBooks = await GetBooksByUserIdAsync(userId);
        var userBookIds = userBooks.Select(b => b.Id).ToList();

        var recommendedBooks = await _context
            .Reviews.Where(r => r.Rating >= 4)
            .Where(r => !userBookIds.Contains(r.BookId))
            .GroupBy(r => r.BookId)
            .OrderByDescending(g => g.Average(r => r.Rating))
            .Take(count)
            .Select(g => g.First().Book)
            .ToListAsync();

        return recommendedBooks;
    }
}
