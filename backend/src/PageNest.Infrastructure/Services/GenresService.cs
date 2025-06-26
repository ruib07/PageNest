using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Services;

public class GenresService : IGenresService
{
    private readonly IGenreRepository _genreRepository;

    public GenresService(IGenreRepository genreRepository)
    {
        _genreRepository = genreRepository;
    }

    public async Task<IEnumerable<Genre>> GetGenres()
    {
        return await _genreRepository.GetGenres();
    }

    public async Task<IEnumerable<Book>> GetBooksByGenre(Guid genreId)
    {
        return await _genreRepository.GetBooksByGenre(genreId);
    }

    public async Task<Result<Genre>> GetGenreById(Guid genreId)
    {
        var genre = await _genreRepository.GetGenreById(genreId);

        if (genre == null) return Result<Genre>.Fail("Genre not found.", 404);

        return Result<Genre>.Success(genre);
    }

    public async Task<Result<Genre>> GetGenreByName(string genreName)
    {
        var genre = await _genreRepository.GetGenreByName(genreName);

        if (genre == null) return Result<Genre>.Fail("Genre not found.", 404);

        return Result<Genre>.Success(genre);
    }
}
