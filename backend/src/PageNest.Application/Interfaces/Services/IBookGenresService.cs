using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Services;

public interface IBookGenresService
{
    Task<IEnumerable<BookGenre>> GetBookGenres();
    Task<IEnumerable<BookGenre>> GetBookGenresByBookId(Guid bookId);
    Task<IEnumerable<BookGenre>> GetBookGenresByGenreId(Guid genreId);
}
