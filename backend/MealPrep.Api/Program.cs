using MealPrep.Api.Data;
using MealPrep.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Set ConnectionStrings:DefaultConnection before starting the API.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString));

builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

var frontendOrigins = builder.Configuration
    .GetSection("FrontendOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(frontendOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGroup("/api/auth").MapIdentityApi<ApplicationUser>();

app.MapGet("/api/auth/me", async (
    HttpContext httpContext,
    UserManager<ApplicationUser> userManager) =>
{
    var user = await userManager.GetUserAsync(httpContext.User);

    return user is null
        ? Results.Unauthorized()
        : Results.Ok(new
        {
            id = user.Id,
            email = user.Email,
            displayName = user.DisplayName
        });
}).RequireAuthorization();

app.Run();
