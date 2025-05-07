namespace BookShelf.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid DiscussionId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Discussion Discussion { get; set; } = null!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
