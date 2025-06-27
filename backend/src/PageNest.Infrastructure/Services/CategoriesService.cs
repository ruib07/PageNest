using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Services;

public class CategoriesService : ICategoriesService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<Category>> GetCategories()
    {
        return await _categoryRepository.GetCategories();
    }

    public async Task<IEnumerable<Book>> GetBooksByCategoryId(Guid categoryId)
    {
        return await _categoryRepository.GetBooksByCategoryId(categoryId);
    }

    public async Task<Result<Category>> GetCategoryById(Guid categoryId)
    {
        var category = await _categoryRepository.GetCategoryById(categoryId);

        if (category == null) return Result<Category>.Fail("Category not found.", 404);

        return Result<Category>.Success(category);
    }

    public async Task<Result<Category>> GetCategoryByName(string categoryName)
    {
        var category = await _categoryRepository.GetCategoryByName(categoryName);

        if (category == null) return Result<Category>.Fail("Category not found.", 404);

        return Result<Category>.Success(category);
    }
}
