using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.API.Controllers;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Controllers;

public class GenresControllerTests : TestBase
{
    private readonly Mock<IGenreRepository> _genreRepositoryMock;
    private readonly GenresService _genresService;
    private readonly GenresController _genresController;

    public GenresControllerTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _genreRepositoryMock = new Mock<IGenreRepository>();
        _genresService = new GenresService(_genreRepositoryMock.Object);
        _genresController = new GenresController(_genresService);
    }

    [Fact]
    public async Task GetGenres_ShouldReturnOkResult_WithAllGenres()
    {
        var genres = GenresBuilder.CreateGenres();

        _genreRepositoryMock.Setup(repo => repo.GetGenres()).ReturnsAsync(genres);

        var result = await _genresController.GetGenres();
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<Genre>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(genres.Count, response.Count());
            Assert.Equal(genres.First().Id, response.First().Id);
            Assert.Equal(genres.Last().Id, response.Last().Id);
        });
    }

    [Fact]
    public async Task GetBooksByGenreId_ShouldReturnOkResult_WithAllBooks_WhenGenreExists()
    {
        var genres = GenresBuilder.CreateGenres();
        var booksByGenreList = new List<Book>()
        {
            new Book()
            {
                Id = Guid.NewGuid(),
                Title = "Test Book",
                Author = "Test Author",
                Description = "Test Description",
                PublishedDate = DateTime.UtcNow,
                ISBN = "1234567890",
                PageCount = 100,
                LanguageId = Guid.NewGuid(),
                CoverImageUrl = "http://example.com/cover.jpg",
                Stock = 10,
                Price = 9.99m,
                CategoryId = genres.First().Id,
                BookGenres = new List<BookGenre>()
            }
        };

        var bookGenre = new BookGenre()
        {
            BookId = booksByGenreList.First().Id,
            GenreId = genres.First().Id
        };

        booksByGenreList.First().BookGenres.Add(bookGenre);
        await _context.Books.AddRangeAsync(booksByGenreList);
        await _context.BookGenres.AddAsync(bookGenre);
        await _context.SaveChangesAsync();

        _genreRepositoryMock.Setup(repo => repo.GetBooksByGenre(genres[0].Id))
                                               .ReturnsAsync(booksByGenreList);

        var result = await _genresController.GetBooksByGenreId(genres[0].Id);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<Book>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(genres[0].Id, response.First().BookGenres.First().GenreId);
        });
    }

    [Fact]
    public async Task GetGenreById_ShouldReturnOkResult_WithGenre()
    {
        var genre = GenresBuilder.CreateGenres().First();

        _genreRepositoryMock.Setup(repo => repo.GetGenreById(genre.Id)).ReturnsAsync(genre);

        var result = await _genresController.GetGenreById(genre.Id);
        var okResult = result as OkObjectResult;
        var response = okResult.Value as Genre;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(genre.Id, response.Id);
            Assert.Equal(genre.Name, response.Name);
        });
    }

    [Fact]
    public async Task GetGenreById_ShouldReturnNotFoundResult_WhenGenreDoesNotExist()
    {
        var result = await _genresController.GetGenreById(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Genre not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task GetGenreByName_ShouldReturnOkResult_WithGenre()
    {
        var genre = GenresBuilder.CreateGenres().First();

        _genreRepositoryMock.Setup(repo => repo.GetGenreByName(genre.Name)).ReturnsAsync(genre);

        var result = await _genresController.GetGenreByName(genre.Name);
        var okResult = result as OkObjectResult;
        var response = okResult.Value as Genre;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(genre.Id, response.Id);
            Assert.Equal(genre.Name, response.Name);
        });
    }

    [Fact]
    public async Task GetGenreByName_ShouldReturnNotFoundResult_WhenGenreDoesNotExist()
    {
        var result = await _genresController.GetGenreByName("Non Existent Genre");

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Genre not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }
}
