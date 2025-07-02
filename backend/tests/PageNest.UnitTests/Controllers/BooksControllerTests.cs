using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.API.Controllers;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Controllers;

public class BooksControllerTests : TestBase
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly BooksService _booksService;
    private readonly BooksController _booksController;

    public BooksControllerTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _booksService = new BooksService(_bookRepositoryMock.Object);
        _booksController = new BooksController(_booksService);
    }

    [Fact]
    public async Task GetBooks_ShouldReturnOkResult_WithAllBooks()
    {
        var books = BooksBuilder.CreateBooks();

        _bookRepositoryMock.Setup(repo => repo.GetBooks()).ReturnsAsync(books);

        var result = await _booksController.GetBooks();
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<Book>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(books.Count, response.Count());
        });
    }

    [Fact]
    public async Task GetBooksByAuthor_ShouldReturnOkResult_WithListOfBooksByAuthor()
    {
        var books = BooksBuilder.CreateBooks();
        var booksByAuthorList = books.Where(b => b.Author == books[0].Author).ToList();

        _bookRepositoryMock.Setup(repo => repo.GetBooksByAuthor(books[0].Author))
                                              .ReturnsAsync(booksByAuthorList);

        var result = await _booksController.GetBooksByAuthor(books[0].Author);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<Book>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(books[0].Author, response.First().Author);
        });
    }

    [Fact]
    public async Task GetBookById_ShouldReturnOkResult_WithBook()
    {
        var book = BooksBuilder.CreateBooks().First();

        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);

        var result = await _booksController.GetBookById(book.Id);
        var okResult = result as OkObjectResult;
        var response = okResult.Value as Book;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(book.Id, response.Id);
            Assert.Equal(book.Title, response.Title);
            Assert.Equal(book.Author, response.Author);
            Assert.Equal(book.LanguageId, response.LanguageId);
            Assert.Equal(book.CategoryId, response.CategoryId);
        });
    }

    [Fact]
    public async Task GetBookById_ShouldReturnNotFoundResult_WhenBookDoesNotExist()
    {
        var result = await _booksController.GetBookById(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Book not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task GetBookByTitle_ShouldReturnOkResult_WithBook()
    {
        var book = BooksBuilder.CreateBooks().First();

        _bookRepositoryMock.Setup(repo => repo.GetBookByTitle(book.Title)).ReturnsAsync(book);

        var result = await _booksController.GetBookByTitle(book.Title);
        var okResult = result as OkObjectResult;
        var response = okResult.Value as Book;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(book.Id, response.Id);
            Assert.Equal(book.Title, response.Title);
        });
    }

    [Fact]
    public async Task GetBookByTitle_ShouldReturnNotFoundResult_WhenBookDoesNotExist()
    {
        var result = await _booksController.GetBookByTitle("Non Existent Book Title");

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Book not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnCreatedResult_WhenBookIsCreated()
    {
        var newBook = BooksBuilder.CreateBooks().First();

        _bookRepositoryMock.Setup(repo => repo.CreateBook(newBook)).ReturnsAsync(newBook);

        var result = await _booksController.CreateBook(newBook);
        var createdResult = result as CreatedAtActionResult;
        var response = createdResult.Value as ResponsesDTO.Creation;

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal("Book created successfully.", response.Message);
            Assert.Equal(newBook.Id, response.Id);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation(string.Empty, "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksController.CreateBook(book);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Title cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenAuthorIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", string.Empty, "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksController.CreateBook(book);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Author cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenDescriptionIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", string.Empty, DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksController.CreateBook(book);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Description cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPublishedDateIsMinValue()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.MinValue, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksController.CreateBook(book);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Published date is required.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenISBNIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, string.Empty,
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksController.CreateBook(book);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("ISBN cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPageCountIsNegative()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        -1, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksController.CreateBook(book);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Page count must be greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPageCountIsZero()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        0, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksController.CreateBook(book);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Page count must be greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenLanguageIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.Empty, "https://example.com/cover.jpg", 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksController.CreateBook(book);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Language cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenCoverImageURLIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), string.Empty, 12, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksController.CreateBook(book);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Cover Image URL cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenStockIsNegative()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", -1, 19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksController.CreateBook(book);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Stock cannot be negative.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPriceIsNegative()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, -19.99m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksController.CreateBook(book);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Price must be greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPriceIsZero()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 00.00m);

        _bookRepositoryMock.Setup(repo => repo.CreateBook(book)).ReturnsAsync(book);

        var result = await _booksController.CreateBook(book);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Price must be greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnOkResult_WhenBookIsUpdated()
    {
        var book = BooksBuilder.CreateBooks().First();
        var updatedBook = BooksBuilder.UpdateBook(book.Id, book.CategoryId, book.LanguageId);

        _bookRepositoryMock.Setup(repo => repo.GetBookById(book.Id)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(repo => repo.UpdateBook(It.IsAny<Book>())).Returns(Task.CompletedTask);

        var result = await _booksController.UpdateBook(book.Id, updatedBook);
        var okResult = result as OkObjectResult;

        Assert.NotNull(okResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("Book updated successfully.", okResult.Value);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation(string.Empty, "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var result = await _booksController.UpdateBook(book.Id, invalidUpdatedBook);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Title cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenAuthorIsEmpty()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", string.Empty, "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var result = await _booksController.UpdateBook(book.Id, invalidUpdatedBook);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Author cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenDescriptionIsEmpty()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", string.Empty, DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var result = await _booksController.UpdateBook(book.Id, invalidUpdatedBook);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Description cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPublishedDateIsMinValue()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.MinValue, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var result = await _booksController.UpdateBook(book.Id, invalidUpdatedBook);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Published date is required.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenISBNIsEmpty()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, string.Empty,
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var result = await _booksController.UpdateBook(book.Id, invalidUpdatedBook);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("ISBN cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPageCountIsNegative()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    -1, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var result = await _booksController.UpdateBook(book.Id, invalidUpdatedBook);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Page count must be greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPageCountIsZero()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    0, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var result = await _booksController.UpdateBook(book.Id, invalidUpdatedBook);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Page count must be greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenLanguageIsEmpty()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.Empty, "https://example.com/cover.jpg", 12, 19.99m);

        var result = await _booksController.UpdateBook(book.Id, invalidUpdatedBook);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Language cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenCoverImageURLIsEmpty()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), string.Empty, 12, 19.99m);

        var result = await _booksController.UpdateBook(book.Id, invalidUpdatedBook);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Cover Image URL cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenStockIsNegative()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", -1, 19.99m);

        var result = await _booksController.UpdateBook(book.Id, invalidUpdatedBook);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Stock cannot be negative.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPriceIsNegative()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, -19.99m);

        var result = await _booksController.UpdateBook(book.Id, invalidUpdatedBook);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Price must be greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPriceIsZero()
    {
        var book = BooksBuilder.CreateBooks().First();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 00.00m);

        var result = await _booksController.UpdateBook(book.Id, invalidUpdatedBook);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Price must be greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ShouldReturnNoContentResult_WhenBookIsDeleted()
    {
        var book = BooksBuilder.CreateBooks().First();

        _bookRepositoryMock.Setup(repo => repo.DeleteBook(book.Id)).Returns(Task.CompletedTask);

        var result = await _booksController.DeleteBook(book.Id);
        var noContentResult = result as NoContentResult;

        Assert.Equal(204, noContentResult.StatusCode);
    }
}
