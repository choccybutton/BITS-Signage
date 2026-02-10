namespace BITS.Signage.Api.Common;

/// <summary>
/// Helper for parsing and applying common filter parameters.
/// Supports: q (search), status, scope, type filters.
/// </summary>
public class FilteringHelper
{
    public string? SearchQuery { get; set; }
    public string? Status { get; set; }
    public string? Scope { get; set; }
    public string? Type { get; set; }
    public Dictionary<string, string> CustomFilters { get; set; } = new();

    /// <summary>
    /// Extracts filtering parameters from query string.
    /// </summary>
    public static FilteringHelper FromQuery(IQueryCollection query)
    {
        var filter = new FilteringHelper
        {
            SearchQuery = query["q"].ToString(),
            Status = query["status"].ToString(),
            Scope = query["scope"].ToString(),
            Type = query["type"].ToString()
        };

        // Clean up empty strings
        if (string.IsNullOrWhiteSpace(filter.SearchQuery))
            filter.SearchQuery = null;
        if (string.IsNullOrWhiteSpace(filter.Status))
            filter.Status = null;
        if (string.IsNullOrWhiteSpace(filter.Scope))
            filter.Scope = null;
        if (string.IsNullOrWhiteSpace(filter.Type))
            filter.Type = null;

        return filter;
    }

    /// <summary>
    /// Checks if any filters are applied.
    /// </summary>
    public bool HasFilters =>
        !string.IsNullOrEmpty(SearchQuery) ||
        !string.IsNullOrEmpty(Status) ||
        !string.IsNullOrEmpty(Scope) ||
        !string.IsNullOrEmpty(Type) ||
        CustomFilters.Count > 0;
}
