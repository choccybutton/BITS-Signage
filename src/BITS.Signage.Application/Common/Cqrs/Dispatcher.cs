using Microsoft.Extensions.DependencyInjection;

namespace BITS.Signage.Application.Common.Cqrs;

/// <summary>
/// Dispatcher for routing commands and queries to their handlers.
/// This is a lightweight, open-source CQRS implementation.
/// Uses dependency injection to resolve handlers.
/// </summary>
public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Dispatches a command and returns a result.
    /// </summary>
    public async Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for command type '{commandType.Name}'");

        var method = handlerType.GetMethod("HandleAsync")
            ?? throw new InvalidOperationException($"Handler for '{commandType.Name}' does not have a HandleAsync method");

        var result = method.Invoke(handler, new object[] { command, cancellationToken });

        if (result is Task<TResult> task)
        {
            return await task;
        }

        throw new InvalidOperationException($"Handler for '{commandType.Name}' did not return a Task<TResult>");
    }

    /// <summary>
    /// Dispatches a command without returning a value.
    /// </summary>
    public async Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for command type '{commandType.Name}'");

        var method = handlerType.GetMethod("HandleAsync")
            ?? throw new InvalidOperationException($"Handler for '{commandType.Name}' does not have a HandleAsync method");

        var result = method.Invoke(handler, new object[] { command, cancellationToken });

        if (result is Task task)
        {
            await task;
            return;
        }

        throw new InvalidOperationException($"Handler for '{commandType.Name}' did not return a Task");
    }

    /// <summary>
    /// Dispatches a query and returns a result.
    /// </summary>
    public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var queryType = query.GetType();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for query type '{queryType.Name}'");

        var method = handlerType.GetMethod("HandleAsync")
            ?? throw new InvalidOperationException($"Handler for '{queryType.Name}' does not have a HandleAsync method");

        var result = method.Invoke(handler, new object[] { query, cancellationToken });

        if (result is Task<TResult> task)
        {
            return await task;
        }

        throw new InvalidOperationException($"Handler for '{queryType.Name}' did not return a Task<TResult>");
    }
}
