using Microsoft.AspNetCore.Mvc;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces.Services;
using PageNest.Domain.Entities;

namespace PageNest.API.Controllers;

[Route($"api/{AppSettings.ApiVersion}/languages")]
public class LanguagesController : ControllerBase
{
    private readonly ILanguagesService _languagesService;

    public LanguagesController(ILanguagesService languagesService)
    {
        _languagesService = languagesService;
    }

    // GET api/v1/languages
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Language>>> GetLanguages()
    {
        return Ok(await _languagesService.GetLanguages());
    }

    // GET api/v1/languages/{languageId}
    [HttpGet("{languageId}")]
    public async Task<IActionResult> GetLanguageById(Guid languageId)
    {
        var result = await _languagesService.GetLanguageById(languageId);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Data);
    }
}
