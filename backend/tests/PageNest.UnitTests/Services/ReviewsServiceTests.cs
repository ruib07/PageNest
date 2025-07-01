using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Services;

public class ReviewsServiceTests : TestBase
{
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly ReviewsService _reviewsService;

    public ReviewsServiceTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _reviewRepositoryMock = new Mock<IReviewRepository>();
        _reviewsService = new ReviewsService(_reviewRepositoryMock.Object);
    }

    [Fact]
    public async Task GetReviews_ShouldReturnAllReviews()
    {
        var expected = ReviewsBuilder.CreateReviews(3);
        _reviewRepositoryMock.Setup(r => r.GetReviews()).ReturnsAsync(expected);

        var result = await _reviewsService.GetReviews();

        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetReviewsByBookId_ShouldReturnReviewsForBook()
    {
        var bookId = Guid.NewGuid();
        var expected = ReviewsBuilder.CreateReviews().Select(r => { r.BookId = bookId; return r; }).ToList();

        _reviewRepositoryMock.Setup(r => r.GetReviewsByBookId(bookId)).ReturnsAsync(expected);

        var result = await _reviewsService.GetReviewsByBookId(bookId);

        Assert.All(result, r => Assert.Equal(bookId, r.BookId));
    }

    [Fact]
    public async Task GetReviewsByUserId_ShouldReturnReviewsForUser()
    {
        var userId = Guid.NewGuid();
        var expected = ReviewsBuilder.CreateReviews().Select(r => { r.UserId = userId; return r; }).ToList();

        _reviewRepositoryMock.Setup(r => r.GetReviewsByUserId(userId)).ReturnsAsync(expected);

        var result = await _reviewsService.GetReviewsByUserId(userId);

        Assert.All(result, r => Assert.Equal(userId, r.UserId));
    }

    [Fact]
    public async Task GetReviewById_ShouldReturnReview_WhenExists()
    {
        var review = ReviewsBuilder.CreateReviews().First();
        _reviewRepositoryMock.Setup(r => r.GetReviewById(review.Id)).ReturnsAsync(review);

        var result = await _reviewsService.GetReviewById(review.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(review.Id, result.Data.Id);
    }

    [Fact]
    public async Task GetReviewById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        _reviewRepositoryMock.Setup(r => r.GetReviewById(It.IsAny<Guid>())).ReturnsAsync((Review)null);

        var result = await _reviewsService.GetReviewById(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error.StatusCode);
    }

    [Fact]
    public async Task CreateReview_ShouldCreateSuccessfully_WhenValid()
    {
        var review = ReviewsBuilder.CreateReviews().First();

        _reviewRepositoryMock.Setup(r => r.CreateReview(It.IsAny<Review>()))
                             .ReturnsAsync((Review r) => r);

        var result = await _reviewsService.CreateReview(review);

        Assert.True(result.IsSuccess);
        Assert.Equal("Review created successfully.", result.Message);
    }

    [Theory]
    [InlineData(0, "Good book")]
    [InlineData(6, "Excellent")]
    [InlineData(3, "")]
    [InlineData(4, null)]
    public async Task CreateReview_ShouldFailValidation_WhenInvalid(int rating, string comment)
    {
        var invalidReview = ReviewsBuilder.InvalidReviewCreation(rating, comment);

        var result = await _reviewsService.CreateReview(invalidReview);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Error.StatusCode);
    }

    [Fact]
    public async Task DeleteReview_ShouldCallRepositoryDelete()
    {
        var reviewId = Guid.NewGuid();

        await _reviewsService.DeleteReview(reviewId);

        _reviewRepositoryMock.Verify(r => r.DeleteReview(reviewId), Times.Once);
    }
}
