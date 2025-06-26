using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Services;

public interface ICategoriesService
{
    Task<IEnumerable<Category>> GetCategories();
    Task<IEnumerable<Book>> GetBooksByCategory(Guid categoryId);
    Task<Result<Category>> GetCategoryById(Guid categoryId);
    Task<Result<Category>> GetCategoryByName(string categoryName);
}
