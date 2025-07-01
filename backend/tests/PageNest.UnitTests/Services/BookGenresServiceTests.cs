using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Services;

public class BookGenresServiceTests : TestBase
{
    private readonly Mock<IBookGenreRepository> _bookGenreRepositoryMock;
    private readonly BookGenresService _bookGenresService;

    public BookGenresServiceTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _bookGenreRepositoryMock = new Mock<IBookGenreRepository>();
        _bookGenresService = new BookGenresService(_bookGenreRepositoryMock.Object);
    }

    [Fact]
    public async Task GetBookGenres_ShouldReturnAllBookGenres()
    {
        var bookGenres = BookGenresBuilder.CreateBookGenres();
        await _context.BookGenres.AddRangeAsync(bookGenres);
        await _context.SaveChangesAsync();

        _bookGenreRepositoryMock.Setup(repo => repo.GetBookGenres()).ReturnsAsync(bookGenres);

        var result = await _bookGenresService.GetBookGenres();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(bookGenres.Count, result.Count());
            Assert.Equal(bookGenres.First().BookId, result.First().BookId);
            Assert.Equal(bookGenres.First().GenreId, result.First().GenreId);
            Assert.Equal(bookGenres.Last().BookId, result.Last().BookId);
            Assert.Equal(bookGenres.Last().GenreId, result.Last().GenreId);
        });
    }

    [Fact]
    public async Task GetBookGenresByBookId_ShouldReturnBookGenres_WhenBookExists()
    {
        var bookGenres = BookGenresBuilder.CreateBookGenres();
        var bookGenresByBookList = bookGenres.Where(p => p.BookId == bookGenres[0].BookId).ToList();

        _bookGenreRepositoryMock.Setup(repo => repo.GetBookGenresByBookId(bookGenres[0].BookId))
                                                   .ReturnsAsync(bookGenresByBookList);

        var result = await _bookGenresService.GetBookGenresByBookId(bookGenres[0].BookId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(bookGenres.First().BookId, result.First().BookId);
    }

    [Fact]
    public async Task GetBookGenresByGenreId_ShouldReturnBookGenres_WhenGenreExists()
    {
        var bookGenres = BookGenresBuilder.CreateBookGenres();
        var bookGenresByGenreList = bookGenres.Where(p => p.GenreId == bookGenres[0].GenreId).ToList();

        _bookGenreRepositoryMock.Setup(repo => repo.GetBookGenresByGenreId(bookGenres[0].GenreId))
                                                   .ReturnsAsync(bookGenresByGenreList);

        var result = await _bookGenresService.GetBookGenresByGenreId(bookGenres[0].GenreId);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Single(result);
            Assert.Equal(bookGenres.First().GenreId, result.First().GenreId);
        });
    }
}
