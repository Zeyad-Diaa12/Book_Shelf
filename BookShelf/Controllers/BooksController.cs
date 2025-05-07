using BookShelf.Application.DTOs;
using BookShelf.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookShelf.Controllers;

public class BooksController : BaseController
{
    private readonly IBookService _bookService;
    private readonly IReviewService _reviewService;
    private readonly IReadingService _readingService;

    public BooksController(
        IBookService bookService,
        IReviewService reviewService,
        IReadingService readingService)
    {
        _bookService = bookService;
        _reviewService = reviewService;
        _readingService = readingService;
    }

    public async Task<IActionResult> Index()
    {
        var books = await _bookService.GetAllBooksAsync();
        return View(books);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        // Get reviews for this book
        var reviews = await _bookService.GetBookReviewsAsync(id);
        ViewData["Reviews"] = reviews;

        // Get average rating
        var averageRating = await _reviewService.GetAverageRatingForBookAsync(id);
        ViewData["AverageRating"] = averageRating;

        // Get rating distribution
        var ratingDistribution = await _reviewService.GetRatingDistributionForBookAsync(id);
        ViewData["RatingDistribution"] = ratingDistribution;

        return View(book);
    }

    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookDto createBookDto)
    {
        if (ModelState.IsValid)
        {
            await _bookService.AddBookAsync(createBookDto);
            return RedirectToAction(nameof(Index));
        }
        return View(createBookDto);
    }

    [Authorize]
    public async Task<IActionResult> Edit(Guid id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        var updateDto = new UpdateBookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Description = book.Description,
            ISBN = book.ISBN,
            CoverImageUrl = book.CoverImageUrl,
            PublishedDate = book.PublishedDate,
            PageCount = book.PageCount
        };

        return View(updateDto);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateBookDto updateBookDto)
    {
        if (ModelState.IsValid)
        {
            await _bookService.UpdateBookAsync(updateBookDto);
            return RedirectToAction(nameof(Index));
        }
        return View(updateBookDto);
    }

    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    [Authorize]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _bookService.DeleteBookAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Search(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            return RedirectToAction(nameof(Index));
        }

        var books = await _bookService.SearchBooksAsync(searchTerm);
        ViewData["SearchTerm"] = searchTerm;
        return View("Index", books);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> StartReading(Guid bookId)
    {
        var userId = GetCurrentUserId();

        await _readingService.StartReadingBookAsync(userId, bookId);
        return RedirectToAction(nameof(Details), new { id = bookId });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> FinishReading(Guid bookId)
    {
        var userId = GetCurrentUserId();

        await _readingService.FinishReadingBookAsync(userId, bookId);
        return RedirectToAction(nameof(Details), new { id = bookId });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UpdateReadingProgress(UpdateReadingProgressDto progressDto)
    {
        var userId = GetCurrentUserId();

        await _readingService.UpdateReadingProgressAsync(userId, progressDto);
        return RedirectToAction(nameof(Details), new { id = progressDto.BookId });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddReview(CreateReviewDto reviewDto)
    {
        var userId = GetCurrentUserId();

        if (ModelState.IsValid)
        {
            await _reviewService.CreateReviewAsync(userId, reviewDto);
            return RedirectToAction(nameof(Details), new { id = reviewDto.BookId });
        }

        var book = await _bookService.GetBookByIdAsync(reviewDto.BookId);
        if (book == null)
        {
            return NotFound();
        }

        var reviews = await _bookService.GetBookReviewsAsync(reviewDto.BookId);
        ViewData["Reviews"] = reviews;
        return View("Details", book);
    }

    [Authorize]
    public async Task<IActionResult> AddToReading()
    {
        // Get a list of all books that the user can add to their reading list
        var books = await _bookService.GetAllBooksAsync();
        return View(books);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddToReading(Guid bookId)
    {
        var userId = GetCurrentUserId();

        try
        {
            // Start reading the selected book
            await _readingService.StartReadingBookAsync(userId, bookId);

            TempData["Success"] = "Book added to your reading list.";
            return RedirectToAction("Profile", "Users");
        }
        catch (Exception ex)
        {
            // Log the error
            TempData["Error"] = $"Error adding book to reading list: {ex.Message}";

            // Return to the AddToReading view
            var books = await _bookService.GetAllBooksAsync();
            return View(books);
        }
    }
}