using BookShelf.Domain.Enums;

namespace BookShelf.Application.DTOs;

public class ReadingProgressDto
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? FinishDate { get; set; }
    public int PagesReadToday { get; set; }
    public double CompletionPercentage { get; set; }
    public DateTime LastUpdated { get; set; }
    public BookDto Book { get; set; } = null!;
    public ReadingStatus Status { get; set; } = ReadingStatus.NotStarted;
    public string Notes { get; set; } = string.Empty;
    public DateTime? CompletedDate { get; set; }

    // Adding missing properties referenced in views
    public string CoverImageUrl => Book?.CoverImageUrl ?? string.Empty;
    public string Title => BookTitle;
    public string Author => Book?.Author ?? string.Empty;
    public double ProgressPercentage => CompletionPercentage;
}

public class UpdateReadingProgressDto
{
    public Guid BookId { get; set; }
    public int CurrentPage { get; set; }
    public int PagesReadToday { get; set; }
}

public class ReadingGoalDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public GoalType Type { get; set; }
    public int Target { get; set; }
    public int Current { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCompleted { get; set; }
    public string Name { get; set; } = string.Empty;
    public GoalType GoalType => Type;
    public int TargetValue => Target;
    public int Progress => Current;
    public string Period => $"{StartDate:d} - {EndDate:d}";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Adding missing properties referenced in views
    public string Title => Name;
    public string Description { get; set; } = string.Empty;
    //public int CurrentCount => Current;
    //public int TargetCount => Target;
}

public class CreateReadingGoalDto
{
    public GoalType Type { get; set; }
    public int Target { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Adding properties to match the form fields in ReadingGoals view
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GoalType { get; set; } = string.Empty;
    public int TargetCount { get; set; }
    public int CurrentCount { get; set; }
    public int Current { get; set; }
}
