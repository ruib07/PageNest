using Microsoft.Extensions.DependencyInjection;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.IntegrationTests.Helpers;
using PageNest.TestUtils.Builders;
using System.Net;
using System.Net.Http.Json;

namespace PageNest.IntegrationTests.Tests;

public class ReviewsTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private const string _baseURL = "/api/v1/reviews";

    public ReviewsTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task GetReviews_ShouldReturnOkResult_WithAllReviews()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var reviews = ReviewsBuilder.CreateReviews();
        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync(_baseURL);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetReviewsByUserId_ShouldReturnOkResult_WithAllReviews_WhenUserExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var reviews = ReviewsBuilder.CreateReviews();
        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/user/{reviews[0].UserId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetReviewsByBookId_ShouldReturnOkResult_WithAllReviews_WhenBookExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var reviews = ReviewsBuilder.CreateReviews();
        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/book/{reviews[0].BookId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetReviewById_ShouldReturnOkResult_WithReview()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var review = ReviewsBuilder.CreateReviews().First();
        await context.Reviews.AddAsync(review);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/{review.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<Review>();

        Assert.NotNull(responseContent);
        Assert.Equal(review.Id, responseContent.Id);
    }

    [Fact]
    public async Task GetReviewById_ShouldReturnNotFoundResult_WhenReviewDoesNotExist()
    {
        var response = await _httpClient.GetAsync($"{_baseURL}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Review not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task CreateReview_ShouldReturnCreatedResult_WhenReviewIsCreated()
    {
        var newReview = ReviewsBuilder.CreateReviews().First();

        var response = await _httpClient.PostAsJsonAsync(_baseURL, newReview);
        var content = await response.Content.ReadFromJsonAsync<ResponsesDTO.Creation>();

        Assert.NotNull(content);
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("Review created successfully.", content.Message);
            Assert.Equal(newReview.Id, content.Id);
        });
    }

    [Theory]
    [InlineData(0, "Good book")]
    [InlineData(6, "Excellent")]
    [InlineData(3, "")]
    [InlineData(4, null)]
    public async Task CreateReview_ShouldFailValidation_WhenInvalid(int rating, string comment)
    {
        var invalidReview = ReviewsBuilder.InvalidReviewCreation(rating, comment);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, invalidReview);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task DeleteReview_ShouldReturnNoContentResult_WhenReviewIsDeleted()
    {
        var review = ReviewsBuilder.CreateReviews().First();

        var createdResponse = await _httpClient.PostAsJsonAsync(_baseURL, review);
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var createdReview = await createdResponse.Content.ReadFromJsonAsync<Review>();

        var response = await _httpClient.DeleteAsync($"{_baseURL}/{createdReview.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
