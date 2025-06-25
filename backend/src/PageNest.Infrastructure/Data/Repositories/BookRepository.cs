using Microsoft.EntityFrameworkCore;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.Infrastructure.Data.Repositories;

public class BookRepository : IBookRepository
{
    private readonly ApplicationDbContext _context;
    private DbSet<Book> Books => _context.Books;

    public BookRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetBooks()
    {
        return await Books.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetBooksByAuthor(string authorName)
    {
        return await Books.AsNoTracking().Where(b => b.Author == authorName).ToListAsync();
    }

    public async Task<Book> GetBookById(Guid bookId)
    {
        return await Books.FirstOrDefaultAsync(b => b.Id == bookId);
    }

    public async Task<Book> GetBookByTitle(string bookTitle)
    {
        return await Books.FirstOrDefaultAsync(b => b.Title == bookTitle);
    }

    public async Task<Book> CreateBook(Book book)
    {
        await Books.AddAsync(book);
        await _context.SaveChangesAsync();

        return book;
    }

    public async Task UpdateBook(Book book)
    {
        Books.Update(book);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBook(Guid bookId)
    {
        var book = await GetBookById(bookId);

        Books.Remove(book);
        await _context.SaveChangesAsync();
    }
}
