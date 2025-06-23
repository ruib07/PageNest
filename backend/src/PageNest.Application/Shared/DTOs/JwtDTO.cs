namespace PageNest.Application.Shared.DTOs;

public record JwtDTO(string Issuer, string Audience, string Key);
