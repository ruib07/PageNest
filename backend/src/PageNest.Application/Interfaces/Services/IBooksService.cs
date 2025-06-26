using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Services;

public interface IBooksService
{
    Task<IEnumerable<Book>> GetBooks();
    Task<IEnumerable<Book>> GetBooksByAuthor(string authorName);
    Task<Result<Book>> GetBookById(Guid bookId);
    Task<Result<Book>> GetBookByTitle(string bookTitle);
    Task<Result<Book>> CreateBook(Book book);
    Task<Result<Book>> UpdateBook(Guid bookId, Book updateBook);
    Task DeleteBook(Guid bookId);
}
