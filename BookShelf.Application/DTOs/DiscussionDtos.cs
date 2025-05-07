using BookShelf.Domain.Enums;

namespace BookShelf.Application.DTOs;

public class BookClubDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CreatorId { get; set; }
    public string CreatorUsername { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int MemberCount { get; set; }
    public bool IsPublic { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> Genres { get; set; } = new List<string>();
    public BookDto? CurrentBook { get; set; }
    public List<DiscussionDto> RecentDiscussions { get; set; } = new List<DiscussionDto>();
    public List<BookClubMembershipDto> Members { get; set; } = new List<BookClubMembershipDto>();
    public bool IsMember { get; set; }
    public string CreatorName => CreatorUsername;
    public string CurrentBookTitle => CurrentBook?.Title ?? "No book selected";
}

public class CreateBookClubDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = true;
    public string ImageUrl { get; set; } = string.Empty;
}

public class UpdateBookClubDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}

public class BookClubMembershipDto
{
    public Guid Id { get; set; }
    public Guid BookClubId { get; set; }
    public string BookClubName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public MemberRole Role { get; set; }
    public DateTime JoinedDate { get; set; }
    public string UserName => Username;
    public string UserEmail { get; set; } = string.Empty;
    public bool IsCreator => Role == MemberRole.Creator;
}

public class DiscussionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public Guid? BookId { get; set; }
    public string? BookTitle { get; set; }
    public Guid? BookClubId { get; set; }
    public string? BookClubName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int CommentCount { get; set; }
    public bool IsActive { get; set; } = true;
    public string CreatorName => Username;
}

public class DiscussionDetailsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public Guid? BookId { get; set; }
    public string? BookTitle { get; set; }
    public Guid? BookClubId { get; set; }
    public string? BookClubName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
    public bool IsActive { get; set; } = true;
    public string CreatorName => Username;
}

public class CreateDiscussionDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? BookId { get; set; }
    public Guid? BookClubId { get; set; }
}

public class CommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public Guid DiscussionId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public class CreateCommentDto
{
    public string Content { get; set; } = string.Empty;
    public Guid DiscussionId { get; set; }
    public Guid? ParentCommentId { get; set; }
}
