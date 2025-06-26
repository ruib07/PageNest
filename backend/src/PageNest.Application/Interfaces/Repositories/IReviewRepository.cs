using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<IEnumerable<Review>> GetReviews();
    Task<IEnumerable<Review>> GetReviewsByUserId(Guid userId);
    Task<IEnumerable<Review>> GetReviewsByBookId(Guid bookId);
    Task<Review> GetReviewById(Guid reviewId);
    Task<Review> CreateReview(Review review);
    Task DeleteReview(Guid reviewId);
}
