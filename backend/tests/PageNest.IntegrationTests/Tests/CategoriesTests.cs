using Microsoft.Extensions.DependencyInjection;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.IntegrationTests.Helpers;
using PageNest.TestUtils.Builders;
using System.Net;
using System.Net.Http.Json;

namespace PageNest.IntegrationTests.Tests;

public class CategoriesTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private const string _baseURL = "/api/v1/categories";

    public CategoriesTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task GetCategories_ShouldReturnOkResult_WithAllCategories()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var categories = CategoriesBuilder.CreateCategories();
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync(_baseURL);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBooksByCategoryId_ShouldReturnOkResult_WithAllBooksByCategory()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var categories = CategoriesBuilder.CreateCategories();
        var booksByCategoryList = new List<Book>()
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
                CategoryId = categories.First().Id
            }
        };

        await context.Books.AddRangeAsync(booksByCategoryList);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/books/{categories[0].Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnOkResult_WithCategory()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = CategoriesBuilder.CreateCategories().First();
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/{category.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<Category>();

        Assert.NotNull(responseContent);
        Assert.Equal(category.Id, responseContent.Id);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnNotFoundResult_WhenCategoryDoesNotExist()
    {
        var response = await _httpClient.GetAsync($"{_baseURL}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Category not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task GetCategoryByName_ShouldReturnOkResult_WithCategory()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = CategoriesBuilder.CreateCategories().First();
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/name/{category.Name}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<Category>();

        Assert.NotNull(responseContent);
        Assert.Equal(category.Id, responseContent.Id);
    }

    [Fact]
    public async Task GetCategoryByName_ShouldReturnNotFoundResult_WhenCategoryDoesNotExist()
    {
        var response = await _httpClient.GetAsync($"{_baseURL}/name/non-existent-category");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Category not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }
}
