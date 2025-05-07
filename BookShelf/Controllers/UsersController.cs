using BookShelf.Application.DTOs;
using BookShelf.Application.Interfaces;
using BookShelf.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookShelf.Controllers
{
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IReadingService _readingService;
        private readonly IBookService _bookService;

        public UsersController(IUserService userService, IReadingService readingService, IBookService bookService)
        {
            _userService = userService;
            _readingService = readingService;
            _bookService = bookService;
        }

        public IActionResult Register()
        {
            // If user is already logged in, redirect to home
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterUserDto registerDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userService.RegisterUserAsync(registerDto);
                    // Create claims for the user
                    await SignInUserAsync(user);

                    TempData["Success"] = "Registration successful! Welcome to BookShelf.";
                    return RedirectToAction("Index", "Home");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(registerDto);
        }

        public IActionResult Login(string returnUrl)
        {
            // If user is already logged in, redirect to home
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            // Create a new LoginUserDto with the returnUrl
            var model = new LoginUserDto
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginUserDto loginDto)
        {
            // Remove ReturnUrl from ModelState validation if it's causing issues
            ModelState.Remove("ReturnUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    // Use the updated LoginAsync method to get the user directly
                    var user = await _userService.LoginAsync(loginDto);

                    // Sign in the user
                    await SignInUserAsync(user);

                    TempData["Success"] = "Welcome back!";

                    // Redirect to the return URL if available or to the home page
                    return RedirectToLocal(loginDto.ReturnUrl ?? "");
                }
                catch (InvalidOperationException)
                {
                    ModelState.AddModelError("", "Invalid username or password");
                }
            }
            return View(loginDto);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "You have been successfully logged out.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile(Guid? id)
        {
            // If no ID provided, show current user's profile
            if (!id.HasValue)
            {
                id = GetCurrentUserId();
            }

            var user = await _userService.GetUserByIdAsync(id.Value);
            if (user == null)
            {
                return NotFound();
            }

            // Get user's books, reviews, reading progress, etc.
            user.Books = (await _userService.GetUserBooksAsync(id.Value)).ToList();
            user.Reviews = (await _userService.GetUserReviewsAsync(id.Value)).ToList();
            user.ReadingProgress = (await _userService.GetUserReadingProgressAsync(id.Value)).ToList();

            // Get currently reading books
            user.CurrentlyReading = (await _readingService.GetCurrentlyReadingBooksAsync(id.Value)).ToList();

            user.ReadingGoals = (await _userService.GetUserReadingGoalsAsync(id.Value)).ToList();

            // ReadingStats is already initialized in the UserDto constructor

            // Populate reading statistics - Fix for stats showing as zero
            var startOfYear = new DateTime(DateTime.Now.Year, 1, 1);
            var today = DateTime.Now;
            var stats = await _readingService.GetReadingStatsAsync(id.Value, startOfYear, today);

            // Update the reading stats with values from the stats dictionary
            // Fix: Use BooksReadTotal instead of BooksRead which is a computed property
            user.ReadingStats.BooksReadTotal = stats.GetValueOrDefault("BooksCompleted", 0);
            user.ReadingStats.PagesRead = stats.GetValueOrDefault("TotalPagesRead", 0);
            user.ReadingStats.ReviewsWritten = user.Reviews.Count;

            // These can remain unchanged for other statistics
            user.ReadingStats.BooksReadThisYear = stats.GetValueOrDefault("BooksCompleted", 0);
            user.ReadingStats.CurrentGoalsCount = user.ReadingGoals.Count(g => !g.IsCompleted);
            user.ReadingStats.CompletedGoalsCount = user.ReadingGoals.Count(g => g.IsCompleted);

            return View(user);
        }

        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var userId = GetCurrentUserId();
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateUserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Bio = user.Bio
            };

            return View(updateDto);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserDto updateDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    await _userService.UpdateUserAsync(userId, updateDto);

                    TempData["Success"] = "Your profile has been updated successfully.";
                    return RedirectToAction(nameof(Profile));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (KeyNotFoundException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(updateDto);
        }

        [Authorize]
        public async Task<IActionResult> ReadingGoals()
        {
            var userId = GetCurrentUserId();

            // Force a refresh of reading goals data from the database
            var goals = await _userService.GetUserReadingGoalsAsync(userId);

            // Add some diagnostic information to help troubleshoot
            foreach (var goal in goals)
            {
                // Log the current progress values
                Console.WriteLine($"Goal {goal.Id}: {goal.Title} - Current: {goal.Current}/{goal.Target} ({goal.ProgressPercentage}%)");
            }

            return View(goals);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReadingGoal(CreateReadingGoalDto goalDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = GetCurrentUserId();

                    // Set dates if the goal has a period
                    if (Request.Form.ContainsKey("HasPeriod") && Request.Form["HasPeriod"] == "on")
                    {
                        if (string.IsNullOrEmpty(Request.Form["StartDate"]) || string.IsNullOrEmpty(Request.Form["EndDate"]))
                        {
                            ModelState.AddModelError("", "Start and end dates are required when setting a time period.");
                            return RedirectToAction(nameof(ReadingGoals));
                        }

                        // Safe DateTime parsing with null checks
                        if (DateTime.TryParse(Request.Form["StartDate"], out var startDate))
                        {
                            goalDto.StartDate = startDate;
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid start date format.");
                            return RedirectToAction(nameof(ReadingGoals));
                        }

                        if (DateTime.TryParse(Request.Form["EndDate"], out var endDate))
                        {
                            goalDto.EndDate = endDate;
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid end date format.");
                            return RedirectToAction(nameof(ReadingGoals));
                        }
                    }
                    else
                    {
                        // Default to year-long goal
                        goalDto.StartDate = DateTime.UtcNow;
                        goalDto.EndDate = DateTime.UtcNow.AddYears(1);
                    }

                    // Convert GoalType if needed - Improved handling with enum parsing
                    if (Enum.TryParse<BookShelf.Domain.Enums.GoalType>(goalDto.GoalType.ToString(), out var goalType))
                    {
                        goalDto.Type = goalType;
                    }
                    else
                    {
                        // Fallback based on string comparison if direct enum parsing fails
                        if (goalDto.GoalType.ToString() == "BooksRead")
                        {
                            goalDto.Type = BookShelf.Domain.Enums.GoalType.BooksCompleted;
                        }
                        else if (goalDto.GoalType.ToString() == "PagesRead")
                        {
                            goalDto.Type = BookShelf.Domain.Enums.GoalType.PagesRead;
                        }
                    }

                    // Map from form values to DTO
                    goalDto.Target = goalDto.TargetCount;
                    goalDto.Current = goalDto.CurrentCount;

                    await _readingService.CreateReadingGoalAsync(userId, goalDto);
                    TempData["Success"] = "Reading goal created successfully!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating reading goal: {ex.Message}";
                }
            }
            else
            {
                TempData["Error"] = "Please correct the errors in the form.";
                // Log model state errors for debugging
                foreach (var modelState in ModelState)
                {
                    var key = modelState.Key;
                    var errors = modelState.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Error in {key}: {error.ErrorMessage}");
                    }
                }
            }

            return RedirectToAction(nameof(ReadingGoals));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGoalProgress(Guid id, int incrementAmount)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Directly use the value from the form as the increment amount
                // This will add to the current progress instead of replacing it
                int increment = incrementAmount;

                // Only update if there's a real change
                if (increment > 0)
                {
                    var updatedGoal = await _readingService.UpdateReadingGoalProgressAsync(id, increment);
                    TempData["Success"] = $"Goal progress updated successfully! New progress: {updatedGoal.Current}/{updatedGoal.Target}";
                }
                else
                {
                    TempData["Info"] = "No change in progress value.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating goal progress: {ex.Message}";
            }

            return RedirectToAction(nameof(ReadingGoals));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReadingGoal(Guid id)
        {
            try
            {
                await _readingService.DeleteReadingGoalAsync(id);
                TempData["Success"] = "Reading goal deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting reading goal: {ex.Message}";
            }

            return RedirectToAction(nameof(ReadingGoals));
        }

        public async Task<IActionResult> Reviews(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Get all user reviews
            user.Reviews = (await _userService.GetUserReviewsAsync(id)).ToList();

            // Sort reviews by most recent first
            user.Reviews = user.Reviews.OrderByDescending(r => r.CreatedDate).ToList();

            return View(user);
        }

        [Authorize]
        public async Task<IActionResult> MyBooks()
        {
            var userId = GetCurrentUserId();

            // Get all user books - instead of only getting books created by the user,
            // we want to get all books the user has in their collection
            var userReadingProgress = await _userService.GetUserReadingProgressAsync(userId);

            // Get all books that the user has reading progress for
            var bookIds = userReadingProgress.Select(rp => rp.BookId).Distinct().ToList();

            // Get the book details for each book ID
            var books = new List<BookDto>();
            foreach (var bookId in bookIds)
            {
                var book = await _bookService.GetBookByIdAsync(bookId);
                if (book != null)
                {
                    books.Add(book);
                }
            }

            // Create a view model that combines books with their reading progress
            var viewModel = new MyBooksViewModel
            {
                Books = books,
                ReadingProgress = userReadingProgress.ToList()
            };

            return View(viewModel);
        }

        private async Task SignInUserAsync(UserDto user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Clear any existing authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Sign in with the new identity
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}