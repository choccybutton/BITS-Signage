using BITS.Signage.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

// Add Aspire service defaults (service discovery, OpenTelemetry, health checks, resilience)
builder.AddServiceDefaults();

// Configure DbContext (Aspire connection string from service discovery)
var connectionString = builder.Configuration.GetConnectionString("bits-signage")
    ?? "Host=localhost;Port=5432;Database=bits_signage;Username=bits_user;Password=bits_password";
builder.Services.AddDbContext<BitsSignageDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure Redis (Aspire service discovery)
var redisConnectionString = builder.Configuration.GetConnectionString("redis")
    ?? "localhost:6379";
var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

// Add hosted services (to be implemented in Phase 1+)
// builder.Services.AddHostedService<SomeBackgroundWorker>();

var host = builder.Build();

host.Run();
