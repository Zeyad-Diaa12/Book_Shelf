using AutoMapper;
using BookShelf.Application.DTOs;
using BookShelf.Application.Interfaces;
using BookShelf.Domain.Entities;
using BookShelf.Domain.Interfaces;

namespace BookShelf.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IRepository<Review> _reviewRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public ReviewService(
        IRepository<Review> reviewRepository,
        IBookRepository bookRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<ReviewDto?> GetReviewByIdAsync(Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        return review != null ? _mapper.Map<ReviewDto>(review) : null;
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsByBookIdAsync(Guid bookId)
    {
        var reviews = await _reviewRepository.FindAsync(r => r.BookId == bookId);
        return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(Guid userId)
    {
        var reviews = await _reviewRepository.FindAsync(r => r.UserId == userId);
        return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
    }

    public async Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewDto reviewDto)
    {
        // Check if user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Check if book exists
        var book = await _bookRepository.GetByIdAsync(reviewDto.BookId);
        if (book == null)
        {
            throw new KeyNotFoundException($"Book with ID {reviewDto.BookId} not found");
        }

        // Check if user already reviewed this book
        var existingReviews = await _reviewRepository.FindAsync(r =>
            r.UserId == userId && r.BookId == reviewDto.BookId);

        if (existingReviews.Any())
        {
            throw new InvalidOperationException("User has already reviewed this book");
        }

        // Validate rating
        if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5");
        }

        // Create new review with UTC datetime
        var review = new Review
        {
            Id = Guid.NewGuid(),
            BookId = reviewDto.BookId,
            UserId = userId,
            Rating = reviewDto.Rating,
            Content = reviewDto.Content,
            CreatedDate = DateTime.UtcNow
        };

        var createdReview = await _reviewRepository.AddAsync(review);
        await _reviewRepository.SaveChangesAsync();

        // Map the result including book title and username from relationships
        var result = _mapper.Map<ReviewDto>(createdReview);
        result.BookTitle = book.Title;
        result.Username = user.Username;

        return result;
    }

    public async Task<ReviewDto> UpdateReviewAsync(Guid userId, UpdateReviewDto reviewDto)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewDto.Id);
        if (review == null)
        {
            throw new KeyNotFoundException($"Review with ID {reviewDto.Id} not found");
        }

        // Check if the review belongs to the user
        if (review.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only update your own reviews");
        }

        // Validate rating
        if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5");
        }

        // Update review
        review.Rating = reviewDto.Rating;
        review.Content = reviewDto.Content;
        review.UpdatedDate = DateTime.UtcNow;

        await _reviewRepository.UpdateAsync(review);
        await _reviewRepository.SaveChangesAsync();

        // Get additional data to include in response
        var book = await _bookRepository.GetByIdAsync(review.BookId);
        var user = await _userRepository.GetByIdAsync(review.UserId);

        var result = _mapper.Map<ReviewDto>(review);
        result.BookTitle = book?.Title ?? string.Empty;
        result.Username = user?.Username ?? string.Empty;

        return result;
    }

    public async Task DeleteReviewAsync(Guid userId, Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
        {
            throw new KeyNotFoundException($"Review with ID {reviewId} not found");
        }

        // Check if the review belongs to the user
        if (review.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own reviews");
        }

        await _reviewRepository.DeleteAsync(review);
        await _reviewRepository.SaveChangesAsync();
    }

    public async Task<double> GetAverageRatingForBookAsync(Guid bookId)
    {
        var reviews = await _reviewRepository.FindAsync(r => r.BookId == bookId);
        if (!reviews.Any())
        {
            return 0;
        }

        return reviews.Average(r => r.Rating);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionForBookAsync(Guid bookId)
    {
        var reviews = await _reviewRepository.FindAsync(r => r.BookId == bookId);

        var distribution = new Dictionary<int, int>
        {
            { 1, 0 },
            { 2, 0 },
            { 3, 0 },
            { 4, 0 },
            { 5, 0 }
        };

        foreach (var review in reviews)
        {
            if (review.Rating >= 1 && review.Rating <= 5)
            {
                distribution[review.Rating]++;
            }
        }

        return distribution;
    }

    public async Task<IEnumerable<BookDto>> GetTopRatedBooksAsync(int count = 10)
    {
        // Get all reviews grouped by book
        var reviews = await _reviewRepository.GetAllAsync();
        var reviewsByBook = reviews.GroupBy(r => r.BookId);

        // Calculate average rating for each book
        var bookRatings = reviewsByBook
            .Select(group => new
            {
                BookId = group.Key,
                AverageRating = group.Average(r => r.Rating),
                ReviewCount = group.Count()
            })
            .OrderByDescending(x => x.AverageRating)
            .ThenByDescending(x => x.ReviewCount)
            .Take(count)
            .ToList();

        // Get book details for the top rated books
        var topRatedBooks = new List<Book>();
        foreach (var ratedBook in bookRatings)
        {
            var book = await _bookRepository.GetByIdAsync(ratedBook.BookId);
            if (book != null)
            {
                topRatedBooks.Add(book);
            }
        }

        return _mapper.Map<IEnumerable<BookDto>>(topRatedBooks);
    }
}