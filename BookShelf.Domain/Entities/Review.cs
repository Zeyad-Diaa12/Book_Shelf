namespace BookShelf.Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    // Navigation properties
    public Book Book { get; set; } = null!;
    public User User { get; set; } = null!;
}
