using Microsoft.AspNetCore.Mvc;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces.Services;
using PageNest.Domain.Entities;

namespace PageNest.API.Controllers;

[Route($"api/{AppSettings.ApiVersion}/genres")]
[ApiController]
public class GenresController : ControllerBase
{
    private readonly IGenresService _genresService;

    public GenresController(IGenresService genresService)
    {
        _genresService = genresService;
    }

    // GET api/v1/genres
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
    {
        return Ok(await _genresService.GetGenres());
    }

    // GET api/v1/genres/books/{genreId}
    [HttpGet("books/{genreId}")]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooksByGenreId(Guid genreId) 
    {
        return Ok(await _genresService.GetBooksByGenreId(genreId));
    }

    // GET api/v1/genres/{genreId}
    [HttpGet("{genreId}")]
    public async Task<IActionResult> GetGenreById(Guid genreId)
    {
        var result = await _genresService.GetGenreById(genreId);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Data);
    }

    // GET api/v1/genres/name/{genreName}
    [HttpGet("name/{genreName}")]
    public async Task<IActionResult> GetGenreByName(string genreName)
    {
        var result = await _genresService.GetGenreByName(genreName);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Data);
    }
}
