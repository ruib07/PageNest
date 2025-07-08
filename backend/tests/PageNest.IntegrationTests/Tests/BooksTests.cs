using Microsoft.Extensions.DependencyInjection;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.IntegrationTests.Helpers;
using PageNest.TestUtils.Builders;
using System.Net;
using System.Net.Http.Json;

namespace PageNest.IntegrationTests.Tests;

public class BooksTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private const string _baseURL = "/api/v1/books";

    public BooksTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
        _serviceProvider = _factory.Services;
    }

    public async Task InitializeAsync()
    {
        await AuthHelper.AuthenticateUser(_httpClient, _serviceProvider);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetBooks_ShouldReturnOkResult_WithAllBooks()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var books = BooksBuilder.CreateBooks();
        context.Books.AddRange(books);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync(_baseURL);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBooksByAuthor_ShouldReturnOkResult_WithAllBooks_WhenAuthorExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var books = BooksBuilder.CreateBooks();
        context.Books.AddRange(books);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/author/{books[0].Author}");
        var responseContent = await response.Content.ReadFromJsonAsync<IEnumerable<Book>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.All(responseContent, b => Assert.Equal(books[0].Author, b.Author));
    }

    [Fact]
    public async Task GetBookById_ShouldReturnOkResult_WithBook()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/{book.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<Book>();

        Assert.NotNull(responseContent);
        Assert.Equal(book.Id, responseContent.Id);
    }

    [Fact]
    public async Task GetBookById_ShouldReturnNotFoundResult_WhenBookDoesNotExist()
    {
        var response = await _httpClient.GetAsync($"{_baseURL}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Book not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task GetBookByTitle_ShouldReturnOkResult_WithBook()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/title/{book.Title}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<Book>();

        Assert.NotNull(responseContent);
        Assert.Equal(book.Title, responseContent.Title);
    }

    [Fact]
    public async Task GetBookByTitle_ShouldReturnNotFoundResult_WhenBookDoesNotExist()
    {
        var bookTitle = "Non Existent Book";
        var response = await _httpClient.GetAsync($"{_baseURL}/title/{bookTitle}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Book not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnCreatedResult_WhenBookIsCreated()
    {
        var newBook = BooksBuilder.CreateBooks().First();

        var response = await _httpClient.PostAsJsonAsync(_baseURL, newBook);
        var content = await response.Content.ReadFromJsonAsync<ResponsesDTO.Creation>();

        Assert.NotNull(content);
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("Book created successfully.", content.Message);
            Assert.Equal(newBook.Id, content.Id);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation(string.Empty, "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, book);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Title cannot be empty.", error.Message);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenAuthorIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", string.Empty, "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, book);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Author cannot be empty.", error.Message);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenDescriptionIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", string.Empty, DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, book);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Description cannot be empty.", error.Message);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPublishedDateIsMinValue()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.MinValue, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, book);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Published date is required.", error.Message);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenISBNIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, string.Empty,
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, book);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("ISBN cannot be empty.", error.Message);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPageCountIsNegative()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        -1, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, book);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Page count must be greater than zero.", error.Message);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPageCountIsZero()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        0, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, book);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Page count must be greater than zero.", error.Message);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenLanguageIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.Empty, "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, book);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Language cannot be empty.", error.Message);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenCoverImageURLIsEmpty()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), string.Empty, 12, 19.99m);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, book);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Cover Image URL cannot be empty.", error.Message);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenStockIsNegative()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", -1, 19.99m);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, book);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Stock cannot be negative.", error.Message);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPriceIsNegative()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, -19.99m);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, book);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Price must be greater than zero.", error.Message);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnBadRequest_WhenPriceIsZero()
    {
        var book = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                        120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 00.00m);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, book);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Price must be greater than zero.", error.Message);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnOkResult_WhenBookIsUpdated()
    {
        var book = BooksBuilder.CreateBooks().First();

        var createdResponse = await _httpClient.PostAsJsonAsync(_baseURL, book);
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var createdBook = await createdResponse.Content.ReadFromJsonAsync<Book>();

        var updateBook = BooksBuilder.UpdateBook(createdBook.Id, book.CategoryId, book.LanguageId);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{createdBook.Id}", updateBook);
        var responseMessage = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Book updated successfully.", responseMessage);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation(string.Empty, "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{book.Id}", invalidUpdatedBook);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Title cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenAuthorIsEmpty()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", string.Empty, "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{book.Id}", invalidUpdatedBook);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Author cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenDescriptionIsEmpty()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", string.Empty, DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{book.Id}", invalidUpdatedBook);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Description cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPublishedDateIsMinValue()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.MinValue, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{book.Id}", invalidUpdatedBook);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Published date is required.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenISBNIsEmpty()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, string.Empty,
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{book.Id}", invalidUpdatedBook);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("ISBN cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPageCountIsNegative()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    -1, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{book.Id}", invalidUpdatedBook);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Page count must be greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPageCountIsZero()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    0, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{book.Id}", invalidUpdatedBook);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Page count must be greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenLanguageIsEmpty()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.Empty, "https://example.com/cover.jpg", 12, 19.99m);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{book.Id}", invalidUpdatedBook);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Language cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenCoverImageURLIsEmpty()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), string.Empty, 12, 19.99m);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{book.Id}", invalidUpdatedBook);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Cover Image URL cannot be empty.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenStockIsNegative()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", -1, 19.99m);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{book.Id}", invalidUpdatedBook);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Stock cannot be negative.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPriceIsNegative()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, -19.99m);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{book.Id}", invalidUpdatedBook);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Price must be greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ShouldReturnBadRequest_WhenPriceIsZero()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();
        var invalidUpdatedBook = BooksBuilder.InvalidBookCreation("Book Title", "Author", "Book Description", DateTime.UtcNow, "123456789",
                                                                    120, Guid.NewGuid(), "https://example.com/cover.jpg", 12, 00.00m);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{book.Id}", invalidUpdatedBook);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Price must be greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ShouldReturnNoContentResult_WhenBookIsDeleted()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var book = BooksBuilder.CreateBooks().First();
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();

        var response = await _httpClient.DeleteAsync($"{_baseURL}/{book.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
