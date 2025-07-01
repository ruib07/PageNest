using PageNest.Domain.Entities;

namespace PageNest.TestUtils.Builders;

public class BooksBuilder
{
    private static int _counter = 2;

    public static List<Book> CreateBooks(int quantity = 2)
    {
        var books = new List<Book>();

        for (int i = 0; i < quantity; i++)
        {
            books.Add(new Book()
            {
                Id = Guid.NewGuid(),
                Title = $"Book {_counter}",
                Author = $"Author {_counter}",
                Description = $"Description for book {_counter}",
                PublishedDate = DateTime.UtcNow,
                ISBN = $"ISBN-{_counter}",
                PageCount = 100 + _counter,
                Language = "PT",
                CoverImageUrl = $"https://example.com/cover{_counter}.jpg",
                Stock = 10 + _counter,
                Price = 19.99m + _counter,
                CategoryId = Guid.NewGuid()
            });

            _counter++;
        }

        return books;
    }

    public static Book InvalidBookCreation(string title, string author, string description, DateTime publishedDate, string isbn, int pageCount, 
                                                string language, string coverImageUrl, int stock, decimal price)
    {
        return new Book()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Author = author,
            Description = description,
            PublishedDate = publishedDate,
            ISBN = isbn,
            PageCount = pageCount,
            Language = language,
            CoverImageUrl = coverImageUrl,
            Stock = stock,
            Price = price,
            CategoryId = Guid.NewGuid()
        };
    }

    public static Book UpdateBook(Guid id, Guid categoryId)
    {
        return new Book()
        {
            Id = id,
            Title = $"Updated Book {_counter}",
            Author = $"Updated Author {_counter}",
            Description = $"Updated description for book {_counter}",
            PublishedDate = DateTime.UtcNow,
            ISBN = $"Updated-ISBN-{_counter}",
            PageCount = 150 + _counter,
            Language = "EN",
            CoverImageUrl = $"https://example.com/updated-cover{_counter}.jpg",
            Stock = 20 + _counter,
            Price = 29.99m + _counter,
            CategoryId = categoryId
        };
    }
}
