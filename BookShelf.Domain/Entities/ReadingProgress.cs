using BookShelf.Domain.Enums;

namespace BookShelf.Domain.Entities;

public class ReadingProgress
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid UserId { get; set; }
    public int CurrentPage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? FinishDate { get; set; }
    public int PagesReadToday { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Book Book { get; set; } = null!;
    public User User { get; set; } = null!;
    public int TotalPages { get; set; }
    public double CompletionPercentage { get; set; }
    public ReadingStatus Status { get; set; }
    public DateTime CompletedDate { get; set; }
}
