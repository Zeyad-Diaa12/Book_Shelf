namespace BookShelf.Application.DTOs;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt => CreatedDate;
    public string Comment => Content;
    public string BookCoverUrl { get; set; } = string.Empty;
}

public class CreateReviewDto
{
    public Guid BookId { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdateReviewDto
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; } = string.Empty;
}
