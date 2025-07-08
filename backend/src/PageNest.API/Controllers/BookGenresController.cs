using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces.Services;
using PageNest.Domain.Entities;

namespace PageNest.API.Controllers;

[Route($"api/{AppSettings.ApiVersion}/bookgenres")]
[ApiController]
public class BookGenresController : ControllerBase
{
    private readonly IBookGenresService _bookGenresService;

    public BookGenresController(IBookGenresService bookGenresService)
    {
        _bookGenresService = bookGenresService;
    }

    // GET api/v1/bookgenres
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookGenre>>> GetBookGenres()
    {
        return Ok(await _bookGenresService.GetBookGenres());
    }

    // GET api/v1/bookgenres/book/{bookId}
    [Authorize]
    [HttpGet("book/{bookId}")]
    public async Task<ActionResult<IEnumerable<BookGenre>>> GetBookGenresByBookId(Guid bookId)
    {
        return Ok(await _bookGenresService.GetBookGenresByBookId(bookId));
    }

    // GET api/v1/bookgenres/genre/{genreId}
    [Authorize]
    [HttpGet("genre/{genreId}")]
    public async Task<ActionResult<IEnumerable<BookGenre>>> GetBookGenresByGenreId(Guid genreId)
    {
        return Ok(await _bookGenresService.GetBookGenresByGenreId(genreId));
    }
}
