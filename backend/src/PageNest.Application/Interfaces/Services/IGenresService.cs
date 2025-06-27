using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Services;

public interface IGenresService
{
    Task<IEnumerable<Genre>> GetGenres();
    Task<IEnumerable<Book>> GetBooksByGenreId(Guid genreId);
    Task<Result<Genre>> GetGenreById(Guid genreId);
    Task<Result<Genre>> GetGenreByName(string genreName);
}
