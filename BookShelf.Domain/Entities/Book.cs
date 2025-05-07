namespace BookShelf.Domain.Entities;

public class Book
{
    private ICollection<BookShelf> bookshelves = new List<BookShelf>();

    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public DateTime PublishedDate { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<BookShelf> Bookshelves
    {
        get => bookshelves;
        set => bookshelves = value;
    }
    public ICollection<ReadingProgress> ReadingProgresses { get; set; } =
        new List<ReadingProgress>();
}
