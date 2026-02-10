namespace BITS.Signage.Application.Common.Cqrs;

/// <summary>
/// Base interface for commands (write operations).
/// </summary>
/// <typeparam name="TResult">The type of result returned by the command.</typeparam>
public interface ICommand<out TResult>
{
}

/// <summary>
/// Base interface for commands that don't return a value.
/// </summary>
public interface ICommand
{
}
