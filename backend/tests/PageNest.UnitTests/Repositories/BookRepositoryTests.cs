using Microsoft.EntityFrameworkCore;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Data.Repositories;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Repositories;

public class BookRepositoryTests : TestBase
{
    private readonly BookRepository _bookRepository;

    public BookRepositoryTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _bookRepository = new BookRepository(_context);
    }

    [Fact]
    public async Task GetBooks_ShouldReturnAllBooks()
    {
        var books = BooksBuilder.CreateBooks();
        await _context.Books.AddRangeAsync(books);
        await _context.SaveChangesAsync();

        var result = await _bookRepository.GetBooks();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(books.Count, result.Count());
            Assert.Equal(books.First().Id, result.First().Id);
            Assert.Equal(books.First().Title, result.First().Title);
            Assert.Equal(books.Last().Id, result.Last().Id);
            Assert.Equal(books.Last().Title, result.Last().Title);
        });
    }

    [Fact]
    public async Task GetBooksByAuthor_ShouldReturnAllBooks_WhenAuthorExists()
    {
        var books = BooksBuilder.CreateBooks();
        await _context.Books.AddRangeAsync(books);
        await _context.SaveChangesAsync();

        var result = await _bookRepository.GetBooksByAuthor(books.First().Author);

        Assert.NotNull(result);
        Assert.Equal(books.First().Id, result.First().Id);
    }

    [Fact]
    public async Task GetBookById_ShouldReturnBook_WhenBookExists()
    {
        var book = BooksBuilder.CreateBooks().First();
        await _bookRepository.CreateBook(book);

        var result = await _bookRepository.GetBookById(book.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(book.Id, result.Id);
            Assert.Equal(book.Title, result.Title);
            Assert.Equal(book.Author, result.Author);
            Assert.Equal(book.Description, result.Description);
            Assert.Equal(book.PublishedDate, result.PublishedDate);
            Assert.Equal(book.ISBN, result.ISBN);
            Assert.Equal(book.PageCount, result.PageCount);
            Assert.Equal(book.Language, result.Language);
            Assert.Equal(book.CoverImageUrl, result.CoverImageUrl);
            Assert.Equal(book.Stock, result.Stock);
            Assert.Equal(book.Price, result.Price);
            Assert.Equal(book.CategoryId, result.CategoryId);
        });
    }

    [Fact]
    public async Task GetBookByTitle_ShouldReturnBook_WhenBookExists()
    {
        var book = BooksBuilder.CreateBooks().First();
        await _bookRepository.CreateBook(book);

        var result = await _bookRepository.GetBookByTitle(book.Title);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(book.Id, result.Id);
            Assert.Equal(book.Title, result.Title);
            Assert.Equal(book.Author, result.Author);
            Assert.Equal(book.Description, result.Description);
            Assert.Equal(book.PublishedDate, result.PublishedDate);
            Assert.Equal(book.ISBN, result.ISBN);
            Assert.Equal(book.PageCount, result.PageCount);
            Assert.Equal(book.Language, result.Language);
            Assert.Equal(book.CoverImageUrl, result.CoverImageUrl);
            Assert.Equal(book.Stock, result.Stock);
            Assert.Equal(book.Price, result.Price);
            Assert.Equal(book.CategoryId, result.CategoryId);
        });
    }

    [Fact]
    public async Task CreateBook_ShouldCreateBook()
    {
        var newBook = BooksBuilder.CreateBooks().First();
        await _bookRepository.CreateBook(newBook);

        var result = await _bookRepository.GetBookById(newBook.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(newBook.Id, result.Id);
            Assert.Equal(newBook.Title, result.Title);
            Assert.Equal(newBook.Author, result.Author);
            Assert.Equal(newBook.Description, result.Description);
            Assert.Equal(newBook.PublishedDate, result.PublishedDate);
            Assert.Equal(newBook.ISBN, result.ISBN);
            Assert.Equal(newBook.PageCount, result.PageCount);
            Assert.Equal(newBook.Language, result.Language);
            Assert.Equal(newBook.CoverImageUrl, result.CoverImageUrl);
            Assert.Equal(newBook.Stock, result.Stock);
            Assert.Equal(newBook.Price, result.Price);
            Assert.Equal(newBook.CategoryId, result.CategoryId);
        });
    }

    [Fact]
    public async Task UpdateBook_ShouldUpdateBook()
    {
        var createBook = BooksBuilder.CreateBooks().First();
        await _bookRepository.CreateBook(createBook);

        _context.Entry(createBook).State = EntityState.Detached;

        var updatedBook = BooksBuilder.UpdateBook(createBook.Id, createBook.CategoryId);
        await _bookRepository.UpdateBook(updatedBook);

        var result = await _bookRepository.GetBookById(createBook.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(updatedBook.Id, result.Id);
            Assert.Equal(updatedBook.Title, result.Title);
            Assert.Equal(updatedBook.Author, result.Author);
            Assert.Equal(updatedBook.Description, result.Description);
            Assert.Equal(updatedBook.PublishedDate, result.PublishedDate);
            Assert.Equal(updatedBook.ISBN, result.ISBN);
            Assert.Equal(updatedBook.PageCount, result.PageCount);
            Assert.Equal(updatedBook.Language, result.Language);
            Assert.Equal(updatedBook.CoverImageUrl, result.CoverImageUrl);
            Assert.Equal(updatedBook.Stock, result.Stock);
            Assert.Equal(updatedBook.Price, result.Price);
            Assert.Equal(updatedBook.CategoryId, result.CategoryId);
        });
    }

    [Fact]
    public async Task DeleteBook_ShouldDeleteBook()
    {
        var book = BooksBuilder.CreateBooks().First();

        await _bookRepository.CreateBook(book);
        await _bookRepository.DeleteBook(book.Id);

        var result = await _bookRepository.GetBookById(book.Id);

        Assert.Null(result);
    }
}
