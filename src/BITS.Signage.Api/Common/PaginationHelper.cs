namespace BITS.Signage.Api.Common;

/// <summary>
/// Helper for cursor-based pagination.
/// Provides standard pagination parameters and response headers.
/// </summary>
public class PaginationHelper
{
    public const int DEFAULT_LIMIT = 50;
    public const int MAX_LIMIT = 200;

    public string? Cursor { get; set; }
    public int Limit { get; set; } = DEFAULT_LIMIT;

    /// <summary>
    /// Extracts pagination parameters from query string.
    /// </summary>
    public static PaginationHelper FromQuery(IQueryCollection query)
    {
        var cursor = query["cursor"].ToString();
        var limitStr = query["limit"].ToString();

        if (!int.TryParse(limitStr, out var limit))
        {
            limit = DEFAULT_LIMIT;
        }

        // Clamp limit between 1 and MAX_LIMIT
        limit = Math.Max(1, Math.Min(limit, MAX_LIMIT));

        return new PaginationHelper
        {
            Cursor = string.IsNullOrWhiteSpace(cursor) ? null : cursor,
            Limit = limit
        };
    }

    /// <summary>
    /// Adds pagination headers to response.
    /// </summary>
    public void AddResponseHeaders(HttpResponse response, string? nextCursor)
    {
        response.Headers.Add("X-Pagination-Limit", Limit.ToString());
        if (!string.IsNullOrEmpty(nextCursor))
        {
            response.Headers.Add("X-Pagination-Cursor", nextCursor);
        }
    }
}

/// <summary>
/// Paginated response wrapper.
/// </summary>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int Total { get; set; }
    public string? NextCursor { get; set; }
    public int Limit { get; set; }
}
