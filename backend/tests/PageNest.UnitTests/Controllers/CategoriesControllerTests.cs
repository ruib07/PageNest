using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.API.Controllers;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Controllers;

public class CategoriesControllerTests : TestBase
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CategoriesService _categoriesService;
    private readonly CategoriesController _categoriesController;

    public CategoriesControllerTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _categoriesService = new CategoriesService(_categoryRepositoryMock.Object);
        _categoriesController = new CategoriesController(_categoriesService);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnOkResult_WithAllCategories()
    {
        var categories = CategoriesBuilder.CreateCategories();

        _categoryRepositoryMock.Setup(repo => repo.GetCategories()).ReturnsAsync(categories);

        var result = await _categoriesController.GetCategories();
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<Category>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(categories.Count, response.Count());
            Assert.Equal(categories.First().Id, response.First().Id);
            Assert.Equal(categories.Last().Id, response.Last().Id);
        });
    }

    [Fact]
    public async Task GetBooksByCategoryId_ShouldReturnOkResult_WithAllBooks_WhenCategoryExists()
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

        _categoryRepositoryMock.Setup(repo => repo.GetBooksByCategoryId(categories[0].Id))
                                                  .ReturnsAsync(booksByCategoryList);

        var result = await _categoriesController.GetBooksByCategoryId(categories[0].Id);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<Book>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(categories[0].Id, response.First().CategoryId);
        });
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnOkResult_WithCategory()
    {
        var category = CategoriesBuilder.CreateCategories().First();

        _categoryRepositoryMock.Setup(repo => repo.GetCategoryById(category.Id)).ReturnsAsync(category);

        var result = await _categoriesController.GetCategoryById(category.Id);
        var okResult = result as OkObjectResult;
        var response = okResult.Value as Category;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(category.Id, response.Id);
            Assert.Equal(category.Name, response.Name);
        });
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnNotFoundResult_WhenCategoryDoesNotExist()
    {
        var result = await _categoriesController.GetCategoryById(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Category not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task GetCategoryByName_ShouldReturnOkResult_WithCategory()
    {
        var category = CategoriesBuilder.CreateCategories().First();

        _categoryRepositoryMock.Setup(repo => repo.GetCategoryByName(category.Name)).ReturnsAsync(category);

        var result = await _categoriesController.GetCategoryByName(category.Name);
        var okResult = result as OkObjectResult;
        var response = okResult.Value as Category;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(category.Id, response.Id);
            Assert.Equal(category.Name, response.Name);
        });
    }

    [Fact]
    public async Task GetCategoryByName_ShouldReturnNotFoundResult_WhenCategoryDoesNotExist()
    {
        var result = await _categoriesController.GetCategoryByName("Non Existent Category");

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Category not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }
}
