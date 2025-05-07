using BookShelf.Application.DTOs;

namespace BookShelf.Application.Interfaces;

public interface IBookService
{
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<BookDto?> GetBookByIdAsync(Guid id);
    Task<IEnumerable<BookDto>> SearchBooksAsync(string searchTerm);
    Task<BookDto> AddBookAsync(CreateBookDto bookDto);
    Task UpdateBookAsync(UpdateBookDto bookDto);
    Task DeleteBookAsync(Guid id);
    Task<IEnumerable<BookDto>> GetBooksByUserIdAsync(Guid userId);
    Task<IEnumerable<ReviewDto>> GetBookReviewsAsync(Guid bookId);
    Task<IEnumerable<BookDto>> GetRecommendationsAsync(Guid userId, int count = 10);
}
