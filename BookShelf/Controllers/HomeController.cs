using System.Diagnostics;
using System.Security.Claims;
using BookShelf.Application.Interfaces;
using BookShelf.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookShelf.Controllers;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly IBookService _bookService;
    private readonly IReviewService _reviewService;
    private readonly IBookClubService _bookClubService;
    private readonly IReadingService _readingService;

    public HomeController(
        ILogger<HomeController> logger,
        IBookService bookService,
        IReviewService reviewService,
        IBookClubService bookClubService,
        IReadingService readingService)
    {
        _logger = logger;
        _bookService = bookService;
        _reviewService = reviewService;
        _bookClubService = bookClubService;
        _readingService = readingService;
    }

    public async Task<IActionResult> Index()
    {
        // Get currently logged in user ID (this is a placeholder)
        var userId = GetCurrentUserId();

        // Get top rated books
        var topRatedBooks = await _reviewService.GetTopRatedBooksAsync(6);
        ViewData["TopRatedBooks"] = topRatedBooks;

        // Get personalized recommendations for the user
        var recommendations = await _bookService.GetRecommendationsAsync(userId, 6);
        ViewData["Recommendations"] = recommendations;

        // Get popular book clubs
        var popularBookClubs = await _bookClubService.GetAllPublicBookClubsAsync();
        ViewData["PopularBookClubs"] = popularBookClubs.Take(4);

        // Get current user's reading progress
        if (userId != Guid.Empty)
        {
            var currentlyReading = await _readingService.GetCurrentlyReadingBooksAsync(userId);
            ViewData["CurrentlyReading"] = currentlyReading.Take(3);

            var readingGoals = await _readingService.GetActiveReadingGoalsAsync(userId);
            ViewData["ReadingGoals"] = readingGoals;
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
