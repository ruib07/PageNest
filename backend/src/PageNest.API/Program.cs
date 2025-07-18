using PageNest.API.Configurations;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Seed;
using PageNest.Application.Interfaces.Services;
using PageNest.Infrastructure.Data.Repositories;
using PageNest.Infrastructure.Data.Seed;
using PageNest.Infrastructure.Services;
using PageNest.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Seed.json", optional: true, reloadOnChange: false);

ConfigurationManager configuration = builder.Configuration;

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddControllers();
builder.Services.AddCustomSecurity(configuration);
builder.Services.AddCustomDatabaseConfiguration(configuration);

builder.Services.AddScoped<ISeeder, Seeder>();

builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();
builder.Services.AddScoped<IBookGenreRepository, BookGenreRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

builder.Services.AddScoped<ILanguagesService, LanguagesService>();
builder.Services.AddScoped<IBookGenresService, BookGenresService>();
builder.Services.AddScoped<IBooksService, BooksService>();
builder.Services.AddScoped<ICartItemsService, CartItemsService>();
builder.Services.AddScoped<ICategoriesService, CategoriesService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGenresService, GenresService>();
builder.Services.AddScoped<IOrderItemsService, OrderItemsService>();
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IReviewsService, ReviewsService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IPaymentsService, PaymentsService>();
builder.Services.AddScoped<IStripeService, StripeService>();

builder.Services.AddAuthorizationBuilder()
                .AddPolicy(AppSettings.AdminRole, policy => policy.RequireRole(AppSettings.AdminRole))
                .AddPolicy(AppSettings.UserRole, policy => policy.RequireRole(AppSettings.UserRole));

builder.Services.AddCors(options =>
{
    options.AddPolicy(AppSettings.AllowLocalhost,
        builder =>
        {
            builder.WithOrigins(AppSettings.ClientOrigin)
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        });
});

var app = builder.Build();

app.UseCors(AppSettings.AllowLocalhost);
SeedDatabase(app.Services);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static void SeedDatabase(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var seedService = scope.ServiceProvider.GetRequiredService<ISeeder>();

    seedService.SeedGenres();
    seedService.SeedCategories();
    seedService.SeedLanguages();
    seedService.SeedAdmins();
}

public partial class Program { }
