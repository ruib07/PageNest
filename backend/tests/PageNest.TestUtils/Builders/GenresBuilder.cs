using PageNest.Domain.Entities;

namespace PageNest.TestUtils.Builders;

public class GenresBuilder
{
    private static int _counter = 2;

    public static List<Genre> CreateGenres(int quantity = 2)
    {
        var genres = new List<Genre>();

        for (int i = 0; i < quantity; i++)
        {
            genres.Add(new Genre()
            {
                Id = Guid.NewGuid(),
                Name = $"Genre {_counter}"
            });

            _counter++;
        }

        return genres;
    }
}
