using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Services;

public interface IReviewsService
{
    Task<IEnumerable<Review>> GetReviews();
    Task<IEnumerable<Review>> GetReviewsByUserId(Guid userId);
    Task<IEnumerable<Review>> GetReviewsByBookId(Guid bookId);
    Task<Result<Review>> GetReviewById(Guid reviewId);
    Task<Result<Review>> CreateReview(Review review);
    Task DeleteReview(Guid reviewId);
}
