namespace BITS.Signage.Application.Common.Cqrs;

/// <summary>
/// Base interface for queries (read operations).
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query.</typeparam>
public interface IQuery<out TResult>
{
}
