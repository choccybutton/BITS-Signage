var builder = DistributedApplication.CreateBuilder(args);

// Add infrastructure services
var postgres = builder.AddPostgres("postgres")
    .WithImage("postgres:16-alpine")
    .WithImageTag("16-alpine")
    .WithEnvironment("POSTGRES_USER", "bits_user")
    .WithEnvironment("POSTGRES_PASSWORD", "bits_password")
    .WithEnvironment("POSTGRES_DB", "bits_signage")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("bits-signage");

var redis = builder.AddRedis("redis")
    .WithImage("redis:7-alpine")
    .WithImageTag("7-alpine")
    .WithLifetime(ContainerLifetime.Persistent);

// Add core projects
var api = builder.AddProject<Projects.BITS_Signage_Api>("api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithExternalHttpEndpoints()
    .WaitFor(postgres)
    .WaitFor(redis);

var workers = builder.AddProject<Projects.BITS_Signage_Workers>("workers")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

builder.Build().Run();
