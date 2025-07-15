using Microsoft.AspNetCore.Mvc;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces.Services;
using PageNest.Domain.Entities;

namespace PageNest.API.Controllers;

[Route($"api/{AppSettings.ApiVersion}/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoriesService _categoriesService;

    public CategoriesController(ICategoriesService categoriesService)
    {
        _categoriesService = categoriesService;
    }

    // GET api/v1/categories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        return Ok(await _categoriesService.GetCategories());
    }

    // GET api/v1/categories/books/{categoryId}
    [HttpGet("books/{categoryId}")]
    public async Task<ActionResult<IEnumerable<Category>>> GetBooksByCategoryId(Guid categoryId)
    { 
        return Ok(await _categoriesService.GetBooksByCategoryId(categoryId));
    }

    // GET api/v1/categories/{categoryId}
    [HttpGet("{categoryId}")]
    public async Task<IActionResult> GetCategoryById(Guid categoryId)
    {
        var result = await _categoriesService.GetCategoryById(categoryId);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Data);
    }

    // GET api/v1/categories/name/{categoryName}
    [HttpGet("name/{categoryName}")]
    public async Task<IActionResult> GetCategoryByName(string categoryName)
    {
        var result = await _categoriesService.GetCategoryByName(categoryName);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Data);
    }
}
