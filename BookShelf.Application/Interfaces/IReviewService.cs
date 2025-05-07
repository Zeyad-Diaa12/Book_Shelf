using BookShelf.Application.DTOs;

namespace BookShelf.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewDto?> GetReviewByIdAsync(Guid reviewId);
    Task<IEnumerable<ReviewDto>> GetReviewsByBookIdAsync(Guid bookId);
    Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(Guid userId);
    Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewDto reviewDto);
    Task<ReviewDto> UpdateReviewAsync(Guid userId, UpdateReviewDto reviewDto);
    Task DeleteReviewAsync(Guid userId, Guid reviewId);
    Task<double> GetAverageRatingForBookAsync(Guid bookId);
    Task<Dictionary<int, int>> GetRatingDistributionForBookAsync(Guid bookId);
    Task<IEnumerable<BookDto>> GetTopRatedBooksAsync(int count = 10);
}
