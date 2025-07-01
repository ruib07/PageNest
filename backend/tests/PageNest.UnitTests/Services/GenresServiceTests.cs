using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Services;

public class GenresServiceTests : TestBase
{
    private readonly Mock<IGenreRepository> _genreRepositoryMock;
    private readonly GenresService _genresService;

    public GenresServiceTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _genreRepositoryMock = new Mock<IGenreRepository>();
        _genresService = new GenresService(_genreRepositoryMock.Object);
    }

    [Fact]
    public async Task GetGenres_ShouldReturnAllGenres()
    {
        var genres = GenresBuilder.CreateGenres();
        await _context.Genres.AddRangeAsync(genres);
        await _context.SaveChangesAsync();

        _genreRepositoryMock.Setup(repo => repo.GetGenres()).ReturnsAsync(genres);

        var result = await _genresService.GetGenres();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(genres.Count, result.Count());
            Assert.Equal(genres.First().Id, result.First().Id);
            Assert.Equal(genres.First().Name, result.First().Name);
            Assert.Equal(genres.Last().Id, result.Last().Id);
            Assert.Equal(genres.Last().Name, result.Last().Name);
        });
    }

    [Fact]
    public async Task GetBooksByGenreId_ShouldReturnBooks_WhenGenreExists()
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
                Language = "EN",
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

        _genreRepositoryMock.Setup(repo => repo.GetBooksByGenre(genres.First().Id))
                                               .ReturnsAsync(booksByGenreList);

        var result = await _genresService.GetBooksByGenreId(genres.First().Id);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.NotNull(result.First().BookGenres);
        Assert.Contains(result.First().BookGenres, bg => bg.GenreId == genres.First().Id);
    }

    [Fact]
    public async Task GetGenreById_ShouldReturnGenre_WhenGenreExists()
    {
        var genre = GenresBuilder.CreateGenres().First();

        _genreRepositoryMock.Setup(repo => repo.GetGenreById(genre.Id)).ReturnsAsync(genre);

        var result = await _genresService.GetGenreById(genre.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(genre.Id, result.Data.Id);
            Assert.Equal(genre.Name, result.Data.Name);
        });
    }

    [Fact]
    public async Task GetGenreById_ShouldReturnNotFound_WhenGenreDoesNotExist()
    {
        _genreRepositoryMock.Setup(repo => repo.GetGenreById(It.IsAny<Guid>())).ReturnsAsync((Genre)null);

        var result = await _genresService.GetGenreById(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("Genre not found.", result.Error.Message);
        });
    }

    [Fact]
    public async Task GetGenreByName_ShouldReturnGenre_WhenGenreNameExists()
    {
        var genre = GenresBuilder.CreateGenres().First();

        _genreRepositoryMock.Setup(repo => repo.GetGenreByName(genre.Name)).ReturnsAsync(genre);

        var result = await _genresService.GetGenreByName(genre.Name);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(genre.Id, result.Data.Id);
            Assert.Equal(genre.Name, result.Data.Name);
        });
    }

    [Fact]
    public async Task GetGenreByName_ShouldReturnNotFound_WhenGenreNameDoesNotExist()
    {
        _genreRepositoryMock.Setup(repo => repo.GetGenreByName("Non Existent Genre")).ReturnsAsync((Genre)null);

        var result = await _genresService.GetGenreByName("Non Existent Category");

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("Genre not found.", result.Error.Message);
        });
    }
}
