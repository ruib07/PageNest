using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Services;

public class CategoriesServiceTests : TestBase
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CategoriesService _categoriesService;

    public CategoriesServiceTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _categoriesService = new CategoriesService(_categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnAllCategories()
    {
        var categories = CategoriesBuilder.CreateCategories();
        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        _categoryRepositoryMock.Setup(repo => repo.GetCategories()).ReturnsAsync(categories);

        var result = await _categoriesService.GetCategories();

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
    public async Task GetBooksByCategoryId_ShouldReturnBooks_WhenCategoryExists()
    {
        var categories = CategoriesBuilder.CreateCategories();
        var booksByCategoryList = new List<Book>()
        {
            new Book()
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
            }
        };

        await _context.Books.AddRangeAsync(booksByCategoryList);
        await _context.SaveChangesAsync();

        _categoryRepositoryMock.Setup(repo => repo.GetBooksByCategoryId(categories.First().Id))
                               .ReturnsAsync(booksByCategoryList);

        var result = await _categoriesService.GetBooksByCategoryId(categories.First().Id);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(categories.First().Id, result.First().CategoryId);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnCategory_WhenCategoryExists()
    {
        var category = CategoriesBuilder.CreateCategories().First();

        _categoryRepositoryMock.Setup(repo => repo.GetCategoryById(category.Id)).ReturnsAsync(category);

        var result = await _categoriesService.GetCategoryById(category.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(category.Id, result.Data.Id);
            Assert.Equal(category.Name, result.Data.Name);
        });
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        _categoryRepositoryMock.Setup(repo => repo.GetCategoryById(It.IsAny<Guid>())).ReturnsAsync((Category)null);

        var result = await _categoriesService.GetCategoryById(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("Category not found.", result.Error.Message);
        });
    }

    [Fact]
    public async Task GetCategoryByName_ShouldReturnCategory_WhenCategoryNameExists()
    {
        var category = CategoriesBuilder.CreateCategories().First();

        _categoryRepositoryMock.Setup(repo => repo.GetCategoryByName(category.Name)).ReturnsAsync(category);

        var result = await _categoriesService.GetCategoryByName(category.Name);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(category.Id, result.Data.Id);
            Assert.Equal(category.Name, result.Data.Name);
        });
    }

    [Fact]
    public async Task GetCategoryByName_ShouldReturnNotFound_WhenCategoryNameDoesNotExist()
    {
        _categoryRepositoryMock.Setup(repo => repo.GetCategoryByName("Non Existent Category")).ReturnsAsync((Category)null);

        var result = await _categoriesService.GetCategoryByName("Non Existent Category");

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("Category not found.", result.Error.Message);
        });
    }
}
