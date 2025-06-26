using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Services;

public class ReviewsService : IReviewsService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewsService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<IEnumerable<Review>> GetReviews()
    {
        return await _reviewRepository.GetReviews();
    }

    public async Task<IEnumerable<Review>> GetReviewsByBookId(Guid bookId)
    {
        return await _reviewRepository.GetReviewsByBookId(bookId);
    }

    public async Task<IEnumerable<Review>> GetReviewsByUserId(Guid userId)
    {
        return await _reviewRepository.GetReviewsByUserId(userId);
    }

    public async Task<Result<Review>> GetReviewById(Guid reviewId)
    {
        var review = await _reviewRepository.GetReviewById(reviewId);

        if (review == null) return Result<Review>.Fail("Review not found.", 404);

        return Result<Review>.Success(review);
    }

    public async Task<Result<Review>> CreateReview(Review review)
    {
        var validation = ValidateReviewFields(review);

        if (!validation.IsSuccess) 
            return Result<Review>.Fail(validation.Error.Message, validation.Error.StatusCode);

        var createdReview = await _reviewRepository.CreateReview(review);

        return Result<Review>.Success(createdReview, "Review created successfully.");
    }

    public async Task DeleteReview(Guid reviewId)
    {
        await _reviewRepository.DeleteReview(reviewId);
    }

    #region Private Methods

    private static Result<bool> ValidateReviewFields(Review review)
    {
        if (review.Rating < 1 || review.Rating > 5)
            return Result<bool>.Fail("Rating must be between 1 and 5.", 400);

        if (string.IsNullOrWhiteSpace(review.Comment))
            return Result<bool>.Fail("Comment cannot be empty.", 400);

        return Result<bool>.Success(true);
    }

    #endregion
}
