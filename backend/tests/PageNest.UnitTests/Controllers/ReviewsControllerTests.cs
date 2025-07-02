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

public class ReviewsControllerTests : TestBase
{
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly ReviewsService _reviewsService;
    private readonly ReviewsController _reviewsController;

    public ReviewsControllerTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _reviewRepositoryMock = new Mock<IReviewRepository>();
        _reviewsService = new ReviewsService(_reviewRepositoryMock.Object);
        _reviewsController = new ReviewsController(_reviewsService);
    }

    [Fact]
    public async Task GetReviews_ShouldReturnOkResult_WithAllReviews()
    {
        var reviews = ReviewsBuilder.CreateReviews();

        _reviewRepositoryMock.Setup(repo => repo.GetReviews()).ReturnsAsync(reviews);

        var result = await _reviewsController.GetReviews();
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<Review>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(reviews.Count, response.Count());
            Assert.Equal(reviews.First().Id, response.First().Id);
            Assert.Equal(reviews.Last().Id, response.Last().Id);
        });
    }

    [Fact]
    public async Task GetReviewsByUserId_ShouldReturnOkResult_WithAllReviews_WhenUserExists()
    {
        var reviews = ReviewsBuilder.CreateReviews();
        var reviewsByUserList = reviews.Where(r => r.UserId == reviews[0].UserId).ToList();

        _reviewRepositoryMock.Setup(repo => repo.GetReviewsByUserId(reviews[0].UserId))
                                                .ReturnsAsync(reviewsByUserList);

        var result = await _reviewsController.GetReviewsByUserId(reviews[0].UserId);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<Review>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(reviews[0].UserId, response.First().UserId);
        });
    }

    [Fact]
    public async Task GetReviewsByBookId_ShouldReturnOkResult_WithAllReviews_WhenBookExists()
    {
        var reviews = ReviewsBuilder.CreateReviews();
        var reviewsByBookList = reviews.Where(r => r.BookId == reviews[0].BookId).ToList();

        _reviewRepositoryMock.Setup(repo => repo.GetReviewsByBookId(reviews[0].BookId))
                                                .ReturnsAsync(reviewsByBookList);

        var result = await _reviewsController.GetReviewsByBookId(reviews[0].BookId);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<Review>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(reviews[0].BookId, response.First().BookId);
        });
    }

    [Fact]
    public async Task GetReviewById_ShouldReturnOkResult_WithReview()
    {
        var review = ReviewsBuilder.CreateReviews().First();

        _reviewRepositoryMock.Setup(repo => repo.GetReviewById(review.Id)).ReturnsAsync(review);

        var result = await _reviewsController.GetReviewById(review.Id);
        var okResult = result as OkObjectResult;
        var response = okResult.Value as Review;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(review.Id, response.Id);
            Assert.Equal(review.BookId, response.BookId);
            Assert.Equal(review.UserId, response.UserId);
            Assert.Equal(review.Rating, response.Rating);
            Assert.Equal(review.Comment, response.Comment);
        });
    }

    [Fact]
    public async Task GetReviewById_ShouldReturnNotFoundResult_WhenReviewDoesNotExist()
    {
        var result = await _reviewsController.GetReviewById(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Review not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task CreateReview_ShouldReturnCreatedResult_WhenReviewIsCreated()
    {
        var newReview = ReviewsBuilder.CreateReviews().First();

        _reviewRepositoryMock.Setup(repo => repo.CreateReview(newReview)).ReturnsAsync(newReview);

        var result = await _reviewsController.CreateReview(newReview);
        var createdResult = result as CreatedAtActionResult;
        var response = createdResult.Value as ResponsesDTO.Creation;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal("Review created successfully.", response.Message);
            Assert.Equal(newReview.Id, response.Id);
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

        _reviewRepositoryMock.Setup(repo => repo.CreateReview(invalidReview)).ReturnsAsync(invalidReview);

        var result = await _reviewsController.CreateReview(invalidReview);
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task DeleteReview_ShouldReturnNoContentResult_WhenReviewIsDeleted()
    {
        var review = ReviewsBuilder.CreateReviews().First();

        _reviewRepositoryMock.Setup(repo => repo.DeleteReview(review.Id)).Returns(Task.CompletedTask);

        var result = await _reviewsController.DeleteReview(review.Id);
        var noContentResult = result as NoContentResult;

        Assert.Equal(204, noContentResult.StatusCode);
    }
}
