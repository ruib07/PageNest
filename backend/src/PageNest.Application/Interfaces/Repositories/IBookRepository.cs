using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetBooks();
    Task<IEnumerable<Book>> GetBooksByAuthor(string authorName);
    Task<Book> GetBookById(Guid bookId);
    Task<Book> GetBookByTitle(string bookTitle);
    Task<Book> CreateBook(Book book);
    Task UpdateBook(Book book);
    Task DeleteBook(Guid bookId);
}
