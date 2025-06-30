using PageNest.Domain.Entities;

namespace PageNest.TestUtils.Builders;

public class BookGenresBuilder
{
    private static int _counter = 2;

    public static List<BookGenre> CreateBookGenres(int quantity = 2)
    {
        var bookGenres = new List<BookGenre>();

        for (int i = 0; i < quantity; i++)
        {
            bookGenres.Add(new BookGenre()
            {
                BookId = Guid.NewGuid(),
                GenreId = Guid.NewGuid()
            });

            _counter++;
        }

        return bookGenres;
    }
}
