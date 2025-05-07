using BookShelf.Application.Interfaces;
using BookShelf.Application.Mappings;
using BookShelf.Application.Services;
using BookShelf.Domain.Interfaces;
using BookShelf.Infrastructure.Data;
using BookShelf.Infrastructure.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.NpgSql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext with the appropriate connection string based on environment
string connectionString;
// Check if running in Docker (environment variable set in docker-compose.yml)
bool isRunningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
if (isRunningInContainer)
{
    connectionString = builder.Configuration.GetConnectionString("DockerConnection")
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string is not configured.");
    Console.WriteLine("Using Docker database connection");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string is not configured.");
    Console.WriteLine("Using local database connection");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "service" });

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Users/Login";
        options.LogoutPath = "/Users/Logout";
        options.AccessDeniedPath = "/Users/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
    });

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookClubRepository, BookClubRepository>();

// Add Application Services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReadingService, ReadingService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IBookClubService, BookClubService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    if (!isRunningInContainer)
    {
        app.UseHsts();
    }
}

// Only use HTTPS redirection when not running in a container
if (!isRunningInContainer)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

// Add authentication and session middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Map health check endpoint
app.MapHealthChecks("/health");

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
