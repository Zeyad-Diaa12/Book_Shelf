namespace BookShelf.Domain.Entities;

public class BookClub
{
    public Guid Id { get; set; }
    public Guid CreatorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public bool IsPublic { get; set; } = true;
    public string ImageUrl { get; set; } = string.Empty;

    // Navigation properties
    public User Creator { get; set; } = null!;
    public ICollection<BookClubMembership> Memberships { get; set; } =
        new List<BookClubMembership>();
    public ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();
}
