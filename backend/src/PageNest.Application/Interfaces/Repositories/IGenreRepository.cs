using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Repositories;

public interface IGenreRepository
{
    Task<IEnumerable<Genre>> GetGenres();
    Task<IEnumerable<Book>> GetBooksByGenre(Guid genreId);
    Task<Genre> GetGenreById(Guid genreId);
    Task<Genre> GetGenreByName(string genreName);
}
