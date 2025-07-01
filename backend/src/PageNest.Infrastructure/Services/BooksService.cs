using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Services;

public class BooksService : IBooksService
{
    private readonly IBookRepository _bookRepository;

    public BooksService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IEnumerable<Book>> GetBooks()
    {
        return await _bookRepository.GetBooks();
    }

    public async Task<IEnumerable<Book>> GetBooksByAuthor(string authorName)
    {
        return await _bookRepository.GetBooksByAuthor(authorName);
    }

    public async Task<Result<Book>> GetBookById(Guid bookId)
    {
        var book = await _bookRepository.GetBookById(bookId);

        if (book == null) return Result<Book>.Fail("Book not found.", 404);

        return Result<Book>.Success(book);
    }

    public async Task<Result<Book>> GetBookByTitle(string bookTitle)
    {
        var book =  await _bookRepository.GetBookByTitle(bookTitle);

        if (book == null) return Result<Book>.Fail("Book not found.", 404);

        return Result<Book>.Success(book);
    }

    public async Task<Result<Book>> CreateBook(Book book)
    {
        var validation = ValidateBookFields(book);

        if (!validation.IsSuccess)
            return Result<Book>.Fail(validation.Error.Message, validation.Error.StatusCode);

        var createdBook = await _bookRepository.CreateBook(book);

        return Result<Book>.Success(createdBook, "Book created successfully.");
    }

    public async Task<Result<Book>> UpdateBook(Guid bookId, Book updateBook)
    {
        var currentBook = await _bookRepository.GetBookById(bookId);

        var validation = ValidateBookFields(updateBook);

        if (!validation.IsSuccess)
            return Result<Book>.Fail(validation.Error.Message, validation.Error.StatusCode);

        currentBook.Title = updateBook.Title;
        currentBook.Author = updateBook.Author;
        currentBook.Description = updateBook.Description;
        currentBook.PublishedDate = updateBook.PublishedDate;
        currentBook.ISBN = updateBook.ISBN;
        currentBook.PageCount = updateBook.PageCount;
        currentBook.Language = updateBook.Language;
        currentBook.CoverImageUrl = updateBook.CoverImageUrl;
        currentBook.Stock = updateBook.Stock;
        currentBook.Price = updateBook.Price;

        await _bookRepository.UpdateBook(currentBook);

        return Result<Book>.Success(currentBook, "Book updated successfully.");
    }

    public async Task DeleteBook(Guid bookId)
    {
        await _bookRepository.DeleteBook(bookId);
    }

    #region Private Methods

    private static Result<bool> ValidateBookFields(Book book)
    {
        if (string.IsNullOrWhiteSpace(book.Title))
            return Result<bool>.Fail("Title cannot be empty.", 400);

        if (string.IsNullOrWhiteSpace(book.Author))
            return Result<bool>.Fail("Author cannot be empty.", 400);

        if (string.IsNullOrWhiteSpace(book.Description))
            return Result<bool>.Fail("Description cannot be empty.", 400);

        if (book.PublishedDate == DateTime.MinValue)
            return Result<bool>.Fail("Published date is required.", 400);

        if (string.IsNullOrWhiteSpace(book.ISBN))
            return Result<bool>.Fail("ISBN cannot be empty.", 400);

        if (book.PageCount <= 0) 
            return Result<bool>.Fail("Page count must be greater than zero.", 400);

        if (book.LanguageId == Guid.Empty)
            return Result<bool>.Fail("Language cannot be empty.", 400);

        if (string.IsNullOrWhiteSpace(book.CoverImageUrl))
            return Result<bool>.Fail("Cover Image URL cannot be empty.", 400);

        if (book.Stock < 0)
            return Result<bool>.Fail("Stock cannot be negative.", 400);

        if (book.Price <= 0)
            return Result<bool>.Fail("Price must be greater than zero.", 400);

        return Result<bool>.Success(true);
    }

    #endregion
}
