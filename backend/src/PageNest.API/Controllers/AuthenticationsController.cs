using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PageNest.Application.Constants;
using PageNest.Application.Helpers;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;
using PageNest.Infrastructure.Data.Context;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PageNest.API.Controllers;

[Route($"api/{AppSettings.ApiVersion}/auth")]
[ApiController]
public class AuthenticationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IUsersService _usersService;
    private readonly IEmailService _emailService;
    private readonly JwtDTO _jwtDTO;

    public AuthenticationsController(ApplicationDbContext context, IUsersService usersService, IEmailService emailService, JwtDTO jwtDTO)
    {
        _context = context;
        _usersService = usersService;
        _emailService = emailService;
        _jwtDTO = jwtDTO;
    }

    // POST api/v1/auth/signup
    [AllowAnonymous]
    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody, Required] AuthenticationDTO.SignUp.Request signupRequest)
    {
        if (signupRequest == null) return BadRequest("All fields are required.");

        if (string.IsNullOrWhiteSpace(signupRequest.Name) || 
            string.IsNullOrWhiteSpace(signupRequest.Email) ||
            string.IsNullOrWhiteSpace(signupRequest.Password)) return BadRequest("Invalid input.");

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == signupRequest.Email);

        if (existingUser != null) return Conflict("Email already in use.");

        var newUser = new User()
        {
            Id = Guid.NewGuid(),
            Name = signupRequest.Name,
            Email = signupRequest.Email,
            PasswordHash = signupRequest.Password,
            Role = Roles.User
        };

        var createdUser = await _usersService.CreateUser(newUser);
        var response = new ResponsesDTO.Creation(createdUser.Message, createdUser.Data.Id);

        return CreatedAtAction(nameof(SignUp), new { userId = createdUser.Data.Id }, response);
    }

    // POST api/v1/auth/signin
    [AllowAnonymous]
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] AuthenticationDTO.SignIn.Request signinRequest)
    {
        if (signinRequest == null) return BadRequest("Email and password are mandatory.");
        if (string.IsNullOrWhiteSpace(signinRequest.Email)) return BadRequest("Email is required.");
        if (string.IsNullOrWhiteSpace(signinRequest.Password)) return BadRequest("Password is required.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == signinRequest.Email);

        if (user == null) return Unauthorized("User not found.");

        if (!PasswordHasherHelper.VerifyPassword(signinRequest.Password, user.PasswordHash))
            return Unauthorized("Incorrect password.");

        var oldTokens = await _context.RefreshTokens
                                      .Where(ot => ot.UserId == user.Id && !ot.IsRevoked && ot.ExpiresAt > DateTime.UtcNow)
                                      .ToListAsync();

        foreach (var token in oldTokens)
        {
            token.IsRevoked = true;
        }

        var (Token, ExpiresAt) = GenerateAccessToken(user, user.Role);
        var refreshToken = new RefreshToken()
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return Ok(new AuthenticationDTO.SignIn.Response(Token, refreshToken.Token, AppSettings.TokenType, ExpiresAt));
    }

    // POST api/v1/auth/refresh
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] AuthenticationDTO.SignIn.RefreshTokenRequest request)
    {
        var storedToken = await _context.RefreshTokens
                                        .Include(rt => rt.User)
                                        .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken)) 
            return BadRequest("Refresh token is required.");

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token.");

        storedToken.IsRevoked = true;

        var user = storedToken.User;
        var role = user.Role;

        var newRefreshToken = new RefreshToken()
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        var (Token, ExpiresAt) = GenerateAccessToken(user, role);

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return Ok(new AuthenticationDTO.SignIn.Response(Token, newRefreshToken.Token, AppSettings.TokenType, ExpiresAt));
    }

    // POST api/v1/auth/logout
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] AuthenticationDTO.SignIn.RefreshTokenRequest request)
    {
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

        if (token == null) return NotFound("Token not found.");

        token.IsRevoked = true;
        await _context.SaveChangesAsync();

        return Ok("Logged out.");
    }

    // POST api/v1/auth/recover-password
    [HttpPost("recover-password")]
    public async Task<IActionResult> RecoverPasswordSendEmail([FromBody] PasswordResetDTO.Request request)
    {
        var token = await _usersService.GeneratePasswordResetToken(request.Email);

        await _emailService.SendPasswordResetEmail(request.Email, token);

        return Ok();
    }

    // PUT api/v1/auth/update-password
    [HttpPut("update-password")]
    public async Task<IActionResult> UpdatePassword([FromBody] PasswordResetDTO.UpdatePassword updatePassword)
    {
        var result = await _usersService.UpdatePassword(updatePassword.Token, updatePassword.NewPassword, updatePassword.ConfirmNewPassword);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Message);
    }

    #region Private Methods

    private (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, Roles role)
    {
        var claims = new List<Claim>()
        {
            new(AppSettings.ClaimId, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Name),
            new(AppSettings.ClaimEmail, user.Email),
            new(ClaimTypes.Role, role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtDTO.Key));
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwtDTO.Issuer,
            Audience = _jwtDTO.Audience,
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), tokenDescriptor.Expires!.Value);
    }

    #endregion
}
