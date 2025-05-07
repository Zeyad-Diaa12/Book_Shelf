using BookShelf.Domain.Enums;

namespace BookShelf.Domain.Entities;

public class ReadingGoal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public GoalType Type { get; set; }
    public int Target { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public bool IsCompleted { get; set; }
    public int Current { get; set; }
    public int ProgressPercentage { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
