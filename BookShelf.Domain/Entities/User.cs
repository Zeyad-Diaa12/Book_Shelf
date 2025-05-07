using System.Xml.Linq;

namespace BookShelf.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ProfilePictureUrl { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<BookShelf> Bookshelves { get; set; } = new List<BookShelf>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<ReadingGoal> ReadingGoals { get; set; } = new List<ReadingGoal>();
    public ICollection<BookClubMembership> BookClubMemberships { get; set; } =
        new List<BookClubMembership>();
    public ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
