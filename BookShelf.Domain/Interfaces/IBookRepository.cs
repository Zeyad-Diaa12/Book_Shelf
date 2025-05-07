using BookShelf.Domain.Entities;

namespace BookShelf.Domain.Interfaces;

public interface IBookRepository : IRepository<Book>
{
    Task<IEnumerable<Book>> GetByAuthorAsync(string author);
    Task<IEnumerable<Book>> GetByTitleAsync(string title);
    Task<IEnumerable<Book>> GetByISBNAsync(string isbn);
    Task<IEnumerable<Book>> GetBooksByUserIdAsync(Guid userId);
    Task<IEnumerable<Book>> GetRecommendationsAsync(Guid userId, int count = 10);
}
