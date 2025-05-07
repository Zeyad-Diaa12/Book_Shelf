using AutoMapper;
using BookShelf.Application.DTOs;
using BookShelf.Application.Interfaces;
using BookShelf.Domain.Entities;
using BookShelf.Domain.Enums;
using BookShelf.Domain.Interfaces;

namespace BookShelf.Application.Services;

public class BookClubService : IBookClubService
{
    private readonly IBookClubRepository _bookClubRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRepository<Discussion> _discussionRepository;
    private readonly IRepository<Comment> _commentRepository;
    private readonly IMapper _mapper;

    public BookClubService(
        IBookClubRepository bookClubRepository,
        IBookRepository bookRepository,
        IUserRepository userRepository,
        IRepository<Discussion> discussionRepository,
        IRepository<Comment> commentRepository,
        IMapper mapper
    )
    {
        _bookClubRepository = bookClubRepository;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
        _discussionRepository = discussionRepository;
        _commentRepository = commentRepository;
        _mapper = mapper;
    }

    public async Task<BookClubDto?> GetBookClubByIdAsync(Guid id)
    {
        var bookClub = await _bookClubRepository.GetByIdAsync(id);
        if (bookClub == null)
        {
            return null;
        }

        var bookClubDto = _mapper.Map<BookClubDto>(bookClub);

        // Get member count
        var members = await _bookClubRepository.GetMembersAsync(id);
        bookClubDto.MemberCount = members.Count();
        bookClubDto.Members = _mapper.Map<List<BookClubMembershipDto>>(members);

        // Get recent discussions
        var discussions = await _bookClubRepository.GetDiscussionsByBookClubIdAsync(id);
        bookClubDto.RecentDiscussions = _mapper.Map<List<DiscussionDto>>(
            discussions.OrderByDescending(d => d.CreatedDate).Take(5)
        );

        // Manually update the comment counts for recent discussions
        foreach (var discussion in bookClubDto.RecentDiscussions)
        {
            var comments = await _commentRepository.FindAsync(c => c.DiscussionId == discussion.Id);
            discussion.CommentCount = comments.Count();
        }

        return bookClubDto;
    }

    public async Task<IEnumerable<BookClubDto>> GetAllPublicBookClubsAsync()
    {
        var bookClubs = await _bookClubRepository.GetPublicBookClubsAsync();
        var bookClubDtos = _mapper.Map<IEnumerable<BookClubDto>>(bookClubs).ToList();

        // Update member counts for each book club
        foreach (var bookClubDto in bookClubDtos)
        {
            var members = await _bookClubRepository.GetMembersAsync(bookClubDto.Id);
            bookClubDto.MemberCount = members.Count();

            // Get creator username if needed
            if (string.IsNullOrEmpty(bookClubDto.CreatorUsername) && bookClubDto.CreatorId != Guid.Empty)
            {
                var creator = await _userRepository.GetByIdAsync(bookClubDto.CreatorId);
                if (creator != null)
                {
                    bookClubDto.CreatorUsername = creator.Username;
                }
            }
        }

        return bookClubDtos;
    }

    public async Task<IEnumerable<BookClubDto>> GetBookClubsByUserIdAsync(Guid userId)
    {
        var bookClubs = await _bookClubRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<BookClubDto>>(bookClubs);
    }

    public async Task<BookClubDto> CreateBookClubAsync(Guid creatorId, CreateBookClubDto clubDto)
    {
        // Check if user exists
        var creator = await _userRepository.GetByIdAsync(creatorId);
        if (creator == null)
        {
            throw new KeyNotFoundException($"User with ID {creatorId} not found");
        }

        // Create new book club
        var bookClub = new BookClub
        {
            Id = Guid.NewGuid(),
            Name = clubDto.Name,
            Description = clubDto.Description,
            CreatorId = creatorId,
            CreatedDate = DateTime.UtcNow,
            IsPublic = clubDto.IsPublic,
            ImageUrl = clubDto.ImageUrl,
        };

        var createdBookClub = await _bookClubRepository.AddAsync(bookClub);

        // Add creator as admin
        var membership = new BookClubMembership
        {
            Id = Guid.NewGuid(),
            BookClubId = createdBookClub.Id,
            UserId = creatorId,
            Role = MemberRole.Admin,
            JoinedDate = DateTime.UtcNow,
        };

        await _bookClubRepository.AddMemberAsync(membership);
        await _bookClubRepository.SaveChangesAsync();

        var result = _mapper.Map<BookClubDto>(createdBookClub);
        result.CreatorUsername = creator.Username;
        result.MemberCount = 1; // Just the creator initially
        result.Members = new List<BookClubMembershipDto>
        {
            new BookClubMembershipDto
            {
                Id = membership.Id,
                BookClubId = createdBookClub.Id,
                BookClubName = createdBookClub.Name,
                UserId = creatorId,
                Username = creator.Username,
                Role = MemberRole.Admin,
                JoinedDate = DateTime.UtcNow,
            },
        };

        return result;
    }

    public async Task<BookClubDto> UpdateBookClubAsync(Guid userId, UpdateBookClubDto clubDto)
    {
        var bookClub = await _bookClubRepository.GetByIdAsync(clubDto.Id);
        if (bookClub == null)
        {
            throw new KeyNotFoundException($"Book club with ID {clubDto.Id} not found");
        }

        // Check if user is admin or creator
        var membership = await _bookClubRepository.GetMembershipAsync(clubDto.Id, userId);
        if (
            membership == null
            || (membership.Role != MemberRole.Admin && bookClub.CreatorId != userId)
        )
        {
            throw new UnauthorizedAccessException(
                "Only admins or the creator can update the book club"
            );
        }

        // Update book club
        bookClub.Name = clubDto.Name;
        bookClub.Description = clubDto.Description;
        bookClub.IsPublic = clubDto.IsPublic;
        bookClub.ImageUrl = clubDto.ImageUrl;

        await _bookClubRepository.UpdateAsync(bookClub);
        await _bookClubRepository.SaveChangesAsync();

        return await GetBookClubByIdAsync(bookClub.Id)
            ?? throw new Exception("Failed to retrieve updated book club");
    }

    public async Task DeleteBookClubAsync(Guid userId, Guid bookClubId)
    {
        var bookClub = await _bookClubRepository.GetByIdAsync(bookClubId);
        if (bookClub == null)
        {
            throw new KeyNotFoundException($"Book club with ID {bookClubId} not found");
        }

        // Only creator can delete book club
        if (bookClub.CreatorId != userId)
        {
            throw new UnauthorizedAccessException("Only the creator can delete the book club");
        }

        await _bookClubRepository.DeleteAsync(bookClub);
        await _bookClubRepository.SaveChangesAsync();
    }

    public async Task<BookClubMembershipDto> JoinBookClubAsync(Guid userId, Guid bookClubId)
    {
        // Check if user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Check if book club exists
        var bookClub = await _bookClubRepository.GetByIdAsync(bookClubId);
        if (bookClub == null)
        {
            throw new KeyNotFoundException($"Book club with ID {bookClubId} not found");
        }

        // Check if user is already a member
        var existingMembership = await _bookClubRepository.GetMembershipAsync(bookClubId, userId);
        if (existingMembership != null)
        {
            throw new InvalidOperationException("User is already a member of this book club");
        }

        // Check if book club is public
        if (!bookClub.IsPublic)
        {
            throw new UnauthorizedAccessException(
                "This book club is private and requires an invitation"
            );
        }

        // Add user as member
        var membership = new BookClubMembership
        {
            Id = Guid.NewGuid(),
            BookClubId = bookClubId,
            UserId = userId,
            Role = MemberRole.Member,
            JoinedDate = DateTime.UtcNow,
        };

        await _bookClubRepository.AddMemberAsync(membership);
        await _bookClubRepository.SaveChangesAsync();

        var result = _mapper.Map<BookClubMembershipDto>(membership);
        result.BookClubName = bookClub.Name;
        result.Username = user.Username;

        return result;
    }

    public async Task LeaveBookClubAsync(Guid userId, Guid bookClubId)
    {
        // Check if book club exists
        var bookClub = await _bookClubRepository.GetByIdAsync(bookClubId);
        if (bookClub == null)
        {
            throw new KeyNotFoundException($"Book club with ID {bookClubId} not found");
        }

        // Creator cannot leave the book club, they must delete it
        if (bookClub.CreatorId == userId)
        {
            throw new InvalidOperationException(
                "The creator cannot leave the book club, they must delete it instead"
            );
        }

        // Check if user is a member
        var membership = await _bookClubRepository.GetMembershipAsync(bookClubId, userId);
        if (membership == null)
        {
            throw new InvalidOperationException("User is not a member of this book club");
        }

        await _bookClubRepository.RemoveMemberAsync(bookClubId, userId);
        await _bookClubRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<BookClubMembershipDto>> GetBookClubMembersAsync(Guid bookClubId)
    {
        // Check if book club exists
        var bookClub = await _bookClubRepository.GetByIdAsync(bookClubId);
        if (bookClub == null)
        {
            throw new KeyNotFoundException($"Book club with ID {bookClubId} not found");
        }

        var members = await _bookClubRepository.GetMembersAsync(bookClubId);
        var memberDtos = new List<BookClubMembershipDto>();

        foreach (var membership in members)
        {
            var memberDto = _mapper.Map<BookClubMembershipDto>(membership);
            var user = await _userRepository.GetByIdAsync(membership.UserId);
            if (user != null)
            {
                memberDto.Username = user.Username;
            }
            memberDto.BookClubName = bookClub.Name;
            memberDtos.Add(memberDto);
        }

        return memberDtos;
    }

    public async Task UpdateMemberRoleAsync(
        Guid adminId,
        Guid bookClubId,
        Guid userId,
        MemberRole newRole
    )
    {
        // Check if book club exists
        var bookClub = await _bookClubRepository.GetByIdAsync(bookClubId);
        if (bookClub == null)
        {
            throw new KeyNotFoundException($"Book club with ID {bookClubId} not found");
        }

        // Check if admin is an admin or creator
        var adminMembership = await _bookClubRepository.GetMembershipAsync(bookClubId, adminId);
        if (
            adminMembership == null
            || (adminMembership.Role != MemberRole.Admin && bookClub.CreatorId != adminId)
        )
        {
            throw new UnauthorizedAccessException(
                "Only admins or the creator can update member roles"
            );
        }

        // Cannot change creator's role
        if (bookClub.CreatorId == userId)
        {
            throw new InvalidOperationException("Cannot change the creator's role");
        }

        // Check if user is a member
        var userMembership = await _bookClubRepository.GetMembershipAsync(bookClubId, userId);
        if (userMembership == null)
        {
            throw new InvalidOperationException("User is not a member of this book club");
        }

        await _bookClubRepository.UpdateMemberRoleAsync(bookClubId, userId, newRole);
        await _bookClubRepository.SaveChangesAsync();
    }

    public async Task<DiscussionDto> CreateDiscussionAsync(
        Guid userId,
        CreateDiscussionDto discussionDto
    )
    {
        // Check if user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // If book club is specified, check if it exists and user is a member
        if (discussionDto.BookClubId.HasValue)
        {
            var bookClub = await _bookClubRepository.GetByIdAsync(discussionDto.BookClubId.Value);
            if (bookClub == null)
            {
                throw new KeyNotFoundException(
                    $"Book club with ID {discussionDto.BookClubId} not found"
                );
            }

            var membership = await _bookClubRepository.GetMembershipAsync(
                discussionDto.BookClubId.Value,
                userId
            );
            if (membership == null)
            {
                throw new UnauthorizedAccessException(
                    "Only members can create discussions in the book club"
                );
            }
        }

        // If book is specified, check if it exists
        if (discussionDto.BookId.HasValue)
        {
            var book = await _bookRepository.GetByIdAsync(discussionDto.BookId.Value);
            if (book == null)
            {
                throw new KeyNotFoundException($"Book with ID {discussionDto.BookId} not found");
            }
        }

        // Create discussion
        var discussion = new Discussion
        {
            Id = Guid.NewGuid(),
            Title = discussionDto.Title,
            Content = discussionDto.Content,
            UserId = userId,
            BookId = discussionDto.BookId,
            BookClubId = discussionDto.BookClubId,
            CreatedDate = DateTime.UtcNow,
        };

        var createdDiscussion = await _discussionRepository.AddAsync(discussion);
        await _discussionRepository.SaveChangesAsync();

        // Prepare response with additional context
        var result = _mapper.Map<DiscussionDto>(createdDiscussion);
        result.Username = user.Username;

        if (discussionDto.BookId.HasValue)
        {
            var book = await _bookRepository.GetByIdAsync(discussionDto.BookId.Value);
            if (book != null)
            {
                result.BookTitle = book.Title;
            }
        }

        if (discussionDto.BookClubId.HasValue)
        {
            var bookClub = await _bookClubRepository.GetByIdAsync(discussionDto.BookClubId.Value);
            if (bookClub != null)
            {
                result.BookClubName = bookClub.Name;
            }
        }

        return result;
    }

    public async Task<IEnumerable<DiscussionDto>> GetDiscussionsByBookClubIdAsync(Guid bookClubId)
    {
        // Check if book club exists
        var bookClub = await _bookClubRepository.GetByIdAsync(bookClubId);
        if (bookClub == null)
        {
            throw new KeyNotFoundException($"Book club with ID {bookClubId} not found");
        }

        var discussions = await _bookClubRepository.GetDiscussionsByBookClubIdAsync(bookClubId);
        var discussionDtos = _mapper.Map<IEnumerable<DiscussionDto>>(discussions);

        // Enrich DTOs with additional information
        foreach (var dto in discussionDtos)
        {
            // Get username
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user != null)
            {
                dto.Username = user.Username;
            }

            // Get book title if applicable
            if (dto.BookId.HasValue)
            {
                var book = await _bookRepository.GetByIdAsync(dto.BookId.Value);
                if (book != null)
                {
                    dto.BookTitle = book.Title;
                }
            }

            dto.BookClubName = bookClub.Name;

            // Get comment count by explicitly loading comments from the comment repository
            var comments = await _commentRepository.FindAsync(c => c.DiscussionId == dto.Id);
            dto.CommentCount = comments.Count();
        }

        return discussionDtos;
    }

    public Task<IEnumerable<DiscussionDto>> GetDiscussionsByBookIdAsync(Guid bookId)
    {
        throw new NotImplementedException();
    }

    public async Task<DiscussionDetailsDto?> GetDiscussionByIdAsync(Guid discussionId)
    {
        // Get discussion with its comments explicitly loaded
        var comments = await _commentRepository.FindAsync(c => c.DiscussionId == discussionId);

        var discussion = await _discussionRepository.GetByIdAsync(discussionId);
        if (discussion == null)
        {
            return null;
        }

        // Make sure discussion has comments collection initialized
        if (discussion.Comments == null)
        {
            discussion.Comments = new List<Comment>();
        }

        // Add all comments found to the discussion
        foreach (var comment in comments)
        {
            // Avoid duplicates
            if (!discussion.Comments.Any(c => c.Id == comment.Id))
            {
                discussion.Comments.Add(comment);
            }
        }

        var result = _mapper.Map<DiscussionDetailsDto>(discussion);

        // Get username
        var user = await _userRepository.GetByIdAsync(discussion.UserId);
        if (user != null)
        {
            result.Username = user.Username;
        }

        // Get book club info if applicable
        if (discussion.BookClubId.HasValue)
        {
            var bookClub = await _bookClubRepository.GetByIdAsync(discussion.BookClubId.Value);
            if (bookClub != null)
            {
                result.BookClubName = bookClub.Name;
            }
        }

        // Get book info if applicable
        if (discussion.BookId.HasValue)
        {
            var book = await _bookRepository.GetByIdAsync(discussion.BookId.Value);
            if (book != null)
            {
                result.BookTitle = book.Title;
            }
        }

        // Map comments
        result.Comments = _mapper.Map<List<CommentDto>>(discussion.Comments.OrderBy(c => c.CreatedDate));

        // Enrich comments with usernames
        foreach (var comment in result.Comments)
        {
            var commentUser = await _userRepository.GetByIdAsync(comment.UserId);
            if (commentUser != null)
            {
                comment.Username = commentUser.Username;
            }
        }

        return result;
    }

    public async Task<CommentDto> AddCommentAsync(Guid userId, CreateCommentDto commentDto)
    {
        // Check if user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Check if discussion exists
        var discussion = await _discussionRepository.GetByIdAsync(commentDto.DiscussionId);
        if (discussion == null)
        {
            throw new KeyNotFoundException($"Discussion with ID {commentDto.DiscussionId} not found");
        }

        // If the discussion is in a book club, verify user is a member
        if (discussion.BookClubId.HasValue)
        {
            var membership = await _bookClubRepository.GetMembershipAsync(
                discussion.BookClubId.Value,
                userId
            );
            if (membership == null)
            {
                throw new UnauthorizedAccessException(
                    "Only members can comment on discussions in the book club"
                );
            }
        }

        // Create comment
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = commentDto.Content,
            UserId = userId,
            DiscussionId = commentDto.DiscussionId,
            ParentCommentId = commentDto.ParentCommentId,
            CreatedDate = DateTime.UtcNow
        };

        // Use the injected comment repository instead of creating one
        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();

        var result = _mapper.Map<CommentDto>(comment);
        result.Username = user.Username;

        return result;
    }
}
