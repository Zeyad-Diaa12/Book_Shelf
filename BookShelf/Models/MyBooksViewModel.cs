using BookShelf.Application.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace BookShelf.Models
{
    public class MyBooksViewModel
    {
        public List<BookDto> Books { get; set; } = new List<BookDto>();
        public List<ReadingProgressDto> ReadingProgress { get; set; } = new List<ReadingProgressDto>();

        // Helper method to get reading progress for a specific book
        public ReadingProgressDto? GetProgressForBook(BookDto book)
        {
            return ReadingProgress?.FirstOrDefault(rp => rp.BookId == book.Id);
        }

        // Helper method to check if a book is in the reading list
        public bool IsInReadingList(BookDto book)
        {
            return ReadingProgress?.Any(rp => rp.BookId == book.Id) == true;
        }
    }
}