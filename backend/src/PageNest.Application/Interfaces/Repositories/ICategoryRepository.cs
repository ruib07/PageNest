using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetCategories();
    Task<IEnumerable<Book>> GetBooksByCategoryId(Guid categoryId);
    Task<Category> GetCategoryById(Guid categoryId);
    Task<Category> GetCategoryByName(string categoryName);
}
