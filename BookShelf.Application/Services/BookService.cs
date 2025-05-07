using AutoMapper;
using BookShelf.Application.DTOs;
using BookShelf.Application.Interfaces;
using BookShelf.Domain.Entities;
using BookShelf.Domain.Interfaces;

namespace BookShelf.Application.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public BookService(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<BookDto> AddBookAsync(CreateBookDto bookDto)
    {
        // Convert PublishedDate to UTC
        var book = _mapper.Map<Book>(bookDto);
        book.PublishedDate = DateTime.SpecifyKind(bookDto.PublishedDate, DateTimeKind.Utc);

        var addedBook = await _bookRepository.AddAsync(book);
        await _bookRepository.SaveChangesAsync();
        return _mapper.Map<BookDto>(addedBook);
    }

    public async Task DeleteBookAsync(Guid id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null)
        {
            throw new KeyNotFoundException($"Book with ID {id} not found");
        }

        await _bookRepository.DeleteAsync(book);
        await _bookRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<BookDto>>(books);
    }

    public async Task<BookDto?> GetBookByIdAsync(Guid id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        return book != null ? _mapper.Map<BookDto>(book) : null;
    }

    public async Task<IEnumerable<ReviewDto>> GetBookReviewsAsync(Guid bookId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book == null)
        {
            throw new KeyNotFoundException($"Book with ID {bookId} not found");
        }

        return _mapper.Map<IEnumerable<ReviewDto>>(book.Reviews);
    }

    public async Task<IEnumerable<BookDto>> GetBooksByUserIdAsync(Guid userId)
    {
        var books = await _bookRepository.GetBooksByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<BookDto>>(books);
    }

    public async Task<IEnumerable<BookDto>> GetRecommendationsAsync(Guid userId, int count = 10)
    {
        var recommendations = await _bookRepository.GetRecommendationsAsync(userId, count);
        return _mapper.Map<IEnumerable<BookDto>>(recommendations);
    }

    public async Task<IEnumerable<BookDto>> SearchBooksAsync(string searchTerm)
    {
        var titleResults = await _bookRepository.GetByTitleAsync(searchTerm);
        var authorResults = await _bookRepository.GetByAuthorAsync(searchTerm);
        var isbnResults = await _bookRepository.GetByISBNAsync(searchTerm);

        var combinedResults = titleResults
            .Concat(authorResults)
            .Concat(isbnResults)
            .DistinctBy(b => b.Id);

        return _mapper.Map<IEnumerable<BookDto>>(combinedResults);
    }

    public async Task UpdateBookAsync(UpdateBookDto bookDto)
    {
        var existingBook = await _bookRepository.GetByIdAsync(bookDto.Id);
        if (existingBook == null)
        {
            throw new KeyNotFoundException($"Book with ID {bookDto.Id} not found");
        }

        _mapper.Map(bookDto, existingBook);
        // Ensure PublishedDate is in UTC
        existingBook.PublishedDate = DateTime.SpecifyKind(bookDto.PublishedDate, DateTimeKind.Utc);

        await _bookRepository.UpdateAsync(existingBook);
        await _bookRepository.SaveChangesAsync();
    }
}
