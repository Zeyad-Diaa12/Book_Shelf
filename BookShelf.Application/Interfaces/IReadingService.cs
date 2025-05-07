using BookShelf.Application.DTOs;

namespace BookShelf.Application.Interfaces;

public interface IReadingService
{
    Task<ReadingProgressDto?> GetReadingProgressAsync(Guid userId, Guid bookId);
    Task<ReadingProgressDto> UpdateReadingProgressAsync(
        Guid userId,
        UpdateReadingProgressDto progressDto
    );
    Task<ReadingProgressDto> StartReadingBookAsync(Guid userId, Guid bookId);
    Task<ReadingProgressDto> FinishReadingBookAsync(Guid userId, Guid bookId);
    Task<IEnumerable<ReadingProgressDto>> GetCurrentlyReadingBooksAsync(Guid userId);
    Task<IEnumerable<ReadingProgressDto>> GetReadingHistoryAsync(Guid userId);

    Task<ReadingGoalDto?> GetReadingGoalByIdAsync(Guid goalId);
    Task<IEnumerable<ReadingGoalDto>> GetActiveReadingGoalsAsync(Guid userId);
    Task<ReadingGoalDto> CreateReadingGoalAsync(Guid userId, CreateReadingGoalDto goalDto);
    Task<ReadingGoalDto> UpdateReadingGoalProgressAsync(Guid goalId, int progress);
    Task DeleteReadingGoalAsync(Guid goalId);
    Task<Dictionary<string, int>> GetReadingStatsAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate
    );
}
