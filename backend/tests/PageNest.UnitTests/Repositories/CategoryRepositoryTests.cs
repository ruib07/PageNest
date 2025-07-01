using Microsoft.EntityFrameworkCore;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Data.Repositories;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Repositories;

public class CategoryRepositoryTests : TestBase
{
    private readonly CategoryRepository _categoryRepository;

    public CategoryRepositoryTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _categoryRepository = new CategoryRepository(_context);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnAllCategories()
    {
        var categories = CategoriesBuilder.CreateCategories();
        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        var result = await _categoryRepository.GetCategories();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(categories.Count, result.Count());
            Assert.Equal(categories.First().Id, result.First().Id);
            Assert.Equal(categories.First().Name, result.First().Name);
            Assert.Equal(categories.Last().Id, result.Last().Id);
            Assert.Equal(categories.Last().Name, result.Last().Name);
        });
    }

    [Fact]
    public async Task GetBooksByCategoryId_ShouldReturnCategories_WhenCategoryExists()
    {
        var categories = CategoriesBuilder.CreateCategories();
        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        var book = new Book()
        {
            Id = Guid.NewGuid(),
            Title = "Test Book",
            Author = "Test Author",
            Description = "Test Description",
            PublishedDate = DateTime.UtcNow,
            ISBN = "1234567890",
            PageCount = 100,
            LanguageId = Guid.NewGuid(),
            CoverImageUrl = "http://example.com/cover.jpg",
            Stock = 10,
            Price = 9.99m,
            CategoryId = categories.First().Id
        };
        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();

        var result = await _categoryRepository.GetBooksByCategoryId(categories.First().Id);

        Assert.NotNull(result);
        Assert.Equal(categories.First().Id, result.First().CategoryId);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnCategory_WhenCategoryExists()
    {
        var category = CategoriesBuilder.CreateCategories().First();
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var result = await _categoryRepository.GetCategoryById(category.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(category.Name, result.Name);
        });
    }

    [Fact]
    public async Task GetCategoryByName_ShouldReturnCategory_WhenCategoryNameExists()
    {
        var category = CategoriesBuilder.CreateCategories().First();
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var result = await _categoryRepository.GetCategoryByName(category.Name);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(category.Name, result.Name);
        });
    }
}
