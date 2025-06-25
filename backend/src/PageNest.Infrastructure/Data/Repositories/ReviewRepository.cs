using Microsoft.EntityFrameworkCore;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.Infrastructure.Data.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _context;
    private DbSet<Review> Reviews => _context.Reviews;

    public ReviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Review>> GetReviews()
    {
        return await Reviews.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsByBookId(Guid bookId)
    {
        return await Reviews.AsNoTracking().Where(r => r.BookId == bookId).ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsByUserId(Guid userId)
    {
        return await Reviews.AsNoTracking().Where(r => r.UserId == userId).ToListAsync();
    }

    public async Task<Review> GetReviewById(Guid reviewId)
    {
        return await Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
    }

    public async Task<Review> CreateReview(Review review)
    {
        await Reviews.AddAsync(review);
        await _context.SaveChangesAsync();

        return review;
    }

    public async Task UpdateReview(Review review)
    {
        Reviews.Update(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteReview(Guid reviewId)
    {
        var review = await GetReviewById(reviewId);

        Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }
}
