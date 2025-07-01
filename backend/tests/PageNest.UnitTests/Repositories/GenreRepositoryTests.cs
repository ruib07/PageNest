using Microsoft.EntityFrameworkCore;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Data.Repositories;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Repositories;

public class GenreRepositoryTests : TestBase
{
    private readonly GenreRepository _genreRepository;

    public GenreRepositoryTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _genreRepository = new GenreRepository(_context);
    }

    [Fact]
    public async Task GetGenres_ShouldReturnAllGenres()
    {
        var genres = GenresBuilder.CreateGenres();
        await _context.Genres.AddRangeAsync(genres);
        await _context.SaveChangesAsync();

        var result = await _genreRepository.GetGenres();

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
    public async Task GetBooksByGenre_ShouldReturnGenres_WhenGenreExists()
    {
        var genres = GenresBuilder.CreateGenres();
        await _context.Genres.AddRangeAsync(genres);
        await _context.SaveChangesAsync();

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Test Book",
            Author = "Test Author",
            Description = "Test Description",
            PublishedDate = DateTime.UtcNow,
            ISBN = "1234567890",
            PageCount = 100,
            LanguageId = Guid.NewGuid(),
            CoverImageUrl = "",
            Stock = 10,
            Price = 9.99m,
            CategoryId = Guid.NewGuid(),
            BookGenres = new List<BookGenre>() 
        };

        var bookGenre = new BookGenre()
        {
            BookId = book.Id,
            GenreId = genres.First().Id
        };
        book.BookGenres.Add(bookGenre);

        await _context.Books.AddAsync(book);
        await _context.BookGenres.AddAsync(bookGenre);
        await _context.SaveChangesAsync();

        var result = (await _genreRepository.GetBooksByGenre(genres.First().Id)).ToList();

        Assert.NotNull(result);
        Assert.NotEmpty(result); 
        Assert.NotNull(result.First().BookGenres);
        Assert.Contains(result.First().BookGenres, bg => bg.GenreId == genres.First().Id);
    }

    [Fact]
    public async Task GetGenreById_ShouldReturnGenre_WhenGenreExists()
    {
        var genre = GenresBuilder.CreateGenres().First();
        await _context.Genres.AddAsync(genre);
        await _context.SaveChangesAsync();

        var result = await _genreRepository.GetGenreById(genre.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(genre.Id, result.Id);
            Assert.Equal(genre.Name, result.Name);
        });
    }

    [Fact]
    public async Task GetGenreByName_ShouldReturnGenre_WhenGenreNameExists()
    {
        var genre = GenresBuilder.CreateGenres().First();
        await _context.Genres.AddAsync(genre);
        await _context.SaveChangesAsync();

        var result = await _genreRepository.GetGenreByName(genre.Name);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(genre.Id, result.Id);
            Assert.Equal(genre.Name, result.Name);
        });
    }
}
