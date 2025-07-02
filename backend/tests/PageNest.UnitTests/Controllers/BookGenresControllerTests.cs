using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.API.Controllers;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Controllers;

public class BookGenresControllerTests : TestBase
{
    private readonly Mock<IBookGenreRepository> _bookGenreRepositoryMock;
    private readonly BookGenresService _bookGenresService;
    private readonly BookGenresController _bookGenresController;

    public BookGenresControllerTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _bookGenreRepositoryMock = new Mock<IBookGenreRepository>();
        _bookGenresService = new BookGenresService(_bookGenreRepositoryMock.Object);
        _bookGenresController = new BookGenresController(_bookGenresService);
    }

    [Fact]
    public async Task GetBookGenres_ShouldReturnOkResult_WithAllBookGenres()
    {
        var bookGenres = BookGenresBuilder.CreateBookGenres();

        _bookGenreRepositoryMock.Setup(repo => repo.GetBookGenres()).ReturnsAsync(bookGenres);

        var result = await _bookGenresController.GetBookGenres();
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<BookGenre>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(bookGenres.Count, response.Count());
            Assert.Equal(bookGenres.First().BookId, response.First().BookId);
            Assert.Equal(bookGenres.First().GenreId, response.First().GenreId);
            Assert.Equal(bookGenres.Last().BookId, response.Last().BookId);
            Assert.Equal(bookGenres.Last().GenreId, response.Last().GenreId);
        });
    }

    [Fact]
    public async Task GetBookGenresByBookId_ShouldReturnOkResult_WithAllBookGenres_WhenBookExists()
    {
        var bookGenres = BookGenresBuilder.CreateBookGenres();
        var bookGenresByBookList = bookGenres.Where(p => p.BookId == bookGenres[0].BookId).ToList();

        _bookGenreRepositoryMock.Setup(repo => repo.GetBookGenresByBookId(bookGenres[0].BookId))
                                                   .ReturnsAsync(bookGenresByBookList);

        var result = await _bookGenresController.GetBookGenresByBookId(bookGenres[0].BookId);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<BookGenre>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(bookGenres[0].BookId, response.First().BookId);
        });
    }

    [Fact]
    public async Task GetBookGenresByGenreId_ShouldReturnOkResult_WithAllBookGenres_WhenGenreExists()
    {
        var bookGenres = BookGenresBuilder.CreateBookGenres();
        var bookGenresByGenreList = bookGenres.Where(p => p.GenreId == bookGenres[0].GenreId).ToList();

        _bookGenreRepositoryMock.Setup(repo => repo.GetBookGenresByGenreId(bookGenres[0].GenreId))
                                                   .ReturnsAsync(bookGenresByGenreList);

        var result = await _bookGenresController.GetBookGenresByGenreId(bookGenres[0].GenreId);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<BookGenre>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(bookGenres[0].GenreId, response.First().GenreId);
        });
    }
}
