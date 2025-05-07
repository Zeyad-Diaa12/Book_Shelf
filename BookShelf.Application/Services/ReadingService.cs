using System.Linq.Expressions;
using AutoMapper;
using BookShelf.Application.DTOs;
using BookShelf.Application.Interfaces;
using BookShelf.Domain.Entities;
using BookShelf.Domain.Enums;
using BookShelf.Domain.Interfaces;

namespace BookShelf.Application.Services;

public class ReadingService : IReadingService
{
    private readonly IRepository<ReadingProgress> _readingProgressRepository;
    private readonly IRepository<ReadingGoal> _readingGoalRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public ReadingService(
        IRepository<ReadingProgress> readingProgressRepository,
        IRepository<ReadingGoal> readingGoalRepository,
        IBookRepository bookRepository,
        IUserRepository userRepository,
        IMapper mapper
    )
    {
        _readingProgressRepository = readingProgressRepository;
        _readingGoalRepository = readingGoalRepository;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<ReadingProgressDto?> GetReadingProgressAsync(Guid userId, Guid bookId)
    {
        var progressEntities = await _readingProgressRepository.FindAsync(rp =>
            rp.UserId == userId && rp.BookId == bookId
        );

        var progressEntity = progressEntities.FirstOrDefault();

        return progressEntity != null ? _mapper.Map<ReadingProgressDto>(progressEntity) : null;
    }

    public async Task<ReadingProgressDto> UpdateReadingProgressAsync(
        Guid userId,
        UpdateReadingProgressDto progressDto
    )
    {
        // Remove unnecessary parsing - BookId should already be a Guid
        var bookId = progressDto.BookId;
        var progressEntities = await _readingProgressRepository.FindAsync(rp =>
            rp.UserId == userId && rp.BookId == bookId
        );

        var progress = progressEntities.FirstOrDefault();

        if (progress == null)
        {
            throw new KeyNotFoundException(
                $"Reading progress for user {userId} and book {bookId} not found"
            );
        }

        // Update fields
        progress.CurrentPage = progressDto.CurrentPage;
        progress.LastUpdated = DateTime.UtcNow;
        progress.PagesReadToday += progressDto.PagesReadToday;

        // Update completion percentage
        if (progress.TotalPages > 0)
        {
            progress.CompletionPercentage = Math.Min(100,
                (double)progress.CurrentPage / progress.TotalPages * 100);
        }

        // Check if book is completed
        if (progress.CurrentPage >= progress.TotalPages)
        {
            progress.Status = ReadingStatus.Completed;
            progress.CompletedDate = DateTime.UtcNow;
            progress.FinishDate = DateTime.UtcNow;
        }
        else if (progress.Status == ReadingStatus.NotStarted)
        {
            progress.Status = ReadingStatus.InProgress;
        }

        await _readingProgressRepository.UpdateAsync(progress);
        await _readingProgressRepository.SaveChangesAsync();

        // Update any active reading goals
        await UpdateReadingGoalsForUserAsync(userId);

        return _mapper.Map<ReadingProgressDto>(progress);
    }

    public async Task<ReadingProgressDto> StartReadingBookAsync(Guid userId, Guid bookId)
    {
        // Check if user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Check if book exists
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book == null)
        {
            throw new KeyNotFoundException($"Book with ID {bookId} not found");
        }

        // Check if reading progress already exists
        var progressEntities = await _readingProgressRepository.FindAsync(rp =>
            rp.UserId == userId && rp.BookId == bookId
        );

        var existingProgress = progressEntities.FirstOrDefault();

        if (existingProgress != null)
        {
            // If already started, just return the existing progress
            if (existingProgress.Status == ReadingStatus.InProgress)
            {
                return _mapper.Map<ReadingProgressDto>(existingProgress);
            }

            // If completed before, create a new reading session
            if (existingProgress.Status == ReadingStatus.Completed)
            {
                existingProgress.Status = ReadingStatus.InProgress;
                existingProgress.StartDate = DateTime.UtcNow;
                existingProgress.FinishDate = null;
                existingProgress.CurrentPage = 0;
                existingProgress.CompletionPercentage = 0;
                existingProgress.LastUpdated = DateTime.UtcNow;
                existingProgress.PagesReadToday = 0;

                await _readingProgressRepository.UpdateAsync(existingProgress);
                await _readingProgressRepository.SaveChangesAsync();

                return _mapper.Map<ReadingProgressDto>(existingProgress);
            }
        }

        // Create new reading progress
        var readingProgress = new ReadingProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BookId = bookId,
            Status = ReadingStatus.InProgress,
            StartDate = DateTime.UtcNow,
            CurrentPage = 0,
            TotalPages = book.PageCount,
            CompletionPercentage = 0,
            LastUpdated = DateTime.UtcNow,
            PagesReadToday = 0,
        };

        var createdProgress = await _readingProgressRepository.AddAsync(readingProgress);
        await _readingProgressRepository.SaveChangesAsync();

        return _mapper.Map<ReadingProgressDto>(createdProgress);
    }

    public async Task<ReadingProgressDto> FinishReadingBookAsync(Guid userId, Guid bookId)
    {
        var progressEntities = await _readingProgressRepository.FindAsync(rp =>
            rp.UserId == userId && rp.BookId == bookId
        );

        var progress = progressEntities.FirstOrDefault();

        if (progress == null)
        {
            throw new KeyNotFoundException(
                $"Reading progress for user {userId} and book {bookId} not found"
            );
        }

        progress.Status = ReadingStatus.Completed;
        progress.CurrentPage = progress.TotalPages;
        progress.CompletionPercentage = 100;
        progress.CompletedDate = DateTime.UtcNow;
        progress.FinishDate = DateTime.UtcNow;
        progress.LastUpdated = DateTime.UtcNow;

        await _readingProgressRepository.UpdateAsync(progress);
        await _readingProgressRepository.SaveChangesAsync();

        // Update any active reading goals
        await UpdateReadingGoalsForUserAsync(userId);

        return _mapper.Map<ReadingProgressDto>(progress);
    }

    public async Task<IEnumerable<ReadingProgressDto>> GetCurrentlyReadingBooksAsync(Guid userId)
    {
        var progressEntities = await _readingProgressRepository.FindAsync(rp =>
            rp.UserId == userId && rp.Status == ReadingStatus.InProgress
        );

        return _mapper.Map<IEnumerable<ReadingProgressDto>>(progressEntities);
    }

    public async Task<IEnumerable<ReadingProgressDto>> GetReadingHistoryAsync(Guid userId)
    {
        var progressEntities = await _readingProgressRepository.FindAsync(rp =>
            rp.UserId == userId
        );
        return _mapper.Map<IEnumerable<ReadingProgressDto>>(progressEntities);
    }

    public async Task<ReadingGoalDto?> GetReadingGoalByIdAsync(Guid goalId)
    {
        var goal = await _readingGoalRepository.GetByIdAsync(goalId);
        return goal != null ? _mapper.Map<ReadingGoalDto>(goal) : null;
    }

    public async Task<IEnumerable<ReadingGoalDto>> GetActiveReadingGoalsAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var goals = await _readingGoalRepository.FindAsync(g =>
            g.UserId == userId && g.StartDate <= now && g.EndDate >= now && !g.IsCompleted
        );

        return _mapper.Map<IEnumerable<ReadingGoalDto>>(goals);
    }

    public async Task<ReadingGoalDto> CreateReadingGoalAsync(
        Guid userId,
        CreateReadingGoalDto goalDto
    )
    {
        // Check if user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Validate dates
        if (goalDto.StartDate >= goalDto.EndDate)
        {
            throw new ArgumentException("End date must be after start date");
        }

        // Create new reading goal - ensure dates are in UTC
        var readingGoal = new ReadingGoal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = goalDto.Type,
            Target = goalDto.Target > 0 ? goalDto.Target : 1, // Ensure target is at least 1
            Current = goalDto.Current > 0 ? goalDto.Current : 0,
            StartDate = DateTime.SpecifyKind(goalDto.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(goalDto.EndDate, DateTimeKind.Utc),
            CreatedDate = DateTime.UtcNow,
            IsCompleted = false,
            ProgressPercentage = 0,
            Name = goalDto.Title ?? "Reading Goal", // Provide default if Title is null
            Description = goalDto.Description
        };

        var createdGoal = await _readingGoalRepository.AddAsync(readingGoal);
        await _readingGoalRepository.SaveChangesAsync();

        // Update goal progress immediately to reflect current progress
        await UpdateReadingGoalsForUserAsync(userId);

        return _mapper.Map<ReadingGoalDto>(createdGoal);
    }

    public async Task<ReadingGoalDto> UpdateReadingGoalProgressAsync(Guid goalId, int progress)
    {
        var goal = await _readingGoalRepository.GetByIdAsync(goalId);
        if (goal == null)
        {
            throw new KeyNotFoundException($"Reading goal with ID {goalId} not found");
        }

        // Increment the current count instead of replacing it
        goal.Current += progress;

        // Fix potential division by zero and ensure proper percentage calculation
        if (goal.Target > 0)
        {
            goal.ProgressPercentage = Math.Min(100, (int)((double)goal.Current / goal.Target * 100));
        }
        else
        {
            goal.ProgressPercentage = 0;
        }

        // Check if goal is completed
        if (goal.Current >= goal.Target && goal.Target > 0)
        {
            goal.IsCompleted = true;
            goal.ProgressPercentage = 100;
        }

        await _readingGoalRepository.UpdateAsync(goal);
        await _readingGoalRepository.SaveChangesAsync();

        return _mapper.Map<ReadingGoalDto>(goal);
    }

    public async Task DeleteReadingGoalAsync(Guid goalId)
    {
        var goal = await _readingGoalRepository.GetByIdAsync(goalId);
        if (goal == null)
        {
            throw new KeyNotFoundException($"Reading goal with ID {goalId} not found");
        }

        await _readingGoalRepository.DeleteAsync(goal);
        await _readingGoalRepository.SaveChangesAsync();
    }

    public async Task<Dictionary<string, int>> GetReadingStatsAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate
    )
    {
        // Convert DateTime parameters to UTC if they are not already
        startDate = DateTime.SpecifyKind(startDate.ToUniversalTime(), DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate.ToUniversalTime(), DateTimeKind.Utc);

        var readingProgress = await _readingProgressRepository.FindAsync(rp =>
            rp.UserId == userId && rp.LastUpdated >= startDate && rp.LastUpdated <= endDate
        );

        var stats = new Dictionary<string, int>
        {
            {
                "BooksCompleted",
                readingProgress.Count(rp =>
                    rp.Status == ReadingStatus.Completed
                    && DateTime.SpecifyKind(rp.CompletedDate, DateTimeKind.Utc) >= startDate
                    && DateTime.SpecifyKind(rp.CompletedDate, DateTimeKind.Utc) <= endDate
                )
            },
            {
                "BooksStarted",
                readingProgress.Count(rp =>
                    DateTime.SpecifyKind(rp.StartDate, DateTimeKind.Utc) >= startDate &&
                    DateTime.SpecifyKind(rp.StartDate, DateTimeKind.Utc) <= endDate)
            },
            { "TotalPagesRead", readingProgress.Sum(rp => rp.CurrentPage) },
            {
                "AveragePagesPerDay",
                CalculateAveragePagesPerDay(readingProgress, startDate, endDate)
            },
        };

        return stats;
    }

    // Helper methods
    private int CalculateAveragePagesPerDay(
        IEnumerable<ReadingProgress> readingProgress,
        DateTime startDate,
        DateTime endDate
    )
    {
        var totalDays = (endDate - startDate).Days + 1;
        if (totalDays <= 0)
            return 0;

        var totalPages = readingProgress.Sum(rp => rp.CurrentPage);
        // Prevent division by zero
        return totalDays > 0 ? totalPages / totalDays : 0;
    }

    private async Task UpdateReadingGoalsForUserAsync(Guid userId)
    {
        var activeGoals = await GetActiveReadingGoalsAsync(userId);

        foreach (var goalDto in activeGoals)
        {
            var goal = await _readingGoalRepository.GetByIdAsync(goalDto.Id);
            if (goal == null)
                continue;

            switch (goal.Type)
            {
                case GoalType.BooksCompleted:
                    // Get all completed books regardless of when they were updated
                    var completedBooks = await _readingProgressRepository.FindAsync(rp =>
                        rp.UserId == userId
                        && rp.Status == ReadingStatus.Completed
                        // Only count books completed within the goal period
                        && rp.CompletedDate >= goal.StartDate
                        && rp.CompletedDate <= goal.EndDate
                    );

                    goal.Current = completedBooks.Count();
                    break;

                case GoalType.PagesRead:
                    // Track total pages read within the goal period
                    var readingProgress = await _readingProgressRepository.FindAsync(rp =>
                        rp.UserId == userId
                        // Include all progress within the goal period
                        && ((rp.LastUpdated >= goal.StartDate && rp.LastUpdated <= goal.EndDate)
                            // Also include books completed within the goal period
                            || (rp.CompletedDate >= goal.StartDate && rp.CompletedDate <= goal.EndDate))
                    );

                    goal.Current = readingProgress.Sum(rp => rp.CurrentPage);
                    break;
            }

            // Calculate percentage (avoid division by zero)
            if (goal.Target > 0)
            {
                goal.ProgressPercentage = Math.Min(100, (int)((double)goal.Current / goal.Target * 100));
            }
            else
            {
                goal.ProgressPercentage = 0;
            }

            if (goal.Current >= goal.Target && goal.Target > 0)
            {
                goal.IsCompleted = true;
                goal.ProgressPercentage = 100;
            }

            await _readingGoalRepository.UpdateAsync(goal);
        }

        await _readingGoalRepository.SaveChangesAsync();
    }
}
