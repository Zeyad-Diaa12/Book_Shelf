using AutoMapper;
using BookShelf.Application.DTOs;
using BookShelf.Domain.Entities;

namespace BookShelf.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Book mappings
        CreateMap<Book, BookDto>()
            .ForMember(
                dest => dest.AverageRating,
                opt =>
                    opt.MapFrom(src => src.Reviews.Any() ? src.Reviews.Average(r => r.Rating) : 0)
            )
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Reviews.Count));
        CreateMap<CreateBookDto, Book>();
        CreateMap<UpdateBookDto, Book>();

        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(
                dest => dest.BookCount,
                opt =>
                    opt.MapFrom(src =>
                        src.Bookshelves.SelectMany(bs => bs.Books).DistinctBy(b => b.Id).Count()
                    )
            )
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Reviews.Count));
        CreateMap<RegisterUserDto, User>();

        // Review mappings
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book.Title))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username));
        CreateMap<CreateReviewDto, Review>();
        CreateMap<UpdateReviewDto, Review>();

        // Reading progress mappings
        CreateMap<ReadingProgress, ReadingProgressDto>()
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book.Title))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.Book.PageCount))
            .ForMember(
                dest => dest.CompletionPercentage,
                opt =>
                    opt.MapFrom(src =>
                        src.Book.PageCount > 0
                            ? (double)src.CurrentPage / src.Book.PageCount * 100
                            : 0
                    )
            );
        CreateMap<UpdateReadingProgressDto, ReadingProgress>();

        // Reading goal mappings
        CreateMap<ReadingGoal, ReadingGoalDto>()
            .ForMember(dest => dest.Current, opt => opt.MapFrom(src => src.Current))
            .ForMember(
                dest => dest.ProgressPercentage,
                opt =>
                    opt.MapFrom(src =>
                        src.Target > 0 ? (double)src.Current / src.Target * 100 : 0
                    )
            )
            .ForMember(
                dest => dest.IsCompleted,
                opt => opt.MapFrom(src => src.Current >= src.Target)
            );
        CreateMap<CreateReadingGoalDto, ReadingGoal>();

        // BookClub mappings
        CreateMap<BookClub, BookClubDto>()
            .ForMember(
                dest => dest.CreatorUsername,
                opt => opt.MapFrom(src => src.Creator.Username)
            )
            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Memberships.Count));
        CreateMap<CreateBookClubDto, BookClub>();
        CreateMap<UpdateBookClubDto, BookClub>();

        // BookClubMembership mappings
        CreateMap<BookClubMembership, BookClubMembershipDto>()
            .ForMember(dest => dest.BookClubName, opt => opt.MapFrom(src => src.BookClub.Name))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username));

        // Discussion mappings
        CreateMap<Discussion, DiscussionDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(
                dest => dest.BookTitle,
                opt => opt.MapFrom(src => src.Book != null ? src.Book.Title : null)
            )
            .ForMember(
                dest => dest.BookClubName,
                opt => opt.MapFrom(src => src.BookClub != null ? src.BookClub.Name : null)
            )
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count));
        CreateMap<Discussion, DiscussionDetailsDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(
                dest => dest.BookTitle,
                opt => opt.MapFrom(src => src.Book != null ? src.Book.Title : null)
            )
            .ForMember(
                dest => dest.BookClubName,
                opt => opt.MapFrom(src => src.BookClub != null ? src.BookClub.Name : null)
            );
        CreateMap<CreateDiscussionDto, Discussion>();

        // Comment mappings
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username));
        CreateMap<CreateCommentDto, Comment>();
    }

    private static int CalculateGoalProgress(ReadingGoal goal)
    {
        // This would need to be implemented based on the goal type and user's reading history
        // For example, for BooksPerMonth, count completed books in the date range
        return 0; // Placeholder
    }
}
