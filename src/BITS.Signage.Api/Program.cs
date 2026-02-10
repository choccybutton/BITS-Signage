using BITS.Signage.Api.Common;
using BITS.Signage.Api.Middleware;
using BITS.Signage.Api.Services;
using BITS.Signage.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Configure JWT Authentication (Phase 0.4)
var jwtKey = builder.Configuration["Jwt:SecretKey"] ?? "default-insecure-key-change-in-production-12345678";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "https://bits-signage.local";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "https://bits-signage.local";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(10)
        };
    });

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("PostgreSql")
    ?? "Host=localhost;Port=5432;Database=bits_signage;Username=bits_user;Password=bits_password";
builder.Services.AddDbContext<BitsSignageDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure Redis (Phase 0.4 - Rate limiting)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? "localhost:6379";
var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

// Register Phase 0.4 services
builder.Services.AddScoped<RequestContext>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

// Configure HTTP request pipeline (Phase 0.4 - Middleware order is critical)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Phase 0.4 Middleware Pipeline (in correct order)
app.UseMiddleware<ErrorHandlingMiddleware>(); // Must be first to catch all exceptions
app.UseMiddleware<ETagMiddleware>(); // Check preconditions before authentication
app.UseAuthentication(); // Validate JWT tokens
app.UseMiddleware<TenantIsolationMiddleware>(); // Extract tenant and user info
app.UseMiddleware<RateLimitingMiddleware>(); // Rate limit after auth

// Health check endpoint (per implementation plan 0.1)
app.MapHealthChecks("/health");

app.Run();

