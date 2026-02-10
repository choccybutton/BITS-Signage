# Custom CQRS Implementation

This folder contains a lightweight, open-source CQRS (Command Query Responsibility Segregation) implementation designed as an alternative to MediatR (which has licensing concerns).

## Overview

CQRS is an architectural pattern that separates **read operations (Queries)** from **write operations (Commands)**. This implementation provides:

- Simple, transparent interfaces
- Dependency injection-based handler routing
- Automatic handler registration
- Zero external dependencies beyond Microsoft.Extensions.DependencyInjection

## Components

### 1. Commands

Commands represent operations that **modify state**.

```csharp
// Define a command that returns a result
public class CreatePlaylistCommand : ICommand<PlaylistDto>
{
    public string Name { get; set; }
    public string TenantId { get; set; }
}

// Define a command that doesn't return a value
public class DeleteAssetCommand : ICommand
{
    public string AssetId { get; set; }
}
```

### 2. Command Handlers

Handlers process commands and execute business logic.

```csharp
// Handler for a command returning a result
public class CreatePlaylistCommandHandler : ICommandHandler<CreatePlaylistCommand, PlaylistDto>
{
    private readonly IPlaylistRepository _repository;

    public CreatePlaylistCommandHandler(IPlaylistRepository repository)
    {
        _repository = repository;
    }

    public async Task<PlaylistDto> HandleAsync(CreatePlaylistCommand command, CancellationToken cancellationToken)
    {
        var playlist = new Playlist(command.Name, command.TenantId);
        await _repository.AddAsync(playlist, cancellationToken);
        return MapToDto(playlist);
    }
}

// Handler for a command with no return value
public class DeleteAssetCommandHandler : ICommandHandler<DeleteAssetCommand>
{
    private readonly IAssetRepository _repository;

    public DeleteAssetCommandHandler(IAssetRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(DeleteAssetCommand command, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(command.AssetId, cancellationToken);
    }
}
```

### 3. Queries

Queries represent operations that **retrieve data without modifying state**.

```csharp
public class GetPlaylistQuery : IQuery<PlaylistDto>
{
    public string PlaylistId { get; set; }
}

public class ListAssetsQuery : IQuery<IEnumerable<AssetDto>>
{
    public string TenantId { get; set; }
    public int Limit { get; set; } = 50;
}
```

### 4. Query Handlers

Handlers process queries and return results.

```csharp
public class GetPlaylistQueryHandler : IQueryHandler<GetPlaylistQuery, PlaylistDto>
{
    private readonly IPlaylistRepository _repository;

    public GetPlaylistQueryHandler(IPlaylistRepository repository)
    {
        _repository = repository;
    }

    public async Task<PlaylistDto> HandleAsync(GetPlaylistQuery query, CancellationToken cancellationToken)
    {
        var playlist = await _repository.GetByIdAsync(query.PlaylistId, cancellationToken);
        if (playlist == null)
            throw new EntityNotFoundException("Playlist not found");
        return MapToDto(playlist);
    }
}
```

### 5. Dispatcher

The `IDispatcher` is the entry point for dispatching commands and queries.

```csharp
public class SomeApiController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public SomeApiController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost("playlists")]
    public async Task<IActionResult> CreatePlaylist(CreatePlaylistRequest request)
    {
        var command = new CreatePlaylistCommand
        {
            Name = request.Name,
            TenantId = request.TenantId
        };

        var result = await _dispatcher.DispatchAsync(command);
        return CreatedAtAction(nameof(GetPlaylist), new { id = result.Id }, result);
    }

    [HttpGet("playlists/{id}")]
    public async Task<IActionResult> GetPlaylist(string id)
    {
        var query = new GetPlaylistQuery { PlaylistId = id };
        var result = await _dispatcher.QueryAsync(query);
        return Ok(result);
    }
}
```

## Registration

In `Program.cs`, register all handlers in an assembly:

```csharp
builder.Services.AddCqrs(typeof(Program).Assembly);
```

This automatically:
1. Registers the `IDispatcher` singleton
2. Scans the assembly for `ICommandHandler<>`, `ICommandHandler<,>`, and `IQueryHandler<,>` implementations
3. Registers each handler as scoped

## How It Works

### Command Dispatching with Result

```
User calls: dispatcher.DispatchAsync<TResult>(command)
    ↓
Dispatcher resolves ICommandHandler<TCommand, TResult> from DI
    ↓
Dispatcher calls handler.HandleAsync(command, cancellationToken)
    ↓
Handler executes business logic and returns TResult
    ↓
Result returned to caller
```

### Command Dispatching without Result

```
User calls: dispatcher.DispatchAsync(command)
    ↓
Dispatcher resolves ICommandHandler<TCommand> from DI
    ↓
Dispatcher calls handler.HandleAsync(command, cancellationToken)
    ↓
Handler executes business logic and completes
    ↓
Task awaited and returned
```

### Query Dispatching

```
User calls: dispatcher.QueryAsync<TResult>(query)
    ↓
Dispatcher resolves IQueryHandler<TQuery, TResult> from DI
    ↓
Dispatcher calls handler.HandleAsync(query, cancellationToken)
    ↓
Handler retrieves and returns TResult
    ↓
Result returned to caller
```

## Benefits vs MediatR

| Aspect | Custom CQRS | MediatR |
|--------|-------------|---------|
| Licensing | Open-source, no concerns | Proprietary, licensing required |
| Size | Minimal, ~100 LOC | Larger library |
| Learning curve | Very simple, easy to debug | Steeper, more "magic" |
| Control | Full transparency | Abstracted behavior |
| Dependencies | Only DI abstractions | External package |
| Performance | Lightweight reflection | Optimized but opaque |

## Error Handling

If a handler is not registered for a command/query, the dispatcher throws `InvalidOperationException`:

```csharp
try
{
    await dispatcher.DispatchAsync(unknownCommand);
}
catch (InvalidOperationException ex)
{
    // "No handler registered for command type 'UnknownCommand'"
}
```

## Testing

Testing is straightforward with this implementation:

```csharp
[Fact]
public async Task CreatePlaylistCommand_ShouldCreatePlaylist()
{
    // Arrange
    var mockRepository = new Mock<IPlaylistRepository>();
    var handler = new CreatePlaylistCommandHandler(mockRepository.Object);
    var command = new CreatePlaylistCommand { Name = "Test", TenantId = "tenant1" };

    // Act
    var result = await handler.HandleAsync(command);

    // Assert
    Assert.NotNull(result);
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Playlist>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

## Best Practices

1. **Keep handlers focused** — One responsibility per handler
2. **Use dependency injection** — Never use `new` in handlers
3. **Validate early** — Validate command/query inputs at the beginning
4. **Async by default** — Always use `async/await`
5. **Map DTOs** — Keep domain entities out of API responses
6. **Handle errors gracefully** — Throw domain exceptions that are caught by API middleware

## Future Enhancements

- Query result caching decorator
- Command validation pipeline
- Event publishing on command completion
- Audit logging decorator
