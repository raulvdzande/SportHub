using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SportHub.API.Application.Interfaces;
using SportHub.API.Application.Services;
using SportHub.API.Configuration;
using SportHub.API.Domain.Entities;
using SportHub.API.Infrastructure.Authentication;
using SportHub.API.Infrastructure.Data.DbContext;
using SportHub.API.Infrastructure.Services;
using SportHub.API.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<SeedStaffUserOptions>(builder.Configuration.GetSection(SeedStaffUserOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();
builder.Services.AddScoped<IInstructorService, InstructorService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPhotoStorageService, LocalPhotoStorageService>();
builder.Services.AddScoped<StaffUserSeeder>();
builder.Services.AddScoped<IPasswordHasher<StaffUser>, PasswordHasher<StaffUser>>();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var jwtKey = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7071",
                "http://localhost:5003",
                "https://localhost:5003",
                "http://localhost:5002",
                "https://localhost:5002")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStaticFiles();
app.UseCors("WebClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<StaffUserSeeder>();
    await seeder.SeedAsync();
}

app.Run();