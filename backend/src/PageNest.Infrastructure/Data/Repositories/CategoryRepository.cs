using Microsoft.EntityFrameworkCore;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.Infrastructure.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;
    private DbSet<Category> Categories => _context.Categories;
    private DbSet<Book> Books => _context.Books;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetCategories()
    {
        return await Categories.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetBooksByCategoryId(Guid categoryId)
    {
        return await Books.AsNoTracking().Where(b => b.CategoryId == categoryId).ToListAsync();
    }

    public async Task<Category> GetCategoryById(Guid categoryId)
    {
        return await Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == categoryId);
    }

    public async Task<Category> GetCategoryByName(string categoryName)
    {
        return await Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Name == categoryName);
    }
}
