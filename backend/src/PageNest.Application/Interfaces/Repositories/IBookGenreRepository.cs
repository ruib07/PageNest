using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Repositories;

public interface IBookGenreRepository
{
    Task<IEnumerable<BookGenre>> GetBookGenres();
    Task<IEnumerable<BookGenre>> GetBookGenresByBookId(Guid bookId);
    Task<IEnumerable<BookGenre>> GetBookGenresByGenreId(Guid genreId);
}
