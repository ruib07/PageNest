using Microsoft.EntityFrameworkCore;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Data.Repositories;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Repositories;

public class ReviewRepositoryTests : TestBase
{
    private readonly ReviewRepository _reviewRepository;

    public ReviewRepositoryTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _reviewRepository = new ReviewRepository(_context);
    }

    [Fact]
    public async Task GetReviews_ShouldReturnAllReviews()
    {
        var reviews = ReviewsBuilder.CreateReviews();
        await _context.Reviews.AddRangeAsync(reviews);
        await _context.SaveChangesAsync();

        var result = await _reviewRepository.GetReviews();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(reviews.Count, result.Count());
            Assert.Equal(reviews.First().Id, result.First().Id);
            Assert.Equal(reviews.First().BookId, result.First().BookId);
            Assert.Equal(reviews.Last().Id, result.Last().Id);
            Assert.Equal(reviews.Last().BookId, result.Last().BookId);
        });
    }

    [Fact]
    public async Task GetReviewsByBookId_ShouldReturnReviews_WhenBookExists()
    {
        var reviews = ReviewsBuilder.CreateReviews();
        await _context.Reviews.AddRangeAsync(reviews);
        await _context.SaveChangesAsync();

        var result = await _reviewRepository.GetReviewsByBookId(reviews.First().BookId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(reviews.First().BookId, result.First().BookId);
    }

    [Fact]
    public async Task GetReviewsByUserId_ShouldReturnReviews_WhenUserExists()
    {
        var reviews = ReviewsBuilder.CreateReviews();
        await _context.Reviews.AddRangeAsync(reviews);
        await _context.SaveChangesAsync();

        var result = await _reviewRepository.GetReviewsByUserId(reviews.First().UserId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(reviews.First().UserId, result.First().UserId);
    }

    [Fact]
    public async Task GetReviewById_ShouldReturnReview_WhenReviewExists()
    {
        var review = ReviewsBuilder.CreateReviews().First();
        await _reviewRepository.CreateReview(review);

        var result = await _reviewRepository.GetReviewById(review.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(review.Id, result.Id);
            Assert.Equal(review.BookId, result.BookId);
            Assert.Equal(review.UserId, result.UserId);
            Assert.Equal(review.Rating, result.Rating);
            Assert.Equal(review.Comment, result.Comment);
        });
    }

    [Fact]
    public async Task CreateReview_ShouldCreateReview()
    {
        var newReview = ReviewsBuilder.CreateReviews().First();
        await _reviewRepository.CreateReview(newReview);

        var result = await _reviewRepository.GetReviewById(newReview.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(newReview.Id, result.Id);
            Assert.Equal(newReview.BookId, result.BookId);
            Assert.Equal(newReview.UserId, result.UserId);
            Assert.Equal(newReview.Rating, result.Rating);
            Assert.Equal(newReview.Comment, result.Comment);
        });
    }

    [Fact]
    public async Task DeleteReview_ShouldDeleteReview()
    {
        var review = ReviewsBuilder.CreateReviews().First();

        await _reviewRepository.CreateReview(review);
        await _reviewRepository.DeleteReview(review.Id);

        var result = await _reviewRepository.GetReviewById(review.Id);

        Assert.Null(result);
    }
}
