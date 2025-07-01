namespace PageNest.Application.Shared.DTOs;

public static class SeederDTO
{
    public record GenreSeederDTO(Guid Id, string Name);
    public record CategorySeederDTO(Guid Id, string Name);
    public record LanguageSeederDTO(Guid Id, string Name, string Code, string CultureCode);
    public record AdminSeederDTO(Guid Id, string Name, string Email, string Password);
}
