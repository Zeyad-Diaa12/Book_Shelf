using BookShelf.Domain.Entities;
using BookShelf.Domain.Interfaces;
using BookShelf.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookShelf.Infrastructure.Repos;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context)
        : base(context) { }

    // Override the base GetByIdAsync to include reviews
    public override async Task<User?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(u => u.Reviews)
                .ThenInclude(r => r.Book)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.Where(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.Where(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<User>> GetUsersByBookClubIdAsync(Guid bookClubId)
    {
        return await _context
            .BookClubMemberships.Where(bcm => bcm.BookClubId == bookClubId)
            .Select(bcm => bcm.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReadingProgress>> GetReadingProgressByUserIdAsync(Guid userId)
    {
        return await _context
            .ReadingProgresses.Where(rp => rp.UserId == userId)
            .Include(rp => rp.Book)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReadingGoal>> GetReadingGoalsByUserIdAsync(Guid userId)
    {
        return await _context.ReadingGoals.Where(rg => rg.UserId == userId).ToListAsync();
    }
}
