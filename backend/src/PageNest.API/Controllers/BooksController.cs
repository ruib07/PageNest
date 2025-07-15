using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;

namespace PageNest.API.Controllers;

[Route($"api/{AppSettings.ApiVersion}/books")]
public class BooksController : ControllerBase
{
    private readonly IBooksService _booksService;

    public BooksController(IBooksService booksService)
    {
        _booksService = booksService;
    }

    // GET api/v1/books
    [Authorize(Policy = AppSettings.AdminRole)]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
    {
        return Ok(await _booksService.GetBooks());
    }

    // GET api/v1/books/author/{authorName}
    [HttpGet("author/{authorName}")]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooksByAuthor(string authorName)
    {
        return Ok(await _booksService.GetBooksByAuthor(authorName));
    }

    // GET api/v1/books/{bookId}
    [HttpGet("{bookId}")]
    public async Task<IActionResult> GetBookById(Guid bookId)
    {
        var result = await _booksService.GetBookById(bookId);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Data);
    }

    // GET api/v1/books/title/{bookTitle}
    [HttpGet("title/{bookTitle}")]
    public async Task<IActionResult> GetBookByTitle(string bookTitle)
    {
        var result = await _booksService.GetBookByTitle(bookTitle);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Data);
    }

    // POST api/v1/books
    [Authorize(Policy = AppSettings.AdminRole)]
    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] Book book)
    {
        var result = await _booksService.CreateBook(book);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        var response = new ResponsesDTO.Creation(result.Message, result.Data.Id);

        return CreatedAtAction(nameof(GetBookById), new { bookId = result.Data.Id }, response);
    }

    // PUT api/v1/books/{bookId}
    [Authorize(Policy = AppSettings.AdminRole)]
    [HttpPut("{bookId}")]
    public async Task<IActionResult> UpdateBook(Guid bookId, [FromBody] Book updateBook)
    {
        var result = await _booksService.UpdateBook(bookId, updateBook);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Message);
    }

    // DELETE api/v1/books/{bookId}
    [Authorize(Policy = AppSettings.AdminRole)]
    [HttpDelete("{bookId}")]
    public async Task<IActionResult> DeleteBook(Guid bookId)
    {
        await _booksService.DeleteBook(bookId);

        return NoContent();
    }
}
