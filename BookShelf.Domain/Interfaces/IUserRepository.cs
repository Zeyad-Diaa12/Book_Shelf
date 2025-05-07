using BookShelf.Domain.Entities;

namespace BookShelf.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersByBookClubIdAsync(Guid bookClubId);
    Task<IEnumerable<ReadingProgress>> GetReadingProgressByUserIdAsync(Guid userId);
    Task<IEnumerable<ReadingGoal>> GetReadingGoalsByUserIdAsync(Guid userId);
}
