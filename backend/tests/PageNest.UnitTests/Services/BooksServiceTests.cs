using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Services;

public class BooksServiceTests : TestBase
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly BooksService _booksService;

    public BooksServiceTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _booksService = new BooksService(_bookRepositoryMock.Object);
    }

    [Fact]
    public async Task GetBooks_ShouldReturnAllBooks()
    {
        var books = BooksBuilder.CreateBooks();
        await _context.Books.AddRangeAsync(books);
        await _context.SaveChangesAsync();

        _bookRepositoryMock.Setup(repo => repo.GetBooks()).ReturnsAsync(books);

        var result = await _booksService.GetBooks();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(books.Count, result.Count());
            Assert.Equal(books.First().Id, result.First().Id);
            Assert.Equal(books.Last().Id, result.Last().Id);
        });
    }

    [Fact]
    public async Task GetBooksByAuthor_ShouldReturnAllBooks_WhenAuthorNameExists()
    {
        var books = BooksBuilder.CreateBooks();
        var booksByAuthorList = books.Where(b => b.Author == books[0].Author).ToList();

        _bookRepositoryMock.Setup(repo => repo.GetBooksByAuthor(books[0].Author)).ReturnsAsync(booksByAuthorList);

        var result = await _booksService.GetBooksByAuthor(books[0].Author);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Single(result);
            Assert.Equal(books.First().Id, result.First().Id);
        });
    }

    [Fact]
    public async Task GetBookById_ShouldReturnBook_WhenBookExists()
    {
        var book = BooksBuilder.CreateBooks().First();

        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksService.GetBookById(book.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(book.Id, result.Data.Id);
            Assert.Equal(book.Title, result.Data.Title);
            Assert.Equal(book.Author, result.Data.Author);
            Assert.Equal(book.Description, result.Data.Description);
            Assert.Equal(book.PublishedDate, result.Data.PublishedDate);
            Assert.Equal(book.ISBN, result.Data.ISBN);
            Assert.Equal(book.PageCount, result.Data.PageCount);
            Assert.Equal(book.Language, result.Data.Language);
            Assert.Equal(book.CoverImageUrl, result.Data.CoverImageUrl);
            Assert.Equal(book.Stock, result.Data.Stock);
            Assert.Equal(book.Price, result.Data.Price);
            Assert.Equal(book.CategoryId, result.Data.CategoryId);
        });
    }

    [Fact]
    public async Task GetBookById_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        _bookRepositoryMock.Setup(repo => repo.GetBookById(It.IsAny<Guid>())).ReturnsAsync((Book)null);

        var result = await _booksService.GetBookById(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("Book not found.", result.Error.Message);
        });
    }

    [Fact]
    public async Task GetBookByTitle_ShouldReturnBook_WhenBookTitleExists()
    {
        var book = BooksBuilder.CreateBooks().First();

        _bookRepositoryMock.Setup(repo => repo.GetBookByTitle(book.Title)).ReturnsAsync(book);

        var result = await _booksService.GetBookByTitle(book.Title);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(book.Id, result.Data.Id);
            Assert.Equal(book.Title, result.Data.Title);
        });
    }

    [Fact]
    public async Task GetBookByTitle_ShouldReturnNotFound_WhenBookTitleDoesNotExist()
    {
        _bookRepositoryMock.Setup(repo => repo.GetBookByTitle("Non Existent Book Title")).ReturnsAsync((Book)null);

        var result = await _booksService.GetBookByTitle("Non Existent Book Title");

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("Book not found.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldCreateBookSuccessfully()
    {
        var book = BooksBuilder.CreateBooks().First();

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksService.CreateBook(book);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(book.Id, result.Data.Id);
            Assert.Equal(book.Title, result.Data.Title);
            Assert.Equal("Book created successfully.", result.Message);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation(string.Empty, "Author", "Book Description", DateTime.UtcNow, "123456789", 
                                                        120, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksService.CreateBook(book);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Title cannot be empty.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenAuthorIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", string.Empty, "Book Description", DateTime.UtcNow, "123456789", 
                                                        120, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksService.CreateBook(book);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Author cannot be empty.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenDescriptionIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", string.Empty, DateTime.UtcNow, "123456789", 
                                                        120, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksService.CreateBook(book);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Description cannot be empty.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPublishedDateIsMinValue()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.MinValue, "123456789",
                                                        120, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksService.CreateBook(book);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Published date is required.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenISBNIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, string.Empty,
                                                        120, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksService.CreateBook(book);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("ISBN cannot be empty.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPageCountIsNegative()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        -1, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksService.CreateBook(book);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Page count must be greater than zero.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPageCountIsZero()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        0, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksService.CreateBook(book);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Page count must be greater than zero.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenLanguageIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, string.Empty, "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksService.CreateBook(book);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Language cannot be empty.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenCoverImageURLIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, "PT", string.Empty, 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksService.CreateBook(book);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Cover Image URL cannot be empty.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenStockIsNegative()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, "PT", "https://example.com/cover.jpg", -1, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksService.CreateBook(book);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Stock cannot be negative.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPriceIsNegative()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, "PT", "https://example.com/cover.jpg", 12, -19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksService.CreateBook(book);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Price must be greater than zero.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPriceIsZero()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, "PT", "https://example.com/cover.jpg", 12, 00.00m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksService.CreateBook(book);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Price must be greater than zero.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldUpdateBookSuccessfully()
    {
        var book = BooksBuilder.CreateBooks().First();
        var updatedBook = BooksBuilder.UpdateBook(book.Id, book.CategoryId);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(updatedBook)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var updatedResult = await _booksService.UpdateBook(book.Id, updatedBook);
        var result = await _booksService.GetBookById(book.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(updatedBook.Id, result.Data.Id);
            Assert.Equal(updatedBook.Title, result.Data.Title);
            Assert.Equal("Book updated successfully.", updatedResult.Message);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation(string.Empty, "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(invalidUpdatedBook)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksService.UpdateBook(book.Id, invalidUpdatedBook);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Title cannot be empty.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenAuthorIsEmpty()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", string.Empty, "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(invalidUpdatedBook)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksService.UpdateBook(book.Id, invalidUpdatedBook);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Author cannot be empty.", result.Error.Message);
        });
    }
    
    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenDescriptionIsEmpty()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", string.Empty, DateTime.UtcNow, "123456789",
                                                                    120, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(invalidUpdatedBook)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksService.UpdateBook(book.Id, invalidUpdatedBook);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Description cannot be empty.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPublishedDateIsMinValue()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.MinValue, "123456789",
                                                                    120, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(invalidUpdatedBook)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksService.UpdateBook(book.Id, invalidUpdatedBook);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Published date is required.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenISBNIsEmpty()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, string.Empty,
                                                                    120, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(invalidUpdatedBook)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksService.UpdateBook(book.Id, invalidUpdatedBook);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("ISBN cannot be empty.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPageCountIsNegative()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    -1, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(invalidUpdatedBook)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksService.UpdateBook(book.Id, invalidUpdatedBook);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Page count must be greater than zero.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPageCountIsZero()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    0, "PT", "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(invalidUpdatedBook)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksService.UpdateBook(book.Id, invalidUpdatedBook);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Page count must be greater than zero.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenLanguageIsEmpty()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, string.Empty, "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(invalidUpdatedBook)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksService.UpdateBook(book.Id, invalidUpdatedBook);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Language cannot be empty.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenCoverImageURLIsEmpty()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, "PT", string.Empty, 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(invalidUpdatedBook)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksService.UpdateBook(book.Id, invalidUpdatedBook);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Cover Image URL cannot be empty.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenStockIsNegative()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, "PT", "https://example.com/cover.jpg", -1, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(invalidUpdatedBook)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksService.UpdateBook(book.Id, invalidUpdatedBook);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Stock cannot be negative.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPriceIsNegative()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, "PT", "https://example.com/cover.jpg", 12, -19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(invalidUpdatedBook)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksService.UpdateBook(book.Id, invalidUpdatedBook);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Price must be greater than zero.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPriceIsZero()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, "PT", "https://example.com/cover.jpg", 12, 00.00m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(invalidUpdatedBook)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksService.UpdateBook(book.Id, invalidUpdatedBook);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Price must be greater than zero.", result.Error.Message);
        });
    }

    [Fact]
    public async Task DeleteBook_ShouldDeleteBookSuccessfully()
    {
        var book = BooksBuilder.CreateBooks().First();

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.DeleteBook(book.Id)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync((Book)null);

        await _booksService.CreateBook(book);
        await _booksService.DeleteBook(book.Id);

        var result = await _booksService.GetBookById(book.Id);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("Book not found.", result.Error.Message);
        });
    }
}
