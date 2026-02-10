namespace BITS.Signage.Application.Common.Cqrs;

/// <summary>
/// Interface for the CQRS dispatcher that routes commands and queries to their handlers.
/// This is a simple, open-source alternative to MediatR (which is licensed).
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Dispatches a command and returns a result.
    /// </summary>
    /// <typeparam name="TResult">The type of result.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result from the command handler.</returns>
    Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a command without returning a value.
    /// </summary>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a query and returns a result.
    /// </summary>
    /// <typeparam name="TResult">The type of result.</typeparam>
    /// <param name="query">The query to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result from the query handler.</returns>
    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
