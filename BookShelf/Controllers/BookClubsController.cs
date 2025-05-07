using BookShelf.Application.DTOs;
using BookShelf.Application.Interfaces;
using BookShelf.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShelf.Controllers;

public class BookClubsController : BaseController
{
    private readonly IBookClubService _bookClubService;

    public BookClubsController(IBookClubService bookClubService)
    {
        _bookClubService = bookClubService;
    }

    public async Task<IActionResult> Index()
    {
        var bookClubs = await _bookClubService.GetAllPublicBookClubsAsync();
        return View(bookClubs);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var bookClub = await _bookClubService.GetBookClubByIdAsync(id);
        if (bookClub == null)
        {
            return NotFound();
        }

        // Check if current user is a member
        var userId = GetCurrentUserId();
        bookClub.IsMember = bookClub.Members.Any(m => m.UserId == userId);

        return View(bookClub);
    }

    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookClubDto createDto)
    {
        if (ModelState.IsValid)
        {
            var userId = GetCurrentUserId();
            var bookClub = await _bookClubService.CreateBookClubAsync(userId, createDto);
            return RedirectToAction(nameof(Details), new { id = bookClub.Id });
        }
        return View(createDto);
    }

    [Authorize]
    public async Task<IActionResult> Edit(Guid id)
    {
        var bookClub = await _bookClubService.GetBookClubByIdAsync(id);
        if (bookClub == null)
        {
            return NotFound();
        }

        // Check if user is admin or creator
        var userId = GetCurrentUserId();
        var membership = bookClub.Members.FirstOrDefault(m => m.UserId == userId);
        if (membership == null || (membership.Role != MemberRole.Admin && bookClub.CreatorId != userId))
        {
            return Forbid();
        }

        var updateDto = new UpdateBookClubDto
        {
            Id = bookClub.Id,
            Name = bookClub.Name,
            Description = bookClub.Description,
            IsPublic = bookClub.IsPublic,
            ImageUrl = bookClub.ImageUrl
        };

        return View(updateDto);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateBookClubDto updateDto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _bookClubService.UpdateBookClubAsync(userId, updateDto);
                return RedirectToAction(nameof(Details), new { id = updateDto.Id });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
        return View(updateDto);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _bookClubService.DeleteBookClubAsync(userId, id);
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Join(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _bookClubService.JoinBookClubAsync(userId, id);
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException)
        {
            // User already a member, just redirect back
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Leave(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _bookClubService.LeaveBookClubAsync(userId, id);
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    public async Task<IActionResult> Members(Guid id)
    {
        try
        {
            var members = await _bookClubService.GetBookClubMembersAsync(id);
            ViewData["BookClubId"] = id;
            return View(members);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UpdateMemberRole(Guid bookClubId, Guid userId, MemberRole newRole)
    {
        try
        {
            var adminId = GetCurrentUserId();
            await _bookClubService.UpdateMemberRoleAsync(adminId, bookClubId, userId, newRole);
            return RedirectToAction(nameof(Members), new { id = bookClubId });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Members), new { id = bookClubId });
        }
    }

    [Authorize]
    public async Task<IActionResult> MyBookClubs()
    {
        var userId = GetCurrentUserId();
        var bookClubs = await _bookClubService.GetBookClubsByUserIdAsync(userId);
        return View(bookClubs);
    }

    [Authorize]
    public IActionResult CreateDiscussion(Guid? bookClubId)
    {
        var discussionDto = new CreateDiscussionDto
        {
            BookClubId = bookClubId
        };
        return View(discussionDto);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDiscussion(CreateDiscussionDto discussionDto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var userId = GetCurrentUserId();
                var discussion = await _bookClubService.CreateDiscussionAsync(userId, discussionDto);

                if (discussionDto.BookClubId.HasValue)
                {
                    return RedirectToAction(nameof(Details), new { id = discussionDto.BookClubId });
                }

                return RedirectToAction("Index", "Discussions");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
        return View(discussionDto);
    }

    public async Task<IActionResult> Discussions(Guid id)
    {
        try
        {
            var discussions = await _bookClubService.GetDiscussionsByBookClubIdAsync(id);
            ViewData["BookClubId"] = id;

            // Get book club name
            var bookClub = await _bookClubService.GetBookClubByIdAsync(id);
            if (bookClub != null)
            {
                ViewData["BookClubName"] = bookClub.Name;
            }

            return View(discussions);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> DiscussionDetails(Guid id)
    {
        try
        {
            var discussion = await _bookClubService.GetDiscussionByIdAsync(id);
            if (discussion == null)
            {
                return NotFound();
            }

            return View(discussion);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(CreateCommentDto commentDto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _bookClubService.AddCommentAsync(userId, commentDto);

                return RedirectToAction(nameof(DiscussionDetails), new { id = commentDto.DiscussionId });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // If we get here, something failed, redisplay form
        var discussion = await _bookClubService.GetDiscussionByIdAsync(commentDto.DiscussionId);
        return View("DiscussionDetails", discussion);
    }
}