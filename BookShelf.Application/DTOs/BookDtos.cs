namespace BookShelf.Application.DTOs;

public class BookDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public DateTime PublishedDate { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

public class CreateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public DateTime PublishedDate { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
}

public class UpdateBookDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public DateTime PublishedDate { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
}
