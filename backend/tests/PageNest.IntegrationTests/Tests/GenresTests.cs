using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.IntegrationTests.Helpers;
using PageNest.TestUtils.Builders;
using System.Net;
using System.Net.Http.Json;

namespace PageNest.IntegrationTests.Tests;

public class GenresTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private const string _baseURL = "/api/v1/genres";

    public GenresTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task GetGenres_ShouldReturnOkResult_WithAllGenres()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var genres = GenresBuilder.CreateGenres();
        context.Genres.AddRange(genres);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync(_baseURL);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBooksByGenreId_ShouldReturnOkResult_WithAllBooksByGenre()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var genres = GenresBuilder.CreateGenres();
        await context.Genres.AddRangeAsync(genres);

        var booksByGenreList = new List<Book>()
        {
            new Book()
            {
                Id = Guid.NewGuid(),
                Title = "Test Book",
                Author = "Test Author",
                Description = "Test Description",
                PublishedDate = DateTime.UtcNow,
                ISBN = "1234567890",
                PageCount = 100,
                LanguageId = Guid.NewGuid(),
                CoverImageUrl = "http://example.com/cover.jpg",
                Stock = 10,
                Price = 9.99m,
                CategoryId = genres.First().Id,
                BookGenres = new List<BookGenre>()
            }
        };

        var bookGenre = new BookGenre()
        {
            BookId = booksByGenreList.First().Id,
            GenreId = genres.First().Id
        };

        booksByGenreList.First().BookGenres.Add(bookGenre);
        await context.Books.AddRangeAsync(booksByGenreList);
        await context.BookGenres.AddAsync(bookGenre);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/books/{genres[0].Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetGenreById_ShouldReturnOkResult_WithGenre()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var genre = GenresBuilder.CreateGenres().First();
        await context.Genres.AddAsync(genre);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/{genre.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<Genre>();

        Assert.NotNull(responseContent);
        Assert.Equal(genre.Id, responseContent.Id);
    }

    [Fact]
    public async Task GetGenreById_ShouldReturnNotFoundResult_WhenGenreDoesNotExist()
    {
        var response = await _httpClient.GetAsync($"{_baseURL}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Genre not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task GetGenreByName_ShouldReturnOkResult_WithGenre()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var genre = GenresBuilder.CreateGenres().First();
        await context.Genres.AddAsync(genre);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/name/{genre.Name}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<Genre>();

        Assert.NotNull(responseContent);
        Assert.Equal(genre.Id, responseContent.Id);
    }

    [Fact]
    public async Task GetGenreByName_ShouldReturnNotFoundResult_WhenGenreDoesNotExist()
    {
        var response = await _httpClient.GetAsync($"{_baseURL}/name/non-existent-genre");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Genre not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }
}
