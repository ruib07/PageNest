using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PageNest.Application.Constants;
using PageNest.Application.Shared.DTOs;
using System.Text;

namespace PageNest.API.Configurations;

public static class JwtConfig
{
    public static void AddCustomSecurity(this IServiceCollection services, ConfigurationManager configuration)
    {
        var securitySettings = configuration.GetSection(AppSettings.JwtConfigSection).Get<JwtDTO>();
        services.AddSingleton(securitySettings);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = securitySettings.Issuer,
                ValidAudience = securitySettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey
                (Encoding.UTF8.GetBytes(securitySettings.Key)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };
        });
    }
}
