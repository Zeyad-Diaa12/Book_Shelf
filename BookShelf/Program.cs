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

// Update the connection string logic to handle Railway environment
string connectionString;
// Check if running in Docker (environment variable set in docker-compose.yml)
bool isRunningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
if (isRunningInContainer)
{
    connectionString = builder.Configuration.GetConnectionString("RailwayConnection")
        ?? builder.Configuration.GetConnectionString("DockerConnection")
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string is not configured.");

    // Ensure SSL is enabled for Railway
    if (connectionString.Contains("Host=") && !connectionString.Contains("SSL Mode="))
    {
        connectionString += ";SSL Mode=Require;Trust Server Certificate=true";
    }

    Console.WriteLine("Using Railway or Docker database connection");
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

// Apply migrations automatically during startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log the exception (you can replace this with a proper logging mechanism)
        Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
    }
}

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
