using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BITS.Signage.Application.Common.Cqrs;

/// <summary>
/// Extension methods for registering CQRS handlers in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the CQRS dispatcher and all command/query handlers in an assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCqrs(this IServiceCollection services, Assembly assembly)
    {
        // Register the dispatcher
        services.AddSingleton<IDispatcher, Dispatcher>();

        // Find and register all command handlers
        var commandHandlerType = typeof(ICommandHandler<,>);
        var commandHandlerOpenType = typeof(ICommandHandler<>);

        foreach (var type in assembly.GetTypes())
        {
            // Skip abstract types and interfaces
            if (type.IsAbstract || type.IsInterface)
                continue;

            // Check for ICommandHandler<TCommand, TResult>
            var commandHandlerInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == commandHandlerType)
                .ToList();

            foreach (var handlerInterface in commandHandlerInterfaces)
            {
                services.AddScoped(handlerInterface, type);
            }

            // Check for ICommandHandler<TCommand>
            var commandVoidHandlerInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == commandHandlerOpenType)
                .ToList();

            foreach (var handlerInterface in commandVoidHandlerInterfaces)
            {
                services.AddScoped(handlerInterface, type);
            }
        }

        // Find and register all query handlers
        var queryHandlerType = typeof(IQueryHandler<,>);

        foreach (var type in assembly.GetTypes())
        {
            // Skip abstract types and interfaces
            if (type.IsAbstract || type.IsInterface)
                continue;

            var queryHandlerInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == queryHandlerType)
                .ToList();

            foreach (var handlerInterface in queryHandlerInterfaces)
            {
                services.AddScoped(handlerInterface, type);
            }
        }

        return services;
    }
}
