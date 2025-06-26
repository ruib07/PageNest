using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Services;

public class BookGenresService : IBookGenresService
{
    private readonly IBookGenreRepository _bookGenreRepository;

    public BookGenresService(IBookGenreRepository bookGenreRepository)
    {
        _bookGenreRepository = bookGenreRepository;
    }

    public async Task<IEnumerable<BookGenre>> GetBookGenres()
    {
        return await _bookGenreRepository.GetBookGenres();
    }

    public async Task<IEnumerable<BookGenre>> GetBookGenresByBookId(Guid bookId)
    {
        return await _bookGenreRepository.GetBookGenresByBookId(bookId);
    }

    public async Task<IEnumerable<BookGenre>> GetBookGenresByGenreId(Guid genreId)
    {
        return await _bookGenreRepository.GetBookGenresByGenreId(genreId);
    }
}
