using AutoMapper;
using BookShelf.Application.DTOs;
using BookShelf.Application.Interfaces;
using BookShelf.Domain.Entities;
using BookShelf.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace BookShelf.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IBookRepository bookRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<UserDto> RegisterUserAsync(RegisterUserDto registerDto)
    {
        // Check if username or email already exists
        var existingByUsername = await _userRepository.GetByUsernameAsync(registerDto.Username);
        if (existingByUsername != null)
        {
            throw new InvalidOperationException("Username is already taken");
        }

        var existingByEmail = await _userRepository.GetByEmailAsync(registerDto.Email);
        if (existingByEmail != null)
        {
            throw new InvalidOperationException("Email is already registered");
        }

        // Create new user with UTC datetime
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = HashPassword(registerDto.Password),
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            CreatedDate = DateTime.UtcNow,
            ProfilePictureUrl = registerDto.ProfilePictureUrl ?? "https://www.gravatar.com/avatar/00000000000000000000000000000000?d=mp&f=y",
            Bio = registerDto.Bio ?? ""
        };

        var createdUser = await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<UserDto>(createdUser);
    }

    public async Task<UserDto> LoginAsync(LoginUserDto loginDto)
    {
        var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
        if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid username or password");
        }

        // Return the user DTO for proper authentication in the controller
        return _mapper.Map<UserDto>(user);
    }

    public async Task UpdateUserAsync(Guid userId, UpdateUserDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // If email is changed, check if new email is already in use
        if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != user.Email)
        {
            var existingByEmail = await _userRepository.GetByEmailAsync(updateDto.Email);
            if (existingByEmail != null)
            {
                throw new InvalidOperationException("Email is already registered");
            }
            user.Email = updateDto.Email;
        }

        // Update other properties
        user.FirstName = updateDto.FirstName;
        user.LastName = updateDto.LastName;
        user.ProfilePictureUrl = updateDto.ProfilePictureUrl;
        user.Bio = updateDto.Bio;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        await _userRepository.DeleteAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<BookDto>> GetUserBooksAsync(Guid userId)
    {
        var books = await _bookRepository.GetBooksByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<BookDto>>(books);
    }

    public async Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        return _mapper.Map<IEnumerable<ReviewDto>>(user.Reviews);
    }

    public async Task<IEnumerable<ReadingProgressDto>> GetUserReadingProgressAsync(Guid userId)
    {
        var readingProgress = await _userRepository.GetReadingProgressByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<ReadingProgressDto>>(readingProgress);
    }

    public async Task<IEnumerable<ReadingGoalDto>> GetUserReadingGoalsAsync(Guid userId)
    {
        var readingGoals = await _userRepository.GetReadingGoalsByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<ReadingGoalDto>>(readingGoals);
    }

    // Helper methods for password handling
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        var inputHash = HashPassword(password);
        return inputHash == hash;
    }
}