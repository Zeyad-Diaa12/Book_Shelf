using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelf.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ProfilePictureUrl { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int BookCount { get; set; }
    public int ReviewCount { get; set; }

    // Collections for profile view
    public List<BookDto> Books { get; set; } = new List<BookDto>();
    public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
    public List<ReadingProgressDto> ReadingProgress { get; set; } = new List<ReadingProgressDto>();
    public List<ReadingGoalDto> ReadingGoals { get; set; } = new List<ReadingGoalDto>();

    // Properties for profile view
    public string Name => $"{FirstName} {LastName}";
    public string Location { get; set; } = string.Empty;
    public DateTime JoinDate => CreatedDate;

    // Reading stats
    public UserReadingStatsDto ReadingStats { get; set; } = new UserReadingStatsDto();

    // Currently reading books
    public List<ReadingProgressDto> CurrentlyReading { get; set; } = new List<ReadingProgressDto>();

    // Recent reviews
    public List<ReviewDto> RecentReviews =>
        Reviews.OrderByDescending(r => r.CreatedDate).Take(3).ToList();
}

public class UserReadingStatsDto
{
    public int BooksReadThisYear { get; set; }
    public int BooksReadTotal { get; set; }
    public int CurrentGoalsCount { get; set; }
    public int CompletedGoalsCount { get; set; }

    // Adding missing properties referenced in views
    public int BooksRead => BooksReadTotal;
    public int PagesRead { get; set; }
    public int ReviewsWritten { get; set; }
}

public class RegisterUserDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(
        50,
        MinimumLength = 3,
        ErrorMessage = "Username must be between 3 and 50 characters"
    )]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(
        100,
        MinimumLength = 6,
        ErrorMessage = "Password must be at least 6 characters long"
    )]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; internal set; }
    public string? Bio { get; internal set; }
}

public class LoginUserDto
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    // No validation attributes for ReturnUrl
    public string? ReturnUrl { get; set; }
}

public class UpdateUserDto
{
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Url(ErrorMessage = "Invalid URL format")]
    public string ProfilePictureUrl { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Bio cannot be longer than 500 characters")]
    public string Bio { get; set; } = string.Empty;
}
