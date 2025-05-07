using BookShelf.Application.DTOs;

namespace BookShelf.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<UserDto> RegisterUserAsync(RegisterUserDto registerDto);
    Task<UserDto> LoginAsync(LoginUserDto loginDto);
    Task UpdateUserAsync(Guid userId, UpdateUserDto updateDto);
    Task DeleteUserAsync(Guid id);
    Task<IEnumerable<BookDto>> GetUserBooksAsync(Guid userId);
    Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(Guid userId);
    Task<IEnumerable<ReadingProgressDto>> GetUserReadingProgressAsync(Guid userId);
    Task<IEnumerable<ReadingGoalDto>> GetUserReadingGoalsAsync(Guid userId);
}
