using BookShelf.Domain.Enums;

namespace BookShelf.Domain.Entities;

public class BookClubMembership
{
    public Guid Id { get; set; }
    public Guid BookClubId { get; set; }
    public Guid UserId { get; set; }
    public MemberRole Role { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public BookClub BookClub { get; set; } = null!;
    public User User { get; set; } = null!;
}
