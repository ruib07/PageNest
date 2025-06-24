namespace PageNest.Application.Shared.DTOs;

public static class SeederDTO
{
    public record GenreSeederDTO(Guid Id, string Name);
    public record CategorySeederDTO(Guid Id, string Name);
}
