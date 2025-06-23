namespace PageNest.Application.Shared.DTOs;

public static class PasswordResetDTO
{
    public record Request(string Email);
    public record UpdatePassword(string Token, string NewPassword, string ConfirmNewPassword);
}
