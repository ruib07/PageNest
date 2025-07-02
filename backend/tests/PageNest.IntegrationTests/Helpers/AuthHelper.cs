using Microsoft.Extensions.DependencyInjection;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.TestUtils.Builders;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PageNest.IntegrationTests.Helpers;

public static class AuthHelper
{
    public static async Task<User> AuthenticateUser(HttpClient client, IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = UsersBuilder.CreateAdmin();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var request = new AuthenticationDTO.SignIn.Request(user.Email, "User@Password-123");
        var response = await client.PostAsJsonAsync("/api/v1/auth/signin", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Authentication failed: {response.StatusCode} - {errorContent}");
        }

        var content = await response.Content.ReadFromJsonAsync<AuthenticationDTO.SignIn.Response>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content.AccessToken);

        return user;
    }
}
