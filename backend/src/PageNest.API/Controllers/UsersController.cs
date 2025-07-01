using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces.Services;
using PageNest.Domain.Entities;

namespace PageNest.API.Controllers;

[Route($"api/{AppSettings.ApiVersion}/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    // GET api/v1/users
    [Authorize(Policy = AppSettings.AdminRole)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return Ok(await _usersService.GetUsers());
    }

    // GET api/v1/users/{userId}
    [Authorize]
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var result = await _usersService.GetUserById(userId);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Data);
    }

    // PUT api/v1/users/{userId}
    [Authorize]
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] User updateUser)
    {
        var result = await _usersService.UpdateUser(userId, updateUser);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Message);
    }

    // DELETE api/v1/users/{userId}
    [Authorize]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        await _usersService.DeleteUser(userId);

        return NoContent();
    }
}
