using System.Xml.Linq;

namespace BookShelf.Domain.Entities;

public class Discussion
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? BookId { get; set; }
    public Guid? BookClubId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Book? Book { get; set; }
    public BookClub? BookClub { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
