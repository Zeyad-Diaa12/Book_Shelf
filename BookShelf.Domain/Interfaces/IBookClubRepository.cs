using BookShelf.Domain.Entities;
using BookShelf.Domain.Enums;

namespace BookShelf.Domain.Interfaces;

public interface IBookClubRepository : IRepository<BookClub>
{
    Task<IEnumerable<BookClub>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<BookClub>> GetPublicBookClubsAsync();
    Task<IEnumerable<Discussion>> GetDiscussionsByBookClubIdAsync(Guid bookClubId);
    Task<BookClubMembership?> GetMembershipAsync(Guid bookClubId, Guid userId);
    Task<IEnumerable<BookClubMembership>> GetMembersAsync(Guid bookClubId);
    Task AddMemberAsync(BookClubMembership membership);
    Task UpdateMemberRoleAsync(Guid bookClubId, Guid userId, MemberRole newRole);
    Task RemoveMemberAsync(Guid bookClubId, Guid userId);
}
