using BITS.Signage.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("PostgreSql")
    ?? "Host=localhost;Port=5432;Database=bits_signage;Username=bits_user;Password=bits_password";
builder.Services.AddDbContext<BitsSignageDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Health check endpoint (per implementation plan 0.1)
app.MapHealthChecks("/health");

app.Run();

