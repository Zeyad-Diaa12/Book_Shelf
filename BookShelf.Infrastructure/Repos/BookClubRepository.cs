using BookShelf.Domain.Entities;
using BookShelf.Domain.Enums;
using BookShelf.Domain.Interfaces;
using BookShelf.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookShelf.Infrastructure.Repos;

public class BookClubRepository : Repository<BookClub>, IBookClubRepository
{
    public BookClubRepository(ApplicationDbContext context)
        : base(context) { }

    public async Task<IEnumerable<BookClub>> GetByUserIdAsync(Guid userId)
    {
        return await _context
            .BookClubMemberships.Where(bcm => bcm.UserId == userId)
            .Select(bcm => bcm.BookClub)
            .ToListAsync();
    }

    public async Task<IEnumerable<BookClub>> GetPublicBookClubsAsync()
    {
        return await _dbSet.Where(bc => bc.IsPublic).ToListAsync();
    }

    public async Task<IEnumerable<Discussion>> GetDiscussionsByBookClubIdAsync(Guid bookClubId)
    {
        return await _context.Discussions
            .Where(d => d.BookClubId == bookClubId)
            .Include(d => d.User)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<BookClubMembership?> GetMembershipAsync(Guid bookClubId, Guid userId)
    {
        return await _context
            .BookClubMemberships.Where(bcm => bcm.BookClubId == bookClubId && bcm.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task AddMemberAsync(BookClubMembership membership)
    {
        await _context.BookClubMemberships.AddAsync(membership);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateMemberRoleAsync(Guid bookClubId, Guid userId, MemberRole newRole)
    {
        var membership = await GetMembershipAsync(bookClubId, userId);
        if (membership != null)
        {
            membership.Role = newRole;
            _context.BookClubMemberships.Update(membership);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveMemberAsync(Guid bookClubId, Guid userId)
    {
        var membership = await GetMembershipAsync(bookClubId, userId);
        if (membership != null)
        {
            _context.BookClubMemberships.Remove(membership);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<BookClubMembership>> GetMembersAsync(Guid bookClubId)
    {
        return await _context
            .BookClubMemberships
            .Where(bcm => bcm.BookClubId == bookClubId)
            .Include(bcm => bcm.User)
            .ToListAsync();
    }
}
