using BookShelf.Application.DTOs;
using BookShelf.Domain.Enums;

namespace BookShelf.Application.Interfaces;

public interface IBookClubService
{
    Task<BookClubDto?> GetBookClubByIdAsync(Guid id);
    Task<IEnumerable<BookClubDto>> GetAllPublicBookClubsAsync();
    Task<IEnumerable<BookClubDto>> GetBookClubsByUserIdAsync(Guid userId);
    Task<BookClubDto> CreateBookClubAsync(Guid creatorId, CreateBookClubDto clubDto);
    Task<BookClubDto> UpdateBookClubAsync(Guid userId, UpdateBookClubDto clubDto);
    Task DeleteBookClubAsync(Guid userId, Guid bookClubId);

    Task<BookClubMembershipDto> JoinBookClubAsync(Guid userId, Guid bookClubId);
    Task LeaveBookClubAsync(Guid userId, Guid bookClubId);
    Task<IEnumerable<BookClubMembershipDto>> GetBookClubMembersAsync(Guid bookClubId);
    Task UpdateMemberRoleAsync(Guid adminId, Guid bookClubId, Guid userId, MemberRole newRole);

    Task<DiscussionDto> CreateDiscussionAsync(Guid userId, CreateDiscussionDto discussionDto);
    Task<IEnumerable<DiscussionDto>> GetDiscussionsByBookClubIdAsync(Guid bookClubId);
    Task<IEnumerable<DiscussionDto>> GetDiscussionsByBookIdAsync(Guid bookId);
    Task<DiscussionDetailsDto?> GetDiscussionByIdAsync(Guid discussionId);
    Task<CommentDto> AddCommentAsync(Guid userId, CreateCommentDto commentDto);
}
