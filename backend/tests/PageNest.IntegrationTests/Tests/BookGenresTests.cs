using Microsoft.Extensions.DependencyInjection;
using PageNest.Infrastructure.Data.Context;
using PageNest.IntegrationTests.Helpers;
using PageNest.TestUtils.Builders;
using System.Net;

namespace PageNest.IntegrationTests.Tests;

public class BookGenresTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private const string _baseURL = "/api/v1/bookgenres";

    public BookGenresTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task GetBookGenres_ShouldReturnOkResult_WithAllBookGenres()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var bookGenres = BookGenresBuilder.CreateBookGenres();
        context.BookGenres.AddRange(bookGenres);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync(_baseURL);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBookGenresByBookId_ShouldReturnOkResult_WithAllBookGenres_WhenBookExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var bookGenres = BookGenresBuilder.CreateBookGenres();
        context.BookGenres.AddRange(bookGenres);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/book/{bookGenres[0].BookId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBookGenresByGenreId_ShouldReturnOkResult_WithAllBookGenres_WhenGenreExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var bookGenres = BookGenresBuilder.CreateBookGenres();
        context.BookGenres.AddRange(bookGenres);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/genre/{bookGenres[0].GenreId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
