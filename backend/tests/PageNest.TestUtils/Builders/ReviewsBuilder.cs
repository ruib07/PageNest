using PageNest.Domain.Entities;

namespace PageNest.TestUtils.Builders;

public class ReviewsBuilder
{
    private static int _counter = 2;

    public static List<Review> CreateReviews(int quantity = 2)
    {
        var reviews = new List<Review>();

        for (int i = 0; i < quantity; i++)
        {
            reviews.Add(new Review()
            {
                Id = Guid.NewGuid(),
                BookId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Rating = 5,
                Comment = $"Review comment {_counter}"
            });

            _counter++;
        }

        return reviews;
    }

    public static Review InvalidReviewCreation(int rating, string comment)
    {
        return new Review()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BookId = Guid.NewGuid(),
            Rating = rating,
            Comment = comment
        };
    }
}
