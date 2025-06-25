using Microsoft.EntityFrameworkCore;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.Infrastructure.Data.Repositories;

public class BookGenreRepository : IBookGenreRepository
{
    private readonly ApplicationDbContext _context;
    private DbSet<BookGenre> BookGenres => _context.BookGenres;

    public BookGenreRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BookGenre>> GetBookGenres()
    {
        return await BookGenres.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<BookGenre>> GetBookGenresByBookId(Guid bookId)
    {
        return await BookGenres.AsNoTracking().Where(bg => bg.BookId == bookId).ToListAsync();
    }

    public async Task<IEnumerable<BookGenre>> GetBookGenresByGenreId(Guid genreId)
    {
        return await BookGenres.AsNoTracking().Where(bg => bg.GenreId == genreId).ToListAsync();
    }
}
