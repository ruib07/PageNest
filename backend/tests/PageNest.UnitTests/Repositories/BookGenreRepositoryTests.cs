using Microsoft.EntityFrameworkCore;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Data.Repositories;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Repositories;

public class BookGenreRepositoryTests : TestBase
{
    private readonly BookGenreRepository _bookGenreRepository;

    public BookGenreRepositoryTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _bookGenreRepository = new BookGenreRepository(_context);
    }

    [Fact]
    public async Task GetBookGenres_ShouldReturnAllBookGenres()
    {
        var bookGenres = BookGenresBuilder.CreateBookGenres();
        await _context.BookGenres.AddRangeAsync(bookGenres);
        await _context.SaveChangesAsync();

        var result = await _bookGenreRepository.GetBookGenres();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(bookGenres.Count, result.Count());
            Assert.Equal(bookGenres.First().GenreId, result.First().GenreId);
            Assert.Equal(bookGenres.First().BookId, result.First().BookId);
            Assert.Equal(bookGenres.Last().GenreId, result.Last().GenreId);
            Assert.Equal(bookGenres.Last().BookId, result.Last().BookId);
        });
    }

    [Fact]
    public async Task GetBookGenresByBookId_ShouldReturnBookGenres_WhenBookExists()
    {
        var bookGenres = BookGenresBuilder.CreateBookGenres();
        await _context.BookGenres.AddRangeAsync(bookGenres);
        await _context.SaveChangesAsync();

        var result = await _bookGenreRepository.GetBookGenresByBookId(bookGenres.First().BookId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(bookGenres.First().BookId, result.First().BookId);
    }

    [Fact]
    public async Task GetBookGenresByGenreId_ShouldReturnBookGenres_WhenGenreExists()
    {
        var bookGenres = BookGenresBuilder.CreateBookGenres();
        await _context.BookGenres.AddRangeAsync(bookGenres);
        await _context.SaveChangesAsync();

        var result = await _bookGenreRepository.GetBookGenresByGenreId(bookGenres.First().GenreId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(bookGenres.First().GenreId, result.First().GenreId);
    }
}
