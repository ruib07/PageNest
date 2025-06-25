using Microsoft.EntityFrameworkCore;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.Infrastructure.Data.Repositories;

public class GenreRepository : IGenreRepository
{
    private readonly ApplicationDbContext _context;
    private DbSet<Genre> Genres => _context.Genres;
    private DbSet<Book> Books => _context.Books;

    public GenreRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Genre>> GetGenres()
    {
        return await Genres.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetBooksByGenre(Guid genreId)
    {
        return await Books.AsNoTracking()
                          .Where(b => b.BookGenres.Any(bg => bg.GenreId == genreId))
                          .ToListAsync();
    }

    public async Task<Genre> GetGenreById(Guid genreId)
    {
        return await Genres.AsNoTracking().FirstOrDefaultAsync(g => g.Id == genreId);
    }

    public async Task<Genre> GetGenreByName(string genreName)
    {
        return await Genres.AsNoTracking().FirstOrDefaultAsync(g => g.Name == genreName);
    }
}
