namespace PageNest.Application.Constants;

public class AppSettings
{
    public const string JwtConfigSection = "Jwt";
    public const string DatabaseConfigSection = "PageNestDB";
    public const string AllowLocalhost = "AllowLocalhost";
    public const string ClientOrigin = "http://localhost:3000";
    public const string ApiVersion = "v1";
    public const string TokenType = "Bearer";
    public const string ClaimId = "id";
    public const string UserRole = "User";
    public const string AdminRole = "Admin";
    public const string AdminUserRole = "Admin&User";

    public const string GenreSeedSection = "SeedGenres";
    public const string CategorySeedSection = "SeedCategories";
    public const string AdminSeedSection = "SeedAdmins";
}
